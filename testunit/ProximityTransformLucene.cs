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
    public class ProximityTransformLucene
    {
#if __MonoCS__
        [Test()]
#else
        [TestMethod]
        #endif
        public void Simple()
        {
			composer c = new composer("ana");
			print(c.terms.ToArray());
        }
#if __MonoCS__
        [Test()]
#else
        [TestMethod]
#endif
        public void SimpleSpace()
        {
            composer c = new composer("ana ramire");
            print(c.terms.ToArray());
        }
		private void print(object obj)
        {
            if (obj is System.Array)
            {
                Debug.WriteLine(((Array)obj).Length);
                foreach (var token in (Array)obj)
                {
                    Debug.WriteLine(token);
                }
            }
            else
            {
                Debug.WriteLine(obj);
            }
        }
    }
}
