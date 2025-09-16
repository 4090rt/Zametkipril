using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace v22
{
    public partial class genericpicture : Form
    {
        public genericpicture()
        {
            InitializeComponent();
        }
        private async Task dowload1()
        {
            try
            {
                using (HttpClient http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                    http.DefaultRequestHeaders.Add("Accept", "text/plain,text/html,application/json,text/*");
                    http.DefaultRequestHeaders.Add("Accept-language", "ru-RU,ru;q=0.9,en;q=0.8");
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json", 1.0));
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html", 0.9));
                    http.DefaultRequestHeaders.Referrer = new Uri("https://github.com/");
                    var httpzapros = await http.GetAsync("https://www.gutenberg.org/files/11/11-0.txt");
                    if (httpzapros.IsSuccessStatusCode)
                    {
                        string Filename = "Dowload_picture";
                        var head = httpzapros.Content.Headers.ContentDisposition;
                        if (head != null && !string.IsNullOrEmpty(head.FileName))
                        {
                            Filename = head.FileName.Trim('"');
                        }
                        string filepath = @"C:\Users\artem\Downloads";
                        if (Directory.Exists(filepath))
                        {
                            Directory.CreateDirectory(filepath);
                        }
                        string fullpath = Path.Combine(filepath, Filename);
                        byte[] bytes = await httpzapros.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(fullpath, bytes);
                        MessageBox.Show($"Файл установлен! Его размер {bytes.Length}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("EROR" + ex.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            dowload1();
        }
    }
}
