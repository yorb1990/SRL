using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Newtonsoft.Json.Linq;
using replica;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TokenizeParceLucene;

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
							JObject obj = JObject.Parse(VarTransform.reciveMASKXOR(bytes));
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
							bytes = VarTransform.sendMaskXOR(docs);
							stream.Write(bytes, 0, bytes.Length);
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