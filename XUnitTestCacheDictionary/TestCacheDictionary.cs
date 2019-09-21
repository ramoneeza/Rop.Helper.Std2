using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;
using Rop.CacheDictionary;
namespace XUnitTestCacheDictionary
{
    public class TestCacheDictionary
    {
        public class FakeCacheDictionary1 : CacheDictionary<string, int>
        {
            private static string InternalFactory(int i)
            {
                return i.ToString();
            }

            public FakeCacheDictionary1() : base(InternalFactory)
            {
            }
        }
        public class FakeCacheDictionary2 : CacheDictionary<string,int>
        {
            public int NumberOfFactories { get; private set; }
            private string InternalFactory(int i)
            {
                NumberOfFactories++;
                return i.ToString();
            }

            public FakeCacheDictionary2() : base(null)
            {
                SetInternalFactory(InternalFactory);
            }
        }
        [Fact]
        public void Test_Factory()
        {
            var fakecachedictionary=new FakeCacheDictionary1();
            var i = 1;
            Assert.Equal(i.ToString(),fakecachedictionary.Get(i));
        }
        [Fact]
        public void Test_SetInternalFactory()
        {
            var fakecachedictionary = new FakeCacheDictionary2();
            var i = 1;
            Assert.Equal(i.ToString(), fakecachedictionary.Get(i));
            Assert.True(fakecachedictionary.IsCached(i));
        }
        [Fact]
        public void Test_Cached()
        {
            var fakecachedictionary = new FakeCacheDictionary2();
            var i = 1;
            Assert.Equal(i.ToString(), fakecachedictionary.Get(i));
            Assert.Equal(i.ToString(), fakecachedictionary.Get(i));
            Assert.Equal(1, fakecachedictionary.NumberOfFactories);
            Assert.True(fakecachedictionary.IsCached(i));
        }
        [Fact]
        public void Test_GetValues()
        {
            var fakecachedictionary = new FakeCacheDictionary1();
            var i = 1;
            Assert.Equal(i.ToString(), fakecachedictionary.Get(i));
            i = 2;
            Assert.Equal(i.ToString(), fakecachedictionary.Get(i));
            var values = fakecachedictionary.GetValues();
            Assert.Equal(new[]{"1","2"}, values);
        }
        [Fact]
        public void Test_GetKeyValues()
        {
            var fakecachedictionary = new FakeCacheDictionary1();
            var i = 1;
            Assert.Equal(i.ToString(), fakecachedictionary.Get(i));
            i = 2;
            Assert.Equal(i.ToString(), fakecachedictionary.Get(i));
            var values = fakecachedictionary.GetKeyValues();
            Assert.Equal(new KeyValuePair<int,string>[] {new KeyValuePair<int, string>(1,"1"),new KeyValuePair<int, string>(2,"2"),  }, values);
        }
        [Fact]
        public void Test_UnCache()
        {
            var fakecachedictionary = new FakeCacheDictionary1();
            var i = 1;
            Assert.Equal(i.ToString(), fakecachedictionary.Get(i));
            Assert.True(fakecachedictionary.IsCached(1));
            Assert.True(fakecachedictionary.UnCache(1));
            Assert.False(fakecachedictionary.IsCached(1));
            Assert.Equal(i.ToString(), fakecachedictionary.Get(i));
            Assert.True(fakecachedictionary.IsCached(1));
        }

        [Fact]
        public void CacheTypeDictionary()
        {
            var ct=new CacheTypeDictionary<string>(t=>t.Name);
            var i = typeof(int);
            Assert.Equal(i.Name, ct.Get(i));
        }
        [Fact]
        public void CacheNameDictionary()
        {
            var ct = new CacheNameDictionary<int>((t,_) => int.Parse(t));
            var i = "1";
            Assert.Equal(1, ct.Get(i,null));
        }
    }
}
