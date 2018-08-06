using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System;

namespace TokenizeParceLucene
{
	public struct VarTransform
    {
		public static string reciveMASKXOR(byte[] buffer){
			byte b = buffer[1];
            int dataLength = 0;
            int totalLength = 0;
            int keyIndex = 0;
            if (b - 128 <= 125)
            {
                dataLength = b - 128;
                keyIndex = 2;
                totalLength = dataLength + 6;
            }
            if (b - 128 == 126)
            {
                dataLength = BitConverter.ToInt16(new byte[] { buffer[3], buffer[2] }, 0);
                keyIndex = 4;
                totalLength = dataLength + 8;
            }
            if (b - 128 == 127)
            {
                dataLength = (int)BitConverter.ToInt64(new byte[] { buffer[9], buffer[8], buffer[7], buffer[6], buffer[5], buffer[4], buffer[3], buffer[2] }, 0);
                keyIndex = 10;
                totalLength = dataLength + 14;
            }
            if (totalLength > buffer.Length)
                throw new Exception("The buffer length is small than the data length");
            var key = new byte[] { buffer[keyIndex], buffer[keyIndex + 1], buffer[keyIndex + 2], buffer[keyIndex + 3] };
            int dataIndex = keyIndex + 4;
            int count = 0;
            for (int i = dataIndex; i < totalLength; i++)
            {
                buffer[i] = (byte)(buffer[i] ^ key[count % 4]);
                count++;
            }
            return Encoding.UTF8.GetString(buffer, dataIndex, dataLength);
		}
		public static byte[] sendMaskXOR(string text){
			byte[] bytes = Encoding.UTF8.GetBytes(text);
            List<byte> Lbytes = new List<byte>();
            Lbytes.Add((byte)129);
            Lbytes.Add((byte)129);
            int Length = bytes.Length + 1;
            if (Length <= 125)
            {
                Lbytes.Add((byte)(Length));
                Lbytes.Add((byte)(Lbytes.Count));
            }
            else
            {
                if (Length >= 125 && Length <= 65535)
                {
                    Lbytes.Add((byte)126);
                    Lbytes.Add((byte)(Length >> 8));
                    Lbytes.Add((byte)Length);
                    Lbytes.Add((byte)Lbytes.Count);
                }
                else
                {
                    Lbytes.Add((byte)127);
                    Lbytes.Add((byte)(Length >> 56));
                    Lbytes.Add((byte)(Length >> 48));
                    Lbytes.Add((byte)(Length >> 40));
                    Lbytes.Add((byte)(Length >> 32));
                    Lbytes.Add((byte)(Length >> 24));
                    Lbytes.Add((byte)(Length >> 16));
                    Lbytes.Add((byte)(Length >> 8));
                    Lbytes.Add((byte)Length);
                    Lbytes.Add((byte)Lbytes.Count);
                }
            }
            Lbytes.RemoveAt(0);
            Lbytes.AddRange(bytes);
			return Lbytes.ToArray();
		}
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
