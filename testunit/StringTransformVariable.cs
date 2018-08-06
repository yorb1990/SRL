#if __MonoCS__
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using System;
using System.Diagnostics;
using TokenizeParceLucene;

namespace testunit
{
#if __MonoCS__
    [TestFixture()]
#else
    [TestClass]
#endif
	public class StringTransformVariable
    {
#if __MonoCS__
        [Test()]
#else
        [TestMethod]
#endif
        public void InputSearch()
        {
			var dir = VarTransform.conversor("carpeta:\"123456\",placa:abcd",null,':', ',');
			print(dir);
        }
#if __MonoCS__
        [Test()]
#else
        [TestMethod]
#endif
        public void queryRAwReplace()
        {
			var dir = VarTransform.conversor("select * from $Database$", '$', '$', null);
            print(dir);
        }
#if __MonoCS__
        [Test()]
#else
        [TestMethod]
#endif
        public void querySP_double_Replace()
        {
			var dir = VarTransform.conversor("exec sp_selact('$Database$');exec sp_selact('$Database$')", '$', '$', null);
            print(dir);
        }
#if __MonoCS__
        [Test()]
#else
        [TestMethod]
#endif
        public void querySPReplace()
        {
            var dir = VarTransform.conversor("exec sp_selact('$Database$')", '$', '$', null);
            print(dir);
        }
#if __MonoCS__
        [Test()]
#else
        [TestMethod]
#endif
        public void ConnectionReplace()
        {
			var dir = VarTransform.conversor("database=testdb;server=http://localhost;user=sa", null, '=', ';');
            print(dir);
        }
		private void print(object obj){
			if(obj is System.Array){
				foreach (var token in (Array)obj)
                {
                    Debug.WriteLine(token);
                }	
			}else{
                Debug.WriteLine(obj);	
			}            
		}
    }
}
