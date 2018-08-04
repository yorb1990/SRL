using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TokenizeParcerucene
{
	public struct VarTransform
    {
		private static List<KeyValuePair<string, string>> _vars;
		public static KeyValuePair<string,string>[] conversor(string value, char separator, char MultifieldSeparator = ',')
		{
			if (_vars == null)
			{
				_vars = new List<KeyValuePair<string, string>>();
				value = value.Trim();
				Regex NameRegex = new Regex(@"(["+MultifieldSeparator+"]?){1}([A-Z]|[a-z]|[0-9]){5,25}["+separator+"]{1}" );
				string[] values = NameRegex.Split(value);
				MatchCollection names = NameRegex.Matches(value);
				if (names.Count > 0)
				{
					for (int i = 0; i < names.Count; i++)
					{
						_vars.Add(new KeyValuePair<string, string>(names[i].Value.Remove(names[i].Value.Length-1), values[i]));
					}
				}
			}
			return _vars.ToArray();
		}
    }
}
