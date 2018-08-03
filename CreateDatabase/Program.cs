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
            Console.WriteLine("leyendo configuracion de {0}",INICONF);
            if (System.IO.File.Exists(INICONF))
            {
                cnf = new Configuracion(INICONF);
            }
            else
            {
                Console.WriteLine(string.Format("{0} no existe.", INICONF));
            }
            replica.LuceneReplicar lr = new LuceneReplicar(cnf.general_name); ;
            if (cnf.run)
            {
                string error = "";
                lr.reindex(cnf.general_name);                
                for (int i = 0; i < cnf.database_connection.Length; i++)
                {
                    Console.WriteLine("{1} de {2} : datos de conexion {0} ", cnf.database_connection[i],i,cnf.database_connection.Length);                    
                    if (!lr.Start(cnf.database_connection[i],cnf.database_sql[i], cnf.database_mdb[i], ref error))
                    {
                        Console.WriteLine(error);
                    }
                    else
                    {
                        if (error != "")
                        {
                            Console.WriteLine(error);
                        }
                        Console.WriteLine("{0} terminado . . .", i);
                    }
                }
            }
            else
            {
                Console.WriteLine(cnf.GetError());
            }
            Console.ReadLine();
        }
    }
}
