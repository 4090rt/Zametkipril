using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace v22
{

    public partial class TEXTSUPPORT : Form
    {
        private string userQuestion; 
        private string apiKey;
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

        public async Task<string> AskAiAsync(string userQuestion, string apiKey)
        {

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
                    client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Ошибка на этапе авторизации Bearer token" + ex.Message);
                    return ex.Message;
                }
                try
                {
                    var payload = new
                    {
                        model = "",
                        messages = new object[]
                        {
                        new {role = "system", content = "Ты помощник.Отвечай кратко и на русском языке"},
                        new {role = "user", content = questionText }
                        }
                    };

                    var json = JsonSerializer.Serialize(payload);
                    using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        content.Headers.Add("Content Language", "en-US");
                        using (var rep = await client.PostAsync("", content))
                        {
                            client.DefaultRequestHeaders.Accept.Clear();
                            var acceptJson = new MediaTypeWithQualityHeaderValue("application/json") { Quality = 1.0 };
                            var acceptHtml = new MediaTypeWithQualityHeaderValue("text/html") { Quality = 0.9 };
                            client.DefaultRequestHeaders.Accept.Add(acceptJson);
                            client.DefaultRequestHeaders.Accept.Add(acceptHtml);
                            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en;q=0.8");
                            client.DefaultRequestHeaders.Referrer = new Uri("https://github.com/");
                            foreach (var header in rep.Headers)
                            {
                                MessageBox.Show($"Заголовок: {header.Key} = {string.Join(", ", header.Value)}");
                            }
                            if (rep.Headers.TryGetValues("Server", out var serverValues))
                            {
                                MessageBox.Show($"Сервер: {string.Join(", ", serverValues)}");
                            }
                            if (rep.Headers.TryGetValues("Server", out var datevalues))
                            {
                                MessageBox.Show($"Date: {string.Join(", ", datevalues)}");
                            }
                            try
                            {
                                rep.EnsureSuccessStatusCode();
                                MessageBox.Show($"Content type: {rep.Content.Headers.ContentType}");
                                MessageBox.Show($"Lenght: {rep.Content.Headers.ContentLength}");
                                MessageBox.Show($"Content Location: {rep.Content.Headers.ContentLocation}");
                                MessageBox.Show($"Content type: {rep.Content.Headers.ContentEncoding}");
                                var result = await rep.Content.ReadAsStringAsync();
                                using (var doc = JsonDocument.Parse(result))
                                {
                                    var answer = doc.RootElement;
                                    var choise = answer.GetProperty("choices")[0];
                                    var message = answer.GetProperty("message");
                                    var content1 = answer.GetProperty("content");
                                    var none = answer.GetString() ?? "пустой ответ";
                                    return label1.Text = answer.GetString();
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Ошибка на этапе прочтения или парсинга" + ex.Message);
                                return ex.Message;
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Ошибка на этапе создания объекта или сериализации" + ex.Message);
                    return ex.Message;
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
        private void button1_Click(object sender, EventArgs e)
        {
            AskAiAsync(textBox1.Text, apiKey);
        }
    }
}
