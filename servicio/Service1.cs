using replica;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Threading;
using System.Text.RegularExpressions;
using Lucene.Net.Search;

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
                Thread Servidor = new Thread(new ThreadStart(TheadTCPsearch));
                replicar.Start();
                Servidor.Start();
            }
            else
            {
                eventLog.WriteEntry(cnf.GetError(), EventLogEntryType.Error);
            }
        }
        public void TheadTCPsync()
        {
            TcpListener serverSocket = new TcpListener(cnf.sync_port);
            TcpClient clientSocket = default(TcpClient);
            serverSocket.Start();
            while (true)
            {
                clientSocket = serverSocket.AcceptTcpClient();
                WSock client = new WSock(clientSocket)
                {
                    cnf = this.cnf,
                    eventLog = this.eventLog
                };
            }
            //clientSocket.Close();
            //serverSocket.Stop()
        }
        public void TheadTCPsearch()
        {
            TcpListener serverSocket = new TcpListener(cnf.search_port);
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
        public void TheadReplica(){
            replica.LuceneReplicar lr = new replica.LuceneReplicar(cnf.database_connection, cnf.general_dir); ;            
            while (cnf.run)
            {
                Thread.Sleep(cnf.database_sleep);
                eventLog.WriteEntry("generando copia", EventLogEntryType.Information);
                string error = "";
                lr.reindex(cnf.general_dir);
                if (!lr.Start(cnf.database_sql, cnf.database_mdb, ref error))
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
