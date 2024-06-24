using aspnetMCVBSUser1.Models.ViewModels;
using aspnetMCVBSUser1.Server.helper;
using Newtonsoft.Json;
using NuGet.Protocol;

namespace aspnetMCVBSUser1.Server
{
    public class JsonConfigHelper
    {

        private static Dictionary<string, string> configDic = new Dictionary<string, string>();

        /// <summary>
        /// 读取配置信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ReadConfig(string key)
        {
            if (File.Exists("config.json") == false)//如果不存在就创建file文件夹
            {
                FileStream f = File.Create("config.json");
                f.Close();
            }
            string s = File.ReadAllText("config.json");
            try
            {
                configDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(s)!;
            }
            catch
            {
                configDic = new Dictionary<string, string>();
            }

            if (configDic != null && configDic.ContainsKey(key))
            {
                return configDic[key];
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 添加配置信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void WriteConfig(string key, string value)
        {
            if (configDic == null)
            {
                configDic = new Dictionary<string, string>();
            }
            configDic[key] = value;
            string s = JsonConvert.SerializeObject(configDic);
            File.WriteAllText("config.json", s);
        }

        /// <summary>
        /// 删除配置信息
        /// </summary>
        /// <param name="key"></param>
        public static void DeleteConfig(string key)
        {
            if (configDic != null && configDic.ContainsKey(key))
            {
                configDic.Remove(key);
                string s = JsonConvert.SerializeObject(configDic);
                File.WriteAllText("config.json", s);
            }
        }
    
        public static T getSetting<T>(string filepath)
        {
            string sectionName = "AppSettings";
            var configurationBuilder=new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(filepath,optional:false,reloadOnChange:true);
            IConfiguration config= configurationBuilder.Build();
            T nodes= config.GetSection(sectionName).Get<T>()!;   
            
            //config.GetSection(sectionName).Bind(nodes);
            //config1 = config;
            return nodes!;
        }



        public static bool SaveSetting<T>(string filepath,  T model)
        {

            string sectionName = "AppSettings";
            //var configurationBuilder = new ConfigurationBuilder()
            //   .SetBasePath(Directory.GetCurrentDirectory())
            //   .AddConfiguration(config);
            //IConfiguration config1 = configurationBuilder.Build();
            //config.GetSection(sectionName).(model);
            try
            {
                string json = model.ToJson(Formatting.Indented);
                json = "{\r\n  \"" + sectionName + "\":" + json + "\r\n}";
                helperIO.WritefileAsync(Directory.GetCurrentDirectory() + @"\" + filepath.Replace("/", @"\"), json);
                //Thread.Sleep(10000);
                return true;
            } catch { 
            
            }
            return false;
        }
    }
}
