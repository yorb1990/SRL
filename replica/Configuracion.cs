using IniParser;
using System;

namespace replica
{
    public class Configuracion
    {
        public readonly string general_name;
        public readonly string[] database_connection;
        public readonly replica.sqltype database_mdb;
        public readonly int database_sleep;
        public readonly string[] database_sql;
        public readonly int search_port;
        public readonly int search_limit;
        public readonly string[] search_fields;
        public readonly string[] search_objfields;
        private string error = "";
        public Boolean run = false;
        public string GetError()
        {
            return error;
        }
        public Configuracion(string dir)
        {
            try
            {
                var data = new FileIniDataParser().ReadFile(dir);
                string searcher = Environment.GetEnvironmentVariable("searcher");
                this.general_name = System.IO.Path.Combine(searcher??"",data["general"]["name"]);
                this.database_connection = data["database"]["connection"].Split(',');
                Enum.TryParse(data["database"]["mdb"], out database_mdb);
                this.database_sleep = int.Parse(data["database"]["sleep"])* 60000;
                this.database_sql = data["database"]["sql"].Split(';');
                if (this.database_sql.Length == 1)
                {
                    string sql = this.database_sql[0];
                    this.database_sql = new string[this.database_connection.Length];
                    for(int i = 0; i < this.database_connection.Length ; i++)
                    {
                        this.database_sql[i] = sql;
                    }
                }
                if (this.database_connection.Length != this.database_sql.Length)
                {
                    error = "no coincide el numero de conecciones con el numero de queryes sql";
                    run = false;
                }
                this.search_limit = int.Parse(data["search"]["limit"]);
                this.search_port = int.Parse(data["search"]["port"]) ;
                this.search_fields = data["search"]["fields"].Split(',');
                this.search_objfields = data["search"]["objfields"].Split(',');
                run = true;
            }catch(Exception e)
            {
                error = e.Message;
                run = false;
            }
        }
    }
}
