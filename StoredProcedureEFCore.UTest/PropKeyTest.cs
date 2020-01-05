using System;
using NUnit.Framework;

namespace StoredProcedureEFCore.UTest
{
    public class PropKeyTest
    {
        [Test]
        public void SameKeyIfSameType()
        {
            var key1 = Mapper<TestModel>.ComputePropertyKey(Array.Empty<string>());
            var key2 = Mapper<TestModel>.ComputePropertyKey(Array.Empty<string>());
            Assert.AreEqual(key1, key2);
        }

        [Test]
        public void SameKeyIfSameTypeSameColumns1()
        {
            var key1 = Mapper<TestModel>.ComputePropertyKey(new[] { "abc" });
            var key2 = Mapper<TestModel>.ComputePropertyKey(new[] { "abc" });
            Assert.AreEqual(key1, key2);
        }

        [Test]
        public void SameKeyIfSameTypeSameColumns2()
        {
            var key1 = Mapper<TestModel>.ComputePropertyKey(new[] { "abc", "def" });
            var key2 = Mapper<TestModel>.ComputePropertyKey(new[] { "abc", "def" });
            Assert.AreEqual(key1, key2);
        }

        [Test]
        public void KeyDiffersIfColumnsDiffers1()
        {
            var key1 = Mapper<TestModel>.ComputePropertyKey(new[] { "abc" });
            var key2 = Mapper<TestModel>.ComputePropertyKey(new[] { "def" });
            Assert.AreNotEqual(key1, key2);
        }

        [Test]
        public void KeyDiffersIfColumnsDiffers2()
        {
            var key1 = Mapper<TestModel>.ComputePropertyKey(new[] { "abc", "def" });
            var key2 = Mapper<TestModel>.ComputePropertyKey(new[] { "abc", "ghi" });
            Assert.AreNotEqual(key1, key2);
        }

        [Test]
        public void KeyDiffersIfColumnsOrderDiffers()
        {
            var key1 = Mapper<TestModel>.ComputePropertyKey(new[] { "abc", "def" });
            var key2 = Mapper<TestModel>.ComputePropertyKey(new[] { "def", "abc" });
            Assert.AreNotEqual(key1, key2);
        }

        [Test]
        public void KeyDiffersIfTypeDiffers()
        {
            var key1 = Mapper<TestModel>.ComputePropertyKey(Array.Empty<string>());
            var key2 = Mapper<TestModelClone>.ComputePropertyKey(Array.Empty<string>());
            Assert.AreNotEqual(key1, key2);
        }

        private class TestModelClone : TestModel { }
    }
}
