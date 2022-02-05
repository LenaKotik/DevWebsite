using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Common;
using System.ComponentModel.DataAnnotations;

namespace GooDDevWebSite.Models
{
    public class User
    {
#pragma warning disable CS8618 
        public string Name { set; get; }
        [DataType(DataType.EmailAddress)]
        public string Email { set; get; }

        [DataType(DataType.Password)]
        public string Password { set; get; }
        
        public Role Role { set; get; }
        public static Role GetRole(int? r) => r switch
        {
            1 => Role.Programmer,
            2 => Role.Manager,
            3 => Role.Writer,
            4 => Role.Artist,
            5 => Role.Designer,
            6 => Role.SFXer,
            7 => Role.Admin,
            _ => Role.None,
        };
#pragma warning restore CS8618
    }
    public enum Role { None, Programmer, Manager, Writer, Artist, Designer, SFXer, Admin}
}
