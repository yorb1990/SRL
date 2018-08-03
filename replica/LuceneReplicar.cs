using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System;
using System.Data;
using System.Threading;
using System.Text.RegularExpressions;

namespace replica
{
    public class LuceneReplicar 
    {
        public Directory directory;
        public Analyzer analyzer;
        public LuceneReplicar(string name)
        {
            analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
        }
        public void reindex(string name)
        {
            if (System.IO.Directory.Exists(name))
            {
                System.IO.Directory.Move(name, name + "_" + DateTime.Now.ToString("yyyy-MM-dd HH mm ss"));
            }
            directory = FSDirectory.Open(name);
        }
        public bool Start(string cons,string SQLquery,sqltype sqlt,ref string error)
        {
            int startvar = SQLquery.IndexOf('$');
            if (startvar > 0)
            {
                string name = "",value="";
                var r = new Regex(@"[A-Z]|[a-z]|[0-9]");
                for (int i=startvar+1;i< SQLquery.Length;i++)
                {
                    if (!r.IsMatch(SQLquery[i].ToString()))
                    {
                        break;
                    }
                    name += SQLquery[i];
                }
                startvar = cons.IndexOf(name);
                for (int i = startvar + +1+name.Length; i < cons.Length; i++)
                {
                    if (cons[i]==';')
                    {
                        break;
                    }
                    value += cons[i];
                }
                SQLquery=SQLquery.Replace("$" + name, value);
            }
            using (var writer = new IndexWriter(directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                IDbConnection con;
                switch (sqlt)
                {
                    case sqltype.mysql:
                        con = new MySqlConnection(cons);
                        //cmd = new MySqlCommand(SQLquery, con);
                        break;
                    case sqltype.mssql:
                        con = new SqlConnection(cons);
                        //cmd = new SqlCommand(SQLquery, con);
                        break;
                    default:
                        con = new MySqlConnection(cons);
                        break;
                }
                IDbCommand cmd = con.CreateCommand();
                cmd.CommandText = SQLquery;
                try
                {
                    cmd.Connection.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        //if (rd.HasRows)
                        //{
                        while (rd.Read())
                        {
                            Document doc = new Document();
                            for (int i = 0; i < rd.FieldCount; i++)
                            {
                                doc.Add(new Field(rd.GetName(i), rd.GetValue(i).ToString(), Field.Store.YES, Field.Index.ANALYZED));
                            }
                            writer.AddDocument(doc);
                            doc = null;
                        }
                        writer.Optimize();
                        writer.Commit();
                        //writer.Dispose();
                        //}
                        //else
                        //{
                        //error = "query sin datos de retorno";
                        //}
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    return false;
                }
            }
            return true;
        }
    }
}
