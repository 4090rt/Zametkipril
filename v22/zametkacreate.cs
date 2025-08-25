using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace v22
{
    public partial class zametkacreate : Form
    {
        public zametkacreate()
        {
            InitializeComponent();

        }
        // метод добавления заметки пользователем и сохранение в файл
        private void DObavit()
        {
            string putipath = System.IO.Path.GetFullPath("Userzametka.txt");
            string zametka = textBox1.Text;

                if (!File.Exists("Userzametka.txt"))
                {
                    File.Create("Userzametka.txt").Close();
                }
                try
                {
                    using (StreamWriter stream = new StreamWriter("Userzametka.txt", true))
                    {
                            stream.Write(zametka);
                            MessageBox.Show("Заметка создана!");
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка" + ex.Message);
                }
        }
        // Добавление заметки по нажатию
        private void button1_Click(object sender, EventArgs e)
        {
            DObavit();
        }
    }
}
