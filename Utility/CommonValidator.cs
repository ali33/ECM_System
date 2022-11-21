using System;
using System.Linq;
using System.Text.RegularExpressions;
using Ecm.Utility.Exceptions;

namespace Ecm.Utility
{
    public class CommonValidator
    {
        public static void CheckNull(params object[] objects)
        {
            if (objects == null)
            {
                throw new ArgumentNullException();
            }

            if (objects.Any(obj => obj == null))
            {
                throw new ArgumentNullException();
            }
        }

        public static void CheckEmail(string email)
        {
            if (!IsEmail(email))
            {
                throw new ArgumentException("Email is not valid.");
            }

        }

        public static bool IsEmail(string email)
        {
            const string emailPattern = @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@" +
                                        @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\." +
                                        @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|" +
                                        @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";
            return !string.IsNullOrEmpty(email) && Regex.IsMatch(email, emailPattern);
        }
    }
}
