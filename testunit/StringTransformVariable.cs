using NUnit.Framework;
using System;

namespace testunit
{
    [TestFixture()]
	public class StringTransformVariable
    {
        [Test()]
        public void InputSearch()
        {
			var dir = TokenizeParcerucene.VarTransform.conversor("carpeta:\"123456\",placa:abcd",null,':', ',');
			print(dir);
        }
		[Test()]
        public void queryRAwReplace()
        {
			var dir = TokenizeParcerucene.VarTransform.conversor("select * from $Database$", '$', '$', null);
            print(dir);
        }
		[Test()]
        public void querySP_double_Replace()
        {
			var dir = TokenizeParcerucene.VarTransform.conversor("exec sp_selact('$Database$');exec sp_selact('$Database$')", '$', '$', null);
            print(dir);
        }
		[Test()]
        public void querySPReplace()
        {
            var dir = TokenizeParcerucene.VarTransform.conversor("exec sp_selact('$Database$')", '$', '$', null);
            print(dir);
        }
		[Test()]
        public void ConnectionReplace()
        {
			var dir = TokenizeParcerucene.VarTransform.conversor("database=testdb;server=http://localhost;user=sa", null, '=', ';');
            print(dir);
        }
		private void print(object obj){
			if(obj is System.Array){
				foreach (var token in (Array)obj)
                {
                    Console.WriteLine(token);
                }	
			}else{
				Console.WriteLine(obj);	
			}            
		}
    }
}
