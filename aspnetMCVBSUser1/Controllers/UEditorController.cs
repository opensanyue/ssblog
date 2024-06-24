using Microsoft.AspNetCore.Mvc;
using UEditor.Core;


namespace aspnetMCVBSUser1.Controllers
{
    public class UEditorController : Controller
    {
        private readonly UEditorService _ueditorService;
       
        public UEditorController(UEditorService ueditorService)
        {
            this._ueditorService = ueditorService;
        }

        [HttpGet, HttpPost]
        [RequestSizeLimit(1000_000_000)]
        public ContentResult Upload()
        {
            var response = _ueditorService.UploadAndGetResponse(HttpContext);
            return Content(response.Result, response.ContentType);
        }
        public IActionResult Index()
        {
            return View();
        }

        public JsonResult UpImage(long? id)//id传过来，如需保存可以备用
        {
            int success = 0;
            string msg = "";
            string pathNew = "";
            try
            {
                var date = Request;
                var files = Request.Form.Files;
                foreach (var formFile in files)
                {
                    if (formFile.Length > 0)
                    {
                        string fileExt = formFile.FileName.Substring(formFile.FileName.LastIndexOf(".") + 1, (formFile.FileName.Length - formFile.FileName.LastIndexOf(".") - 1)); //扩展名
                        
                        long fileSize = formFile.Length; //获得文件大小，以字节为单位
                        //string md5 = CommonHelper.CalcMD5(formFile.OpenReadStream());
                        //string newFileName = md5 + "." + fileExt; //MD5加密生成文件名保证文件不会重复上传
                        var pathimg = Path.Combine("image", DateTime.Now.ToString("yyyyMMdd"));
                        var pathStart = Path.Combine(Directory.GetCurrentDirectory(),  "upload", pathimg); //AppContext.BaseDirectory + "/upload/" + DateTime.Now.Year + "/" + DateTime.Now.Month + "/";
                        if (System.IO.Directory.Exists(pathStart) == false)//如果不存在新建
                        {
                            System.IO.Directory.CreateDirectory(pathStart);
                        }

                        var index = 0;
                        var filePathExt = "";
                        var filePath = "";
                        do
                        {
                            filePathExt = string.Format(@"\" + GetUniqueFileName("img") + @"{0}." + fileExt, index++);
                            filePath = string.Format(pathStart+filePathExt);
                           
                        } while (System.IO.File.Exists(filePath));
                        
                        pathNew = "/upload/"+ pathimg.Replace(@"\","/")+filePathExt.Replace(@"\", "/");
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {

                            formFile.CopyTo(stream);
                            success = 1;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                success = 0;
                msg = ex.ToString();
            }
            var obj = new { success = success, url = pathNew, message = msg };
            return Json(obj);
        }

        public static string GetUniqueFileName(string prefix="")
        {
            string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            return $"{prefix}_{timeStamp}";
        }
    }
}
