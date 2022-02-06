using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GooDDevWebSite.Models;

namespace GooDDevWebSite.Controllers
{
    public class TasksController : Controller
    {
        IWebHostEnvironment environment;
        public TasksController (IWebHostEnvironment env) => this.environment = env;

        async public Task<IActionResult> Index()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            List<MyTask> model = await Database.Read<MyTask>("SELECT * FROM Tasks", Parsers.ParseTasks);
            return View("Tasks",model);
        }
        async public Task<IActionResult> At(string name)
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            DoubleEncoding encoder = new();
            ViewData["user"] = (await Database.Read<User>($"SELECT * FROM Users WHERE name='{encoder.Encode(HttpContext.Request.Cookies["username"])}';", Parsers.ParseUsers)).Single();
            name = encoder.Encode(name);
            var model = (await Database.Read<MyTask>($"SELECT * FROM Tasks WHERE name='{name}'", Parsers.ParseTasks)).Single();
            model.Images = Directory.GetFiles(environment.WebRootPath + '/' + model.FoulderName)
                .Where(x => x.Contains("img")).Select(x => x.Replace(environment.WebRootPath, "")).ToList(); // looks scary
            return View("Task",model);
        }
        async public Task<IActionResult> Create()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            List<Material> model = await Database.Read<Material>("SELECT * FROM Materials", Parsers.ParseMaterials);
            return View("Editor", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Flag(string name, string task, string flag)
        {
            DoubleEncoding encoder = new DoubleEncoding();
            string enctask = encoder.Encode(task);
            name = encoder.Encode(name);
            flag = encoder.Encode(flag);
            Database.Execute($"UPDATE Tasks SET flags=flags + '{name}:{flag};' WHERE name='{enctask}';");
            return Redirect($"/Tasks/At?name={task}");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upload(Role role, string name, string description, IFormFileCollection? images, string author, string materialLinks)
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            FileUploadManager f = new FileUploadManager(environment.WebRootPath);
            f.UploadTask(role, name, description, images, author, materialLinks);
            return Redirect("/");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Comment(string text, string author, string task)
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            var encoder = new DoubleEncoding();
            author = encoder.Encode(author);
            text = encoder.Encode(text);
            string etask = encoder.Encode(Path.GetFileNameWithoutExtension(task)) + Path.GetExtension(task);
            string str = $"{author}:{text};";
            Database.Execute($"UPDATE Tasks SET comments=comments + '{str}' where name='{etask}' ");
            return Redirect($"/Tasks/At?name={task}");
        }
    }
}
