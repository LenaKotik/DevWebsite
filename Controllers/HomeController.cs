using GooDDevWebSite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

/*
 TODO:
 * add task classification
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
                ViewData["User"] = users.Single();
                List<MyTask> model = await Database.Read<MyTask>("SELECT * FROM Tasks", Parsers.ParseTasks);
                model = (model.Where(x => x.Role == users.Single().Role).Where(x => x.Flags.Any(x => x.Value == "Срочно")).Concat(model.Where(x => !x.Flags.Any(x => x.Value == "Срочно")))).Take(10).ToList(); // scary LINQ thing that takes 10 tasks, ordered by "Срочно" flag
                return View(model);
            }
            ViewData["User"] = null;
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
        async public Task<IActionResult> GetBackup()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("username")) return Redirect("/Home/SignIn");
            List<User> u = await Database.Read<User>("SELECT * FROM Users", Parsers.ParseUsersEncoded);
            DoubleEncoding decoder = new();
            User? admin = u.SingleOrDefault(x => decoder.Decode(x.Name) == HttpContext.Request.Cookies["username"]);
            if (admin == null) return NotFound();
            List<Material> m = await Database.Read<Material>("SELECT * FROM Materials", Parsers.ParseMaterialsEncoded);
            List<MyTask> t = await Database.Read<MyTask>("SELECT * FROM Tasks", Parsers.ParseTasksEncoded);
            AdminViewModel model = new AdminViewModel()
            { Users = u, Materials = m, Tasks = t };
            string json = JsonSerializer.Serialize(model);
            byte[] buffer = ASCIIEncoding.ASCII.GetBytes(json);
            return File(buffer, "application/json", "database.json");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    List<MyTask> t = await Database.Read<MyTask>("SELECT * FROM Tasks", Parsers.ParseTasks);
                    AdminViewModel model = new AdminViewModel()
                    {Users = u,Materials = m,Tasks = t};
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
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
