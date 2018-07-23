using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Newtonsoft.Json;
using replica;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static servicio.Service1;

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
            while (true)
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
                    Byte[] response = Encoding.UTF8.GetBytes(SetHeaders(key,"",101));
                    stream.Write(response, 0, response.Length);
                }
                else
                {
                    var docs = SearhLucene(GetDecodedData(bytes, bytes.Length));
                    if (docs == null)
                    {
                        string msg = JsonConvert.SerializeObject("Sin resultados");
                        byte[] response = Encoding.UTF8.GetBytes(msg);
                        stream.Write(response, 0, response.Length);
                    }
                    else
                    {
                        string msg = JsonConvert.SerializeObject(docs);
                        byte[] response = Encoding.UTF8.GetBytes(msg);
                        stream.Write(response, 0, response.Length);
                    }
                }
            }
        }
        public string SetHeaders(string key,string data,int edo=200)
        {
            return "HTTP/1.1 "+edo+" Switching Protocols" + Environment.NewLine
                         + "Upgrade: websocket" + Environment.NewLine
                         + "Connection: Upgrade" + Environment.NewLine
                         + "Sec-WebSocket-Accept: " + key + Environment.NewLine
                         //+"data"+data+Environment.NewLine
                         + Environment.NewLine
                         ;
        }
        public byte[] GetEncodedData(string data)
        {
            bytes[] bytes=Encoding.UTF8.GetBytes(data);
            byte b = bytes[1];
            int dataLength = 0;
            int totalLength = 0;
            int keyIndex = 0;
            if (b - 128 > 125)
            {
                dataLength = b + 128;
                keyIndex = 2;
                totalLength = dataLength + 6;
            }
            return bytes;
        }
        public string GetDecodedData(byte[] buffer, int length)
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
            if (totalLength > length)
                throw new Exception("The buffer length is small than the data length");
            byte[] key = new byte[] { buffer[keyIndex], buffer[keyIndex + 1], buffer[keyIndex + 2], buffer[keyIndex + 3] };
            int dataIndex = keyIndex + 4;
            int count = 0;
            for (int i = dataIndex; i < totalLength; i++)
            {
                buffer[i] = (byte)(buffer[i] ^ key[count % 4]);
                count++;
            }
            return Encoding.UTF8.GetString(buffer, dataIndex, dataLength);
        }
        public Document[] SearhLucene(string word)
        {
            Directory directory = FSDirectory.Open(new System.IO.DirectoryInfo(cnf.name));
            Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "nombres", analyzer);
            Grammar g = new Grammar(word, cnf.field);
            string _query = g.Build();
            Query query = parser.Parse(_query);
            var searcher = new IndexSearcher(directory, true);
            TopDocs topDocs = searcher.Search(query, cnf.limitd);
            List<Document> docs = new List<Document>(cnf.limit);
            for (int i = 0, j = 0; i < topDocs.ScoreDocs.Length; i++, j++)
            {
                if (j >= cnf.limit)
                {
                    break;
                }
                docs.Add(searcher.Doc(topDocs.ScoreDocs[i].Doc));
            }
            return docs.ToArray(); 
        }
    }
}
