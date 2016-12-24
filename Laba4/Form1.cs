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

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            try
            {
                LawsEnergy.Init(openFileDialog1.FileName);
                var threads = Convert.ToInt32(threadCount.Text);
                LawsEnergy.SetThreads(threads > 0 ? threads : 2);

                pictureBox1.Image = Image.FromFile(openFileDialog1.FileName);
                button3.Enabled = true;
            }
            catch { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var threads = Convert.ToInt32(threadCount.Text);
            LawsEnergy.SetThreads(threads > 0 ? threads : 2);
            Stopwatch timer = new Stopwatch();
            timer.Start();
            LawsEnergy.Processing();
            timer.Stop();
            timingLbl.Text = timer.ElapsedMilliseconds + " ms";
            pictureBox1.Image = LawsEnergy.Image;
        }


        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = LawsEnergy.Image;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = LawsEnergy.Temp_image;
        }

        private void Form1_Resize(object sender, System.EventArgs e)
        {
            Control control = (Control)sender;

            pictureBox1.Height = control.Height - groupBox2.Height - 60;
            pictureBox1.Width = control.Width - 40;
        }
    }
}
