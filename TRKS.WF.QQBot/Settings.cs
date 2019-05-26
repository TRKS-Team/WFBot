using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GammaLibrary.Extensions;
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
            label2.Text = $"当前的口令为: {Config.Instance.Code}";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label2.Text = $"当前的口令为: {Config.Instance.Code}";
            if (string.IsNullOrEmpty(Config.Instance.QQ))
            {
                label5.Text = $"当前不发送任何报错.";
            }
            else
            {
                label5.Text = $"当前的QQ为: {Config.Instance.QQ}";
            }
            UpdateCheckBox();
            checkBox9.Checked = Config.Instance.AcceptInvitation;
            checkBox10.Checked = Config.Instance.AcceptJoiningRequest;
            textBox4.Text = Config.Instance.ClientId;
            textBox5.Text = Config.Instance.ClientSecret;
            checkBox11.Checked = Config.Instance.IsSlashRequired;
            textBox6.Text = Config.Instance.CallperMinute.ToString();
            UpdatePlatformRadioButtons();
            checkBox13.Checked = Config.Instance.IsThirdPartyWM;
            checkBox14.Checked = Config.Instance.IsAlertRequiredRareItem;
            checkBox233.Checked = Config.Instance.AutoUpdate;
            checkBox12.Checked = Config.Instance.UpdateLexion;
            GitHubTokenBox.Text = Config.Instance.GitHubOAuthKey;
            textBox7.Text = Config.Instance.WMSearchCount.ToString();
            textBox8.Text = Config.Instance.WFASearchCount.ToString();
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
        public void UpdateCheckBox()
        {
            var checkboxes = Controls.OfType<CheckBox>().Where(box => box.Tag is string[]);
            foreach (var checkbox in checkboxes)
            {
                var items = new HashSet<string>(((string[])checkbox.Tag));
                var invRewards = Config.Instance.InvationRewardList;
                checkbox.Checked = items.Intersect(invRewards).Any();
 
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

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void checkBox9_Click(object sender, EventArgs e)
        {
            Config.Instance.AcceptInvitation = checkBox9.Checked;
            Config.Save();
        }

        private void checkBox10_Click(object sender, EventArgs e)
        {
            Config.Instance.AcceptJoiningRequest = checkBox10.Checked;
            Config.Save();
        }

        private void textBox3_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Messenger.Broadcast($"[来自管理者]通知: {textBox3.Text}"); 
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            Config.Instance.ClientId = textBox4.Text;
            Config.Instance.ClientSecret = textBox5.Text;
            Config.Save();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void checkBox11_Click(object sender, EventArgs e)
        {
            Config.Instance.IsSlashRequired = checkBox11.Checked;
            Config.Save();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox6.Text.IsNumber())
            {
                Config.Instance.CallperMinute = int.Parse(textBox6.Text);
                Config.Save();
            }
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }


        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void UpdatePlatform(object sender, EventArgs e)
        {
            var radiobutton = (RadioButton)sender;
            if (radiobutton.Checked)
            {
                switch (radiobutton.Text)
                {
                    case "pc":
                        Config.Instance.Platform = Platform.PC;
                        break;
                    case "ps4":
                        Config.Instance.Platform = Platform.PS4;
                        break;
                    case "xbox":
                        Config.Instance.Platform = Platform.XBOX;
                        break;
                    case "switch":
                        Config.Instance.Platform = Platform.NS;
                        break;
                }
                Config.Save();
            }
        }

        private void UpdatePlatformRadioButtons()
        {
            switch (Config.Instance.Platform)
            {
                case Platform.PC:
                    radioButton1.Checked = true;
                    break;
                case Platform.XBOX:
                    radioButton2.Checked = true;
                    break;
                case Platform.PS4:
                    radioButton3.Checked = true;
                    break;
                case Platform.NS:
                    radioButton4.Checked = true;
                    break;
            }
        }

        private void checkBox13_Click(object sender, EventArgs e)
        {
            Config.Instance.IsThirdPartyWM = checkBox13.Checked;
            Config.Save();
        }

        private void checkBox14_Click(object sender, EventArgs e)
        {
            Config.Instance.IsAlertRequiredRareItem = checkBox14.Checked;
            Config.Save();
        }

        private void CheckBox233_CheckedChanged(object sender, EventArgs e)
        {
            Config.Instance.AutoUpdate = checkBox233.Checked;
            Config.Save();
        }

        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
            Config.Instance.UpdateLexion = checkBox12.Checked;
            Config.Save();
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            Config.Instance.GitHubOAuthKey = GitHubTokenBox.Text;
            Config.Save();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (textBox7.Text.IsNumber())
            {
                Config.Instance.WMSearchCount = textBox7.Text.ToInt();
                Config.Save();
            }
            else
            {
                MessageBox.Show("您输入的参数不是数字嗷?");
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (textBox8.Text.IsNumber())
            {
                Config.Instance.WFASearchCount = textBox8.Text.ToInt();
                Config.Save();
            }
            else
            {
                MessageBox.Show("您输入的参数不是数字嗷?");
            }
        }
    }

    public static class DictionaryExtensions
    {
        /*public static Dictionary<T1, T2> Reverse<T1, T2>(this Dictionary<T2, T1> dic)
        {
            dic.SelectMany(k => k.Value
                    .Select(v => new { Key = v, Value = k.Key }))
                .ToDictionary(t => t.Key, t => t.Value);
        }*/ 
        // 不会写泛型(
    }
    public static class StringExtensions
    {
        public static bool IsNumber(this string source)
        {
            return int.TryParse(source, out _);
        }

        public static string ToBase64(this string source)
        {
            var bytes = Encoding.UTF8.GetBytes(source);
            return Convert.ToBase64String(bytes);
        }

        public static string Format(this string source)
        {
            return source.Replace(" ", "").ToLower().Trim();
        }
    }

    public enum Platform
    {
        [Symbol("pc")]PC,
        [Symbol("xbox")]XBOX,
        [Symbol("ps4")]PS4,
        [Symbol("swi")]NS
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)] // 谢啦 嫖代码真的爽
    class SymbolAttribute : Attribute
    {
        public string[] Symbols { get; }

        public SymbolAttribute(params string[] symbols)
        {
            Symbols = symbols;
        }
    }

    public static class EnumExtensions
    {
        public static string[] GetSymbols<TEnum>(this TEnum obj)
        {
            try
            {
                return obj.GetType().GetMember(obj.ToString()).First()
                    .GetCustomAttribute<SymbolAttribute>().Symbols;
            }
            catch (Exception)
            {
                // 在这里给你自己发信息 告诉你自己代码出错了.
                // 非机器人的写法为: 
                // Debug.Assert(xxx); 如果xxx是false就会提醒你这里有一个代码bug
                // assert 为断言 不用看 这只是一种习惯
                Messenger.SendDebugInfo("你猜怎么着?你最不想看到的那部分,也就是自定义平台,他报错啦!!!机器人要炸啦!!!你要被用户骂死啦!!!(检查一下你的enum贴没贴symbol)");
                return new string[0];
            }
        }

    }
}
