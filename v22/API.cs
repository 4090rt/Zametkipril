using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Protobuf;

namespace v22
{
    public partial class API : Form
    {
        private readonly string _City;
        private readonly string _Login;
        private string _resultcity;
        public API(string Login,string City)
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(28, 31, 40);
            this.ForeColor = Color.Gainsboro;
            _Login = Login;
            _City = City;
            pokazcity();
            if (string.IsNullOrEmpty(Login))
            {
                MessageBox.Show("Ошибка: логин не передан в API");
                return;
            }
            weather();
            pokaztime();
            label1.ForeColor = Color.Gainsboro;
            label2.ForeColor = Color.Gainsboro;
            try { pictureBox1.BackColor = Color.FromArgb(28, 31, 40); } catch {}
        }

        private bool pokazcity()
        {
            //MessageBox.Show($"pokazcity: _Login = '{_Login}'");
            if (!System.IO.File.Exists("UserBase.db"))
            {
                MessageBox.Show("Бд не найден!");
                return false;
            }
            string vars = "Data Source=UserBase.db";
            using (var das = new SQLiteConnection(vars))
            { 
                das.Open();
                using (var city = new SQLiteCommand("SELECT City FROM Users WHERE Login = @L LIMIT 1",das))
                {
                    city.Parameters.AddWithValue("@L", _Login);
                    //MessageBox.Show($"SQL запрос: SELECT City FROM Users WHERE Login = '{_Login}'");
                    var result = city.ExecuteScalar();
                    //MessageBox.Show($"SQL результат: {result}");
                    if (result != null && result != DBNull.Value && !string.IsNullOrWhiteSpace(result.ToString()))
                    {
                        _resultcity = result.ToString();
                        return true;
                    }
                    else
                    {
                        MessageBox.Show($"Город не найден для логина: {_Login}");
                        return false;
                    }
                }
            }
        }
        private async Task pokaztime()
        {
            string APIKEY = "818684b83cb44c9f87e6a189bf48bf83";
            string City = string.IsNullOrWhiteSpace(_resultcity) ? _City : _resultcity;
            string Country = "Russia";
            string URL = $"https://api.ipgeolocation.io/timezone?apiKey={APIKEY}&location={Uri.EscapeDataString(City)},%20{Uri.EscapeDataString(Country)}";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage recpon = await client.GetAsync(URL);
                    recpon.EnsureSuccessStatusCode();
                    string result = await recpon.Content.ReadAsStringAsync();

                    using (JsonDocument json = JsonDocument.Parse(result))
                    {
                        JsonElement root = json.RootElement;
                        string timeinfo = null;
                        if (root.TryGetProperty("date_time_txt", out var dateTimeTxt) && dateTimeTxt.ValueKind == JsonValueKind.String)
                        {
                            timeinfo = dateTimeTxt.GetString();
                        }
                        else if (root.TryGetProperty("date", out var dateEl) && root.TryGetProperty("time", out var timeEl))
                        {
                            timeinfo = $"{dateEl.GetString()} {timeEl.GetString()}";
                        }
                        else if (root.TryGetProperty("time_24", out var time24) && time24.ValueKind == JsonValueKind.String)
                        {
                            timeinfo = time24.GetString();
                        }

                        label2.Text = string.IsNullOrWhiteSpace(timeinfo) ? "Время недоступно" : timeinfo;
                    }
                }
            }
            catch (Exception ex)
            { 
                MessageBox.Show(ex.Message);
            }
        }


        private async Task weather()
        {
            string City = _resultcity;
            string APIKey = "6f7b4977c06cf7032b4f49790617fc3d";
            string URL = $"https://api.openweathermap.org/data/2.5/weather?q={City}&appid={APIKey}&units=metric&lang=ru";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage recpon = await client.GetAsync(URL);
                    recpon.EnsureSuccessStatusCode();
                    string res = await recpon.Content.ReadAsStringAsync();

                    using (JsonDocument json = JsonDocument.Parse(res))
                    {
                        JsonElement root = json.RootElement;
                        JsonElement weather = root.GetProperty("weather")[0];
                        JsonElement main = root.GetProperty("main");
                        string description = weather.GetProperty("description").GetString();

                        if (description == "пасмурно")
                        {
                            try
                            {
                                pictureBox1.Image = Properties.Resources.pasmurno1;
                                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                                pictureBox1.Width = 30;
                                pictureBox1.Height = 30;
                                pictureBox1.Location = new Point(15, 10);
                                label1.Location = new Point(45, 15);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Не удалось загрузить изображение" + ex.Message);
                            }
                        }

                        if (description == "ясно")
                        {
                            try
                            {
                                pictureBox1.Image = Properties.Resources.iacno1;
                                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                                pictureBox1.Width = 30;
                                pictureBox1.Height = 30;
                                pictureBox1.Location = new Point(8, 10);
                                label1.Location = new Point(45, 15);

                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Не удалось загрузить изображение" + ex.Message);
                            }
                        }
                        if (description == "облачно с прояснениями")
                        {
                            try
                            {
                                pictureBox1.Image = Properties.Resources.oblacno1;
                                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                                pictureBox1.Width = 30;
                                pictureBox1.Height = 30;
                                pictureBox1.Location = new Point(8, 10);
                                label1.Location = new Point(45, 15);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Не удалось загрузить изображение" + ex.Message);
                            }
                        }

                        if (description == "переменная облачность")
                        {
                            try
                            {
                                pictureBox1.Image = Properties.Resources.peremennaiaoblach1;
                                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                                pictureBox1.Width = 30;
                                pictureBox1.Height = 30;
                                pictureBox1.Location = new Point(8, 10);
                                label1.Location = new Point(45, 15);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Не удалось загрузить изображение" + ex.Message);
                            }
                        }


                        if (description == "дождь")
                        {
                            try
                            {
                                pictureBox1.Image = Properties.Resources.osadki1;
                                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                                pictureBox1.Width = 30;
                                pictureBox1.Height = 30;
                                pictureBox1.Location = new Point(8, 10);
                                label1.Location = new Point(45, 15);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Не удалось загрузить изображение" + ex.Message);
                            }
                        }

                        if (description == "гроза")
                        {
                            try
                            {
                                pictureBox1.Image = Properties.Resources.Groza1;
                                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                                pictureBox1.Width = 30;
                                pictureBox1.Height = 30;
                                pictureBox1.Location = new Point(8, 10);
                                label1.Location = new Point(45, 15);

                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Не удалось загрузить изображение" + ex.Message);
                            }
                        }

                        if (description == "снег")
                        {
                            try
                            {
                                pictureBox1.Image = Properties.Resources.sneg1;
                                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                                pictureBox1.Width = 30;
                                pictureBox1.Height = 30;
                                pictureBox1.Location = new Point(8, 10);
                                label1.Location = new Point(45, 10);

                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Не удалось загрузить изображение" + ex.Message);
                            }
                        }

                        if (description == "небольшой проливной дождь")
                        {
                            try
                            {
                                pictureBox1.Image = Properties.Resources.osadki1;
                                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                                pictureBox1.Width = 30;
                                pictureBox1.Height = 30;
                                pictureBox1.Location = new Point(8, 10);
                                label1.Location = new Point(45, 15);

                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Не удалось загрузить изображение" + ex.Message);
                            }
                        }
                        string weatherInfo =
                            $"{City} + {main.GetProperty("temp").GetDouble()}°C\n"
                        //$"- Влажность: {main.GetProperty("humidity").GetInt32()}%\n" +
                        //$"- Скорость ветра: {wind.GetProperty("speed").GetDouble()} м/с\n" +
                         + $"- {weather.GetProperty("description").GetString()}";
                        label1.Text = weatherInfo;
                        try { label1.ForeColor = Color.Gainsboro; } catch {}
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка" + ex.Message);
            }
        }

        private void API_Load(object sender, EventArgs e)
        {

        }
    }
}
