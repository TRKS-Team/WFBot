using System;
using System.Collections.Generic;
using System.Linq;
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
            label2.Text = $"当前的口令为:{Config.Instance.Code}";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label5.Text = $"当前的口令为:{Config.Instance.Code}";
            if (string.IsNullOrEmpty(Config.Instance.QQ))
            {
                label5.Text = $"当前不发送任何报错.";
            }
            else
            {
                label5.Text = $"当前的QQ为:{Config.Instance.QQ}";
            }
            UpdateCheckBox();
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

        private void label3_Click(object sender, EventArgs e)
        {

        }

        public void InvasionsCheck(object sender, EventArgs e)
        {
            var checkbox = (CheckBox) sender;
            var rewardList = Config.Instance.InvationRewardList;
            var items = (string[]) checkbox.Tag;
            foreach (var item in (string[]) checkbox.Tag)
            {
                if (checkbox.Checked)
                {
                    rewardList.Add(item);
                }
                else
                {
                    rewardList.Remove(item);
                }
            }
            Config.Save();     
        }

        public void UpdateCheckBox()
        {
            foreach (var control in Controls)
            {
                if (control is CheckBox checkbox)
                {
                    var items = (string[])checkbox.Tag;
                    foreach (var item in items)
                    {
                        checkbox.Checked = Config.Instance.InvationRewardList.Contains(item);
                        if (checkbox.Checked)
                        {
                            break;
                        }
                    }

                    foreach (var tag in items)
                    {
                        Config.Instance.InvationRewardList.Remove(tag);
                    }
                }
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var qq = textBox2.Text;
            if (string.IsNullOrEmpty(qq)) 
            {
                Config.Instance.QQ = qq;
                Config.Save();
                label5.Text = $"当前不发送任何报错.";                
            }
            else if (qq.IsNumber())
            {
                Config.Instance.QQ = qq;
                Config.Save();
                label5.Text = $"当前的QQ为: {qq}";
            }
            else
            {
                MessageBox.Show("您的QQ是真的牛逼."); 
            }

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }

    public static class StringExtensions
    {
        public static bool IsNumber(this string source)
        {
            return int.TryParse(source, out _);
        }
    }
}
