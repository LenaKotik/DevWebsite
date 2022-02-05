using GooDDevWebSite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

/*
 TODO:
 * add a task editor
 * * task saving
 * add a task view
 * * flags interaction
 * add task classification
 * refactoring
 * * rewrite file upload manager
 * polishing
 * * html
 * * css
 * * js/jquery
 */
namespace GooDDevWebSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment environment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            this._logger = logger;
            this.environment = env;
        }

        async public Task<IActionResult> Index()//string? email)
        {
            if (HttpContext.Request.Cookies.ContainsKey("email"))
            {
                List<User> users = (await Database.Read<User>($"SELECT * FROM Users WHERE email = '{HttpContext.Request.Cookies["email"]}'", Parsers.ParseUsers));
                return View(users.Single());
            }
            return View();
        }
        async public Task<IActionResult> AdminLogIn()
        {
            if (HttpContext.Request.Cookies.ContainsKey("email"))
            {
                List<User> users = (await Database.Read<User>($"SELECT * FROM Users WHERE email = '{HttpContext.Request.Cookies["email"].Replace('\'', ' ')}'", Parsers.ParseUsers));
                if (users.Single().Role == Role.Admin)
                    return View("Admin");
            }
            return NotFound();
        }
        async public Task<IActionResult> NewUser(User u, string author)
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            var encoder = new DoubleEncoding();
            u.Name = encoder.Encode(u.Name);
            u.Password = encoder.Encode(u.Password);
            u.Email = u.Email.Replace('\'', ' ');
            if (await Database.IsUserUnique(u))
                Database.Execute($"INSERT INTO Users VALUES ('{u.Email}', '{u.Password}', '{(int)u.Role}', '{u.Name}');");
            else
                ViewData["UserFormErr"] = "Введены некоректные данные\n(попробуйте убедиться в уникальности имени и почты)";
            ViewData["username"] = author;
            AdminViewModel model = new();
            model.Users = await Database.Read<User>("SELECT * FROM Users", Parsers.ParseUsers);
            model.Materials = await Database.Read<Material>("SELECT * FROM Materials", Parsers.ParseMaterials);
            return View("Admin", model);
        }
        async public Task<IActionResult> Admin(string username,string pwd)
        {
            if (HttpContext.Request.Cookies.ContainsKey("email") && username != "" && pwd != "")
            {
                var encoder = new DoubleEncoding();
                username = encoder.Encode(username);
                pwd = encoder.Encode(pwd);
                List<User> users = (await Database.Read<User>($"SELECT * FROM Users WHERE email = '{HttpContext.Request.Cookies["email"].Replace('\'', ' ')}' and password = '{pwd}' and name = '{username}'", Parsers.ParseUsers));
                if (users.Count == 1)
                {
                    List<User> u = await Database.Read<User>("SELECT * FROM Users", Parsers.ParseUsers);
                    List<Material> m = await Database.Read<Material>("SELECT * FROM Materials", Parsers.ParseMaterials);
                    AdminViewModel model = new AdminViewModel()
                    {Users = u,Materials=m};
                    ViewData["username"] = encoder.Decode(username);
                    return View(model);
                }
            }
            return NotFound();
        }
        public IActionResult SignIn()
        {
            return View("SignIn");
        }
        async public Task<IActionResult> Send(string email, string pwd)
        {
            //sneaky SQL injections
            pwd = pwd.Replace('\'', ' ');
            email = email.Replace('\'', ' ');
            pwd = new DoubleEncoding().Encode(pwd);
            List<User> data = (List<User>)(await Database.Read<User>($"SELECT * FROM Users WHERE email = '{email}' AND password = '{pwd}'", Parsers.ParseUsers));
            if (data.Count() == 1) // checks if there is such user in the database
            {
                var decoder = new DoubleEncoding();
                HttpContext.Response.Cookies.Append("username", decoder.Decode(data.Single().Name));
                HttpContext.Response.Cookies.Append("email", email);
                return Redirect(Url.Action("Index", "Home", data.Single()));
            }
            else return View("SignIn", "This user doesn't exist");
        }
        new public IActionResult SignOut()
        {
            HttpContext.Response.Cookies.Delete("email");
            HttpContext.Response.Cookies.Delete("username");
            return Redirect(Url.Action("Index", "Home"));
        }
        async public Task<IActionResult> CreateTask()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            List<Material> model = await Database.Read<Material>("SELECT * FROM Materials", Parsers.ParseMaterials);
            return View("Editor", model);
        }
        public IActionResult UploadTask(Role role, string name, string description, IFormFileCollection? images, string author, string materialLinks)
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            FileUploadManager f = new FileUploadManager(environment.WebRootPath);
            f.UploadTask(role, name, description, images, author, materialLinks);
            return Redirect("/");
        }
        async public Task<IActionResult> Tasks()
        {
            List<MyTask> model = await Database.Read<MyTask>("SELECT * FROM Tasks", Parsers.ParseTasks);
            return View(model);
        }
        async public Task<IActionResult> Task(string name)
        {
            DoubleEncoding encoder = new();
            name = encoder.Encode(name);
            MyTask model = (await Database.Read<MyTask>($"SELECT * FROM Tasks WHERE name='{name}'", Parsers.ParseTasks)).Single();
            model.Images = Directory.GetFiles(environment.WebRootPath + '/' + model.FoulderName)
                .Where(x => x.Contains("img")).Select(x => x.Replace(environment.WebRootPath, "")).ToList(); // looks scary
            return View(model);
        }
        public IActionResult Comment(string text, string author, string task)
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            var encoder = new DoubleEncoding();
            author = encoder.Encode(author);
            text = encoder.Encode(text);
            string etask = encoder.Encode(Path.GetFileNameWithoutExtension(task)) + Path.GetExtension(task);
            string str = $"{author}:{text};";
            Database.Execute($"UPDATE Tasks SET comments=comments + '{str}' where name='{etask}' ");
            return Redirect($"/Home/Task?name={task}");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
