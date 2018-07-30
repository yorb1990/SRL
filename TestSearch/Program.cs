using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using replica;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TestSearch
{
    public class Grammar
    {
        string _search ;
        string[] _terms;
        string _field;
        public string search { get { return _search; } private set { _search = value; } }
        public string[] terms { get { if (_terms == null) { this.creator(); } return _terms; } private set { _terms = value; } }
        public string Build()
        {
            string word = "";
            if (_terms == null) { this.creator(); }
            foreach(string term in terms)
            {
                word += string.Format(" {0}: {1}  OR",this._field, term);
            }
            return word.Remove(word.Length - 2);
        }
        public Grammar(string Search,string Field)
        {
            this.search = Search;
            this._field = Field;
            this.mini();
            this.normalizd();
        }
        public void mini()
        {
            search = search.ToLower();
        }
        private void creator()
        {
            List<String> Terms = new List<string>();
            foreach(string rul in rules)
            {
                Regex r = new Regex(rul);
                Match m = r.Match(search);
                if (m.Success)
                {
                    foreach (string key in rul.Split('|'))
                    {
                        Terms.Add(r.Replace(search, key));
                    }
                }
            }
            if (search.Length < 5)
            {
                Terms.Insert(0,search + "*");
            }
            this.terms=Terms.ToArray();
        }
        private string[] rules = {
            "a|á",
            "e|é",
            "i|í",
            "o|ó",
            "u|ú",
            "cc|x",
            "s|z",
            "k|c ",
            "g|j|y"
        };
        public void normalizd()
        {
            search= search.Replace("  ", " ");
        }
        public string[] tokens(ref string search)
        {
            return Regex.Split(search, @"\s|,");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Configuracion cnf = new Configuracion(@"C:\Users\rafit\Documents\Github\SRL\servicio\bin\Debug\Configuration.ini");
            Directory directory = FSDirectory.Open(new System.IO.DirectoryInfo(cnf.general_dir));
            Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            /*var parser = new PhraseQuery(Lucene.Net.Util.Version.LUCENE_30, "", analyzer);
            string word=Console.ReadLine();
            Grammar g = new Grammar(word, "tittle");
            string _query = g.Build();
            Query query = parser.Parse("tittle: gesus* OR gésus* OR gehsus* OR ghesus* OR géhsus* OR ghésus* OR gesus* OR gesús* OR gesuhs* OR geshus* OR gesúhs* OR geshús* OR gesus* OR gezuz* OR gesus* OR jesus* OR yesus* ");            
            var searcher = new IndexSearcher(directory, true);
            TopDocs topDocs = searcher.Search(query,cnf.limitd);*/
            var searcher = new IndexSearcher(directory, true);
            BooleanQuery.MaxClauseCount = int.MaxValue;
            var bq = new BooleanQuery();
            var mf = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, new string[]{ "tittle" }, analyzer);
            int k= 1;
            foreach(string t in "gezuz* jes?s* jesus* yesus*".Split(' '))
            {
                bq.Add(mf.Parse(t), Occur.SHOULD);
                if (k == 5)
                {
                    break;
                }                
                k++;
            }
            TopDocs topDocs = searcher.Search(bq,searcher.MaxDoc); 
            List <Document> docs = new List<Document>(cnf.search_limit);
            for(int i=0,j=0; i< topDocs.ScoreDocs.Length;i++,j++)
            {
                if (j >= cnf.search_limit)
                {
                    break;
                }
                docs.Add(searcher.Doc(topDocs.ScoreDocs[i].Doc));                
            }
            printer(docs);
            Console.ReadLine();
        }
        private static void printer(List<Document> Docs)
        {
            foreach(var doc in Docs)
            {
                Console.WriteLine("DOCUMENT ");
                foreach (var field in doc.GetFields())
                {
                    Console.Write("{0}: {1} ,", field.Name, doc.Get(field.Name));
                }
            }
        }
    }
}