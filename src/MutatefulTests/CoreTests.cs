using System;
using NUnit.Framework;
using Mutateful.Core;

namespace MutatefulTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestDecimalToSpreadshimal()
        {
            Assert.AreEqual("", ClipReference.ToSpreadshimal(0));
            Assert.AreEqual("A", ClipReference.ToSpreadshimal(1));
            Assert.AreEqual("B", ClipReference.ToSpreadshimal(2));
            Assert.AreEqual("Z", ClipReference.ToSpreadshimal(26));
            Assert.AreEqual("AA", ClipReference.ToSpreadshimal(27));
            Assert.AreEqual("AC", ClipReference.ToSpreadshimal(29));
            Assert.AreEqual("ZZ", ClipReference.ToSpreadshimal(702));
            Assert.AreEqual("AAA", ClipReference.ToSpreadshimal(703));
            Assert.AreEqual("AAB", ClipReference.ToSpreadshimal(704));
        }        
        
        [Test]
        public void TestSpreadshimalToDecimal()
        {
            Assert.AreEqual(0, ClipReference.FromSpreadshimal(""));
            Assert.AreEqual(1, ClipReference.FromSpreadshimal("A"));
            Assert.AreEqual(2, ClipReference.FromSpreadshimal("B"));
            Assert.AreEqual(26, ClipReference.FromSpreadshimal("Z"));
            Assert.AreEqual(27, ClipReference.FromSpreadshimal("AA"));
            Assert.AreEqual(29, ClipReference.FromSpreadshimal("AC"));
            Assert.AreEqual(702, ClipReference.FromSpreadshimal("ZZ"));
            Assert.AreEqual(703, ClipReference.FromSpreadshimal("AAA"));
            Assert.AreEqual(704, ClipReference.FromSpreadshimal("AAB"));
            Assert.AreEqual(702, ClipReference.FromSpreadshimal("zz"));
            Assert.AreEqual(703, ClipReference.FromSpreadshimal("aaa"));
            Assert.AreEqual(704, ClipReference.FromSpreadshimal("aab"));
        }
    }
}