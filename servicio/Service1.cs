using replica;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Threading;

namespace servicio
{
    public partial class Service1 : ServiceBase
    {
        Configuracion cnf;
        string INICONF;
        EventLog eventLog;
        public Service1()
        {
            string searcher = Environment.GetEnvironmentVariable("searcher");
            INICONF = System.IO.Path.Combine(searcher??"", "Configuration.ini");
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
                eventLog.WriteEntry(string.Format("{0} no existe.",INICONF), EventLogEntryType.Error);                
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

			while (cnf.run)
			{
				using (replica.LuceneReplicar lr = new replica.LuceneReplicar(cnf.general_name))
				{
					Thread.Sleep(cnf.database_sleep);
					eventLog.WriteEntry(string.Format("generando copia en '{0}' de '{1}'", cnf.general_name, cnf.database_sql), EventLogEntryType.Information);
					string error = "";
					lr.reindex(cnf.general_name);
					for (int i = 0; i < cnf.database_connection.Length; i++)
					{
						if (!lr.Start(cnf.database_connection[i], cnf.database_sql[i], cnf.database_mdb[i], ref error))
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
            }
        }
        protected override void OnStop()
        {
            eventLog.WriteEntry("sevicio detenido", EventLogEntryType.Warning);
        }
    }
}
