using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
namespace v22
{
    public partial class settings : Form
    {
        private readonly OpenFileDialog openFileDialog = new OpenFileDialog();
        private readonly string _Login;
        private readonly string _Password;
        private readonly string _Email;
        private API apiForm;
        private Panel panelApiHost;
        public settings(string Login,string Password, string email)
        {
            InitializeComponent();
            //// Создаём панель-хост для встраивания формы API, если она ещё не создана
            //if (panelApiHost == null)
            //{
            //    panelApiHost = new Panel();
            //    // Пристыковываем панель справа, чтобы она никогда не перекрывала элементы слева
            //    panelApiHost.Dock = DockStyle.Right;
            //    panelApiHost.Width = 330;
            //    this.Controls.Add(panelApiHost);
            //    panelApiHost.SendToBack(); // на задний план, на всякий случай
            //}
            _Login = Login;
            _Password = Password;
            stokavatar();
            pokazcity();
            pokazlogin();
            _Email = email;
            pokazpochta();
            //if (apiForm == null || apiForm.IsDisposed)
            //{
            //    apiForm = new API(_Login,_Password);
            //    apiForm.TopLevel = false;                 // ключевое: не отдельное окно
            //    apiForm.FormBorderStyle = FormBorderStyle.None;
            //    apiForm.Dock = DockStyle.Fill;            // заполнит только панель-хост
            //    panelApiHost.Controls.Clear();
            //    panelApiHost.Controls.Add(apiForm);
            //}
            //apiForm.Show();                               // НЕ ShowDialog
            //apiForm.BringToFront();
        }
        //Метод Для отображения сток аватарки пользователя
        private void stokavatar() 
        {
            this.pictureBox1.Image = Properties.Resources.noneavatar2;
            PictureBoxSizeMode pictureBox1 = PictureBoxSizeMode.AutoSize;
        }

