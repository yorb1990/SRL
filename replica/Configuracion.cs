using IniParser;
using System;

namespace replica
{
    public class Configuracion
    {
        public readonly string general_name;
        public readonly string database_connection;
        public readonly replica.sqltype database_mdb;
        public readonly int database_sleep;
        public readonly string database_sql;
        public readonly int search_port;
        public readonly int search_limit;
        public readonly string[] search_fields;
        public readonly int sync_port;
        public readonly int sync_sleep;
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
                this.general_name = System.IO.Path.Combine(Environment.GetEnvironmentVariable("searcher"),data["general"]["dir"]);
                this.database_connection = data["database"]["connection"];
                Enum.TryParse(data["database"]["mdb"], out database_mdb);
                this.database_sleep = int.Parse(data["database"]["sleep"])* 60000;
                this.database_sql = data["database"]["sql"];
                this.search_limit = int.Parse(data["search"]["limit"]);
                this.search_port = int.Parse(data["search"]["port"]) ;
                this.search_fields = data["search"]["fields"].Split(',');
                this.sync_port = int.Parse(data["sync"]["port"]) ;
                this.sync_sleep = int.Parse(data["sync"]["sleep"]) * 60000;
                run = true;
            }catch(Exception e)
            {
                error = e.Message;
                run = false;
            }
        }
    }
}
