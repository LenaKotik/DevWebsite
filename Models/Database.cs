using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Common;
using System.Text.Json;

namespace GooDDevWebSite.Models
{
    public static class Database
    {
        private static readonly string connString = "Data Source=(localdb)\\MSSQLLocalDB;Integrated Security = True; Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False"; // DB connection string here  
        async public static void Execute(string query)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    connection.Open();
                    await cmd.ExecuteNonQueryAsync();
                    connection.Close();
                }
            }
        }
        async public static Task<List<T>> Read<T>(string query, Func<SqlDataReader, List<T>> parser)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    var parsed = parser(reader);
                    connection.Close();
                    return parsed;
                }
            }
        }
        async public static Task<bool> IsUserUnique(User u)
        {
            if (u.Email == "" || u.Name == "" || u.Password == "") return false; // invalid user instance
            List<User> users = await Read<User>("SELECT * FROM Users", Parsers.ParseUsers);
            foreach (User usr in users)
            {
                if (u.Email == usr.Email) return false; // email isn't unique
                if (u.Name == usr.Name) return false; // name isn't unique
                // it's ok not to have a unique password
            }
            return true;
        }
    }
    public static class Parsers
    {
        public static List<User> ParseUsers(SqlDataReader rdr)
        {
            var res = new List<User>();
            while (rdr.Read())
            {
                var decoder = new DoubleEncoding();
                User u = new User();
                u.Name = decoder.Decode((string)rdr["name"]);
                u.Email = (string)rdr["email"];
                u.Password = decoder.Decode((string)rdr["password"]);
                // 'use enums' they said, 'it will make your code look better!' they said
                u.Role = User.GetRole((int?)((rdr["ROLE"] != DBNull.Value) ? rdr["ROLE"] : null)); //it's uppercase because i'm dum and i can't change it in SQL
                res.Add(u);
            }
            return res;
        }
        public static List<Material> ParseMaterials(SqlDataReader rdr)
        {
            var res = new List<Material>();
            while (rdr.Read())
            {
                var encoder = new DoubleEncoding();
                Material mat = new Material();
                mat.Author = encoder.Decode((string)rdr["author"]);
                mat.Name = encoder.Decode(Path.GetFileNameWithoutExtension((string)rdr["name"])) + Path.GetExtension((string)rdr["name"]);
                mat.commentsRaw = "";
                foreach (string comment in ((string)rdr["comments"]).Split(';'))
                {
                    if (comment == "") break;
                    string[] parts = comment.Split(':');
                    mat.commentsRaw += $"{encoder.Decode(parts[0])}:{encoder.Decode(parts[1])};";
                }
                mat.Description = encoder.Decode((string)rdr["description"]);
                mat.FolderName = (string)rdr["folder"];
                string[] cat=((string)rdr["category"]).Split('-');
                mat.Category = cat[0] + "-" + encoder.Decode(cat[1]);           
                res.Add(mat);
            }
            return res;
        }
        public static List<MyTask> ParseTasks(SqlDataReader rdr)
        {
            var res = new List<MyTask>();
            while (rdr.Read())
            {
                MyTask t = new();
                DoubleEncoding encoder = new();
                t.Author = encoder.Decode((string)rdr["author"]);
                t.Name = encoder.Decode((string)rdr["name"]);
                t.Description = encoder.Decode((string)rdr["description"]);
                t.flagsRaw = "";
                foreach (string flag in ((string)rdr["flags"]).Split(';'))
                {
                    if (flag == "") break;
                    string[] parts = flag.Split(':');
                    t.flagsRaw += $"{encoder.Decode(parts[0])}:{encoder.Decode(parts[1])};";
                }
                t.commentsRaw = "";
                foreach (string comment in ((string)rdr["comments"]).Split(';'))
                {
                    if (comment == "") break;
                    string[] parts = comment.Split(':');
                    t.commentsRaw += $"{encoder.Decode(parts[0])}:{encoder.Decode(parts[1])};";
                }
                string materials = (string)rdr["links"];
                /*
                t.MaterialLinks = materials.Split(':').Select(   // WTF?
                    x => (x != "") ? encoder.Decode(x) : x
                    ).ToList(); 
                */
                t.MaterialLinks = new List<string>();
                foreach (string lnk in materials.Split(':'))
                {
                    if (lnk == null) throw new NullReferenceException("BBBBBBB");
                    t.MaterialLinks.Add(encoder.Decode(lnk));
                }
                t.FoulderName = (string)rdr["fldr"];
                res.Add(t);
            }
            return res;
        }
    }
}   
