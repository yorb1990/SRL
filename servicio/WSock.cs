using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using replica;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TokenizeParcerucene;

namespace servicio
{
    public class WSock
    {
        TcpClient clientSocket;
        public Configuracion cnf;
        public EventLog eventLog;
        public WSock(TcpClient inClientSocket)
        {            
            this.clientSocket = inClientSocket;
            Thread ctThread = new Thread(SendHtTTP);
            ctThread.Start();
        }
        private void SendHtTTP()
        {
            NetworkStream stream = clientSocket.GetStream();
            string key="";
            while (stream.CanRead)
            {
                Byte[] bytes = new Byte[1024];
                stream.Read(bytes, 0, bytes.Length);
                String data = Encoding.UTF8.GetString(bytes);
                if (new Regex("^GET").IsMatch(data))
                {
                    key = Convert.ToBase64String(
                        SHA1.Create().ComputeHash(
                            Encoding.UTF8.GetBytes(
                                new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                            )
                        )
                    );
                    Byte[] response = Encoding.UTF8.GetBytes(SetHeaders(key, ""));
                    stream.Write(response, 0, response.Length);
                }
                else
                {
                    try { 
                        if (bytes.Length > 4)
                        {
							JObject obj = JObject.Parse(GetDecodedData(bytes, out byte[] keys));
							string docs;
							if (new Regex(@"^([a-z]|[A-Z]|\d)*:").IsMatch((string)obj.SelectToken("data")))
							{
								string token = (string)obj.SelectToken("data");
								string name = "";
								var r = new Regex(@"[A-Z]|[a-z]|[0-9]");
								for (int i = token.IndexOf(':') + 1; i < token.Length; i++)
								{
									if (!r.IsMatch(token[i].ToString()))
									{
										break;
									}
									name += token[i];
								}
								string value = token.Split(':')[1];
								docs = SearhLucene((int)obj.SelectToken("point"), value, name);                        
							}
							else
							{
								docs = SearhLucene((int)obj.SelectToken("point"), (string)obj.SelectToken("data"));
							}
                            bytes = Encoding.UTF8.GetBytes(docs);
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
                            stream.Write(Lbytes.ToArray(), 0, Lbytes.Count);
                        }
                    }
                    catch (Newtonsoft.Json.JsonException ex)
                    {
                        clientSocket.Dispose();
                    }
                }
            }
        }
        public string SetHeaders(string key, string data)
        {
            string line = "\n";
            if (data != "")
            {
                line = "\r";
            }
            return "HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                         + "Upgrade: websocket" + Environment.NewLine
                         + "Connection: Upgrade" + Environment.NewLine
                         + "Sec-WebSocket-Accept: " + key + Environment.NewLine
                         + "data:"+data+line+ Environment.NewLine
                         ;
        }
        public byte[] GetEncodedData(string data,byte[] key)
        {
            byte[] toCrypt = Encoding.UTF8.GetBytes(data);
            for (int i = 0; i < toCrypt.Length; i++)
            {
                toCrypt[i] = (byte)(toCrypt[i] ^ key[i%4]);
            }
            return toCrypt;
        }
        public string GetDecodedData(byte[] buffer,out byte[] key)
        {
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
            key = new byte[] { buffer[keyIndex], buffer[keyIndex + 1], buffer[keyIndex + 2], buffer[keyIndex + 3] };
            int dataIndex = keyIndex + 4;
            int count = 0;
            for (int i = dataIndex; i < totalLength; i++)
            {
                buffer[i] = (byte)(buffer[i] ^ key[count % 4]);
                count++;
            }
            return Encoding.UTF8.GetString(buffer, dataIndex, dataLength);
        }
		public string SearhLucene(int point, string word, string field = "")
		{
			JObject mjo = new JObject();
			if (!(new Regex(@"^([']|[0-9]|[a-z]|[A-Z]|[Á]|[Ó],|[Í]|[É]|[Ú]|[Ñ]|[á]|[ó]|[í]|[é]|[ú]|[ñ]|\s){2,50}$")).IsMatch(word))
			{
				mjo.Add("error", "datos de entrada invalidos, vuelve a intentarlo");
				return mjo.ToString();
			}
			if (!System.IO.Directory.Exists(cnf.general_name))
			{
				mjo.Add("error", "busqueda no disponible intentalo en unos minutos mas");
				return mjo.ToString();
			}
			Directory directory = FSDirectory.Open(new System.IO.DirectoryInfo(cnf.general_name));
			IndexSearcher searcher = new IndexSearcher(directory, true);
			composer c = new composer(word);
			var a = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
			MultiFieldQueryParser MulField;
			if (field.Length == 0)
			{
				MulField = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, cnf.search_fields, a);
			}else{
				MulField = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, new string[]{field}, a);
			}
            BooleanQuery.MaxClauseCount=c.terms.Count;
            BooleanQuery BooleanBuild = new BooleanQuery();            
            JArray joa = new JArray();
            try
            {
                foreach (string t in c.terms)
                {
                    BooleanBuild.Add(MulField.Parse(t + "*"), Occur.SHOULD);
                }
                TopDocs topDocs = searcher.Search(BooleanBuild,searcher.MaxDoc);
                mjo.Add("limit", topDocs.ScoreDocs.Length);
                for (int i=point*cnf.search_limit;;i++)
                {   
                    if (i > cnf.search_limit*(point+1))
                    {
                        break;
                    }
                    JObject jo = new JObject();
                    foreach (string _field in cnf.search_objfields)
                    {
                        jo.Add(_field,searcher.Doc(topDocs.ScoreDocs[i].Doc).Get(_field));
                    }
                    joa.Add(jo);
                }
                mjo.Add("point", point);
            }catch(Exception ex)
            {
                mjo.Add("point", 0);
                mjo.Add("error", "error al buscar intentalo de nuevo.");                
                eventLog.WriteEntry(string.Format("MSG:{0}\nTRACE:{1}", ex.Message,ex.StackTrace), EventLogEntryType.Error);
            }
            mjo["data"]=joa;
            return mjo.ToString();
        }
    }
}