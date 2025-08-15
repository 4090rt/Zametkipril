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
    public partial class Form1: Form
    {
        public Form1()
        {
            InitializeComponent();
            fdf();
        }
        private void fdf()
        {
            string dbPath = System.IO.Path.GetFullPath("UserBase.db");
            MessageBox.Show(dbPath);
        }
        private string hashpqpass(string Password)
        {
            Password = textBox2.Text;
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
                return null;
            }
        }

        private string hashemail(string Email)
        {
            Email = textBox3.Text;
            try
            {
                if (!string.IsNullOrEmpty(Email))
                {
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] bytess = sha256.ComputeHash(Encoding.UTF8.GetBytes(Email));
                        StringBuilder stringbulder = new StringBuilder();
                        for (int i = 0; i < bytess.Length; i++)
                        {
                            stringbulder.Append(bytess[i].ToString("x2"));
                        }
                        return stringbulder.ToString();
                    }
                }
                throw new Exception("Ошибка");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка" + ex.Message);
                return null;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists("UserBase.db"))
                {
                    string Email = textBox3.Text;
                    string Password = textBox2.Text;
                    string hashemaill = hashemail(Email);
                    string hashpassword = hashpqpass(Password);
                    string Login = textBox1.Text;
                    string City = textBox4.Text;
                    if (!string.IsNullOrEmpty(Login) && !string.IsNullOrEmpty(hashpassword) && !string.IsNullOrEmpty(hashemaill) && string.IsNullOrEmpty(City))
                    {
                        MessageBox.Show("Заполните все поля!");
                    }
                    string vars = "Data Source=UserBase.db";


                    using (var das = new SQLiteConnection(vars))
                    {
                        das.Open();
                        var cmd2 = new SQLiteCommand(
                            "INSERT INTO [Users] (Name,Password,Email,City) VALUES (@L,@P,@E,@C)", das);
                        cmd2.Parameters.AddWithValue("@L", textBox1.Text);
                        cmd2.Parameters.AddWithValue("@P", hashpassword);
                        cmd2.Parameters.AddWithValue("@E", hashemaill);
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
            catch(Exception ex) 
            {
                MessageBox.Show("Ошибка сохранения" + ex.Message);
            }
        }

    }
}