        // Хэширование почты
        private string hashEmail(string Email)
        {
            try
            {
                if (!string.IsNullOrEmpty(Email))
                {
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(Email));
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
        // Метод хэширования пароля
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
                else
                {
                    return string.Empty; // Возвращаем пустую строку для пустого пароля
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка" + ex.Message);
                throw;
            }
        }
        // Метод смены пароля
        private bool smenaparolya()
        { 
            string newparol = textBox1.Text; string repeatparol = textBox2.Text;
            string Login = _Login; string hashparol = hashpqpass(newparol);
            if (newparol == repeatparol)
            {
                if (newparol.Length < 8)
                {
                    MessageBox.Show("Пароль не может быть меньше 8 символов!");
                    return false;
                }
                bool reg = Regex.IsMatch(newparol, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");
                if (reg == false)
                {
                    MessageBox.Show("Пароль Должен содержать специальные символы! (!@#$%^&*()_+=\\[{\\]};:<>|./?,-)");
                    return false;
                }
                if (!System.IO.File.Exists("UserBase.db"))
                {
                    MessageBox.Show("Бд не найден!1");
                    return false;
                }
                string vars = "Data Source=UserBase.db";
                try
                {
                    using (var ds = new SQLiteConnection(vars))
                    {
                        ds.Open();
                        using (var parol = new SQLiteCommand("UPDATE Users SET Password = @NEWP WHERE Login = @L", ds))
                        {
                            parol.Parameters.AddWithValue("@NEWP", hashparol);
                            parol.Parameters.AddWithValue("@L", _Login);
                            var rowsAffected = parol.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Пароль успешно изменён");
                                return true;
                            }
                            else
                            {
                                MessageBox.Show("Пользователь с таким логином не найден");
                                return false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось применить изменения", "Ошибка" + ex.Message + MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Пароли не совпадают");
                return false;
            }
        }
        // Метод смены почты
        private bool smenapochta()
        {
            string newpochta = textBox3.Text;
            string hashpochta = hashEmail(newpochta);
            string Login = _Login;
            if (newpochta != null)
            {
                bool reg = Regex.IsMatch(newpochta, @"[@.]");
                if (reg == false)
                {
                    MessageBox.Show("Пароль Должен содержать специальные символы! (@.");
                    return false;
                }
                if (!System.IO.File.Exists("UserBase.db"))
                {
                    MessageBox.Show("Бд не найден!2");
                    return false;
                }
                string vars = "Data Source=UserBase.db";
                try
                {
                    using (var ds = new SQLiteConnection(vars))
                    {
                        ds.Open();
                        using (var pochta = new SQLiteCommand("UPDATE Users SET Email = @NEWP WHERE Login = @L", ds))
                        {
                            pochta.Parameters.AddWithValue("@NEWP", hashpochta);
                            pochta.Parameters.AddWithValue("@L", _Login);
                            var rowsAffected = pochta.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Почта успешна изменена");
                                return true;
                            }
                            else
                            {
                                MessageBox.Show("Пользователь с таким логином не найден");
                                return false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось применить изменения", "Ошибка" + ex.Message + MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Почта не найдена?");
                return false;
            }
        }



        // Запрос к бд для отображения текущего Логина
        private bool pokazlogin()
        {
            if (!System.IO.File.Exists("UserBase.db"))
            {
                MessageBox.Show("Бд не найден!3");
                return false;
            }
            string vars = "Data Source=UserBase.db";
            using (var das = new SQLiteConnection(vars))
            {
                try
                {
                    das.Open();
                    using (var fg = new SQLiteCommand("SELECT Login FROM Users WHERE Login = @L", das))
                    {
                        fg.Parameters.AddWithValue("@L", _Login);
                        var result = fg.ExecuteScalar();
                        if (result != null)
                        {
                            label1.Text = result.ToString();
                            return true;
                        }
                        else
                        {
                            label1.Text = "EROR";
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка" + ex.Message);
                    return false;
                }
            }

        }
        // Запрос к бд для отображения текущей почты
        private bool pokazpochta()
        {
            if (!System.IO.File.Exists("UserBase.db"))
            {
                MessageBox.Show("Бд не найден!4");
            }
            string vars = "Data Source=UserBase.db";
            try
            {
                using (var das = new SQLiteConnection(vars))
                {
                    das.Open();
                    using (var command = new SQLiteCommand("SELECT Email FROM Users WHERE Login = @L LIMIT 1", das))
                    {
                        command.Parameters.AddWithValue("@L", _Login);
                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            label3.Text = result.ToString();
                            return true;
                        }
                        else
                        {
                            label1.Text = "EROR";
                            return false;
                        }

                    }
                }
            }
            catch (Exception ex)
            { 
                MessageBox.Show($"Error: {ex.Message}");
                return false;
            }
        }
        // Запрос к бд для отображения текущего города
        private bool pokazcity()
        {
            if (!System.IO.File.Exists("UserBase.db"))
            {
                MessageBox.Show("Бд не найден!5");
            }
            string vars = "Data Source=UserBase.db";
            using (var das = new SQLiteConnection(vars))
            {
                try
                {
                    das.Open();
                    using (var fg = new SQLiteCommand("SELECT City FROM Users WHERE Login = @L Limit 1", das))
                    {
                        fg.Parameters.AddWithValue("@L", _Login);
                        var result = fg.ExecuteScalar();
                        if (result != null)
                        {
                            label2.Text = result.ToString();
                            return true;
                        }

                        else
                        {
                            label2.Text = "EROR";
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка" + ex.Message);
                    return false;
                }
            }

        }
        // Метод Смены Города
        private bool smenascity() 
        {
            label1.Text = _Login;
            string NEWCity = textBox3.Text;
            if (!System.IO.File.Exists("UserBase.db"))
            {
                MessageBox.Show("Бд не найден!6");
            }
            string vars = "Data Source=UserBase.db";
            using (var das = new SQLiteConnection(vars))
            {
                try
                {
                    das.Open();
                    using (var fd = new SQLiteCommand("UPDATE Users SET City = @NEWCity WHERE Login = @L", das))
                    {
                        fd.Parameters.AddWithValue("@NEWCity", NEWCity);
                        fd.Parameters.AddWithValue("@L", _Login);
                        var result = fd.ExecuteScalar();

                            label2.Text = result?.ToString();
                            MessageBox.Show("Город успешно изменен");
                            return true;
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Город не найден" + ex.Message);
                    return false;
                }
            }
        }
        // Метод Для выбора пользователем аватарки по нажатию 
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Image newavatar = Image.FromFile(openFileDialog.FileName);
                    pictureBox1.Image = newavatar;
                }
            }

            catch (Exception ex) {
                MessageBox.Show("Ошибка" + ex.Message);
            }
        }
        // Рестарт форм, выход из профиля
        private void button2_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }
        // Кнопка смены пароля
        private void button3_Click(object sender, EventArgs e)
        {
            smenaparolya();
        }
        //Кнопка смены города
        private void button4_Click(object sender, EventArgs e)
        {
            smenascity();
        }
        // переход на форму c API
        private void button5_Click(object sender, EventArgs e)
        {
        }
        // отобржаение результатов из API 
        private void pokazapi()
        {
            API api = new API(_Login, _Password);
            api.Show(this);
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            smenapochta();
        }
    }
}
