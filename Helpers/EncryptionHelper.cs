using System;
using System.Security.Cryptography;

namespace lu.Helpers
{
    public class EncryptionHelper
    {
        public static string Hash(string password)
        {
            var salt = GetSalt();
            int iterations = 10000;

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
            byte[] hash = pbkdf2.GetBytes(24);

            return Convert.ToBase64String(salt) + "|" + iterations + "|" + Convert.ToBase64String(hash);
        }

        public static bool ValidatePassword(string hashedValue, string password)
        {
            var originalHashArr = hashedValue.Split("|");
            var salt = Convert.FromBase64String(originalHashArr[0]);
            var iterations = Int32.Parse(originalHashArr[1]);
            var hash = originalHashArr[2];

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
            byte[] hashToTest = pbkdf2.GetBytes(24);

            if (Convert.ToBase64String(hashToTest) == hash)
            {
                return true;
            }
            return false;
        }

        private static byte[] GetSalt()
        {
            var salt = new byte[24];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            return salt;
        }
    }
}