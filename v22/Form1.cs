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
            vakidateuser(Login,Password);
        }
        private void fdf()
        {
            string dbPath = System.IO.Path.GetFullPath("UserBase.db");
        }
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
        //private string hashemail(string Email)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(Email))
        //        {
        //            using (SHA256 sha256 = SHA256.Create())
        //            {
        //                byte[] bytess = sha256.ComputeHash(Encoding.UTF8.GetBytes(Email));
        //                StringBuilder stringbulder = new StringBuilder();
        //                for (int i = 0; i < bytess.Length; i++)
        //                {
        //                    stringbulder.Append(bytess[i].ToString("x2"));
        //                }
        //                return stringbulder.ToString();
        //            }
        //        }
        //        throw new Exception("Ошибка");
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Ошибка" + ex.Message);
        //        throw;
        //    }
        //}
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
        private void button2_Click(object sender, EventArgs e)
        {
            zametkacreate zametkacreate = new zametkacreate();
            zametkacreate.Show();
            this.Hide();
        }
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
        private void button3_Click(object sender, EventArgs e)
        {
            settings set = new settings();
            set.Show();
            this.Hide();
        }
    }
}

