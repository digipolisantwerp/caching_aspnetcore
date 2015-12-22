using System;
using System.Collections.Generic;
using Toolbox.Caching;
using Xunit;

namespace Toolbox.Caching.UnitTests.MemoryCacheProviderTests
{
	public static class AddAndGetTests
    {
        [Fact]
        public static void PrimitiveTypeIsAdded()
        {
            using (var cacheProvider = new MemoryCacheProvider())
            {
                cacheProvider.Add<int>("Int", 1, true);

                Assert.Equal(1, cacheProvider.Get<int>("Int")); 
            }
        }

        [Fact]
        public static void ComplexTypeIsAdded()
        {
            using (var cacheProvider = new MemoryCacheProvider())
            {
                var lijst = new List<int>() { 1, 2, 3 };

                cacheProvider.Add<List<int>>("LijstVanInt", lijst, true);

                Assert.Equal(lijst, cacheProvider.Get<List<int>>("LijstVanInt"));
            }
        }

        [Fact]
        public static void KeyNullRaisesArgumentException()
        {
            using (var cacheProvider = new MemoryCacheProvider())
            {
                var ex = Assert.Throws<ArgumentException>(() => cacheProvider.Add<int>(null, 1, true));
                Assert.Equal("key", ex.ParamName);
            }
        }

        [Fact]
        public static void KeyEmptyRaisesArgumentException()
        {
            using (var cacheProvider = new MemoryCacheProvider())
            {
                var ex = Assert.Throws<ArgumentException>(() => cacheProvider.Add<int>("", 1, true));
                Assert.Equal("key", ex.ParamName);
            }
        }

        [Fact]
        public static void ValueNullIsAllowed()
        {
            using (var cacheProvider = new MemoryCacheProvider())
            {
                cacheProvider.Add<object>("object", null, true);
                Assert.Null(cacheProvider.Get<object>("object"));
            }
        }

        [Fact]
        public static void OverwriteTrueOverwritesExistingEntry()
        {
            using (var cacheProvider = new MemoryCacheProvider())
            {
                cacheProvider.Add<int>("Int", 1, true);
                cacheProvider.Add<int>("Int", 2, true);
                Assert.Equal(2, cacheProvider.Get<int>("Int"));
            }
        }

        [Fact]
        public static void OverwriteFalseDoesNotOverwriteExistingEntry()
        {
            using (var cacheProvider = new MemoryCacheProvider())
            {
                cacheProvider.Add<int>("Int", 1, false);
                cacheProvider.Add<int>("Int", 2, false);
                Assert.Equal(1, cacheProvider.Get<int>("Int"));
            }
        }
    }
}