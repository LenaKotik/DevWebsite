using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GooDDevWebSite.Models
{
    public class AdminViewModel
    {
        public List<User> Users { set; get; }
        public List<Material> Materials { set; get; }
        public List<MyTask> Tasks { set; get; }
    }
}
