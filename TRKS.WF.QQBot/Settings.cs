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

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            var types = new List<string> { "vandal", "wraith", "other" };
            if (checkBox4.Checked)
            {
                foreach (var type in types)
                {
                    Config.Instance.InvationRewardList.Add(type);
                }
                Config.Save();
            }
            else
            {
                foreach (var type in types)
                {
                    Config.Instance.InvationRewardList.Remove(type);
                }
                Config.Save();
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        public void InvasionsCheck(object sender, EventArgs e)
        {
            var checkbox = (CheckBox) sender;
            if (checkbox.Tag is List<string>)
            {
                if (checkbox.Checked)
                {
                    foreach (var item in (List<string>) checkbox.Tag)
                    {
                        Config.Instance.InvationRewardList.Add(item);
                        Config.Save();
                    }
                }
                else
                {
                    foreach (var item in (List<string>) checkbox.Tag)
                    {
                        Config.Instance.InvationRewardList.Remove(item);
                        Config.Save();
                    }
                }
            }
            else
            {
                if (checkbox.Checked)
                {
                    Config.Instance.InvationRewardList.Add((string)checkbox.Tag);
                    Config.Save();
                }
                else
                {
                    Config.Instance.InvationRewardList.Remove((string) checkbox.Tag);
                    Config.Save();
                }
            }
        }

        public void UpdateCheckBox()
        {
            foreach (var control in Controls)
            {
                if (control is CheckBox)
                {
                    var checkbox = (CheckBox) control;
                    if (checkbox.Tag is List<string>)
                    {
                        foreach (var item in (List<string>)checkbox.Tag)
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
