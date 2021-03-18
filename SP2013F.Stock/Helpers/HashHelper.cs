using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SP2013F.Stock.Helpers
{
    public static class HashHelper
    {
        public static string GetMd5Hash(string input)
        {
            using (var hash = MD5.Create())
            {
                byte[] data = hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                    sBuilder.Append(data[i].ToString("x2"));
                return sBuilder.ToString();
            }
        }
    }
}
