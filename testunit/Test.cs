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
			var dir = TokenizeParcerucene.VarTransform.conversor("carpeta:123456", ':', ',');
			Console.WriteLine(dir.Length);
			foreach(var token in dir){
				Console.WriteLine("{0}  :  {1}",token.Key,token.Value);
			}            
        }
    }
}
