using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

public class ValidExpirationDateAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value == null)
            return false;

        string dateString = value.ToString();

        if (!System.Text.RegularExpressions.Regex.IsMatch(dateString, @"^(0[1-9]|1[0-2])\/\d{2}$"))
        {
            return false;
        }

        try
        {
            var parts = dateString.Split('/');
            int month = int.Parse(parts[0]);
            int year = int.Parse(parts[1]) + 2000;

            var lastDayOfMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            return lastDayOfMonth >= DateTime.Now.Date;
        }
        catch
        {
            return false;
        }
    }
}
