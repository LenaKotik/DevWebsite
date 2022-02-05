using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace GooDDevWebSite.Models
{
    public class Material
    {
#pragma warning disable CS8618
        public string Author { set; get; }
        public string Name { set; get; }
        public string Description { set; get; }
        public string FolderName { set; get; }
        public List<string> Images{ set; get; }
        public List<KeyValuePair<string, string>> Comments // not a dictinary, cuz it can't contain 2 equal keys
        {
            get
            {
                List<KeyValuePair<string, string>> res = new List<KeyValuePair<string, string>>();
                foreach (string comment in commentsRaw.Split(';'))
                     if (comment != "") // excluding the last split
                        res.Add(new KeyValuePair<string, string>(comment.Split(':')[0], comment.Split(':')[1]));
                return res;
            }
        }
        public string commentsRaw; // format of the comments: "{name1}:{comment1};{name2}:{comment2};...;"
        public string Category { set; get; } // {category}-{subcategory}
    }
}
