using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using MimeMapping;
using GooDDevWebSite.Models;

namespace GooDDevWebSite.Views.Materials
{
    public class MaterialsController : Controller
    {
        private IWebHostEnvironment environment;
        public MaterialsController(IWebHostEnvironment env)
        {
            this.environment = env;
        }
        public IActionResult Index()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            return View();
        }
        async public Task<IActionResult> At(string name)
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            DoubleEncoding encoder = new DoubleEncoding();
            string ext = Path.GetExtension(name);
            name = encoder.Encode(Path.GetFileNameWithoutExtension(name)) + ext;
            Material item = (await Database.Read<Material>($"SELECT * FROM Materials Where name='{name}';", Parsers.ParseMaterials)).Single();
            item.Images = Directory.GetFiles(environment.WebRootPath+'/'+item.FolderName)
                .Where(x => x.Contains("img")).Select(x=>x.Replace(environment.WebRootPath,"")).ToList(); // looks scary
            return View("Material", item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Comment(string text, string author, string material)
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            var encoder = new DoubleEncoding();
            author = encoder.Encode(author);
            text = encoder.Encode(text);
            string ematerial = encoder.Encode(Path.GetFileNameWithoutExtension(material)) + Path.GetExtension(material);
            string str = $"{author}:{text};";
            Database.Execute($"UPDATE Materials SET comments=comments + '{str}' where name='{ematerial}' ");
            return Redirect($"/Materials/At?name={material}");
        }
        public IActionResult Download(string f, string ext) 
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            FileStream stream = new FileStream(this.environment.WebRootPath+'/'+ f + "/file"+ext,FileMode.Open, FileAccess.Read);
            string mime = MimeUtility.GetMimeMapping("file" + ext);
            return File(stream, mime, "file"+ext);
        }
        async public Task<IActionResult> Images()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            // this query selects all the materials whichs category starts with 'image-'
            var model = await Database.Read<Material>("SELECT * FROM Materials WHERE category LIKE 'image-%'", Parsers.ParseMaterials);
            ViewData["Title"] = "Изображения";
            return View("Display", model);
        }
        async public Task<IActionResult> Models()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            // this query selects all the materials whichs category starts with 'model-'
            var model = await Database.Read<Material>("SELECT * FROM Materials WHERE category LIKE 'model-%'", Parsers.ParseMaterials);
            ViewData["Title"] = "Модельки";
            return View("Display", model);
        }
        async public Task<IActionResult> Texts()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            // this query selects all the materials whichs category starts with 'text-'
            var model = await Database.Read<Material>("SELECT * FROM Materials WHERE category LIKE 'text-%'", Parsers.ParseMaterials);
            ViewData["Title"] = "Текстовая информация";
            return View("Display", model);
        }
        async public Task<IActionResult> Sounds()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            // this query selects all the materials whichs category starts with 'sound-'
            var model = await Database.Read<Material>("SELECT * FROM Materials WHERE category LIKE 'sound-%'", Parsers.ParseMaterials);
            ViewData["Title"] = "Звуки";
            return View("Display", model);
        }
        async public Task<IActionResult> Animations()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            // this query selects all the materials whichs category starts with 'animation-'
            var model = await Database.Read<Material>("SELECT * FROM Materials WHERE category LIKE 'animation-%'", Parsers.ParseMaterials);
            ViewData["Title"] = "Анимации";
            return View("Display", model);
        }
        public IActionResult Create(string Category)
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            ViewData["CategoryPre"] = Category;
            return View("Editor", null);
        }
        async public Task<IActionResult> Edit(string name)
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            var data = await Database.Read<Material>($"SELECT * FROM Materials WHERE name = {name}", Parsers.ParseMaterials);
            return View("Editor", data.Single());
        }
        public IActionResult Err(string err)
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            return View("Err", err);
        }
#pragma warning disable CS8604
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit =long.MaxValue)] // hope thats enough lol
        public IActionResult Upload(IFormFile? file, IFormFileCollection? images, string author, string? name, string? description, string? cat, string? subcat)
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            if (file == null || name == null || description == null || cat == null || subcat == null)
            {
                string err = "Вы не ввели необходимые данные";
                err += (file == null) ? " file " : "";
                err += (name == null) ? " name " : "";
                err += (description == null) ? " description " : "";
                err += (cat == null) ? " category " : "";
                err += (subcat == null) ? " subcategory" : "";
                return Redirect("/Materials/Err?err="+err);
            }
            FileUploadManager f = new FileUploadManager(environment.WebRootPath + '/');
            f.Upload(file,images, name, description, cat, subcat, author);
            return Redirect("/Materials");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(IFormFile? file, IFormFileCollection? images, string? name, string? description, string? cat, string? subcat)
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            FileUploadManager f = new FileUploadManager(environment.WebRootPath + '/');
            f.Update(file, images, name, description, cat, subcat);
            return Redirect(Url.Action("Index", "Material"));
        }
    }
}
