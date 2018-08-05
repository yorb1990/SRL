using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TokenizeParcerucene
{
	public struct VarTransform
    {
		//private static List<KeyValuePair<string, string>> _vars;
		public static KeyValuePair<string,string>[] conversor(string value,char? BEGINseparator, char? ENDseparator, char? MultifieldSeparator = ',')
		{
			//if (_vars == null)
			//{
			    List<KeyValuePair<string, string>> _vars = new List<KeyValuePair<string, string>>();
				value = value.Trim();
			Regex NameRegex = new Regex(string.Format(@"([{0}]|^){1}([A-Z]|[a-z]|[0-9])+[{2}]",StringEmpty(MultifieldSeparator),StringEmpty(BEGINseparator),StringEmpty(ENDseparator) ));
			var names = NameRegex.Matches(value).OfType<Match>().Select(m=>m.Value).Distinct().ToArray();

			if (names.Length > 0)
			{
				for (int i = 0; i < names.Length; i++)
				{
					string v = "";
					string n1 = names[i];
					int index_1 = value.IndexOf(n1);
					if (i < names.Length - 1)
					{
						string n2 = names[i + 1];
						int index_2 = value.IndexOf(n2);
						v = value.Substring(index_1 + n1.Length, index_2 - (index_1 + n1.Length));
					}
					else
					{
						v = value.Substring(index_1 + n1.Length);
					}
					if (n1[0] == MultifieldSeparator)
					{
						n1 = n1.Substring(1);
					}
					if (n1[0] == BEGINseparator)
					{
						n1 = n1.Substring(1);
					}
					if (n1[n1.Length - 1] == ENDseparator)
					{
						n1 = n1.Substring(0, n1.Length - 1);
					}
					_vars.Add(new KeyValuePair<string, string>(n1, v));
				}
			}
			//}
			return _vars.ToArray();
		}

		private static string StringEmpty(char? compare)
		{
			if(compare==null){
				return string.Empty;
			}
			return compare.Value.ToString();
		}
	}
}
