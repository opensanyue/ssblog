using aspnetMCVBSUser1.Models.ViewModels;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace aspnetMCVBSUser1.Server
{
    public static class checkAuthor
    {
        public static string getUserWelcomeSpeech(Lic lic)
        {
          
            if (lic != null)
            {
               return (lic.Grade > 0 && lic.Grade < 5) ?
                                            PubServer.WelcomeSpeechs[lic.Grade] :
                                            PubServer.WelcomeSpeechs[0];
                
            }
            else
            {
                return PubServer.WelcomeSpeechs[0];
            }
        }

        public static async Task<Lic?> getUserLic(string Lic, string pubKey)
        {
            Lic? result = await Task.Run(() => getLicInfo(Lic, pubKey));
            return result;
        }
        public static bool checkLic(string Lic, string pubKey,out Lic? data)
        {
            try
            {
                data = null;
                //校验输入的lic是否合法
                Lic licinfo = getLicInfo(Lic, pubKey)!;
                if (licinfo == null) return false;
                data = licinfo;
                string licmac = licinfo.MachineCode!;
                string locmac = GetSystemInfoServer.GetMacAddress();
                if (licmac != locmac)
                {                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                data = null;
                return false;
            }
            return true;
        }
        public static bool checkLic(string Lic, string pubKey)
        {          
            try
            {
                //校验输入的lic是否合法
                Lic licinfo = getLicInfo(Lic, pubKey)!;
                if (licinfo == null) return false;

                string licmac = licinfo.MachineCode!;
                string locmac = getRegInfo(); //GetSystemInfoServer.Get_CPUID();
                if (licmac != locmac)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

      
        //private static string RSA_PUB_KEY="";
        public static Lic? getLicInfo(string lic, string pubKey)
        {
            if (string.IsNullOrWhiteSpace(lic))
            {
                return null;
            }
            try
            {
                string aesKey = lic.Substring(0, 32);
                int encDataLength = Int16.Parse(lic.Substring(32, 4));
                string encData = lic.Substring(36, encDataLength);
                string sign = lic.Substring(36 + encDataLength);
                if (!RsaServer.ValidateLicense(pubKey, encData, sign))
                {
                    return null;
                }
                string data = AesServer.AesDecrypt(encData, aesKey);
                Console.WriteLine(data);
                if(string.IsNullOrWhiteSpace(data)) return null;
                Lic lic1 = JsonConvert.DeserializeObject<Lic>(data)!;
                if (lic == null) return null;
                //string appId = JsonUtil.getString(data, "appId");
                //long notBefore = JsonUtil.getLong(data, "notBefore");
                //long notAfter = JsonUtil.getLong(data, "notAfter");
                //string customerInfo = JsonUtil.getString(data, "customerInfo");
                //if (!MyUtil.getAppId(App.getContext()).equals(appId))
                //{
                //    return false;
                //}
                //long time = System.currentTimeMillis();
                //if (time < notBefore || time > notAfter)
                //{
                //    return false;
                //}
                return lic1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static string getLic(string sourceStr, string aeskey, string priKey)
        {
            string lic = "";
            try
            {
                string aesKey = aeskey;
                string encData = AesServer.AesEncrypt(sourceStr, aesKey);
                string encDataLength = encData.Length.ToString();
                if (encDataLength.Length < 4 && encDataLength.Length > 0)
                {
                    encDataLength = encDataLength.PadLeft(4, '0'); ;
                }
                else if (encDataLength.Length > 4 || encDataLength.Length <= 0)
                {
                    //MessageBox.Show("数据太长,或太短，请重新设置");
                    Console.WriteLine("数据太长,或太短，请重新设置");
                    return "";
                }

                string sign = RsaServer.SignData(encData, priKey);
                lic = aesKey + encDataLength + encData + sign;
                Console.WriteLine("bruce", lic);
                return lic;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return lic;
            }
        }


        //方法1：length为产生的位数
        public static string getRandomString(int length)
        {
            //定义一个字符串（A-Z，a-z，0-9）即62位；
            string str = "zxcvbnmlkjhgfdsaqwertyuiopQWERTYUIOPASDFGHJKLZXCVBNM1234567890";
            //由Random生成随机数
            Random random = new Random();
            StringBuilder sb = new StringBuilder();
            //长度为几就循环几次
            for (int i = 0; i < length; ++i)
            {
                //产生0-61的数字
                int number = (int)random.NextInt64(62);
                //将产生的数字通过length次承载到sb中
                sb.Append(str[number]);
            }
            //将承载的字符转换成字符串
            return sb.ToString();
        }

        public static string getRegInfo()
        {
            string localcpu = GetSystemInfoServer.Get_CPUID();
            string keyinfo = Encrypt(localcpu);
            keyinfo = keyinfo.ToUpper();
            keyinfo = keyinfo.Insert(5, "-").Insert(11, "-").Insert(17, "-").Insert(23, "-").Insert(29, "-");
            return keyinfo;
        }

        /// <summary>
        /// 对遍历到的字节进行加密
        /// </summary>
        /// <param name="strPwd">输入的待加密的字符串</param>
        /// <returns name="str">返回加密后的值</returns>
        public static string Encrypt(string strPwd)
        {
            MD5 md5 = MD5.Create();
            byte[] data = System.Text.Encoding.Default.GetBytes(strPwd);//将字符编码为一个字节序列
            byte[] md5data = md5.ComputeHash(data);                     //计算data字节数组的哈希值
            md5.Clear();       //清空MD5对象
            string str = "";   //定义一个变量，用来记录加密后的密码
            for (int i = 0; i < md5data.Length - 1; i++)
            {
                str += md5data[i].ToString("x").PadLeft(2, '0');
            }
            
            return str;
        }

        
    }

}
