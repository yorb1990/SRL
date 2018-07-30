using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TokenizeParcerucene
{
    public class composer
    {
        public List<string> terms = new List<string>();
        private string[] symbols = new string[] {
            "a|á","e|é","i|í","o|ó","u|ú","q|k","s|z|c","v|b","m|n|ñ","j|y|g","h|[\\?]"
        };
        public composer(string origin)
        {
            tokenizar(origin);
        }
        private void tokenizar(string origins)
        {
            foreach (string origin in Regex.Split(origins, @" |,"))
            {

                //build terms
                Build(
                    //normali<e
                    origin.Trim().Replace("  ", " ").ToLower()
                    );
                //DoubleN();
            }
        }
        private void Build(string origin)
        {
            List<string> neworigin = new List<string>();
            neworigin.Add(string.Empty);
            for (int i = 0; i < origin.Length; i++)
            {
                Boolean pass = true;
                string word = origin.Substring(i, 1);
                foreach (string rul in symbols)
                {
                    Regex rex = new Regex(rul);
                    if (rex.IsMatch(word))
                    {
                        string[] tokens = rul.Split('|');
                        List<string> NUEVOS = new List<string>();
                        foreach (string token in tokens)
                        {
                            foreach (string item in neworigin)
                            {
                                if (token == "[\\?]" || token == "[\\*]")
                                {
                                    if (i != 0)
                                    {
                                        ChechAddList(ref NUEVOS, item + token.Replace("[\\", "").Replace("]", ""));
                                    }
                                    else
                                    {
                                        ChechAddList(ref NUEVOS, item + word);
                                    }
                                }
                                else
                                {
                                    ChechAddList(ref NUEVOS, item + token);
                                }
                            }
                        }
                        neworigin = NUEVOS;
                        pass = false;
                    }
                }
                if (i == origin.Length - 1)
                {
                    for (int k = 0; k < neworigin.Count; k++)
                    {
                        if (neworigin[k].EndsWith("s") || neworigin[k].EndsWith("z"))
                        {
                            neworigin[k] = neworigin[k].Remove(neworigin[k].Length - 1);
                            neworigin[k] += "*";
                        }
                    }
                }
                for (int k = 0; pass && k < neworigin.Count; k++)
                {
                    neworigin[k] += word;
                }
            }
            terms.AddRange(neworigin);
        }
        private void ChechAddList(ref List<string> doc, string value)
        {
            if (!doc.Contains(value))
            {
                doc.Add(value);
            }
        }
    }
}
