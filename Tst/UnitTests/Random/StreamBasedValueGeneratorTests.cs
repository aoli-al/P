using System;
using System.Collections.Generic;
using NUnit.Framework;
using PChecker.Random;

namespace UnitTests.Random
{

    [TestFixture]
    public class StreamBasedValueGeneratorTests
    {

        internal class ControlledRandom : System.Random
        {
            private int i = 0;
            public override int Next()
            {
                return i++;
            }

            public override void NextBytes(byte[] buffer)
            {
                var bytes = BitConverter.GetBytes(i++);
                bytes.CopyTo(buffer, 0);
            }
        }

        [Test]
        public void TestGeneratorGenerateExpectedValue()
        {
            var random = new ControlledRandom();
            var generator = new RandomInputGenerator(random, null);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(generator.Next(), i);
            }
        }

        [Test]
        public void TestGeneratorCopy()
        {
            var generator = new RandomInputGenerator();
            List<int> generated = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                generated.Add(generator.Next());
            }
            var newGenerator = new RandomInputGenerator(generator);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(generated[i], newGenerator.Next());
            }
        }
    }
}
