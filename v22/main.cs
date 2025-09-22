using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace v22
{
    public partial class main : Form
    {
        private readonly OpenFileDialog openFileDialog = new OpenFileDialog();
        private readonly string _Login;
        private readonly string _Password;
        private readonly string _Email;
        private API apiForm;
        private Panel panelApiHost;
        public main(string Login, string Password, string Email)
        {
            InitializeComponent();
            ApplyDarkTheme();
            CreateDatabase();
            if (panelApiHost == null)
            {
                panelApiHost = new Panel();
                // Компактная панель в правом верхнем углу
                panelApiHost.Dock = DockStyle.None;
                panelApiHost.Width = 400;
                panelApiHost.Height = 450;
                panelApiHost.BackColor = Color.FromArgb(28, 31, 40);
                panelApiHost.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                this.Controls.Add(panelApiHost);
                // Располагаем под верхней панелью и прижимаем к правому краю
                panelApiHost.Location = new Point(this.ClientSize.Width - panelApiHost.Width - 12,
                    (this.Controls.Contains(panelTop) ? panelTop.Bottom + 8 : 8));
                panelApiHost.SendToBack(); // на задний план, на всякий случай
            }
            _Login = Login;
            _Password = Password;
            _Email = Email;

            if (apiForm == null || apiForm.IsDisposed)
            {
                apiForm = new API(_Login, "");
                apiForm.TopLevel = false;                 // ключевое: не отдельное окно
                apiForm.FormBorderStyle = FormBorderStyle.None;
                apiForm.Dock = DockStyle.Fill;            // заполнит только панель-хост
                panelApiHost.Controls.Clear();
                panelApiHost.Controls.Add(apiForm);
            }
            apiForm.Show();                               // НЕ ShowDialog
            apiForm.BringToFront();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            settings set = new settings(_Login,_Password,_Email);
            set.Show();
            this.Hide();
        }

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

            foreach (Control child in control.Controls)
            {
                StyleControl(child);
            }
        }
        private async Task<bool> CreateDatabase()
        {
            try
            {
                // Создаем базу данных SQLite
                string connectionString = "Data Source=UserBase.db";
                using (var connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync().ConfigureAwait(false);

                    // Создаем таблицу Users
                    string createTableSql = @"
                        CREATE TABLE IF NOT EXISTS Users (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Login TEXT NOT NULL UNIQUE,
                            Password TEXT NOT NULL,
                            City TEXT,
                            Email TEXT,
                            CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
                        )";

                    using (var command = new SQLiteCommand(createTableSql, connection))
                    {
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }

                    // Создаем индекс для быстрого поиска по логину
                    string createIndexSql = "CREATE INDEX IF NOT EXISTS idx_users_login ON Users(Login)";
                    using (var command = new SQLiteCommand(createIndexSql, connection))
                    {
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }

                    // Добавляем тестового пользователя (опционально)
                    string insertTestUserSql = @"
                        INSERT OR IGNORE INTO Users (Login, Password, City, Email) 
                        VALUES ('admin', 'admin123', 'Москва', 'admin@example.com')";

                    using (var command = new SQLiteCommand(insertTestUserSql, connection))
                    {
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания базы данных: {ex.Message}");
                return false;
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

        private void button4_Click(object sender, EventArgs e)
        {
            TEXTSUPPORT text = new TEXTSUPPORT(_Login,_Email);
            text.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            genericpicture generic = new genericpicture();
            generic.Show();
            this.Hide();
        }
    }
}
