using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using GammaLibrary.Extensions;
using WFBot.Features.Utils;

namespace WFBot
{
    public partial class Settings 
    {
        public Settings()
        {
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
        private void label4_Click(object sender, EventArgs e)
        {

            
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

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

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
