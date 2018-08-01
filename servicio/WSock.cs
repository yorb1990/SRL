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
                while (!stream.DataAvailable) ;
                Byte[] bytes = new Byte[clientSocket.Available];
                stream.Read(bytes, 0, bytes.Length);
                String data = Encoding.UTF8.GetString(bytes);                
                if(new Regex("^GET").IsMatch(data))
                {                    
                    key = Convert.ToBase64String(
                        SHA1.Create().ComputeHash(
                            Encoding.UTF8.GetBytes(
                                new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                            )
                        )
                    );
                    Byte[] response = Encoding.UTF8.GetBytes(SetHeaders(key,""));
                    stream.Write(response, 0, response.Length);
                }
                else
                {
                    if (bytes.Length > 4)
                    {
                        var docs = SearhLucene(GetDecodedData(bytes, out byte[] keys));
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
        /*private ObjJSON DocToOBJ(Document doc)
        {
            return new ObjJSON()
            {
                id=doc.Get("id"),
                tittle= doc.Get("tittle"),
                url= doc.Get("url"),
                body= doc.Get("body")
            };
        }*/
        public string SearhLucene(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return string.Empty;
            }
            Directory directory = FSDirectory.Open(new System.IO.DirectoryInfo(cnf.general_name));                        
            IndexSearcher searcher = new IndexSearcher(directory, true);
            composer c= new composer(word);
            var a = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            var MulField = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, cnf.search_fields, a);
            BooleanQuery.MaxClauseCount=c.terms.Count;
            BooleanQuery BooleanBuild = new BooleanQuery();
            //List<ObjJSON> docs = new List<ObjJSON>();
            JArray joa = new JArray();
            try
            {
                foreach (string t in c.terms)
                {
                    BooleanBuild.Add(MulField.Parse(t + "*"), Occur.SHOULD);
                }
                TopDocs topDocs = searcher.Search(BooleanBuild, searcher.MaxDoc);
                //docs = new List<ObjJSON>(cnf.search_limit);
                foreach(var item in topDocs.ScoreDocs)
                {   
                    if (joa.Count >= cnf.search_limit)
                    {
                        break;
                    }
                    JObject jo = new JObject();
                    foreach (string field in cnf.search_fields)
                    {
                        jo.Add(field,searcher.Doc(item.Doc).Get(field));
                    }
                    joa.Add(jo);
                    //docs.Add(DocToOBJ(searcher.Doc(topDocs.ScoreDocs[i].Doc)));
                }
            }catch(Exception ex)
            {
                eventLog.WriteEntry(string.Format("MSG:{0}\nTRACE:{1}", ex.Message,ex.StackTrace), EventLogEntryType.Error);
            }
            //return docs.ToArray(); 
            return joa.ToString();
        }
    }
}