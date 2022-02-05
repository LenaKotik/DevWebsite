using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GooDDevWebSite.Models
{
    public class DoubleEncoding
    {
        static string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZабвгдеёжзийклмнопрстуфхцчшщьыъэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЬЫЪЭЮЯ @!?.,/\\_=-+0123456789~#:;\'\"";
        public string Encode(string UTFval)
        {
            UTFval = (UTFval == null) ? "" : UTFval; // anti-null
            string encoded = "";
            for (int i = 0; i < UTFval.Length; i++)
            {
                char ch = UTFval[i];
                encoded += allowedChars.IndexOf(ch);
                if (i == UTFval.Length - 1) continue; //skip last one, dont add a point at the end
                encoded += "."; // adds "." only inbetween characters
            }
            return encoded; // string format: {indexOf1Char}.{indexOf2Char}.{indexOf3Char}......
        }
        public string Decode(string ENCval)
        {
            string decoded = "";
            foreach (string chnum in ENCval.Split(".")) // loop thru the array of indexies
            {
                try
                {
                    int num = Convert.ToInt32(chnum);
                    decoded += (num >= 0 && num < allowedChars.Length) ? allowedChars[num] : 'ඞ'; // unknown char is amongus
                }
                catch { return ENCval; } // if it caused an error, it wasn't encoded in the first place, we can mark it as decoded
            }
            return decoded;
        }
    }
}
