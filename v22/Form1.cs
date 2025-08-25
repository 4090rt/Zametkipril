using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using Org.BouncyCastle.Tls;
using System.Security.Cryptography;
using System.IO;
using System.Text.RegularExpressions;
namespace v22
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            fdf();
            string Password = textBox2.Text;
            string Login = textBox1.Text;
        }
        // Путь к бд
        private void fdf()
        {
            string dbPath = System.IO.Path.GetFullPath("UserBase.db");
        }
        // Хэширование пароля
        private string hashpqpass(string Password)
        {
            try
            {
                if (!string.IsNullOrEmpty(Password))
                {
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(Password));
                        StringBuilder stringBuilder = new StringBuilder();
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            stringBuilder.Append(bytes[i].ToString("x2"));
                        }
                        return stringBuilder.ToString();
                    }
                }
                throw new Exception("Ошибка");
            }

            catch (Exception ex)
            {
                MessageBox.Show("Ошибка" + ex.Message);
                throw;
            }
        }
        // добавление данных юзера в бд
        private void dobavitnewuser()
        {
            try
            {
                if (File.Exists("UserBase.db"))
                {
                    string Password = textBox2.Text;
                    string hashpassword = hashpqpass(Password);
                    string Login = textBox1.Text;
                    string City = textBox4.Text;
                    if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(City))
                    {
                        MessageBox.Show("Заполните все поля!");
                        return;
                    }
                    if (Password.Length < 8)
                    {
                        MessageBox.Show("Пароль не может быть меньше 8 символов!");
                        return;
                    }
                    bool reg = Regex.IsMatch(Password, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");
                    if (reg == false) 
                    {
                        MessageBox.Show("Пароль Должен содержать специальные символы! (!@#$%^&*()_+=\\[{\\]};:<>|./?,-)");
                        return;
                    }
                    string vars = "Data Source=UserBase.db";
                    using (var das = new SQLiteConnection(vars))
                    {
                        das.Open();
                        var cmd2 = new SQLiteCommand(
                            "INSERT INTO [Users] (Login,Password,City) VALUES (@L,@P,@C)", das);
                        cmd2.Parameters.AddWithValue("@L", textBox1.Text);
                        cmd2.Parameters.AddWithValue("@P", hashpassword);
                        cmd2.Parameters.AddWithValue("@C", textBox4.Text);
                        cmd2.ExecuteNonQuery();
                        MessageBox.Show("Данные сохранены!");

                    }
                }
                else
                {
                    MessageBox.Show("БД не найдена");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления" + ex.Message);
            }
        }
        // при нажатии на кнопку создается новое бд для хранения данных
        private void button1_Click(object sender, EventArgs e)
        {
            string dbPath = System.IO.Path.GetFullPath("UserBase.db");
              try
              {
                  using (var das = new SQLiteConnection($"Data Source={dbPath}"))
                  {
                        das.Open();
                        var createTableCommand = new SQLiteCommand(
                           @"CREATE TABLE IF NOT EXISTS [Users] (
                                [ID] INTEGER PRIMARY KEY AUTOINCREMENT,
                                [Login] TEXT NOT NULL,
                                [Password] TEXT NOT NULL,
                                [City] TEXT NOT NULL
                            )", das);
                        createTableCommand.ExecuteNonQuery();
                  }
                  dobavitnewuser();
              }
              catch (Exception ex)
              {
              MessageBox.Show("Ошибка" + ex.Message);
              }
        }
        // переход на форму добавления заметки
        private void button2_Click(object sender, EventArgs e)
        {
            zametkacreate zametkacreate = new zametkacreate();
            zametkacreate.Show();
            this.Hide();
        }
        // Метод валидации пользователя
        private bool vakidateuser(string Login, string Password) 
        {
            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("Введите Логин и пароль ");
                return false;
            }
            string dbPath = System.IO.Path.GetFullPath("UserBase.db");
            if (!File.Exists(dbPath))
            {
                MessageBox.Show("БД не найдена");
                return false;
            }
            string hashpass = hashpqpass(Password);
            using (var das = new SQLiteConnection($"Data Source={dbPath}"))
            {
                das.Open();
                try
                {
                    using (var gg = new SQLiteCommand(@"SELECT Password FROM Users WHERE Login = @L LIMIT 1", das))
                    {
                        gg.Parameters.AddWithValue("@L", Login);
                        var value = gg.ExecuteScalar();
                        if (value == null || value == DBNull.Value)
                        {
                            return false;
                        }
                        string pasd = Convert.ToString(value);
                        return string.Equals(pasd, hashpass, StringComparison.Ordinal);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка" + ex.Message);

                }
            }
            return false;
        }
        // переход на форму настроек
        private void button3_Click(object sender, EventArgs e)
        {
            string Login = textBox1.Text;
            string Password = textBox2.Text;
            settings set = new settings(Login,Password);
            set.Show();
            this.Hide();

        }
        // переход на форму настроек после валидации с доп проверками и обработчиками ошибок
        private void button4_Click(object sender, EventArgs e)
        {
            string Login = textBox1.Text;
            string Password = textBox2.Text;
            string City = textBox4.Text;
            vakidateuser(Login,Password);
            try
            {
                if (vakidateuser(Login, Password) == true)
                {
                    settings setting = new settings(Login,Password);
                    setting.Show();
                    this.Hide();
                }
                else 
                {
                    MessageBox.Show("Ошибка ввода логина или пароля");
                    return;              
                }
            }
            catch (Exception ex) 
            {
                    MessageBox.Show("Не удалось войти", "Ошибка!" + MessageBoxIcon.Error + ex.Message);
            }
        }
    }
}
