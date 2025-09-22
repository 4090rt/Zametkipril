using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace v22
{
    public partial class genericpicture : Form
    {
        public genericpicture()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(28, 31, 40);
            this.ForeColor = Color.Gainsboro;

            // Настройка PictureBox
            pictureBox1.BackColor = Color.FromArgb(40, 44, 52);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            // Настройка ProgressBar
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.MarqueeAnimationSpeed = 30;
        }
        private async Task dowload(string[] args)
        {
            try
            {
                // Показываем прогресс-бар
                progressBar1.Visible = true;
                progressBar1.Style = ProgressBarStyle.Marquee;

                using (HttpClient http = new HttpClient())
                {
                    // Простые заголовки для избежания ошибок
                    http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                    http.Timeout = TimeSpan.FromSeconds(10); // Таймаут 10 секунд

                    // Начнем с простого placeholder сервиса
                    string[] imageUrls = {
                        "https://via.placeholder.com/800x600/FF6B6B/FFFFFF?text=Red+Image", // Placeholder красный
                        "https://via.placeholder.com/800x600/4ECDC4/FFFFFF?text=Teal+Image", // Placeholder бирюзовый
                        "https://via.placeholder.com/800x600/45B7D1/FFFFFF?text=Blue+Image", // Placeholder синий
                        "https://via.placeholder.com/800x600/96CEB4/FFFFFF?text=Green+Image", // Placeholder зеленый
                        "https://via.placeholder.com/800x600/FECA57/FFFFFF?text=Yellow+Image" // Placeholder желтый
                    };
                    Random rand = new Random();
                    string random = imageUrls[rand.Next(imageUrls.Length)];

                    // Добавляем задержку между запросами
                    await Task.Delay(1000).ConfigureAwait(false);

                    try
                    {
                        var gg = await http.GetAsync(random).ConfigureAwait(false);
                        if (gg.IsSuccessStatusCode)
                        {
                            byte[] bytes = await gg.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                            // Отображаем изображение в PictureBox
                            using (MemoryStream ms = new MemoryStream(bytes))
                            {
                                Image image = Image.FromStream(ms);
                                pictureBox1.Image?.Dispose(); // Освобождаем предыдущее изображение
                                pictureBox1.Image = new Bitmap(image);
                            }

                            // Скрываем прогресс-бар при успешной загрузке
                            progressBar1.Visible = false;
                            
                            // Всегда показываем информацию о загрузке
                            label1.Text = $"Изображение загружено! Размер: {bytes.Length / 1024} KB";

                            // Сохраняем файл только если CheckBox отмечен
                            if (this.checkBox1 != null && this.checkBox1.Checked)
                            {
                                string filename = $"downloaded_image_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                                string filetaph = @"C:\Users\artem\Downloads";
                                if (!Directory.Exists(filetaph))
                                {
                                    Directory.CreateDirectory(filetaph);
                                }
                                string fullpath = Path.Combine(filetaph, filename);
                                File.WriteAllBytes(fullpath, bytes);
                                label1.Text += " - Файл сохранен";
                            }
                        }
                        else
                        {
                            // Если не удалось загрузить, сразу создаем локальное изображение
                            label1.Text = "Создаем локальное изображение...";
                            CreateLocalImage();
                        }
                    }
                    catch (HttpRequestException httpEx)
                    {
                        // Специальная обработка HTTP ошибок
                        label1.Text = "Ошибка сети, создаем локальное изображение...";
                        CreateLocalImage();
                    }
                    catch (TaskCanceledException tcEx)
                    {
                        // Обработка таймаута
                        label1.Text = "Таймаут запроса, создаем локальное изображение...";
                        CreateLocalImage();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
                label1.Text = "Создаем локальное изображение...";
                CreateLocalImage();
            }
            finally
            {
                // Скрываем прогресс-бар
                progressBar1.Visible = false;
            }
        }

        private async Task TryAlternativeSources(HttpClient http)
        {
            try
            {
                // Дополнительные надежные источники
                string[] alternativeUrls = {
                    "https://via.placeholder.com/800x600/FF6B6B/FFFFFF?text=Red+Image", // Placeholder красный
                    "https://via.placeholder.com/800x600/4ECDC4/FFFFFF?text=Teal+Image", // Placeholder бирюзовый
                    "https://via.placeholder.com/800x600/45B7D1/FFFFFF?text=Blue+Image", // Placeholder синий
                    "https://via.placeholder.com/800x600/96CEB4/FFFFFF?text=Green+Image", // Placeholder зеленый
                    "https://via.placeholder.com/800x600/FECA57/FFFFFF?text=Yellow+Image", // Placeholder желтый
                    "https://via.placeholder.com/800x600/E74C3C/FFFFFF?text=Dark+Red", // Placeholder темно-красный
                    "https://via.placeholder.com/800x600/9B59B6/FFFFFF?text=Purple", // Placeholder фиолетовый
                    "https://via.placeholder.com/800x600/1ABC9C/FFFFFF?text=Emerald" // Placeholder изумрудный
                };

                foreach (string url in alternativeUrls)
                {
                    try
                    {
                        var response = await http.GetAsync(url).ConfigureAwait(false);
                        if (response.IsSuccessStatusCode)
                        {
                            byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                            // Отображаем изображение в PictureBox
                            using (MemoryStream ms = new MemoryStream(bytes))
                            {
                                Image image = Image.FromStream(ms);
                                pictureBox1.Image?.Dispose();
                                pictureBox1.Image = new Bitmap(image);
                            }

                            // Скрываем прогресс-бар при успешной загрузке
                            progressBar1.Visible = false;
                            
                            // Всегда показываем информацию о загрузке
                            label1.Text = $"Изображение загружено из альтернативного источника! Размер: {bytes.Length / 1024} KB";

                            // Сохраняем файл только если CheckBox отмечен
                            if (this.checkBox1 != null && this.checkBox1.Checked)
                            {
                                string filename = $"downloaded_image_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                                string filetaph = @"C:\Users\artem\Downloads";
                                if (!Directory.Exists(filetaph))
                                {
                                    Directory.CreateDirectory(filetaph);
                                }
                                string fullpath = Path.Combine(filetaph, filename);
                                File.WriteAllBytes(fullpath, bytes);
                                label1.Text += " - Файл сохранен";
                            }
                            return; // Успешно загрузили, выходим
                        }
                    }
                    catch
                    {
                        continue; // Пробуем следующий URL
                    }
                }

                // Если все альтернативы не сработали, создаем локальное изображение
                label1.Text = "Создаем локальное изображение...";
                CreateLocalImage();
            }
            catch (Exception ex)
            {
                label1.Text = "Ошибка при попытке загрузки альтернативных источников";
                MessageBox.Show("Ошибка альтернативной загрузки: " + ex.Message);
            }
        }

        private void CreateLocalImage()
        {
            try
            {
                // Скрываем прогресс-бар при создании локального изображения
                progressBar1.Visible = false;
                
                // Создаем локальное изображение с градиентом
                Random rand = new Random();
                Color[] colors = {
                    Color.FromArgb(255, 107, 107), // Красный
                    Color.FromArgb(78, 205, 196),  // Бирюзовый
                    Color.FromArgb(69, 183, 209),  // Синий
                    Color.FromArgb(150, 206, 180), // Зеленый
                    Color.FromArgb(254, 202, 87),  // Желтый
                    Color.FromArgb(155, 89, 182)   // Фиолетовый
                };

                Color bgColor = colors[rand.Next(colors.Length)];
                Color textColor = Color.White;

                Bitmap bitmap = new Bitmap(800, 600);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    // Заливаем фон градиентом
                    using (LinearGradientBrush brush = new LinearGradientBrush(
                        new Point(0, 0), new Point(800, 600), bgColor, Color.FromArgb(bgColor.R / 2, bgColor.G / 2, bgColor.B / 2)))
                    {
                        g.FillRectangle(brush, 0, 0, 800, 600);
                    }

                    // Добавляем текст
                    using (Font font = new Font("Arial", 48, FontStyle.Bold))
                    using (StringFormat format = new StringFormat())
                    {
                        format.Alignment = StringAlignment.Center;
                        format.LineAlignment = StringAlignment.Center;
                        g.DrawString("Локальное изображение", font, new SolidBrush(textColor),
                            new RectangleF(0, 0, 800, 600), format);
                    }
                }

                // Отображаем созданное изображение
                pictureBox1.Image?.Dispose();
                pictureBox1.Image = bitmap;

                label1.Text = "Создано локальное изображение!";

                // Сохраняем если нужно
                if (this.checkBox1 != null && this.checkBox1.Checked)
                {
                    string filename = $"local_image_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                    string filetaph = @"C:\Users\artem\Downloads";
                    if (!Directory.Exists(filetaph))
                    {
                        Directory.CreateDirectory(filetaph);
                    }
                    string fullpath = Path.Combine(filetaph, filename);
                    bitmap.Save(fullpath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    label1.Text += " - Файл сохранен";
                }
            }
            catch (Exception ex)
            {
                // Скрываем прогресс-бар даже при ошибке
                progressBar1.Visible = false;
                label1.Text = "Ошибка создания локального изображения";
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            // Блокируем кнопку во время загрузки
            button4.Enabled = false;
            button4.Text = "Загрузка...";
            
            try
            {
                await dowload(new string[0]);
            }
            finally
            {
                // Разблокируем кнопку после завершения
                button4.Enabled = true;
                button4.Text = "Загрузить картинку";
            }
        }

    }
}