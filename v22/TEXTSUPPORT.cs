using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Security.Cryptography;

namespace v22
{

    public partial class TEXTSUPPORT : Form
    {
        private string userQuestion; 
        private string apiKey;
        private const string OllamaModel = "qwen2.5:0.5b-instruct-q4_K_M"; // модель по умолчанию
        private string _ollamaExeCached;
        private readonly string _Login;
        private readonly string _Email;
        private static bool _ollamaInstalled = false; // статический флаг для установки

        public TEXTSUPPORT(string Login, string Email)
        {
            InitializeComponent();
            _Login = Login;
            _Email = Email;
            bdnew();
            openollama();          
        }
        private void openollama()// октрытие5 ollama и проверка есть ли она - в противном случае устанавливаем и помечаем как установленную
        {
            // Проверяем, не установлена ли уже Ollama
            if (_ollamaInstalled) return;
            
            // Проверяем, есть ли уже установленная Ollama в системе
            if (IsOllamaAlreadyInstalled())
            {
                _ollamaInstalled = true;
                return;
            }
            
            string basedirectory = AppDomain.CurrentDomain.BaseDirectory;
            string fullpath = Path.Combine(basedirectory, "Resuorces", "OllamaSetup.exe");
                try
                {
                var psi = new ProcessStartInfo
                {
                    FileName = fullpath,
                    UseShellExecute = true,
                    Verb = "runas"
                };
                    Process.Start(psi);
                    _ollamaInstalled = true; // Помечаем как установленную
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Возникла ошибка запуска Ollama: " + ex.Message);
                }
        }
        
        private bool IsOllamaAlreadyInstalled()// проверка на наличие установленной ollama
        {
            // Проверяем типичные пути установки Ollama
            var candidates = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Ollama", "ollama.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Ollama", "ollama.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Ollama", "ollama.exe")
            };
            
            foreach (var path in candidates)
            {
                if (File.Exists(path))
                {
                    return true;
                }
            }           
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "ollama",
                    Arguments = "--version",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                using (var process = Process.Start(psi))
                {
                    if (process != null)
                    {
                        process.WaitForExit(3000); // Ждем максимум 3 секунды
                        return process.ExitCode == 0;
                        MessageBox.Show("Ollama уже установлена!");
                    }
                }
            }
            catch
            {
                dowloadollama();
            }
            
