using replica;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Threading;
using System.Text.RegularExpressions;

namespace servicio
{
    public partial class Service1 : ServiceBase
    {
        Configuracion cnf;
        const string INICONF = "Configuration.ini";
        EventLog eventLog;
        public Service1()
        {
            InitializeComponent();
            if (!EventLog.SourceExists("searche"))
            {
                eventLog= new EventLog();
                eventLog.Source = "searcher";
            }
            if (System.IO.File.Exists(INICONF))
            {
                cnf = new Configuracion(INICONF);
            }
            else
            {
                eventLog.WriteEntry(string.Format("{1}/{0} no existe.",INICONF,System.Environment.CurrentDirectory), EventLogEntryType.Error);                
            }            
        }
        protected override void OnStart(string[] args)
        {
            if (cnf.run)
            {
                Thread replicar = new Thread(new ThreadStart(TheadReplica));
                Thread Servidor = new Thread(new ThreadStart(TheadTCP));
                replicar.Start();
                Servidor.Start();
            }
            else
            {
                eventLog.WriteEntry(cnf.GetError(), EventLogEntryType.Error);
            }
        }
        public void TheadTCP()
        {
            TcpListener serverSocket = new TcpListener(cnf.ports);
            TcpClient clientSocket = default(TcpClient);
            serverSocket.Start();
            while (true)
            {
                clientSocket = serverSocket.AcceptTcpClient();
                WSock client = new WSock(clientSocket) {
                    cnf = this.cnf,
                    eventLog=this.eventLog
                };
            }
            //clientSocket.Close();
            //serverSocket.Stop();
        }
        public class Grammar
        {
            string _search;
            string[] _terms;
            string _field;
            public string search { get { return _search; } private set { _search = value; } }
            public string[] terms { get { if (_terms == null) { this.creator(); } return _terms; } private set { _terms = value; } }
            public string Build()
            {
                string word = "";
                if (_terms == null) { this.creator(); }
                foreach (string term in terms)
                {
                    word += string.Format(" {0}: {1}  OR", this._field, term);
                }
                return word.Remove(word.Length - 2);
            }
            public Grammar(string Search, string Field)
            {
                this.search = Search.Trim();
                this._field = Field;
                this.mini();
                this.normalizd();
            }
            public void mini()
            {
                search = search.ToLower();
            }
            private void creator()
            {
                List<String> Terms = new List<string>();
                foreach (string rul in rules)
                {
                    Regex r = new Regex(rul);
                    Match m = r.Match(search);
                    if (m.Success)
                    {
                        foreach (string key in rul.Split('|'))
                        {
                            Terms.Add(r.Replace(search, key));
                        }
                    }
                }
                if (search.Length < 5)
                {
                    Terms.Insert(0, search + "*");
                }
                this.terms = Terms.ToArray();
            }
            private string[] rules = {
            "a|á",
            "e|é",
            "i|í",
            "o|ó",
            "u|ú",
            "cc|x",
            "s|z",
            "k|c ",
            "g|j|y"
        };
            public void normalizd()
            {
                search = search.Replace("  ", " ");
            }
            public string[] tokens(ref string search)
            {
                return Regex.Split(search, @"\s|,");
            }
        }
        public void TheadReplica(){
            replica.LuceneReplicar lr = new replica.LuceneReplicar(cnf.connection, cnf.name); ;            
            while (cnf.run)
            {
                Thread.Sleep(cnf.sleep);
                eventLog.WriteEntry("generando copia", EventLogEntryType.Information);
                string error = "";
                lr.reindex(cnf.name);
                if (!lr.Start(cnf.sql, cnf.sqlt, ref error))
                {
                    eventLog.WriteEntry(error, EventLogEntryType.Error);
                    break;
                }
                else
                {
                    if (error != "")
                    {
                        eventLog.WriteEntry(error, EventLogEntryType.Warning);
                    }
                }
            }
        }
        protected override void OnStop()
        {
        }
    }
}
