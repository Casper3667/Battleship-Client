using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace BattleShip_ClientService.Handlers
{
    public static class PasswordHandler
    {

        public static string HashPassword(string password)
        {
            string sSourceData;
            byte[] tmpSource;
            byte[] tmpHash;

            sSourceData = password;
            tmpSource = ASCIIEncoding.ASCII.GetBytes(sSourceData);
            tmpHash=new MD5CryptoServiceProvider().ComputeHash(tmpSource);
            return ByteArrayToString(tmpHash);

        }
        /// <summary>
        ///  Taken Directly from Here: https://learn.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/compute-hash-values
        ///  It is Used for Easier Storage of Hash Value
        /// </summary>
        /// <param name="arrInput"></param>
        /// <returns></returns>
        static string ByteArrayToString(byte[] arrInput)
        {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            for (i = 0; i < arrInput.Length; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }
        public static bool CompareHash(string s1, string s2)
        { return s1.Equals(s2); }
    }
}