            return false;
        }
        private void dowloadollama()// запуск установщика ollama
        {
            string basedirectory = AppDomain.CurrentDomain.BaseDirectory;
            string fullpath = Path.Combine(basedirectory, "Resuorces", "Ollama", "ollama.exe");
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = fullpath,
                        UseShellExecute = true,
                        Verb = "runas"
                    };
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Возникла ошибка запуска Ollama: " + ex.Message);
                }
        }
        private async Task<bool> EnsureOllamaAsync()// проверка - включенный ли сервер, отвечает ли он  также если сервер ответ  - сразу запрос - в противном случае запускет установку
        {
            // 1) Проверяем доступность локального API
            try
            {
                using (var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) }) //Создаём HttpClient с коротким таймаутом, чтобы быстро понять, жив ли API.
                {
                    var res = await http.GetAsync("http://localhost:11434/api/tags");//Пингуем локальный API Ollama.
                    if (res.IsSuccessStatusCode)
                    {
                        // 2) Подтягиваем (или быстро проверяем наличие) модели
                        var ollamaExe = ResolveOllamaPath();
                        if (!string.IsNullOrWhiteSpace(ollamaExe))
                        {
                            await RunProcessAsync(ollamaExe, $"pull {OllamaModel}", 120000);//Если сервер уже работает, сразу подтягиваем модель (если нет — скачает; если есть — мгновенно проверит) и выходим.
                        }
                        return true;
                    }
                }
            }
            catch { return false; }

            // 5) Подтягиваем модель (если уже есть — быстро вернётся)
            try
            {
                var ollamaExe = ResolveOllamaPath();
                if (!string.IsNullOrWhiteSpace(ollamaExe))
                {
                    await RunProcessAsync(ollamaExe, $"pull {OllamaModel}", 120000);
                }
                return true;
            }
            catch { return false; }
        }

        private string ResolveOllamaPath()// метод делает гибким поиск исполняемого файла
        {
            if (!string.IsNullOrWhiteSpace(_ollamaExeCached)) return _ollamaExeCached;//Если путь уже был найден ранее, возвращает его из кэша.
            // 1) Пользовательская переменная окружения (можно задать вручную)
            var fromEnv = Environment.GetEnvironmentVariable("OLLAMA_PATH");//Ищет пользовательскую переменную окружения OLLAMA_PATH.
            if (!string.IsNullOrWhiteSpace(fromEnv) && File.Exists(fromEnv))
                return _ollamaExeCached = fromEnv;
            // 2) Поиск в стандартных путях установки
            var candidates = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Ollama", "ollama.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Ollama", "ollama.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Ollama", "ollama.exe"),
                "ollama.exe",
                "ollama" // на случай, если доступно в PATH как имя без расширения
            };
            //Проверяет существование файла для каждого пути.
            foreach (var p in candidates)
            {
                try { if (File.Exists(p)) return _ollamaExeCached = p; } catch { }
            }
            return null;
        }

        private static async Task<int> RunProcessAsync(string fileName, string arguments, int timeoutMs)
        {
            var psi = new ProcessStartInfo //Готовим старт: без окна, без шелла, с редиректом вывода/ошибок.
            {
                FileName = fileName,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using (var p = new Process { StartInfo = psi })
            {
                p.Start();
                var exited = await Task.Run(() => p.WaitForExit(timeoutMs));//Ждём завершения процесса с таймаутом (не блокируем UI).
                if (!exited)
                {
                    try { p.Kill(); } catch { }
                    throw new TimeoutException($"Процесс {fileName} {arguments} превысил таймаут {timeoutMs} мс");//Если не уложился — прибиваем и кидаем исключение.
                }
                return p.ExitCode;
            }
        }

        static string hash(string requestBodyHash)
        {
            try
            {
                using (SHA256 sHA256 = SHA256.Create())
                {
                    byte[] bytes = sHA256.ComputeHash(Encoding.UTF8.GetBytes(requestBodyHash));
                    StringBuilder build = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        build.Append(bytes[0].ToString("x2"));
                    }
                    return build.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return "EROR";
            }
        }

        public async Task<string> AskAiAsync(string userQuestion, string apiKey)
        {
            // Не сбрасываем состояние чекбоксов, пользователь сам выбирает режим
            userQuestion = textBox1.Text;

            using (HttpClient client = new HttpClient())
            {
                string questionText = textBox1.Text;
                if (string.IsNullOrEmpty(questionText))
                {
                    MessageBox.Show("Заполните все поля");
                    return "Ошибка";
                }
                try
                {
                    // Вызов локального сервера Ollama без токена
                    var payload = new
                    {
                        model = OllamaModel,
                        messages = new object[]
                        {
                            new { role = "system", content = "Ты помощник. Отвечай кратко и на русском языке." },
                            new { role = "user", content = questionText }
                        },
                        stream = false
                    };

                    var payload2 = new
                    {
                        model = OllamaModel,
                        messages = new object[]
                        {
                            new { role = "system", content = "Ты помощник. Отвечай не объемно, но раскрывая тему, на английском языке." },
                            new { role = "user", content = questionText }
                        },
                        stream = false
                    };

                    // Выбор активного payload по чекбоксам: по умолчанию payload, если выбран checkBox2 — используем payload2
                    var usePayload2 = (this.checkBox2 != null && this.checkBox2.Checked);
                    var selectedPayload = usePayload2 ? payload2 : payload;
                    string endpoint = "http://localhost:11434/api/chat"; // оба варианта совместимы с chat API

                    var jsonSelected = JsonSerializer.Serialize(selectedPayload);
                    var key = "fgfgfg";
                    string time = DateTime.UtcNow.ToString("O");
                    string met = $"POST";
                    string apiPath = "/api/chat";
                    string requestBodyHash = hash(jsonSelected);
                    using (var content = new StringContent(jsonSelected, Encoding.UTF8, "application/json"))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        content.Headers.ContentLanguage.Add("ru-RU");
                        client.DefaultRequestHeaders.Accept.Clear();
                        var acceptJson = new MediaTypeWithQualityHeaderValue("application/json") { Quality = 1.0 };
                        var acceptHtml = new MediaTypeWithQualityHeaderValue("text/html") { Quality = 0.9 };
                        client.DefaultRequestHeaders.Accept.Add(acceptJson);
                        client.DefaultRequestHeaders.Accept.Add(acceptHtml);
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en;q=0.8");
                        client.DefaultRequestHeaders.Referrer = new Uri("https://github.com/");
                        int retryCount = 0;
                        int maxRetries = 3;
                        while (retryCount < maxRetries)
                        {
                            try
                            {
                                using (var rep = await client.PostAsync(endpoint, content))
                                {
                                    //foreach (var header in rep.Headers)
                                    //{
                                    //    MessageBox.Show($"Заголовок: {header.Key} = {string.Join(", ", header.Value)}");
                                    //}
                                    //if (rep.Headers.TryGetValues("Server", out var serverValues))
                                    //{
                                    //    MessageBox.Show($"Сервер: {string.Join(", ", serverValues)}");
                                    //}
                                    //if (rep.Headers.TryGetValues("Date", out var datevalues))
                                    //{
                                    //    MessageBox.Show($"Date: {string.Join(", ", datevalues)}");
                                    //}
                                    if (rep.IsSuccessStatusCode)
                                    {
                                        //MessageBox.Show($"Content type: {rep.Content.Headers.ContentType}");
                                        //MessageBox.Show($"Lenght: {rep.Content.Headers.ContentLength}");
                                        //MessageBox.Show($"Content Location: {rep.Content.Headers.ContentLocation}");
                                        //MessageBox.Show($"Content Encoding: {rep.Content.Headers.ContentEncoding}");
                                        var result = await rep.Content.ReadAsStringAsync();
                                        using (var doc = JsonDocument.Parse(result))
                                        {
                                            var root = doc.RootElement;
                                            string contentStr = null;
                                            if (root.TryGetProperty("message", out var messageEl) && messageEl.ValueKind == JsonValueKind.Object && messageEl.TryGetProperty("content", out var contentEl) && contentEl.ValueKind == JsonValueKind.String)
                                            {
                                                contentStr = contentEl.GetString();
                                            }
                                            if (string.IsNullOrWhiteSpace(contentStr))
                                            {
                                                // Фолбэк для других форматов
                                                if (root.TryGetProperty("response", out var respEl) && respEl.ValueKind == JsonValueKind.String)
                                                {
                                                    contentStr = respEl.GetString();
                                                }
                                            }
                                            if (string.IsNullOrWhiteSpace(contentStr))
                                            {
                                                contentStr = "Пустой ответ";
                                            }
                                            textBox2.Multiline = true;
                                            textBox2.ScrollBars = ScrollBars.Both;
                                            textBox2.Dock = DockStyle.Fill;
                                            textBox2.WordWrap = true;
                                            textBox2.ReadOnly = true;
                                            textBox2.Text = contentStr;
                                            return contentStr;
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show($"Ошибка: {(int)rep.StatusCode} {rep.ReasonPhrase}");
                                        return MessageBox.Show("eror").ToString();
                                    }
                                }
                            }
                            catch (HttpRequestException) when (retryCount < maxRetries - 1)
                            {
                                retryCount++;
                                await Task.Delay(1000 * retryCount); // Ждём 1, 2, 3 секунды...
                            }
                        }
                        return "Ошибка";
                    }
                }
                catch (Exception ex)
                {
                    return MessageBox.Show("eror" + ex.Message).ToString();
                }
            }
        }
        //private async Task dowload1(string[] args)
        //{
        //    try
        //    {
        //        using (HttpClient clienthttp = new HttpClient())
        //        {
        //            clienthttp.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");//Заголовок запроса
        //            clienthttp.DefaultRequestHeaders.Add("Accept", "text/plain,text/html,application/json,text/*");// заголовк запроса
        //            clienthttp.DefaultRequestHeaders.Add("Accept-language", "ru-RU,ru;q=0.9,en;q=0.8");//заголовок запроса
        //            clienthttp.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json", 1.0));
        //            clienthttp.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html", 0.9));
        //            clienthttp.DefaultRequestHeaders.Referrer = new Uri("https://github.com/"); // заголовок запроса
        //            var hgt = await clienthttp.GetAsync("https://www.gutenberg.org/files/11/11-0.txt");
        //            if (hgt.IsSuccessStatusCode)
        //            {
        //                //MessageBox.Show($"Concept Type: {hgt.Content.Headers.ContentType}");
        //                //MessageBox.Show($"Language: {hgt.Content.Headers.ContentLanguage}");
        //                //MessageBox.Show($"Encoding: {hgt.Content.Headers.ContentEncoding}");
        //                //MessageBox.Show($"Disposition: {hgt.Content.Headers.ContentDisposition}");
        //                //MessageBox.Show($"Lenght: {hgt.Content.Headers.ContentLength}");
        //                //MessageBox.Show($"Location: {hgt.Content.Headers.ContentLength}");
        //                //foreach (var hear in hgt.Headers)
        //                //{
        //                //    MessageBox.Show($"Заголовок {hear.Key} = {string.Join(",", hear.Value)}");
        //                //}
        //                //if (hgt.Headers.TryGetValues("Server", out var serverValue))
        //                //{
        //                //    MessageBox.Show($"Server= {string.Join(",", serverValue)}");
        //                //}
        //                //if (hgt.Headers.TryGetValues("Date", out var datevalue))
        //                //{
        //                //    MessageBox.Show($"Date {string.Join(",", datevalue)}");
        //                //}
        //                string filename = "downloaded_file.txt";
        //                var drs = hgt.Content.Headers.ContentDisposition;
        //                if (drs != null && !string.IsNullOrEmpty(drs.FileName))
        //                {
        //                    filename = drs.FileName.Trim('"');
        //                }
        //                string filetaph = @"C:\Users\artem\Downloads";
        //                if (!Directory.Exists(filetaph))
        //                {
        //                    Directory.CreateDirectory(filetaph);
        //                }
        //                string fullpath = Path.Combine(filetaph, filename);
        //                byte[] bytes = await hgt.Content.ReadAsByteArrayAsync();
        //                File.WriteAllBytes(fullpath, bytes);
        //                MessageBox.Show($"рАЗМЕР ФАЙЛА БТ {bytes.Length}");
        //                MessageBox.Show($"Файл успешно хзагружен по пути {fullpath}");
        //            }
        //            else
        //            {
        //                MessageBox.Show($"Ошибка {(int)hgt.StatusCode} {hgt.ReasonPhrase}");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("EROR" + ex.Message);
        //    }
  
        //}
        public bool bdnew()
        {
            string dbPath = System.IO.Path.GetFullPath("UserBase.db");
            try
            {
                using (var das = new SQLiteConnection($"Data Source={dbPath}"))
                {
                    das.Open();
                    
                    var createTableCommand = new SQLiteCommand(
                       @"CREATE TABLE IF NOT EXISTS [UsersZAp] (
                                [ID] INTEGER PRIMARY KEY AUTOINCREMENT,
                                [Login] TEXT NOT NULL,
                                [userQuestion] TEXT NOT NULL,
                                [answer] TEXT NOT NULL,
                                [Email] TEXT
                            )", das);
                    createTableCommand.ExecuteNonQuery();
                    // Добавляем поле Email если его нет (для существующих таблиц)
                    try
                    {
                        var alterTableCommand = new SQLiteCommand("ALTER TABLE [UsersZAp] ADD COLUMN [Email] TEXT", das);
                        alterTableCommand.ExecuteNonQuery();
                    }
                    catch
                    {
                    }                 
                    return true;
                }     
            }
            catch(Exception ex)
            {
                MessageBox.Show("Ошибка создания бд" + ex.Message);
                return false;
            }
        }

        public bool dobavitzap(string contentStr = null)
        {
            if (string.IsNullOrEmpty(contentStr)) return false;

            string dbPath = System.IO.Path.GetFullPath("UserBase.db");
            string Login = _Login;
            string Email = _Email;
            string questin = textBox1.Text;
            string zap = contentStr;

            if (System.IO.File.Exists("UserBase.db"))
            {
                try
                {
                    using (var das = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        das.Open();
                        var gg = new SQLiteCommand("INSERT INTO [UsersZAp] (Login,userQuestion,answer,Email) VALUES (@L,@U,@A,@E)", das);
                        {
                            gg.Parameters.AddWithValue("@L", Login);
                            gg.Parameters.AddWithValue("@U", questin);
                            gg.Parameters.AddWithValue("@A", zap);
                            gg.Parameters.AddWithValue("@E", Email);
                            gg.ExecuteNonQuery();
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка Сохранения данных запроса" + ex.Message);
                    return false;
                }
            }
            else
            {
                MessageBox.Show("БД не найдена");
                return false;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var ok = await EnsureOllamaAsync();
            // Если не удалось автоматически поднять, не блокируем — пробуем отправить запрос прямо сейчас
            if (!ok)
            {
                // опциональное уведомление, но без остановки
                MessageBox.Show("Локальный сервер Ollama не запущен автоматически. Выполняю попытку запроса...");
            }
            var answer = await AskAiAsync(textBox1.Text, apiKey);
            if (!string.IsNullOrWhiteSpace(answer))
            {
                textBox2.Text = answer;
                dobavitzap(answer);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        // Переменные для перетаскивания окна
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private void panelTop_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void panelTop_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void panelTop_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Clear();
        }
    }
}
