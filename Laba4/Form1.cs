using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace LawsEnergyTexture
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //загрузка изображения
        private void button1_Click(object sender, EventArgs e)
        {
            //диалог открытия
            openFileDialog1.ShowDialog();
            try
            {
                //данные изображения
                LawsEnergy.Init(openFileDialog1.FileName);
                //вывод для пользователя
                pictureBox1.Image = Image.FromFile(openFileDialog1.FileName);
                button3.Enabled = true;
            }
            catch { }
        }

        //выход
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            LawsEnergy.Сalculation();
            timer.Stop();
            timingLbl.Text = timer.ElapsedMilliseconds.ToString();
            //вывод для пользователя
            pictureBox1.Image = LawsEnergy.Image;
            button4.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //вывод для пользователя
            pictureBox1.Image = LawsEnergy.Temp_image;
            button4.Visible = false;
            button5.Visible = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //вывод для пользователя
            pictureBox1.Image = LawsEnergy.Image;
            button5.Visible = false;
            button4.Visible = true;
        }
    }
}
