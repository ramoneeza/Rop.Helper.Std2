using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Rop.CacheDictionary
{
    /// <summary>
    /// Create a CacheDictionary
    /// </summary>
    /// <typeparam name="R">Result Type</typeparam>
    /// <typeparam name="I">Interface or index(key) of dictionary</typeparam>
    /// <typeparam name="T">Type of parameter. T must have information about concrete I and concrete factorizarion of R</typeparam>
    public class CacheDictionary<R, I, T>
    {
        private readonly ConcurrentDictionary<I, R> _dic = new ConcurrentDictionary<I, R>();
        private Func<T, R> InternalFactory { get; set; }
        protected void SetInternalFactory(Func<T, R> newfactory) => InternalFactory = newfactory;

        protected R Get(T item, I i)
        {
            var res = _dic.GetOrAdd(i, key => InternalFactory(item));
            // **Concurrent equivalent
            //if (_dic.TryGetValue(i, out var res)) return res;
            //res = InternalFactory(item);
            //_dic[i] = res;
            // **
            return res;
        }

        public Func<T, I> GetIndex { get; } // Function Must be Deterministic

        public R Get(T item)
        {
            var i = GetIndex(item);
            return Get(item, i);
        }

        public CacheDictionary(Func<T, R> internalfactory, Func<T, I> getindex)
        {
            InternalFactory = internalfactory;
            GetIndex = getindex;
        }

        public IEnumerable<R> GetValues() => _dic.Values;
        public IEnumerable<KeyValuePair<I, R>> GetKeyValues() => _dic;
        public bool IsCached(I index) => _dic.ContainsKey(index);
        public bool UnCache(I index) => _dic.TryRemove(index, out var _);
        public void ClearAll() => _dic.Clear();
    }


    /// <summary>
    /// Create a CacheDictionary
    /// </summary>
    /// <typeparam name="R">Result Type</typeparam>
    /// <typeparam name="I">index(key) of dictionary</typeparam>
    public class CacheDictionary<R, I> : CacheDictionary<R, I, I>
    {
        public CacheDictionary(Func<I, R> internalfactory) : base(internalfactory, i => i)
        {
        }
    }
    
    /// <summary>
    /// Create a CacheDictionary. Index by Type
    /// </summary>
    /// <typeparam name="R"></typeparam>
    public class CacheTypeDictionary<R> : CacheDictionary<R, Type>
    {
        public CacheTypeDictionary(Func<Type, R> internalfactory) : base(internalfactory)
        {
        }
    }
    /// <summary>
    /// Create a CacheDictionary. Index by Type and one argument of type A
    /// </summary>
    /// <typeparam name="R">Result Type</typeparam>
    /// <typeparam name="A">Type of Argument</typeparam>
    public class CacheTypeAndArgDictionary<R, A> : CacheDictionary<R, (Type type, A arg)>
    {
        public CacheTypeAndArgDictionary(Func<Type, A, R> internalfactory) : base(t => internalfactory(t.type, t.arg))
        {
        }
        public R Get(Type type, A arg)
        {
            return Get((type, arg));
        }
    }

    /// <summary>
    /// Create a CacheDictionary. Index by Double Type
    /// </summary>
    /// <typeparam name="R">Result Type</typeparam>
    public class CacheDoubleTypeDictionary<R> : CacheDictionary<R, (Type t1, Type t2)>
    {
        public CacheDoubleTypeDictionary(Func<Type, Type, R> internalfactory) : base(tuple => internalfactory(tuple.t1, tuple.t2))
        {
        }
        public R Get(Type type, Type type2)
        {
            return Get((type, type2));
        }
    }
    /// <summary>
    /// Create a CacheDictionary. Index by Double Type and one argument
    /// </summary>
    /// <typeparam name="R">Result Type</typeparam>
    /// <typeparam name="A">Type of Argument</typeparam>
    public class CacheDoubleTypeAndArgumentDictionary<R, A> : CacheDictionary<R, (Type, Type, A)>
    {
        public CacheDoubleTypeAndArgumentDictionary(Func<Type, Type, A, R> internalfactory) : base(tuple => internalfactory(tuple.Item1, tuple.Item2, tuple.Item3))
        {
        }
        public R Get(Type type, Type type2, A arg)
        {
            return Get((type, type2, arg));
        }
    }
    /// <summary>
    /// Create a CacheDictionary. Index by Name and Arguments
    /// </summary>
    /// <typeparam name="R"></typeparam>
    public class CacheNameDictionary<R> : CacheDictionary<R, string, (string name, object[] arg)>
    {
        public virtual R Get(string name, object[] args) => base.Get((name, args));
        public CacheNameDictionary(Func<string, object[], R> internalfactory) : base(a => internalfactory(a.name, a.arg), i => i.name)
        {
        }
    }






}

