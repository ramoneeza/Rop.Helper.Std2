using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Rop.CacheDictionary;

namespace Rop.ObjectActivator
{
    public delegate T ObjectActivator<T>(params object[] args);
    public delegate object ObjectActivator(params object[] args);
    public static class ObjectActivatorFactory
    {
        private static ObjectActivator IntGetActivator(Type type, ConstructorInfo ctor)
        {
            ParameterInfo[] paramsInfo = ctor.GetParameters();
            //create a single param of type object[]
            ParameterExpression param = Expression.Parameter(typeof(object[]), "args");

            Expression[] argsExp = new Expression[paramsInfo.Length];

            //pick each arg from the params array 
            //and create a typed expression of them
            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;

                Expression paramAccessorExp = Expression.ArrayIndex(param, index);

                Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the
            //ctor with the args we just created
            NewExpression newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            LambdaExpression lambda = Expression.Lambda(typeof(ObjectActivator), newExp, param);

            //compile it
            ObjectActivator compiled = (ObjectActivator) lambda.Compile();
            return compiled;
        }
        private class CacheObjectActivator : CacheTypeAndArgDictionary<ObjectActivator,Type[]>
        {
            public CacheObjectActivator() : base(InternalFactory)
            {
            }

            private static ObjectActivator InternalFactory(Type type, Type[] args)
            {
                var types = args ?? Type.EmptyTypes;
                var ctor = type.GetConstructor(types);
                if (ctor == null) return null;
                return IntGetActivator(type, ctor);
            }

            public ObjectActivator Factory<A>(Type[] types) => base.Get((typeof(A), types));
        }

        private static readonly CacheObjectActivator _cacheObjectActivator=new CacheObjectActivator();

        public static T Factory<T>(object[] args,Type[] types)
        {
            var a = _cacheObjectActivator.Factory<T>(types);
            return (T)a?.Invoke(args);
        }
        public static T Factory<T>(Type realtype,object[] args,Type[] types)
        {
            var a = Get(realtype, types);
            return (T)a?.Invoke(args);
        }
       
        public static object Factory(Type realtype,object[] args,Type[] types)
        {
            var a = Get(realtype, types);
            return a?.Invoke(args);
        }

        public static T FactoryExact<T>(params object[] args)
        {
            return Factory<T>(typeof(T), args);
        }

        public static object FactoryGeneric(Type genericdefinition, Type[] subTypes, object[] args, Type[] types)
        {
            var realtype = genericdefinition.MakeGenericType(subTypes);
            var a = Get(realtype, types);
            return a?.Invoke(args);
        }
        public static object FactoryGeneric(Type genericdefinition, Type subType, object[] args, Type[] types)
        {
            var realtype = genericdefinition.MakeGenericType(subType);
            var a = Get(realtype, types);
            return a?.Invoke(args);
        }

        public static object FactoryGeneric(Type genericdefinition, Type[] subTypes, object[] args)
        {
            Debug.Assert(genericdefinition.IsGenericTypeDefinition);
            var types = args?.Select(a => a.GetType()).ToArray();
            return FactoryGeneric(genericdefinition, subTypes, args, types);
        }
        public static object FactoryGeneric(Type genericdefinition, Type subType, object[] args)
        {
            Debug.Assert(genericdefinition.IsGenericTypeDefinition);
            var types = args?.Select(a => a.GetType()).ToArray();
            return FactoryGeneric(genericdefinition, subType, args, types);
        }



        public static T Factory<T>(Type realtype,params object[] args)
        {
            return (T)Factory(realtype, args);
        }
        public static object Factory(Type realtype,params object[] args)
        {
            if (realtype.IsInterface) throw new ArgumentNullException(nameof(realtype),"realtype can't be an interface");
            if (args?.Any(a=>a==null)??false) throw new ArgumentNullException(nameof(args),"Can't automatic factory object with null parameter");
            var types = args?.Select(a => a.GetType()).ToArray();
            return Factory(realtype, args, types);
        }

        public static ObjectActivator Get<T>(params Type[] types)=>_cacheObjectActivator.Factory<T>(types);
        public static ObjectActivator Get(Type typeobject,params Type[] types)=>_cacheObjectActivator.Get((typeobject,types));
    }
    
    public class GenericFactory
    {
        public Type GenericDefinitionType { get; }
        public object Factory<T>(params object[] arguments) => Factory(typeof(T), arguments);
        public object Factory(Type subtype, params object[] arguments)
        {
            return ObjectActivatorFactory.FactoryGeneric(GenericDefinitionType,subtype,arguments);
        }


        public GenericFactory(Type genericDefinitionType)
        {
            Debug.Assert(genericDefinitionType.IsGenericTypeDefinition);
            GenericDefinitionType = genericDefinitionType;
        }
    }
}
