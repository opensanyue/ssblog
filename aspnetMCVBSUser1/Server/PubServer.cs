using aspnetMCVBSUser1.Models.ViewModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;

namespace aspnetMCVBSUser1.Server
{
    public class PubServer
    {
        static PubServer()
        {
            Setting = JsonConfigHelper.getSetting<ConfigModel>("upload/setting.json");
        }

        public static readonly string PUB_KEY = "<RSAKeyValue><Modulus>yOCc0VmjF/wPu2ihMFEMip7OyfVQ2/dMECrcM/nHGNh0AyAFsSlYVBpiTX1FN4wTAcCoSfHIZbecnOD1HSmHNLpW+I9wQtC8QR//LbsyHUZbS2HA+n6kL9mRDmzp5GPzDD/dUlRgpApWbecZDPL3XvY+ajatTwRR2eUg+YZRYBU=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        public static string WelcomeSpeech = "免费版";
        public static Lic? lic = null;
        public static string[] WelcomeSpeechs = new string[] { "免费版", "青铜用户", "白银用户", "黄金用户", "铂金用户", "钻石用户" };
        public static readonly ConfigModel Setting;

    }
}
