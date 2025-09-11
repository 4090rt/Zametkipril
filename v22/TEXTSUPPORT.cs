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
        public TEXTSUPPORT(string Login, string Email)
        {
            InitializeComponent();
            _Login = Login;
            _Email = Email;
            // Инициализация apiKey - вам нужно будет установить правильное значение
            apiKey = "your-api-key-here"; // Замените на ваш реальный API ключ
            
        }

        private async Task<bool> EnsureOllamaAsync()
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
            catch { }

            // 3) Пытаемся запустить сервер Ollama в пользовательском режиме
            try
            {
                var ollamaExe = ResolveOllamaPath();
                if (!string.IsNullOrWhiteSpace(ollamaExe))
                {
                    await RunProcessAsync(ollamaExe, "serve", 2000);
                }
            }
            catch { }

            // 4) Повторная проверка доступности (несколько попыток)
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    using (var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) })
                    {
                        var res = await http.GetAsync("http://localhost:11434/api/tags");
                        if (res.IsSuccessStatusCode) break;
                    }
                    await Task.Delay(800);
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

        private string ResolveOllamaPath()
        {
            if (!string.IsNullOrWhiteSpace(_ollamaExeCached)) return _ollamaExeCached;
            // 1) Пользовательская переменная окружения (можно задать вручную)
            var fromEnv = Environment.GetEnvironmentVariable("OLLAMA_PATH");
            if (!string.IsNullOrWhiteSpace(fromEnv) && File.Exists(fromEnv))
                return _ollamaExeCached = fromEnv;
            // 2) Типичные пути установки
            var candidates = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Ollama", "ollama.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Ollama", "ollama.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Ollama", "ollama.exe"),
                "ollama.exe",
                "ollama" // на случай, если доступно в PATH как имя без расширения
            };
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


        public async Task<string> AskAiAsync(string userQuestion, string apiKey)
        {
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
                        model = "qwen2.5:0.5b-instruct-q4_K_M",
                        messages = new object[]
                        {
                            new { role = "system", content = "Ты помощник. Отвечай кратко и на русском языке." },
                            new { role = "user", content = questionText }
                        },
                        stream = false
                    };
                    var json = JsonSerializer.Serialize(payload);
                    using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        // Правильное имя заголовка без пробела: Content-Language
                        content.Headers.ContentLanguage.Add("ru-RU");
                        // Настраиваем заголовки ЗАРАНЕЕ, до отправки запроса
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
                                using (var rep = await client.PostAsync("http://localhost:11434/api/chat", content))
                                {
                                    foreach (var header in rep.Headers)
                                    {
                                        MessageBox.Show($"Заголовок: {header.Key} = {string.Join(", ", header.Value)}");
                                    }
                                    if (rep.Headers.TryGetValues("Server", out var serverValues))
                                    {
                                        MessageBox.Show($"Сервер: {string.Join(", ", serverValues)}");
                                    }
                                    if (rep.Headers.TryGetValues("Date", out var datevalues))
                                    {
                                        MessageBox.Show($"Date: {string.Join(", ", datevalues)}");
                                    }
                                    if (rep.IsSuccessStatusCode)
                                    {
                                        MessageBox.Show($"Content type: {rep.Content.Headers.ContentType}");
                                        MessageBox.Show($"Lenght: {rep.Content.Headers.ContentLength}");
                                        MessageBox.Show($"Content Location: {rep.Content.Headers.ContentLocation}");
                                        MessageBox.Show($"Content Encoding: {rep.Content.Headers.ContentEncoding}");
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
                                            label1.Text = contentStr;
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
        public bool dobavitzap()
        {
            string dbPath = System.IO.Path.GetFullPath("UserBase.db");
            string Login = _Login;
            string Email = _Email;
            string answer = textBox1.Text;
            string zap = answer;
            if (System.IO.File.Exists("UserBase.db"))
            {
                try
                {
                    using (var das = new SQLiteConnection(dbPath))
                    {
                        das.Open();
                        var gg = new SQLiteCommand("INSERT INTO [Users] (Login,userQuestion,answer,Email) VALUES (@L,@U,@A,@E)", das);
                        {
                            gg.Parameters.AddWithValue("@L", Login);
                            gg.Parameters.AddWithValue("@U", answer);
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
        private async Task dowload1(string[] args)
        {
            try
            {
                using (HttpClient clienthttp = new HttpClient())
                {
                    clienthttp.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");//Заголовок запроса
                    clienthttp.DefaultRequestHeaders.Add("Accept", "text/plain,text/html,application/json,text/*");// заголовк запроса
                    clienthttp.DefaultRequestHeaders.Add("Accept-language", "ru-RU,ru;q=0.9,en;q=0.8");//заголовок запроса
                    clienthttp.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json", 1.0));
                    clienthttp.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html", 0.9));
                    clienthttp.DefaultRequestHeaders.Referrer = new Uri("https://github.com/"); // заголовок запроса
                    var hgt = await clienthttp.GetAsync("https://www.gutenberg.org/files/11/11-0.txt");
                    if (hgt.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Concept Type: {hgt.Content.Headers.ContentType}");
                        MessageBox.Show($"Language: {hgt.Content.Headers.ContentLanguage}");
                        MessageBox.Show($"Encoding: {hgt.Content.Headers.ContentEncoding}");
                        MessageBox.Show($"Disposition: {hgt.Content.Headers.ContentDisposition}");
                        MessageBox.Show($"Lenght: {hgt.Content.Headers.ContentLength}");
                        MessageBox.Show($"Location: {hgt.Content.Headers.ContentLength}");
                        foreach (var hear in hgt.Headers)
                        {
                            MessageBox.Show($"Заголовок {hear.Key} = {string.Join(",", hear.Value)}");
                        }
                        if (hgt.Headers.TryGetValues("Server", out var serverValue))
                        {
                            MessageBox.Show($"Server= {string.Join(",", serverValue)}");
                        }
                        if (hgt.Headers.TryGetValues("Date", out var datevalue))
                        {
                            MessageBox.Show($"Date {string.Join(",", datevalue)}");
                        }
                        string filename = "downloaded_file.txt";
                        var drs = hgt.Content.Headers.ContentDisposition;
                        if (drs != null && !string.IsNullOrEmpty(drs.FileName))
                        {
                            filename = drs.FileName.Trim('"');
                        }
                        string filetaph = @"C:\Users\artem\Downloads";
                        if (!Directory.Exists(filetaph))
                        {
                            Directory.CreateDirectory(filetaph);
                        }
                        string fullpath = Path.Combine(filetaph, filename);
                        byte[] bytes = await hgt.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(fullpath, bytes);
                        MessageBox.Show($"рАЗМЕР ФАЙЛА БТ {bytes.Length}");
                        MessageBox.Show($"Файл успешно хзагружен по пути {fullpath}");
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка {(int)hgt.StatusCode} {hgt.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("EROR" + ex.Message);
            }
  
        }
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
                                [Login] TEXT NOT NULL UNIQUE,
                                [userQuestion] TEXT NOT NULL,
                                [answer] TEXT NOT NULL,
                                [Email] TEXT NOT NULL
                            )", das);
                    createTableCommand.ExecuteNonQuery();
                    return true;
                }
                //         
            }
            catch(Exception ex)
            {
                MessageBox.Show("Ошибка создания бд" + ex.Message);
                return false;
            }
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            var ok = await EnsureOllamaAsync();
            if (!ok)
            {
                MessageBox.Show("Не удалось запустить Ollama. Убедитесь, что Ollama установлена и доступна в PATH.");
                return;
            }
            var answer = await AskAiAsync(textBox1.Text, apiKey);
            if (!string.IsNullOrWhiteSpace(answer))
            {
                label1.Text = answer;
            }
        }
    }
}
