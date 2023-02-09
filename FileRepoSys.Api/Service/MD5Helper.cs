using FileRepoSys.Api.Service.Contract;
using System.Security.Cryptography;
using System.Text;

namespace FileRepoSys.Api.Service
{
    public class MD5Helper:IHashHelper
    {
        public string MD5Encrypt32(string password)
        {
            StringBuilder pwd = new StringBuilder("");
            MD5 md5 = MD5.Create();
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
            for (int i = 0; i < s.Length; i++)
            {
                pwd.Append(s[i].ToString("X"));
            }
            return pwd.ToString();
        }
    }
}
