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
            label2.Text = $"当前的口令为:{Config.Instance.Code}";
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
            if (checkbox.Tag is List<string> list)
            {
                foreach (var item in list)
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
            else
            {
                if (checkbox.Checked)
                {
                    rewardList.Add((string)checkbox.Tag);
                }
                else
                {
                    rewardList.Remove((string) checkbox.Tag);
                }
                Config.Save();
            }
        }

        public void UpdateCheckBox()
        {
            foreach (var control in Controls)
            {
                if (control is CheckBox checkbox)
                {
                    if (checkbox.Tag is List<string> list)
                    {
                        foreach (var item in list)
                        {
                            checkbox.Checked = Config.Instance.InvationRewardList.Contains(item);
                            if (checkbox.Checked)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        checkbox.Checked = Config.Instance.InvationRewardList.Contains((string)checkbox.Tag);
                    }
                }
            }
        }
    }
}
