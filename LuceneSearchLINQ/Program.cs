using replica;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Linq;
using Lucene.Net.Linq.Mapping;
using Lucene.Net.Store;
using Lucene.Net.Documents;
using System.Linq;
using System;
using System.Collections.Generic;

namespace LuceneSearchLINQ
{
    public class ObjectSearch
    {
        [Field(Analyzer = typeof(StandardAnalyzer))]
        public string tittle { get; set; }

        [Field(Analyzer = typeof(StandardAnalyzer))]
        public string body { get; set; }

        [Field(Analyzer = typeof(StandardAnalyzer))]
        public string url { get; set; }

        // Stores the field as a NumericField
        [NumericField]
        public long Id { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            string[] T=new string[]{ "gesus*","gésus*","gehsus*","ghesus*","géhsus*","ghésus*","gesus*","gesús*","gesuhs*","geshus*","gesúhs*","geshús*","gesus*","gezuz*","gesus*","jesus*","yesus*" };
            Configuracion cnf = new Configuracion(@"C:\Users\rafit\Documents\Github\SRL\servicio\bin\Debug\Configuration.ini");
            Directory directory = FSDirectory.Open(new System.IO.DirectoryInfo(cnf.name));
            var provider=new LuceneDataProvider(directory, Lucene.Net.Util.Version.LUCENE_30);
            var OS = provider.AsQueryable<ObjectSearch>();
            /*int i = 0;
            IQueryable<ObjectSearch> oss;
            do
            {
                oss = from o in OS
                      where o.tittle == T[i]
                      select o;
                i++;
            } while (T.Length > i);*/
            Console.ReadLine();
        }
        public static void Print(List<ObjectSearch> objs)
        {
            foreach(var obj in objs)
            {
                Console.WriteLine("id:{0},tittle:{1}", obj.Id, obj.tittle);
            }
        }
    }
}
