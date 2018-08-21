using System;
using System.Windows.Forms;
using TRKS.WF.QQBot;

namespace Settings
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Config.Instance.Code = textBox1.Text;
            Config.Save();
            var formattableString = $"当前的口令为:{Config.Instance.Code}";
            label2.Text = formattableString;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var formattableString = $"当前的口令为:{Config.Instance.Code}";
            label2.Text = formattableString;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
