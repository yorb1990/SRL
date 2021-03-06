﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TokenizeParceLucene
{
    public class composer
    {
        public List<string> terms = new List<string>();
        private string[] Similar = new string[] {
            "a|á","e|é","i|í|y|1","o|ó|0","u|ú","q|k","s|z|c","v|b","m|n|ñ","j|y|g","h|[\\?]"
        };
		private string NormalSearch = @"([']|[0-9]|[a-z]|[A-Z]|[Á]|[Ó],|[Í]|[É]|[Ú]|[Ñ]|[á]|[ó]|[í]|[é]|[ú]|[ñ]|\s){2,50}";
        public composer(string origin)
        {
            tokenizar(origin);
        }
        private void tokenizar(string origins)
        {
			
            foreach (string origin in Regex.Split(origins.Trim(), @" |,"))
            {
				if (origin.StartsWith("'"))
				{
					terms.Add(origin);
				}
				else
				{
					//build terms
					Build(
						//normali<e
						origin
						.Replace("  ", " ")
						.Replace("*", " ")
						.Replace("?", " ")
						.ToLower()
						);
				}
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
				foreach (string rul in Similar)
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
