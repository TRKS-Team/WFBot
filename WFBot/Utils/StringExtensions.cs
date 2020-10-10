using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WFBot.Utils
{
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }
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
}
