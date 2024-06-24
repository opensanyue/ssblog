using System.Security.Cryptography;
using System.Text;

namespace aspnetMCVBSUser1.Server
{
    public class RsaServer
    {
        public static String[] GenerateKey(Int32 keySize = 2048)
        {
            //var rsa = new RSACryptoServiceProvider(keySize);

            var ss = new String[2];



            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                var publicKey = rsa.ToXmlString(false); // 公钥
                var privateKey = rsa.ToXmlString(true); // 私钥

                ss[0] = publicKey;
                ss[1] = privateKey;
            }
            return ss;
        }



        // 生成License的方法
        public static string createLicense(string licenseInfo, string publicKey, string privateKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                // 生成公钥和私钥
                //string publicKey = rsa.ToXmlString(false);
                // string privateKey = rsa.ToXmlString(true);
                // License信息（模拟）
                //string licenseInfo = "ValidLicenseInfo";
                rsa.FromXmlString(privateKey);
                // 使用私钥对License信息进行签名
                byte[] signature = rsa.SignData(Encoding.UTF8.GetBytes(licenseInfo), SHA256.Create());
                // 将公钥、License信息和签名组合成License
                string license = $"{publicKey};{licenseInfo};{Convert.ToBase64String(signature)}";
                return license;
            }
        }

        // 验证License的方法
        public static bool ValidateLicense(string userEnteredKey)
        {
            // 将License拆分成公钥、License信息和签名
            string[] parts = userEnteredKey.Split(';');
            string publicKey = parts[0];
            string licenseInfo = parts[1];
            byte[] signature = Convert.FromBase64String(parts[2]);
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                // 设置公钥
                rsa.FromXmlString(publicKey);
                // 使用公钥验证License信息的签名
                return rsa.VerifyData(Encoding.UTF8.GetBytes(licenseInfo), SHA256.Create(), signature);
            }
        }


        // SignData
        public static string SignData(string licenseInfo, string privateKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                // 使用私钥对License信息进行签名
                byte[] signature = rsa.SignData(Encoding.UTF8.GetBytes(licenseInfo), SHA256.Create());
                // 将公钥、License信息和签名组合成License

                return Convert.ToBase64String(signature);
            }
        }

        // 验证License的方法
        public static bool ValidateLicense(string pubKey, string encData, string sign)
        {
            // 将License拆分成公钥、License信息和签名

            string publicKey = pubKey;
            //byte[] signature = Convert.FromBase64String(encData)!;
            byte[] singnForm64 = Convert.FromBase64String(sign)!;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                // 设置公钥
                rsa.FromXmlString(publicKey);
                // 使用公钥验证License信息的签名
                var result = rsa.VerifyData(Encoding.UTF8.GetBytes(encData), SHA256.Create(), singnForm64);
                return result;
            }
        }
    }
}
