using System;
using System.Linq;
using System.Text.RegularExpressions;
using Ringer.Core.Data;
using Xamarin.Forms;

namespace Ringer.Extensions
{

    public static class DoubleExtensions
    {
        public static double Clamp(this double self, double min, double max)
        {
            return Math.Min(max, Math.Max(self, min));
        }
    }

    public static class StringExtensions
    {
        public static string RemoveWhiteSpaces(this string self)
        {
            return Regex.Replace(self, @"\s+", "");
        }

        public static bool IsKoreanOnly(this string self)
        {
            return self.All(c => c >= 'ㄱ' && c <= '힣');
        }

        public static bool IsSevenNumericDigit(this string self)
        {
            return self.Length == 7 && Regex.IsMatch(self, @"^\d+$");
        }

        //public static bool IsValidBirthDateAndSex(this string self, out int year, out int month, out int day, out GenderType gender)
        public static bool IsValidBirthDateAndSex(this string self, out DateTime birthDate, out GenderType gender)
        {
            birthDate = default;
            gender = default;

            int yearInt;
            int monthInt;
            int dayInt;


            if (int.TryParse(self.Substring(6, 1), out int parsedSex) && parsedSex > 0 && parsedSex < 5)
                gender = parsedSex % 2 == 0 ? GenderType.Female : GenderType.Male;
            else
                return false;

            if (int.TryParse(self.Substring(0, 2), out int parsedYear))
                yearInt = (parsedSex > 2) ? parsedYear + 2000 : parsedYear + 1900;
            else
                return false;

            if (int.TryParse(self.Substring(2, 2), out int parsedMonth) && parsedMonth > 0 && parsedMonth < 13)
                monthInt = parsedMonth;
            else
                return false;

            if (int.TryParse(self.Substring(4, 2), out int parsedDay) && parsedDay > 0)
            {
                if (parsedMonth == 1 || parsedMonth == 3 || parsedMonth == 5 || parsedMonth == 7 || parsedMonth == 8 || parsedMonth == 10 || parsedMonth == 12)
                    if (parsedDay > 31)
                        return false;

                if (parsedMonth == 4 || parsedMonth == 6 || parsedMonth == 9 || parsedMonth == 11)
                    if (parsedDay > 30)
                        return false;

                if (parsedMonth == 2)
                    if (parsedDay > 29)
                        return false;

                dayInt = parsedDay;

                birthDate = new DateTime(yearInt, monthInt, dayInt);

                if (birthDate >= DateTime.UtcNow)
                    return false;

                if (birthDate.AddYears(140) < DateTime.UtcNow) // 140살 이상
                    return false;

                return true;
            }

            return false;
        }
    }
}
