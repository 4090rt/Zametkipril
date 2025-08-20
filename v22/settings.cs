using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
namespace v22
{
    public partial class settings : Form
    {
        private readonly OpenFileDialog openFileDialog = new OpenFileDialog();
        public settings()
        {
            
            InitializeComponent();
            zaproslogin();
            stokavatar();
            zaproscity();
        }
        private void stokavatar() 
        {
            this.pictureBox1.Image = Properties.Resources.noneavatar2;
            PictureBoxSizeMode pictureBox1 = PictureBoxSizeMode.AutoSize;
        }
        private void zaproslogin()
        {
            if (!System.IO.File.Exists("UserBase.db"))
            {
                MessageBox.Show("БД не найдена");
            }
            string vars = "Data Source=UserBase.db";
            using (var das = new SQLiteConnection(vars))
            {
                try
                {
                    das.Open();
                    using (var fd = new SQLiteCommand(@"SELECT Login From Users LIMIT 1", das)) {
                        var result = fd.ExecuteScalar();
                        label1.Text = result?.ToString();
                    }
                }
                catch (Exception ex) {

                    MessageBox.Show("Логин не найден" + ex.Message);
                }

            }
        }
        private void zaproscity() 
        {
            if (!System.IO.File.Exists("UserBase.db"))
            {
                MessageBox.Show("Бд не найден!");
            }
            string vars = "Data Source=UserBase.db";
            using (var das = new SQLiteConnection(vars))
            {
                try
                {
                    das.Open();
                    using (var fd = new SQLiteCommand(@"SELECT City FROM Users LIMIT 1", das))
                    {
                        var result = fd.ExecuteScalar();
                        label2.Text = result?.ToString();
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Город не найден" + ex.Message);
                }
            }
        }
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

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
