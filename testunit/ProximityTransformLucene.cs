using NUnit.Framework;
using System;


namespace testunit
{
    [TestFixture()]
    public class ProximityTransformLucene
    {
        [Test()]
        public void Simple()
        {
			composer c = new TokenizeParcerucene.composer("ana");
			print(c.terms.ToArray());
        }
		[Test()]
        public void SimpleSpace()
        {
            composer c = new TokenizeParcerucene.composer("ana ramire");
            print(c.terms.ToArray());
        }

		private void print(object obj)
        {
            if (obj is System.Array)
            {
				Console.WriteLine(((Array)obj).Length);
                foreach (var token in (Array)obj)
                {
                    Console.WriteLine(token);
                }
            }
            else
            {
                Console.WriteLine(obj);
            }
        }
    }
}
