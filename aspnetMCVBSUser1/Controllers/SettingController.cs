using aspnetMCVBSUser1.Models.ViewModels;
using aspnetMCVBSUser1.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Management;

namespace aspnetMCVBSUser1.Controllers
{
    public class SettingController : Controller
    {
        //private readonly IOptionsSnapshot<ConfigModel> model;
        private readonly IWebHostEnvironment webHostEnvironment;
        public SettingController(IWebHostEnvironment webHostEnvironment)//IConfiguration configuration)
        {
            this.webHostEnvironment = webHostEnvironment;
            //this.model = config;
        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public IActionResult Edit()
        {
            if (PubServer.Setting == null) return NotFound();
            ViewData["AppName"] = PubServer.Setting.AppName;
            return View(PubServer.Setting);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([Bind("AppName,Author,PageSize,DefultEditorIsMarkdown")] ConfigModel configModel)
        {
            if (ModelState.IsValid)
            {
                try
                {       
                    // 读取appsettings.json文本内容
                    //string filePath = Path.Combine(webHostEnvironment.ContentRootPath, "appsettings.json");
                    //string text = System.IO.File.ReadAllText(filePath);

                    // 修改配置项
                    //JObject obj = JObject.Parse(text);

                    //bool hasCode = obj.ContainsKey("AppSettings");
                    //if (hasCode)
                    //{
                    //    JObject objapp = JObject.Parse(obj["AppSettings"]!.ToString());

                    //    hasCode = objapp.ContainsKey("AppName");
                    //    if (hasCode)
                    //    {
                    //        obj["AppSettings"]!["AppName"] = configModel.AppName;
                    //    }

                    //    hasCode = objapp.ContainsKey("Author");
                    //    if (hasCode)
                    //    {
                    //        obj["AppSettings"]!["Author"] = configModel.Author;
                    //    }

                    //    hasCode = objapp.ContainsKey("PageSize");
                    //    if (hasCode)
                    //    {
                    //        obj["AppSettings"]!["PageSize"] = configModel.PageSize;
                    //    }

                    //    hasCode = objapp.ContainsKey("DefultEditorIsMarkdown");
                    //    if (hasCode)
                    //    {
                    //        obj["AppSettings"]!["DefultEditorIsMarkdown"] = configModel.DefultEditorIsMarkdown;
                    //    }

                        PubServer.Setting.AppName = configModel.AppName;
                        PubServer.Setting.Author = configModel.Author;
                        PubServer.Setting.PageSize = configModel.PageSize;
                        PubServer.Setting.DefultEditorIsMarkdown = configModel.DefultEditorIsMarkdown;

                        bool result = JsonConfigHelper.SaveSetting("upload/setting.json", PubServer.Setting);
                    // 重新写入appsettings.json
                    //string result = obj.ToString();
                    //System.IO.File.WriteAllText(filePath, result);

                    // }
                }
                catch (DbUpdateConcurrencyException)
                {

                    return NotFound();
                }
                //return RedirectToAction(nameof(Index), "Categories");
                //return View(Content("<script>alert('保存成功');</script>"));
                ViewData["AppName"] = PubServer.Setting.AppName;
                ViewData["isshowalert"] = "showalert()";

            }

            return View(configModel);
        }



        public IActionResult EditLic()
        {
            if (PubServer.Setting == null) return NotFound();
            ViewData["RegInfo"] =checkAuthor.getRegInfo();
            ViewData["AppName"] = PubServer.Setting.AppName;
            ViewData["WelcomeSpeech"] = PubServer.WelcomeSpeech;
            ViewData["Grade"] = PubServer.lic != null ? PubServer.lic.Grade : 0;
            ViewData["Content"] = PubServer.lic != null ? PubServer.lic.Content : "";
            return View();
        }



        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLic([Bind("Lic")] ConfigModel configModel)
        {
            if (ModelState.IsValid)
            {
                try
                { 
                    ViewData["RegInfo"] = checkAuthor.getRegInfo();
                    //校验输入的lic是否合法
                    Lic? licinfo =await checkAuthor.getUserLic(configModel.Lic!, PubServer.PUB_KEY)!;
                    if (licinfo == null)
                    {
                        ViewData["AppName"] = PubServer.Setting.AppName;
                        //ViewData["isshowalert"] = "showalert()";
                        ViewBag.js = "<script>alert('不合法的License');</script>";
                        return View(configModel);
                    }
                    string locmac = checkAuthor.getRegInfo();//GetSystemInfoServer.Get_CPUID();
                    if (licinfo.MachineCode != locmac)
                    {
                        ViewData["AppName"] = PubServer.Setting.AppName;
                        ViewBag.js = "<script>alert('不适用的License');</script>";
                        return View(configModel);
                    }
                    PubServer.lic = licinfo;
                    PubServer.WelcomeSpeech =  checkAuthor.getUserWelcomeSpeech(licinfo);

                    PubServer.Setting.Lic = configModel.Lic;
                    bool result = JsonConfigHelper.SaveSetting("upload/setting.json", PubServer.Setting);


                    // 读取appsettings.json文本内容
                    //string filePath = Path.Combine(webHostEnvironment.ContentRootPath, "appsettings.json");
                    //string text = System.IO.File.ReadAllText(filePath);

                    // 修改配置项
                    //JObject obj = JObject.Parse(text);

                    //bool hasCode = obj.ContainsKey("AppSettings");
                    //if (hasCode)
                    //{
                    //    JObject objapp = JObject.Parse(obj["AppSettings"]!.ToString());

                    //    hasCode = objapp.ContainsKey("Lic");
                    //    if (hasCode)
                    //    {
                    //        obj["AppSettings"]!["Lic"] = configModel.Lic;
                    //    }

                    //    // 重新写入appsettings.json
                    //    string result = obj.ToString();
                    //    System.IO.File.WriteAllText(filePath, result);
                    //}

                }
                catch (DbUpdateConcurrencyException)
                {

                    return NotFound();
                }
                //return RedirectToAction(nameof(Index), "Categories");
                //return View(Content("<script>alert('保存成功');</script>"));
                ViewData["AppName"] = PubServer.Setting.AppName;
                ViewBag.js = "<script>alert('保存成功');</script>";
                
            }
            return View(configModel);
        }

        //public static string Get_CPUID()
        //{
        //    try
        //    {
        //        //需要在解决方案中引用System.Management.DLL文件
        //        ManagementClass mc = new ManagementClass("Win32_Processor");
        //        ManagementObjectCollection moc = mc.GetInstances();
        //        string strCpuID = null;
        //        foreach (ManagementObject mo in moc)
        //        {
        //            strCpuID = mo.Properties["ProcessorId"].Value.ToString();
        //            mo.Dispose();
        //            break;
        //        }
        //        return strCpuID;
        //    }
        //    catch
        //    {
        //        return "";
        //    }
        //}
        ///// <summary>
        ///// 获取文件路径
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public string GetPath()
        //{
        //    string filePath = Path.Combine(webHostEnvironment.ContentRootPath, "appsettings.json");
        //    return filePath;
        //}
        ///// <summary>
        ///// 读取配置参数
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public string Get()
        //{

        //    string defultEditorIsMarkdown = model.Value.DefultEditorIsMarkdown.ToString(); //configuration["AppSettings:DefultEditorIsMarkdown"];
        //    string pageSize = model.Value.PageSize.ToString(); //configuration["AppSettings:pageSize"];
        //    string appName = model.Value.AppName!.ToString();//configuration["AppSettings:AppName"];
        //    string author = model.Value.Author!.ToString();// configuration["AppSettings:Author"];
        //    string defaultLanguage = model.Value.DefaultLanguage!.ToString();// configuration["AppSettings:DefaultLanguage"];
        //    string minLevel = model.Value.MinLevel!.ToString(); //configuration["AppSettings:MinLevel"];
        //    string maxLevel = model.Value.MaxLevel!.ToString(); //configuration["AppSettings:MaxLevel"];
        //    string longitude = model.Value.Center!.Longitude!.ToString(); //configuration["AppSettings:Center:Longitude"];
        //    string latitude = model.Value.Center.Latitude!.ToString();// configuration["AppSettings:Center:Latitude"];
        //    return defultEditorIsMarkdown + "\n" + pageSize + "\n" + appName + "\n" + author + "\n" +
        //           defaultLanguage + "\n" + minLevel + "\n" + maxLevel + "\n" +
        //           longitude + "\n" + latitude;

        //}

        ///// <summary>
        ///// 修改配置参数
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public string setProperty(string propertyName, string propertyValue)
        //{
        //    try
        //    {
        //        // 读取appsettings.json文本内容
        //        string filePath = Path.Combine(webHostEnvironment.ContentRootPath, "appsettings.json");
        //        string text = System.IO.File.ReadAllText(filePath);

        //        // 修改配置项
        //        JObject obj = JObject.Parse(text);

        //        bool hasCode = obj.ContainsKey("AppSettings");
        //        if (!hasCode) return "failed";
        //        JObject objapp = JObject.Parse(obj["AppSettings"]!.ToString());
        //        hasCode = objapp.ContainsKey(propertyName);
        //        if (!hasCode) return "failed";
        //        obj["AppSettings"]![propertyName] = propertyValue;

        //        // 重新写入appsettings.json
        //        string result = obj.ToString();
        //        System.IO.File.WriteAllText(filePath, result);
        //        return "success";
        //    }
        //    catch
        //    {
        //        return "failed";
        //    }
        //}

        ///// <summary>
        ///// 修改配置参数(支持2级属性)
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public string setProperty2(string propertyName, string childpropertyNamechild, string propertyValue)
        //{
        //    try
        //    {
        //        // 读取appsettings.json文本内容
        //        string filePath = Path.Combine(webHostEnvironment.ContentRootPath, "appsettings.json");
        //        string text = System.IO.File.ReadAllText(filePath);

        //        // 修改配置项
        //        JObject obj = JObject.Parse(text);

        //        bool hasCode = obj.ContainsKey("AppSettings");
        //        if (!hasCode) return "failed";
        //        JObject objapp = JObject.Parse(obj["AppSettings"]!.ToString());
        //        hasCode = objapp.ContainsKey(propertyName);
        //        if (!hasCode) return "failed";
        //        JObject childobjapp = JObject.Parse(objapp[propertyName]!.ToString());
        //        hasCode = childobjapp.ContainsKey(childpropertyNamechild);
        //        if (!hasCode) return "failed";
        //        obj["AppSettings"]![propertyName]![childpropertyNamechild] = propertyValue;

        //        // 重新写入appsettings.json
        //        string result = obj.ToString();
        //        System.IO.File.WriteAllText(filePath, result);
        //        return "success";
        //    }
        //    catch
        //    {
        //        return "failed";
        //    }
        //}

        ///// <summary>
        ///// 修改配置参数
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public string GetUpdata()
        //{
        //    try
        //    {
        //        // 读取appsettings.json文本内容
        //        string filePath = Path.Combine(webHostEnvironment.ContentRootPath, "appsettings.json");
        //        string text = System.IO.File.ReadAllText(filePath);

        //        // 修改配置项
        //        JObject obj = JObject.Parse(text);
        //        obj["AppSettings"]!["AppName"] = "API";


        //        // 重新写入appsettings.json
        //        string result = obj.ToString();
        //        System.IO.File.WriteAllText(filePath, result);
        //        return "success";
        //    }
        //    catch
        //    {
        //        return "failed";
        //    }
        //}

    }
}
