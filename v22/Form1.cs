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
        private readonly string _Login;
        private string _placeholderLogin = "Логин";
        private string _placeholderPassword = "Пароль";
        private string _placeholderCity = "Город";
        private string _placeholderEmail = "Email";
        public Form1()
        {
            InitializeComponent();
            fdf();
            ApplyDarkTheme();
            EnhanceInputs();
        }

        public Form1(string Login)
        {
            InitializeComponent();
            fdf();
            ApplyDarkTheme();
            EnhanceInputs();
            _Login = Login;
            string Password = textBox2.Text;
            string Loginn = textBox1.Text;
            string Email = textBox3.Text;

        }
        // Путь к бд
        private void fdf()
        {
            string dbPath = System.IO.Path.GetFullPath("UserBase.db");
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


        private async Task<bool> loginproverka()
        {
            string logn = textBox1.Text;
            if (string.IsNullOrEmpty(logn))
            {
                MessageBox.Show("Введите логин!");
                return false;
            }          
            string vars = "Data Source=UserBase.db";
            try
            {
                using (var das = new SQLiteConnection(vars))
                {
                    await das.OpenAsync().ConfigureAwait(false);
                    using (var command = new SQLiteCommand("SELECT COUNT(*) FROM Users WHERE Login = @L", das))
                    {
                        command.Parameters.AddWithValue("@L", logn);
                        var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
                        
                        if (result == null || result == DBNull.Value)
                        {
                            return false;
                        }
                        
                        int count = Convert.ToInt32(result);
                        if (count == 0)
                        { 
                            return true;
                        }
                        if (count == 1)
                        {
                            return false;
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при проверке логина: " + ex.Message);
                return false;
            }
        }
        // добавление данных юзера в бд
        private async Task<bool> dobavitnewuser()
        {
            if (!await loginproverka().ConfigureAwait(false))
            {
                MessageBox.Show("Такой логин уже существует! Выберите другой логин.");
                return false;
            }          
            try
            {
                if (File.Exists("UserBase.db"))
                {
                    string Password = textBox2.Text;
                    string Email = textBox3.Text;
                    string Login = textBox1.Text;
                    string City = textBox4.Text;
                    string hashpassword;
                    string hashemail;
                    try
                    {
                        hashpassword = hashpqpass(Password);
                        hashemail = hashEmail(Email);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при хэшировании данных: " + ex.Message);
                        return false;
                    }
                    
                    // Проверяем, что все поля заполнены
                    if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(City) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Email))
                    {
                        MessageBox.Show("Заполните все поля!");
                        return false;
                    }
                    
                    // Проверяем длину пароля
                    if (Password.Length < 8)
                    {
                        MessageBox.Show("Пароль не может быть меньше 8 символов!");
                        return false;
                    }
                    
                    // Проверяем наличие специальных символов в пароле
                    bool hasSpecialChars = Regex.IsMatch(Password, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");
                    if (!hasSpecialChars)
                    {
                        MessageBox.Show("Пароль должен содержать специальные символы! (!@#$%^&*()_+=\\[{]};:<>|./?,-)");
                        return false;
                    }
                    
                    // Проверяем, что Email содержит @gmail.com
                    if (!Email.Contains("@gmail.com"))
                    {
                        MessageBox.Show("Email должен содержать @gmail.com!");
                        return false;
                    }

                    string vars = "Data Source=UserBase.db";
                    using (var das = new SQLiteConnection(vars))
                    {
                        await das.OpenAsync().ConfigureAwait(false);
                        var cmd2 = new SQLiteCommand(
                            "INSERT INTO [Users] (Login,Password,City,Email) VALUES (@L,@P,@C,@E)", das);
                        cmd2.Parameters.AddWithValue("@L", textBox1.Text);
                        cmd2.Parameters.AddWithValue("@P", hashpassword);
                        cmd2.Parameters.AddWithValue("@C", textBox4.Text);
                        cmd2.Parameters.AddWithValue("@E", textBox3.Text);

                        await cmd2.ExecuteNonQueryAsync().ConfigureAwait(false);
                        MessageBox.Show("Данные сохранены!");
                        
                        // Очищаем поля после успешного добавления
                        textBox1.Text = "";
                        textBox2.Text = "";
                        textBox3.Text = "";
                        textBox4.Text = "";
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show("БД не найдена");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления" + ex.Message);
                return false;
            }
            
        }
        // при нажатии на кнопку создается новое бд для хранения данных
        private async void button1_Click(object sender, EventArgs e)
        {
            string dbPath = System.IO.Path.GetFullPath("UserBase.db");
            try
            {
                using (var das = new SQLiteConnection($"Data Source={dbPath}"))
                {
                    await das.OpenAsync().ConfigureAwait(false);
                    var createTableCommand = new SQLiteCommand(
                       @"CREATE TABLE IF NOT EXISTS [Users] (
                                [ID] INTEGER PRIMARY KEY AUTOINCREMENT,
                                [Login] TEXT NOT NULL UNIQUE,
                                [Password] TEXT NOT NULL,
                                [City] TEXT NOT NULL,
                                [Email] TEXT NOT NULL
                            )", das);
                    await createTableCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
                await dobavitnewuser().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка" + ex.Message);
            }
        }
        
        //// Метод для изменения пароля пользователя
        //public static bool ChangeUserPassword(string login, string newPassword)
        //{
        //    if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(newPassword))
        //        return false;
                
        //    if (newPassword.Length < 8)
        //        return false;
                
        //    string dbPath = System.IO.Path.GetFullPath("UserBase.db");
        //    if (!File.Exists(dbPath))
        //        return false;
                
        //    try
        //    {
        //        string hashedPassword;
        //        using (SHA256 sha256 = SHA256.Create())
        //        {
        //            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(newPassword));
        //            StringBuilder stringBuilder = new StringBuilder();
        //            for (int i = 0; i < bytes.Length; i++)
        //            {
        //                stringBuilder.Append(bytes[i].ToString("x2"));
        //            }
        //            hashedPassword = stringBuilder.ToString();
        //        }
                
        //        using (var das = new SQLiteConnection($"Data Source={dbPath}"))
        //        {
        //            das.Open();
        //            using (var command = new SQLiteCommand("UPDATE Users SET Password = @P WHERE Login = @L", das))
        //            {
        //                command.Parameters.AddWithValue("@P", hashedPassword);
        //                command.Parameters.AddWithValue("@L", login);
        //                int rowsAffected = command.ExecuteNonQuery();
        //                return rowsAffected > 0;
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
        // Метод валидации пользователя
        private async Task<bool> vakidateuser(string Login, string Password)
        {
            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("Введите логин и пароль!");
                return false;
            }
            
            string dbPath = System.IO.Path.GetFullPath("UserBase.db");
            if (!File.Exists(dbPath))
            {
                MessageBox.Show("База данных не найдена!");
                return false;
            }
            
            string hashpass;
            try
            {
                hashpass = hashpqpass(Password);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обработке пароля: " + ex.Message);
                return false;
            }
            
            using (var das = new SQLiteConnection($"Data Source={dbPath}"))
            {
                await das.OpenAsync().ConfigureAwait(false);
                try
                {
                    using (var gg = new SQLiteCommand("SELECT Password FROM Users WHERE Login = @L LIMIT 1", das))
                    {
                        gg.Parameters.AddWithValue("@L", Login);
                        var value = await gg.ExecuteScalarAsync().ConfigureAwait(false);
                        if (value == null || value == DBNull.Value)
                        {
                            MessageBox.Show("Пользователь с таким логином не найден!");
                            return false;
                        }
                        string pasd = Convert.ToString(value);
                        bool isValid = string.Equals(pasd, hashpass, StringComparison.Ordinal);
                        
                        if (!isValid)
                        {
                            MessageBox.Show("Неверный пароль!");
                        }
                        
                        return isValid;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при проверке пользователя: " + ex.Message);
                }
            }
            return false;
        }
        // переход на форму настроек
        private void button3_Click(object sender, EventArgs e)
        {
            string Login = textBox1.Text;
            string Password = textBox2.Text;
            string Email = textBox3.Text;
            settings set = new settings(Login, Password,Email);
            set.Show();
            this.Hide();

        }
        // переход на форму настроек после валидации с доп проверками и обработчиками ошибок
        private async void button4_Click(object sender, EventArgs e)
        {
            string Login = textBox1.Text;
            string Password = textBox2.Text;
            string City = textBox4.Text;
            string Email = textBox3.Text;
            
            try
            {
                if (await vakidateuser(Login, Password).ConfigureAwait(false))
                {
                    main mai = new main(Login, Password, Email);
                    mai.Show();
                    this.Hide();
                }
                else
                {
                    // Сообщение об ошибке уже показано в методе vakidateuser
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось войти в систему: " + ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // переход на форму c API
        private void ApplyDarkTheme()
        {
            this.BackColor = Color.FromArgb(24, 26, 32);
            this.ForeColor = Color.Gainsboro;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            foreach (Control control in this.Controls)
            {
                StyleControl(control);
            }
        }

        private void StyleControl(Control control)
        {
            if (control is Button btn)
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = Color.FromArgb(55, 60, 75);
                btn.ForeColor = Color.Gainsboro;
                btn.Height = 28;
            }
            else if (control is Panel pnl)
            {
                pnl.BackColor = pnl.Dock == DockStyle.Top
                    ? Color.FromArgb(30, 34, 45)
                    : Color.FromArgb(28, 31, 40);
            }
            else if (control is Label lbl)
            {
                lbl.ForeColor = Color.Gainsboro;
            }
            else if (control is TextBox tb)
            {
                tb.BorderStyle = BorderStyle.FixedSingle;
                tb.BackColor = Color.FromArgb(36, 39, 50);
                tb.ForeColor = Color.Gainsboro;
                tb.Margin = new Padding(0, 2, 0, 2);
            }

            foreach (Control child in control.Controls)
            {
                StyleControl(child);
            }
        }

        private void EnhanceInputs()
        {
            // Скрывать пароль
            textBox2.UseSystemPasswordChar = true;

            // Плейсхолдеры
            SetPlaceholder(textBox1, _placeholderLogin);
            SetPlaceholder(textBox2, _placeholderPassword);
            SetPlaceholder(textBox4, _placeholderCity);
            SetPlaceholder(textBox3, _placeholderEmail);

            textBox1.GotFocus += (s, e) => RemovePlaceholder(textBox1, _placeholderLogin);
            textBox1.LostFocus += (s, e) => SetPlaceholder(textBox1, _placeholderLogin);

            textBox2.GotFocus += (s, e) => RemovePlaceholder(textBox2, _placeholderPassword, isPassword: true);
            textBox2.LostFocus += (s, e) => SetPlaceholder(textBox2, _placeholderPassword, isPassword: true);

            textBox4.GotFocus += (s, e) => RemovePlaceholder(textBox4, _placeholderCity);
            textBox4.LostFocus += (s, e) => SetPlaceholder(textBox4, _placeholderCity);

            textBox3.GotFocus += (s, e) => RemovePlaceholder(textBox3, _placeholderEmail);
            textBox3.LostFocus += (s, e) => SetPlaceholder(textBox3, _placeholderEmail);
        }

        private void SetPlaceholder(TextBox tb, string text, bool isPassword = false)
        {
            if (string.IsNullOrEmpty(tb.Text))
            {
                tb.ForeColor = Color.Silver;
                tb.Text = text;
                if (isPassword)
                {
                    tb.UseSystemPasswordChar = false;
                }
            }
        }

        private void RemovePlaceholder(TextBox tb, string text, bool isPassword = false)
        {
            if (tb.Text == text)
            {
                tb.Text = string.Empty;
                tb.ForeColor = Color.Gainsboro;
                if (isPassword)
                {
                    tb.UseSystemPasswordChar = true;
                }
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
    }
}
