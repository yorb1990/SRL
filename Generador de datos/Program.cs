using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
namespace Generador_de_datos
{
    class Program
    {
        static void Main(string[] args)
        {
            DataTable nombres=new DataTable(), apellidosP = new DataTable(), apellidosS = new DataTable();
            using (var cmd=new MySql.Data.MySqlClient.MySqlCommand("select nombre from nombres order by rand()",new MySql.Data.MySqlClient.MySqlConnection("Server=localhost;Database=nueva;Uid=root;Pwd=;SslMode=none")))
            {
                cmd.Connection.Open();
                var DA = new MySql.Data.MySqlClient.MySqlDataAdapter(cmd);
                DA.Fill(nombres);
                cmd.CommandText= "select apellido from apellidos order by rand()";
                DA.SelectCommand = cmd;
                DA.Fill(apellidosP);
                cmd.CommandText = "select apellido from apellidos order by rand()";
                DA.SelectCommand = cmd;
                DA.Fill(apellidosS);
                foreach (DataRow nombre in nombres.Rows)
                {                    
                    for (int i = 0; i < apellidosP.Rows.Count; i++)
                    {
                        cmd.CommandText = "insert into personas (nombres,apellidoP,apellidoS) values (";
                        cmd.CommandText += string.Format("'{0}','{1}','{2}')", nombre.ItemArray[0], apellidosP.Rows[i].ItemArray[0], apellidosS.Rows[i].ItemArray[0]);
                        cmd.ExecuteNonQuery();
                    }                    
                }
            }                
        }
    }
}
