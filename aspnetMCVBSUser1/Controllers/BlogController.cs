using aspnetMCVBSUser1.Data;
using aspnetMCVBSUser1.Models;
using aspnetMCVBSUser1.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Net.Http.Headers;
using System.IO;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using UEditor.Core;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Text;
using Microsoft.Extensions.Hosting;
using System.Text.Unicode;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Management;
using aspnetMCVBSUser1.Server;

namespace aspnetMCVBSUser1.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        //private readonly IOptionsSnapshot<ConfigModel> model;

        private static int currentCategory = -1;
        private static int currentPage = 0;


        private static IndexListParm postParm = new IndexListParm();
        private static PaginatedListParm paginatedListParm = new PaginatedListParm();
        private int pageSize = 15;

        private int step = 1000000; //排序字段之间间隔

        public BlogController(ApplicationDbContext context,  IWebHostEnvironment _webHostEnvironment)
        {
            _context = context;
            //this.model = config;
            pageSize = PubServer.Setting.PageSize >0? PubServer.Setting.PageSize : pageSize;
            this._webHostEnvironment = _webHostEnvironment;
        }

        public IActionResult GetUploadMultipleFiles(int ID)
        {
            categoryID = ID;
            return View();
        }

        public async Task<IActionResult> ImportMd()
        {

            string mdstr1 = "";
            Console.WriteLine(mdFileOrDir + " - " + mdFileImgDir);
            string files = mdFileOrDir;
            string dirs = mdFileImgDir;
            List<PostImg> ls = new List<PostImg>();
            if (files == null || string.IsNullOrWhiteSpace(files))
            {
                return NotFound("没有文件");
            }


            if (files.ToLower().EndsWith(".md")) //一个文件
            {
                if (System.IO.File.Exists(files))
                {
                    var lsone = importPostMulite(new List<string>() { files }, dirs);
                    ls.AddRange(lsone);
                }
            }
            else //目录
            {
                if (System.IO.Directory.Exists(files))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(files);
                    //FileInfo[] files1 = directoryInfo.GetFiles("*.md");
                    //对名字按数字顺序排序
                    List<FileInfo> files1 = directoryInfo.GetFiles("*.md").OrderBy(n => Regex.Replace(n.Name, @"\d+", n => n.Value.PadLeft(5, '0'))).ToList();
                    files1.ForEach(x => Console.WriteLine(x.Name));

                    foreach (FileInfo file in files1)
                    {
                        if (System.IO.File.Exists(file.FullName))
                        {
                            var lsMuil = importPostMulite(new List<string>() { file.FullName }, dirs);
                            ls.AddRange(lsMuil);
                        }
                    }
                }
            }

            //导入md及其内的图像
            //找出所有md的name不为空的项。
            var ls1 = ls.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToList();

            foreach (var x in ls1)
            {
                if (System.IO.File.Exists(x.Name))
                {
                    string mdstr = System.IO.File.ReadAllText(x.Name, Encoding.UTF8);
                    if (x.imgList != null && x.imgList.Count > 0)
                    {
                        //导入md                        
                        List<Keyval> Imgs = x.imgList.Where(x => x.IsExists == true).ToList();

                        foreach (Keyval item in Imgs)
                        {
                            //上传图处
                            string imgpath = copyimg(item.Value);
                            //更新图片链接位置
                            if (!string.IsNullOrWhiteSpace(imgpath))
                                mdstr = mdstr.Replace(item.Key, imgpath);


                        }
                    }
                    string postTitle = x.Name.Substring(x.Name.LastIndexOf("\\") + 1, (x.Name.LastIndexOf(".") - x.Name.LastIndexOf("\\") - 1)); //扩展名
                    if (string.IsNullOrWhiteSpace(postTitle)) postTitle = "无标题";                                                                                                                                                                         //保存到数据库
                    await savePostToDb(postTitle, mdstr, categoryID);
                    //mdstr1 = mdstr;

                }
            }

            //ViewBag.mdstr= mdstr1;

            mdFileOrDir = "";
            mdFileImgDir = "";
            categoryID = 0;
            return RedirectToAction(nameof(Index), "Blog");
        }

        private async Task<int> savePostToDb(string title, string content, int cateid)
        {
            //如果分类为-1，找到第一层的第一个分类，如果没有则保存失败。
            if (cateid == -1)
            {
                var postitem = _context.Categorys.Where(x => x.PID <= 0).OrderBy(x => x.CSort).FirstOrDefault();
                if (postitem != null)
                {
                    cateid = postitem.Id;
                }
                else
                {
                    return 0;
                }
            }
            try
            {
            Post post = new Post();
            post.Title = title;
            post.Content = content;
            post.PostType = 1;
            post.CategoryId = cateid;
            post.CreateDate = DateTime.Now;
            post.UpdataDate = DateTime.Now;
            var ii = _context.Posts.Where(x => x.CategoryId == cateid);
            var i= (ii.Any() ? ii.Max(x => x.CSort):0)+step ;
            post.CSort = i;
           
                _context.Add(post);
                return await _context.SaveChangesAsync();
            }
            catch
            {

            }
            return 0;
        }
        private string copyimg(string oldfullname)
        {

            string pathNew = "";
            try
            {
                string fileExt = oldfullname.Substring(oldfullname.LastIndexOf(".") + 1, (oldfullname.Length - oldfullname.LastIndexOf(".") - 1)); //扩展名


                var pathimg = Path.Combine("image", DateTime.Now.ToString("yyyyMMdd"));
                var pathStart = Path.Combine(Directory.GetCurrentDirectory(), "upload", pathimg); //AppContext.BaseDirectory + "/upload/" + DateTime.Now.Year + "/" + DateTime.Now.Month + "/";
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
                    filePath = string.Format(pathStart + filePathExt);

                } while (System.IO.File.Exists(filePath));

                pathNew = "/upload/" + pathimg.Replace(@"\", "/") + filePathExt.Replace(@"\", "/");
                //using (var stream = new FileStream(filePath, FileMode.Create))
                //{

                //    formFile.CopyTo(stream);
                System.IO.File.Copy(oldfullname, filePath);

                //}
                return pathNew;
            }
            catch { }
            return "";
        }
        public static string GetUniqueFileName(string prefix = "")
        {
            string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            return $"{prefix}_{timeStamp}";
        }




        private static string mdFileOrDir = "";
        private static string mdFileImgDir = "";
        private static int categoryID;
        //导入md文件前，先验一下图片地址
        public IActionResult CheckMultipleMdFileImg(string files, string dirs, string cateID)
        {

            List<PostImg> ls = new List<PostImg>();
            if (files == null || string.IsNullOrWhiteSpace(files))
            {
                return NotFound("没有文件");
            }
            mdFileOrDir = files;
            mdFileImgDir = dirs;

            if (files.ToLower().EndsWith(".md")) //一个文件
            {
                if (System.IO.File.Exists(files))
                {
                    var lsone = importPostMulite(new List<string>() { files }, dirs);
                    ls.AddRange(lsone);
                }
            }
            else //目录
            {
                if (System.IO.Directory.Exists(files))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(files);
                    FileInfo[] files1 = directoryInfo.GetFiles("*.md");
                    foreach (FileInfo file in files1)
                    {
                        if (System.IO.File.Exists(file.FullName))
                        {
                            var lsMuil = importPostMulite(new List<string>() { file.FullName }, dirs);
                            ls.AddRange(lsMuil);
                        }
                    }
                }
            }

            //var ls = importPostMulite(files.ToList(), "G:\\docFile\\我的文档\\md\\");

            var ls1 = ls.Where(x => x.imgList == null || x.imgList.Count == 0).ToList();
            List<PostImg> ls2 = new List<PostImg>();
            foreach (var x in ls)
            {
                if (x.imgList != null && x.imgList.Count > 0)
                {
                    List<Keyval> ls3 = new List<Keyval>();
                    List<Keyval> noImg = x.imgList.Where(x => x.IsExists == false).ToList();
                    if (noImg.Count > 0)
                    {
                        noImg.ForEach(x => ls3.Add(x));
                        x.imgList = ls3;
                        ls2.Add(x);
                    }

                }
            }
            ls2.AddRange(ls1);

            return View(ls2); //Ok(new { count = files.Count, message = "Files uploaded successfully" });
        }

        #region CheckMultipleMdFileImg相关
        List<PostImg> importPostMulite(List<string> importMdNames, string mdImgPath = "")
        {
            List<PostImg> list = new List<PostImg>();
            foreach (var item in importMdNames)
            {
                PostImg img = importPostOne(item, mdImgPath);
                list.Add(img);
            }

            return list;
        }
        PostImg importPostOne(string importMdName, string mdImgPath = "")
        {
            List<Keyval> list = new List<Keyval>();
            try
            {
                if (!System.IO.File.Exists(importMdName)) return new PostImg();
                var oldmd = System.IO.File.ReadAllText(importMdName);
                //将+转义%2b，将空格转义为”+“
                var md = oldmd.Replace("+", "%2b").Replace(" ", "+");
                var document = Markdown.Parse(md);
                //两种类型路径，
                //1、指定路径 d:\images\
                //2、md文件所在目录中同名目录 如：d:\md\文件.md 图片目录就是d:\md\文件\ 
                //如果1目录不为空，就在1中先找，找不到找2，所以要两个路径
                string sourcePath = "";
                string sourcePath1 = "";
                string oldmdname = Path.GetFileName(importMdName);
                if (!string.IsNullOrWhiteSpace(mdImgPath))
                {
                    sourcePath = mdImgPath;
                }

                sourcePath1 = importMdName.Substring(0, importMdName.LastIndexOf("\\")) + "\\"; //扩展名
                if (!Path.Exists(sourcePath1)) { sourcePath1 = ""; }
                if (!Path.Exists(sourcePath)) { sourcePath = ""; }
                if (string.IsNullOrWhiteSpace(sourcePath1) && string.IsNullOrWhiteSpace(sourcePath)) return new PostImg();

                //遍历所有图像链接
                foreach (var node in document.AsEnumerable())
                {
                    if (node is not ParagraphBlock { Inline: { } } paragraphBlock) continue;

                    foreach (var inline in paragraphBlock.Inline)
                    {


                        if (inline is not LinkInline { IsImage: true } linkInline) continue;

                        //链接为空或是http就不改
                        if (linkInline.Url == null) continue;
                        if (linkInline.Url.StartsWith("http")) continue;

                        string formFile = System.Web.HttpUtility.UrlDecode(linkInline.Url).Replace("/", "\\");
                        // 路径处理
                        if (formFile.StartsWith("..\\"))
                        {
                            int startPos = formFile.LastIndexOf("..\\") + 3;
                            int endPos = formFile.Length - formFile.LastIndexOf("..\\") - 3;
                            string ls = formFile.Substring(startPos, endPos);
                            ls = sourcePath + ls;

                            list.Add(new Keyval() { Key = linkInline.Url.Replace("+", " ").Replace("%2b", "+"), Value = ls, IsExists = System.IO.File.Exists(ls) });
                        }
                        else
                        {
                            //文件名转义
                            string ls = sourcePath1 + formFile;
                            list.Add(new Keyval() { Key = linkInline.Url.Replace("+", " ").Replace("%2b", "+"), Value = ls, IsExists = System.IO.File.Exists(ls) });
                        }



                        //var imgFilename = Path.GetFileName(linkInline.Url);
                        //var destDir1 = destDir;
                        //if (!Directory.Exists(destDir1)) Directory.CreateDirectory(destDir1);
                        //var destPath = Path.Combine(destDir1, imgFilename);
                        //if (File.Exists(destPath))
                        //{
                        //    // 图片重名处理
                        //    var imgId = Guid.NewGuid().ToString();
                        //    imgFilename = $"{Path.GetFileNameWithoutExtension(imgFilename)}-{imgId}.{Path.GetExtension(imgFilename)}";
                        //    destPath = Path.Combine(destDir, imgFilename);
                        //}

                        //// 复制图片
                        ////File.Copy(imgPath, destPath);
                        //Console.WriteLine($"复制 {imgPath} 到 {destPath}");

                        //
                    }
                }

                //string newPost = oldmd;
                //var list1 = list.Distinct().ToList();
                //foreach (var item in list1)
                //{
                //    newPost = newPost.Replace(item.Key, item.Value);
                //}

            }
            catch
            {

            }

            PostImg postImg = new PostImg();
            postImg.Name = importMdName;
            postImg.imgList = list;

            return postImg;
        }

        #endregion

        public async Task<IActionResult> UploadMultipleFiles()
        {
            var date = Request;
            var files = Request.Form.Files;

            if (files == null || files.Count == 0)
            {
                return BadRequest("No files provided.");
            }
            var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue
                                .Parse(file.ContentDisposition)
                                .FileName
                                .Trim('"');

                    var fullPath = Path.Combine(uploadsDir, fileName);
                    //using (var stream = new FileStream(fullPath, FileMode.Create))
                    //{
                    //    await file.CopyToAsync(stream);
                    //}
                }
            }
            return Ok(new { count = files.Count, message = "Files uploaded successfully" });
        }

        #region 自动生成

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            ViewData["AppName"] = PubServer.Setting.AppName;
            ViewData["WelcomeSpeech"] = PubServer.WelcomeSpeech;
            ViewData["Grade"] = PubServer.lic != null ? PubServer.lic.Grade : 0;
            ViewData["Content"] = PubServer.lic != null ? PubServer.lic.Content : "";
            return View(post);
        }

        [Authorize]
        // GET: Posts/Create
        public IActionResult Create(int? categoryId)
        {
            List<Category> list = LoadDataSelect(_context.Categorys.ToList());
            ViewData["CategoryId"] = new SelectList(list, "Id", "Name", categoryId);
            ViewData["AppName"] = PubServer.Setting.AppName;
            return View();
        }
        [Authorize]
        public IActionResult CreateMD(int? categoryId)
        {
            List<Category> list = LoadDataSelect(_context.Categorys.ToList());
            ViewData["CategoryId"] = new SelectList(list, "Id", "Name", categoryId);
            ViewData["AppName"] = PubServer.Setting.AppName;
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,PostType,hits,CategoryId,CSort")] Post post)
        {
            if (ModelState.IsValid)
            {
                post.CreateDate = DateTime.Now;
                post.UpdataDate = DateTime.Now;
                //生成排序号
                var ii = _context.Posts.Where(x => x.CategoryId == post.CategoryId);
                var i = (ii.Any() ? ii.Max(x => x.CSort) : 0) + step;
                post.CSort = i;
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), "Blog");
            }

            ViewData["CategoryId"] = new SelectList(_context.Categorys, "Id", "Id", post.CategoryId);
            return View(post);
        }

        #region Edit相关方法
        private List<Category> LoadDataSelect(List<Category> data)
        {
            List<Category> list = new List<Category>();
            var nodes = data.Where(x => x.PID == 0).OrderBy(x => x.CSort).Select(x => new Category
            {
                Id = x.Id,
                Name = x.Name,

            }).ToList();
            foreach (var item in nodes)
            {
                list.Add(item);
                LoadChildrensSelect(list, item, data, 1);
            }
            return list;
        }

        private void LoadChildrensSelect(List<Category> list, Category categoryItem, List<Category> data, int Lever)
        {
            var children = data.Where(x => x.PID == categoryItem.Id).OrderBy(x => x.CSort).Select(x => new Category
            {
                Id = x.Id,
                Name = getSpace(Lever) + "" + x.Name,
            }).ToList();
            foreach (var item in children)
            {

                list.Add(item);
                LoadChildrensSelect(list, item, data, Lever + 1);
            }

        }

        private string getSpace(int num = 1)
        {
            string str = "";
            for (int i = 0; i < num; i++)
            {
                str = str + "&nbsp;&nbsp;&nbsp;";
            }
            return System.Web.HttpUtility.HtmlDecode(str);
        }
        #endregion

        [Authorize]
        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            List<Category> list = LoadDataSelect(_context.Categorys.ToList());
            ViewData["CategoryId"] = new SelectList(list, "Id", "Name", post.CategoryId);
            ViewData["AppName"] = PubServer.Setting.AppName;
            return View(post);
        }

        [Authorize]
        public async Task<IActionResult> EditMD(int? id)
        {
            return await Edit(id);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,CreateDate,hits,PostType,CategoryId,CSort")] Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    post.UpdataDate = DateTime.Now;
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), "Blog");
            }
            ViewData["CategoryId"] = new SelectList(_context.Categorys, "Id", "Id", post.CategoryId);
            return View(post);
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }

        [HttpPost]
        public JsonResult IndexListParmAsync()
        {
            return Json(new ReturnJsonInfo(0, "", new { postParm = postParm, paginatedListParm = paginatedListParm })); ;
        }
        private static bool isFirstRun=true;
        public async Task<IActionResult> Index()
        {
            //如果首次运行，设置用户欢迎词
            if (isFirstRun)
            {
                if (checkAuthor.checkLic(PubServer.Setting.Lic!, PubServer.PUB_KEY) == true)
                {
                    PubServer.lic = await checkAuthor.getUserLic(PubServer.Setting.Lic!, PubServer.PUB_KEY);

                    if (PubServer.lic != null)
                    {
                        PubServer.WelcomeSpeech = checkAuthor.getUserWelcomeSpeech(PubServer.lic);
                    }
                }
                    isFirstRun = false;
                
            }
            
            
            ViewData["AppName"] = PubServer.Setting.AppName;
            ViewData["WelcomeSpeech"] =PubServer.WelcomeSpeech;
            ViewData["Grade"] = PubServer.lic!=null? PubServer.lic.Grade:0;
            ViewData["Content"] = PubServer.lic != null ? PubServer.lic.Content : "";
            return View(await _context.Categorys.ToListAsync());
        }


        private bool CategoryExists(int id)
        {
            return _context.Categorys.Any(e => e.Id == id);
        }


        [Authorize]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> EditName([FromBody] Category category)
        {
            if (category == null || category.Id <= 0)
            {
                return Json(new ReturnJsonInfo(1, "参数为空或Id小于0", ""));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Category result = _context.Categorys.Where(c => c.Id == category.Id).FirstOrDefault()!;
                    if (result != null)
                    {
                        if (result.Name != category.Name)
                        {
                            result.Name = category.Name;
                            _context.Update(result);
                            var resultsave = await _context.SaveChangesAsync();
                            if (resultsave == 1) return Json(new ReturnJsonInfo(0, "", ""));
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return Json(new ReturnJsonInfo(1, "不存在的ID。", ""));
                    }
                    else
                    {
                        throw;
                    }
                }

            }
            return Json(new ReturnJsonInfo(1, "编辑名字失败", ""));
        }
        [Authorize]
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //递归删除当前分类极下面的所有分类
            var category = await _context.Categorys.FindAsync(id);
            if (category != null)
            {
                var result1 = _context.Categorys.Where(x => x.PID == category.Id);
                if (result1 != null && result1.Count() > 0)
                    deltree(result1.ToList());
                _context.Categorys.Remove(category);
            }

            var result = await _context.SaveChangesAsync();
            if (result >= 1)
            {
                int filecount = _context.Posts.Count();
                return Json(new ReturnJsonInfo(0, "", new { fileCount = filecount }));
            }
            return Json(new ReturnJsonInfo(1, "删除失败", new object()));
        }
        [Authorize]
        // GET: Posts/Delete/5
        public async Task<IActionResult> PostDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }
        // POST: Posts/Delete/5
        [Authorize]
        [HttpPost, ActionName("PostDeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostDeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private void deltree(List<Category> list)
        {

            foreach (var item in list)
            {
                var result1 = _context.Categorys.Where(x => x.PID == item.Id);
                if (result1 != null && result1.Count() > 0)
                    deltree(result1.ToList());
                _context.Categorys.Remove(item);
            }

        }
        #endregion

        [HttpGet]
        public JsonResult GetAllCategory()
        {
            List<Category> list = _context.Categorys.Include(x => x.Posts).ToList();
            List<CategoryToTree> categories = LoadData(list);
            int postCount = _context.Posts.Count();
            categories.Insert(0, new CategoryToTree { id = -1, label = "全部显示", parentID = -1, value = "-1", expand = true, postCount = postCount });
            // List<int> categoryStates = LoadCategoryStates(categories);
            return Json(new ReturnJsonInfo(0, "ok",
                new
                {
                    categories = categories,
                    currentCategory = currentCategory,
                    currentPage = currentPage,
                    posts = postList()
                }));
        }

        #region GetAllCategory关联方法
        private List<CategoryToTree> LoadData(List<Category> data)
        {
            var nodes = data.Where(x => x.PID == 0).OrderBy(x => x.CSort).Select(x => new CategoryToTree
            {
                id = x.Id,
                value = x.Id.ToString(),
                parentID = x.PID,
                label = x.Name,
                expand = x.CStatus,
                SortType = x.SortType,
                SortDirection = x.SortDirection,
                IsOnlyUserSee = x.IsOnlyUserSee,
                postCount = x.Posts == null ? 0 : x.Posts.Count(),
            }).ToList();
            foreach (var item in nodes)
            {
                item.children = LoadChildrens(item, data);
            }
            return nodes;
        }

        private List<CategoryToTree> LoadChildrens(CategoryToTree item, List<Category> data)
        {
            var children = data.Where(x => x.PID == item.id).OrderBy(x => x.CSort).Select(x => new CategoryToTree
            {
                id = x.Id,
                parentID = x.PID,
                label = x.Name,
                expand = x.CStatus,
                SortType = x.SortType,
                SortDirection = x.SortDirection,
                IsOnlyUserSee = x.IsOnlyUserSee,
                postCount = x.Posts == null ? 0 : x.Posts.Count(),
            }).ToList();
            foreach (var child in children)
            {
                child.children = LoadChildrens(child, data);
            }
            return children;
        }

        private object postList()
        {
            List<Post> postList = new List<Post>();
            int total = 0;
            if (currentCategory == null || currentCategory <= 0)
            {
                if (currentPage <= 0)
                {
                    postList = _context.Posts.OrderByDescending(x => x.UpdataDate).Take(pageSize).ToList();

                }
                else
                {
                    postList = _context.Posts.OrderByDescending(x => x.UpdataDate).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                }
                total = _context.Posts.Count();

            }
            else
            {

                if (currentPage <= 0)
                {
                    postList = _context.Posts.Where(x => x.CategoryId == currentCategory).OrderByDescending(x => x.UpdataDate).Take(pageSize).ToList();

                }
                else
                {
                    try
                    {
                        postList = _context.Posts.Where(x => x.CategoryId == currentCategory).OrderByDescending(x => x.UpdataDate).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
                total = _context.Posts.Where(x => x.CategoryId == currentCategory).Count();
            }
            var posts = postList.Select(x => new
            {
                Id = x.Id,
                CategoryId = x.CategoryId,
                Category = x.Category!.Name,
                Content = x.Content,
                Title = x.Title,
                hits = x.hits,
            }).ToList();
            return new { postList = posts, total = total, pageSize = pageSize };
        }
        #endregion

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="id">父节点id</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> Append([FromBody] AppendParm parm)
        {
            if (ModelState.IsValid)
            {
                int PId = parm.PId;
                int i = step;
                if (PId >= 0)
                {
                    Category category = new Category() { PID = PId, Name = "新建项" };

                    //如果是更新，根据id找到数据
                    if (parm.UPdataId > 0 && CategoryExists(parm.UPdataId) == true)
                        category = _context.Categorys.Where(x => x.Id == parm.UPdataId).FirstOrDefault()!;

                    //如果父下面有多个数据，获取最大的排序值+步长
                    if (_context.Categorys.Where(x => x.PID == PId).Count() > 0)
                    {
                        i = _context.Categorys.Where(x => x.PID == PId).Max(x => x.CSort) + step;
                    }

                    //设置值
                    category.CSort = i;
                    category.PID = parm.PId;

                    if (parm.UPdataId <= 0) _context.Add(category);
                    var result = await _context.SaveChangesAsync();
                    if (result == 1)
                    {
                        CategoryToTree ct = new CategoryToTree
                        {
                            id = category.Id,
                            label = category.Name,
                            expand = category.CStatus,
                            parentID = category.PID,
                            children = new List<CategoryToTree>(),
                        };
                        return Json(new ReturnJsonInfo(0, "", new { category = ct }));
                    }
                }
            }
            return Json(new ReturnJsonInfo(1, "添加失败", new object()));
        }

        //插入之后
        [Authorize]
        [HttpPost]

        public async Task<JsonResult> Insert([FromBody] InsertParm parm)
        {
            if (ModelState.IsValid)
            {
                if (parm.nextId == -1)
                {
                    //添加到尾部，追加


                    Category category1 = _context.Categorys.Where(x => x.Id == parm.preId).FirstOrDefault()!;
                    if (category1 != null)
                    {
                        return await Append(new AppendParm() { PId = category1.PID });
                    }
                    else
                    {
                        Json(new ReturnJsonInfo(2, "不存在的记录", ""));
                    }
                }
                else
                {
                    //
                    Category category1 = _context.Categorys.Where(x => x.Id == parm.nextId).FirstOrDefault()!;
                    if (category1 != null)
                    {
                        //新建项
                        Category category = new Category() { Name = "新建项" };
                        //如果 插入ID>0 就找到这个id 更新它
                        if (parm.InsertId > 0 && CategoryExists(parm.InsertId) == true)
                            category = _context.Categorys.Where(x => x.Id == parm.InsertId).FirstOrDefault()!;

                        if (category != null)
                        {
                            //获取排序号
                            int newSortNum = await getSortNum(parm.preId, parm.nextId);
                            //获取排序号大于0正常，保存新建项
                            if (newSortNum > 0)
                            {
                                category.CSort = newSortNum;
                                category.PID = category1.PID;

                                //如果插入id为空，则是新建，要加入到分类中，否则就是理新，只接保存即可
                                if (parm.InsertId <= 0 || CategoryExists(parm.InsertId) == false) _context.Add(category);
                                var result = await _context.SaveChangesAsync();
                                if (result == 1)
                                {
                                    //保存成功，将分类转成树返回给前端
                                    CategoryToTree ct = new CategoryToTree
                                    {
                                        id = category.Id,
                                        label = category.Name,
                                        expand = category.CStatus,
                                        parentID = category.PID,
                                        children = new List<CategoryToTree>(),
                                    };
                                    return Json(new ReturnJsonInfo(0, "", new { category = ct }));
                                }
                            }
                        }
                    }
                }
            }
            return Json(new ReturnJsonInfo(1, "添加失败", ""));
        }

        #region Insert,append相关
        /// <summary>
        /// 根据相邻的两个记录的排序数，求出中间值
        /// </summary>
        /// <param name="preId">前一记录的ID, -1到头</param>
        /// <param name="nextId">后一记录的ID，-1到尾 </param>
        /// <returns>可用的排序数 -1无可用排序</returns>
        private async Task<int> getSortNum(int preId, int nextId)
        {

            if (nextId == -1)
            {
                //后面没有记录，即当前行是最后一行，排序号直接加
                Category category1 = _context.Categorys.Where(x => x.Id == preId).FirstOrDefault()!;
                if (category1 != null)
                {
                    return category1.CSort + step;
                }
            }
            else if (preId == -1)
            {
                //前面没有记录，即后行是第一行，排序号=后行排序号/2
                Category category1 = _context.Categorys.Where(x => x.Id == nextId).FirstOrDefault()!;

                if (category1 != null)
                {
                    int newsort = category1.CSort / 2;
                    //新排序号小于10，将本记录所在树的记录重新设置排序号
                    if (newsort < 10)
                    {
                        int resultNum = await restSortAsync(category1.PID);
                        if (resultNum > 0)
                        {
                            newsort = category1.CSort / 2;
                        }
                    }

                    return newsort;
                }
                else
                {
                    return -1;
                }
            }
            else if (preId >= 0 && nextId >= 0)
            {
                Category preCategory = _context.Categorys.Where(x => x.Id == preId).FirstOrDefault()!;
                Category nextcategory = _context.Categorys.Where(x => x.Id == nextId).FirstOrDefault()!;
                if (preCategory != null && nextcategory != null)
                {
                    //求中间值 (后项排序值-前项排序值)/2
                    int sortVal = (nextcategory.CSort - preCategory.CSort) / 2;
                    if (sortVal < 10)
                    {
                        int resultNum = await restSortAsync(preCategory.PID);
                        sortVal = (nextcategory.CSort - preCategory.CSort) / 2;
                    }
                    int newsort = Math.Abs(preCategory.CSort + sortVal);
                    return newsort;

                }
            }
            return -1;
        }

        private async Task<int> restSortAsync(int PId)
        {

            var i = _context.Set<Category>().
                    Where(b => b.PID == PId).OrderBy(x => x.CSort);
            //.ExecuteUpdateAsync(b => b.SetProperty(p => p.CSort, m => i1));
            u = 0;
            foreach (var item in i)
            {
                item.CSort = autoAddSortNum();
            }
            var result = await _context.SaveChangesAsync();

            return result;
        }

        int u = 0;
        private int autoAddSortNum()
        {
            u = u + step;
            return u;
        }
        #endregion

        [HttpPost]
        public async Task<IActionResult> IndexList([FromBody] IndexListParm parm)
        {
            //IndexListParm parmbak = new IndexListParm() { CategoryId = -1, FindStr = "", PageIndex = 1, PageSize = 3 };
            if (parm == null)
                return Json(new ReturnJsonInfo(1, "参数无效", new PartialViewResult()));
            if (postParm.CategoryId == null && parm.CategoryId == null)
                return Json(new ReturnJsonInfo(1, "参数无效", new PartialViewResult()));
            if (parm.CategoryId != null)
            {
                if (postParm.CategoryId != parm.CategoryId)
                {
                    postParm.CategoryId = parm.CategoryId;
                    //CategoryId改变，分页索引初始化
                    postParm.PageIndex = 1;
                    //postParm.FindStr = "";
                }
            }


            if (parm.FindStr != null)
            {
                //CategoryId改变，分页索引初始化
                if (postParm.FindStr != parm.FindStr)
                {
                    postParm.PageIndex = 1;
                    postParm.FindStr = parm.FindStr;
                }
            }
            else if (postParm.FindStr == null)
            {
                postParm.FindStr = "";
            }

            //两项为空=初值
            if (postParm.PageIndex == null && parm.PageIndex == null) postParm.PageIndex = 1;
            if (parm.PageIndex != null)
            {
                postParm.PageIndex = parm.PageIndex;
            }

            //排序类型验证数据
            if (postParm.SortType == null && parm.SortType == null) postParm.SortType = 0;
            if (parm.SortType != null)
            {
                postParm.SortType = parm.SortType;
            }

            //排序方向验证数据
            if (postParm.SortDirection == null && parm.SortDirection == null) postParm.SortDirection = 0;
            if (parm.SortDirection != null)
            {
                postParm.SortDirection = parm.SortDirection;
            }
            //两项为空=初值

            ViewData["FindStr"] = postParm.FindStr;
            ViewData["CategoryId"] = postParm.CategoryId;
            ViewData["PageIndex"] = postParm.PageIndex;
            ViewData["SortType"] = postParm.SortType;
            ViewData["SortDirection"] = postParm.SortDirection;

            PartialViewResult posts = new PartialViewResult();
            if (postParm.SortType <= 0)
            {
                var aspnetMvcCVBContext=(postParm != null && CategoryExists(postParm.CategoryId!.Value)) ?
                        (postParm!.SortDirection<=0?
                           _context.Posts.Include(p => p.Category).OrderByDescending(x => x.UpdataDate).Where(x => x.CategoryId == postParm.CategoryId && x.Title!.Contains(postParm.FindStr!)):
                           _context.Posts.Include(p => p.Category).OrderBy(x => x.UpdataDate).Where(x => x.CategoryId == postParm.CategoryId && x.Title!.Contains(postParm.FindStr!))) :
                          (postParm!.SortDirection <= 0 ?
                           _context.Posts.Include(p => p.Category).OrderByDescending(x => x.UpdataDate).Where(x => x.Title!.Contains(postParm!.FindStr!)):
                           _context.Posts.Include(p => p.Category).OrderBy(x => x.UpdataDate).Where(x => x.Title!.Contains(postParm!.FindStr!)));
                PaginatedList<Post> paginatedList = await PaginatedList<Post>.CreateAsync(aspnetMvcCVBContext.AsNoTracking(), postParm!.PageIndex!.Value, pageSize);
                paginatedListParm.HasPreviousPage = paginatedList.HasPreviousPage;
                paginatedListParm.HasNextPage = paginatedList.HasNextPage;
                paginatedListParm.TotalPages = paginatedList.TotalPages;
                paginatedListParm.ShowIndexCount = paginatedList.ShowIndexCount;

                 posts = PartialView(paginatedList);
               
            }else if (postParm.SortType == 1)
            {
                //var aspnetMvcCVBContext = (postParm != null && CategoryExists(postParm.CategoryId!.Value)) ?
                //          _context.Posts.Include(p => p.Category).OrderByDescending(n => Regex.Replace(n.Title!, @"\d+", n => n.Value.PadLeft(5, '0'))).Where(x => x.CategoryId == postParm.CategoryId && x.Title!.Contains(postParm.FindStr!)).AsNoTracking().ToList() :
                //          _context.Posts.Include(p => p.Category).OrderByDescending(n => Regex.Replace(n.Title!, @"\d+", n => n.Value.PadLeft(5, '0'))).Where(x => x.Title!.Contains(postParm!.FindStr!)).AsNoTracking().ToList();
                List<Post> listpost = (postParm != null && CategoryExists(postParm.CategoryId!.Value)) ?
                                        _context.Posts.Include(p => p.Category).Where(x => x.CategoryId == postParm.CategoryId && x.Title!.Contains(postParm.FindStr!)).ToList() :
                                        _context.Posts.Include(p => p.Category).Where(x => x.Title!.Contains(postParm!.FindStr!)).ToList();
                if (postParm!.SortDirection <= 0) {
                    listpost.Sort((a, b) => StrCmpLogicalW(b.Title!, a.Title!));
                        }
                else {
                    listpost.Sort((a, b) => StrCmpLogicalW(a.Title!, b.Title!));
                }


                PaginatedList< Post> paginatedList =  PaginatedList<Post>.CreateAsync(listpost, postParm!.PageIndex!.Value, pageSize);
                paginatedListParm.HasPreviousPage = paginatedList.HasPreviousPage;
                paginatedListParm.HasNextPage = paginatedList.HasNextPage;
                paginatedListParm.TotalPages = paginatedList.TotalPages;
                paginatedListParm.ShowIndexCount = paginatedList.ShowIndexCount;

                 posts = PartialView(paginatedList);
                
            }
            else if (postParm.SortType == 2)
            {
                var aspnetMvcCVBContext = (postParm != null && CategoryExists(postParm.CategoryId!.Value)) ?
                         (postParm!.SortDirection <= 0 ?
                          _context.Posts.Include(p => p.Category).OrderByDescending(x => x.CSort).Where(x => x.CategoryId == postParm.CategoryId && x.Title!.Contains(postParm.FindStr!)) :
                           _context.Posts.Include(p => p.Category).OrderBy(x => x.CSort).Where(x => x.CategoryId == postParm.CategoryId && x.Title!.Contains(postParm.FindStr!)) ):
                         (postParm!.SortDirection <= 0 ?
                          _context.Posts.Include(p => p.Category).OrderByDescending(x => x.CSort).Where(x => x.Title!.Contains(postParm!.FindStr!)):
                          _context.Posts.Include(p => p.Category).OrderBy(x => x.CSort).Where(x => x.Title!.Contains(postParm!.FindStr!)));
                PaginatedList<Post> paginatedList = await PaginatedList<Post>.CreateAsync(aspnetMvcCVBContext.AsNoTracking(), postParm!.PageIndex!.Value, pageSize);
                paginatedListParm.HasPreviousPage = paginatedList.HasPreviousPage;
                paginatedListParm.HasNextPage = paginatedList.HasNextPage;
                paginatedListParm.TotalPages = paginatedList.TotalPages;
                paginatedListParm.ShowIndexCount = paginatedList.ShowIndexCount;

                 posts = PartialView(paginatedList);
               
            }
            return posts;
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string psz1, string psz2);
       

        [Authorize]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdataSortType([FromBody] UpdataParm parm)
        {
            if (ModelState.IsValid)
            {


                //如果是更新，根据id找到数据                   
                var category = _context.Categorys.Where(x => x.Id == parm.Id).FirstOrDefault()!;
                if (category == null) return Json(new ReturnJsonInfo(1, "不存在的分类ID", ""));


                //设置值
                if (category.SortType != parm.Value)
                {
                    category.SortType = parm.Value;
                    var result = await _context.SaveChangesAsync();
                    if (result == 1)
                    {
                        return Json(new ReturnJsonInfo(0, "", ""));
                    }
                }
                else
                {
                    return Json(new ReturnJsonInfo(0, "", ""));
                }
            }

            return Json(new ReturnJsonInfo(1, "添加失败", ""));
        }

        [Authorize]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdataSortDirection([FromBody] UpdataParm parm)
        {
            if (ModelState.IsValid)
            {
                //如果是更新，根据id找到数据                   
                var category = _context.Categorys.Where(x => x.Id == parm.Id).FirstOrDefault()!;
                if (category == null) return Json(new ReturnJsonInfo(1, "不存在的分类ID", ""));


                //设置值
                if (category.SortDirection != parm.Value)
                {
                    category.SortDirection = parm.Value;
                    var result = await _context.SaveChangesAsync();
                    if (result == 1)
                    {
                        return Json(new ReturnJsonInfo(0, "", ""));
                    }
                }
                else
                {
                    return Json(new ReturnJsonInfo(0, "", ""));
                }
            }

            return Json(new ReturnJsonInfo(1, "添加失败", ""));
        }

        [Authorize]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> GetCpuId()
        {
            //var str = Get_CPUID();
           
           var i=  JsonConfigHelper.getSetting<ConfigModel>("upload/setting.json");
            i.MaxLevel = 100;
            i.MinLevel = 100;
            
            bool result= JsonConfigHelper.SaveSetting("upload/setting.json", i);
            return Json(new ReturnJsonInfo(0,"", new {cpuid= result.ToString() }));
        }
       

    }
}

public class PaginatedListParm
{
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    public int TotalPages { get; set; }
    public int ShowIndexCount { get; set; }
}

public class InsertParm
{
    public int preId { get; set; }
    public int nextId { get; set; }
    public int InsertId { get; set; }
}
public class AppendParm
{
    public int PId { get; set; }
    public int UPdataId { get; set; } = -1;

}
public class IndexListParm
{
    public string? FindStr { get; set; }
    public int? CategoryId { get; set; }
    public int? PageIndex { get; set; }
    public int? SortType { get; set; }
    public int? SortDirection { get; set; }
}
public class UpdataParm
{
    public int Id { get; set; }
    public int Value { get; set; }
}