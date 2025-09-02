using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
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
            if (panelApiHost == null)
            {
                panelApiHost = new Panel();
                // Компактная панель в правом верхнем углу
                panelApiHost.Dock = DockStyle.None;
                panelApiHost.Width = 400;
                panelApiHost.Height = 56;
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
            _Email = _Email;

            if (apiForm == null || apiForm.IsDisposed)
            {
                apiForm = new API(_Login, _Password);
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
    }
}
