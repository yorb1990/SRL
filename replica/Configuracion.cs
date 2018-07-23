using IniParser;
using System;

namespace replica
{
    public class Configuracion
    {
        public readonly string sql;
        public readonly int sleep;
        public readonly string connection;
        public readonly string name;
        public readonly string mdb;
        public readonly replica.sqltype sqlt;
        public readonly bool run;
        public readonly int limitd;
        public readonly int limit;
        public readonly int ports;
        public readonly int portd;
        public readonly string field;
        private string error = "";
        public string GetError()
        {
            return error;
        }
        public Configuracion(string dir)
        {
            try
            {
                var data = new FileIniDataParser().ReadFile(dir);
                this.sql = data["conf"]["sql"];
                this.sleep = int.Parse(data["conf"]["sleep"]) * 60000;
                this.connection = data["conf"]["connection"];
                this.name = data["conf"]["name"];
                this.limitd = int.Parse(data["conf"]["limitd"]);
                this.limit = int.Parse(data["conf"]["limit"]);
                this.field = data["conf"]["field"];
                this.ports = int.Parse(data["conf"]["ports"]);
                this.portd = int.Parse(data["conf"]["portd"]);
                replica.sqltype sqlt;
                Enum.TryParse(data["conf"]["mdb"], out sqlt);
                this.sqlt = sqlt;
                run = true;
            }catch(Exception e)
            {
                error = e.Message;
                run = false;
            }
        }
    }
}
