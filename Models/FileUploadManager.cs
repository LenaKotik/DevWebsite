using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace GooDDevWebSite.Models
{
    public class FileUploadManager
    {
        public string RootPath { set; get; }
        public FileUploadManager(string rootPath) => this.RootPath = rootPath;

        async public void Upload(IFormFile file, IFormFileCollection? images, string name, string description, string category, string subcategory,string author)
        {
            string fldr = Hash();
            string extension = Path.GetExtension(file.FileName);
            var encoder = new DoubleEncoding();
            author = encoder.Encode(author);
            name = encoder.Encode(name);
            description = encoder.Encode(description);
            category = category.Replace('\'', ' '); // sneaky SQL injections
            subcategory = encoder.Encode(subcategory); 
            Database.Execute($"INSERT INTO Materials VALUES ('{name + extension}', '{description}', '{fldr}', '{((images != null) ? images.Count : 0)}', '', '{category}-{subcategory}', '{author}')");
            Directory.CreateDirectory(Path.Combine(this.RootPath, "/files/"+fldr));
            using (FileStream newfile = new FileStream(this.RootPath + fldr + "/files/file" + extension, FileMode.CreateNew, FileAccess.Write))
            {
                Stream stream = file.OpenReadStream();
                byte[] data = new byte[stream.Length];
                await stream.ReadAsync(data, 0, (int)stream.Length);
                newfile.Write(data, 0, data.Length);
                newfile.Flush();
            }
            if (images != null)
                for (int x = 0; x < images.Count; x++)
                {
                    string dir = $"{this.RootPath}/images/{fldr}";
                    Directory.CreateDirectory(dir);
                    this.SaveImage(images[x], dir+"/img{x}" + Path.GetExtension(images[x].FileName));
                }
        }
        async public void Update(IFormFile? file, IFormFileCollection? images, string name, string description, string category, string subcategory)
        {
            var encoder = new DoubleEncoding();
            name = encoder.Encode(name);
            description = encoder.Encode(description);
            subcategory = encoder.Encode(subcategory);
            Material material = (await Database.Read<Material>($"SELECT * FROM Materials WHERE name={name};", Parsers.ParseMaterials))[0];
            if (file != null)
            {
                Stream stream = file.OpenReadStream();
                byte[] data = new byte[file.Length];
                await stream.ReadAsync(data, 0, data.Length);
                FileStream fileStr = new FileStream($"{this.RootPath}/files/{material.FolderName}/file" + Path.GetExtension(material.Name), FileMode.OpenOrCreate, FileAccess.Write);
                fileStr.Write(data, 0, data.Length);
                fileStr.Flush();
                fileStr.Close();
            }
            if (images != null)
            {
                for (int x = 0; x < images.Count; x++)
                {
                    this.SaveImage(images[x], $"{this.RootPath}/images/{material.FolderName}/img{x}"+Path.GetExtension(images[x].FileName));
                }
            }
        }
        public void UploadTask(Role role, string name, string description, IFormFileCollection? images, string author, string materialLinks)
        {
            int r = (int)role;
            DoubleEncoding encoder = new();
            name = encoder.Encode(name);
            description = encoder.Encode(description);
            author = encoder.Encode(author);
            string lnks = "";
            foreach (string link in materialLinks.Split(':'))
            {
                lnks += (encoder.Encode(link)) + ":";
            }
            lnks = lnks.Remove(lnks.Length - 1);
            string fldr = this.Hash();
            Database.Execute($"INSERT INTO Tasks VALUES ('{author}', '{name}', '{description}', '', '', 0{/*must be fixed when we change the DB*/""}, '{lnks}', '{fldr}', {r}, '')");
            if (images != null)
            {
                var dir = this.RootPath + "/images/" + fldr;
                Directory.CreateDirectory(dir);
                for (int i = 0;i < images.Count;i++)
                    this.SaveImage(images[i], $"{dir}/img{i}");
            }
        }
        async public void SaveImage(IFormFile image, string path)
        {
            Stream stream = image.OpenReadStream();
            byte[] data = new byte[image.Length];
            await stream.ReadAsync(data, 0, data.Length);
            using (FileStream pic = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                pic.Write(data, 0, data.Length);
                pic.Flush();
            }
        }
        string Hash()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVXYZ_abcdefghijklmnopqrstuvxyz-012345678901234567890123456789";
            Random r = new Random((int)DateTime.Now.Ticks);
            string res = "";
            for (int i = 0; i < 20; i++) // i think 20 is safe enough
            {
                res += chars[r.Next(chars.Length - 1)];
            }
            return res;
        }
    }
}
