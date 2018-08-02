using replica;
using System;

namespace CreateDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            string searcher = Environment.GetEnvironmentVariable("searcher");
            Configuracion cnf=null;
            string INICONF = System.IO.Path.Combine(searcher ?? "", "Configuration.ini");
            if (System.IO.File.Exists(INICONF))
            {
                cnf = new Configuracion(INICONF);
            }
            else
            {
                Console.WriteLine(string.Format("{0} no existe.", INICONF));
            }
            replica.LuceneReplicar lr = new LuceneReplicar(cnf.database_connection, cnf.general_name); ;
            if (cnf.run)
            {
                string error = "";
                lr.reindex(cnf.general_name);
                if (!lr.Start(cnf.database_sql, cnf.database_mdb, ref error))
                {
                    Console.WriteLine(error);
                }
                else
                {
                    if (error != "")
                    {
                        Console.WriteLine(error);
                    }
                }
            }
        }
    }
}
