using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Newbe.Mahua.MahuaEvents;
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
            var checkbox = (CheckBox)sender;
            var rewardList = Config.Instance.InvationRewardList;
            var tags = (string[])checkbox.Tag;
            foreach (var item in tags)
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
        // 关了么
        // resharper激活?
        public void UpdateCheckBox()
        {
            foreach (var checkbox in Controls.OfType<CheckBox>())
            {
                var items = new HashSet<string>(((string[])checkbox.Tag));
                var invRewards = Config.Instance.InvationRewardList;
                checkbox.Checked = items.Intersect(invRewards).Any();
                Config.Instance.InvationRewardList.RemoveAll(item => items.Contains(item));
            }
        }
        private void label4_Click(object sender, EventArgs e)
        {

            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var qq = textBox2.Text;
            Config.Instance.QQ = qq;
            Config.Save();
            if (string.IsNullOrEmpty(qq))
            {
                label5.Text = $"当前不发送任何报错.";
            }
            else if (qq.IsNumber())
            {
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
