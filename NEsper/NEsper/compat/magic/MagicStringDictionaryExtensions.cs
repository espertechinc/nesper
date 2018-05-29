using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using com.espertech.esper.compat.threading;

namespace com.espertech.esper.compat.magic
{
    public static class MagicStringDictionaryExtensions
    {
        private static readonly ILockable FunctionalAtomTableLock = new MonitorSpinLock(60000);

        private static readonly IDictionary<Type, FunctionalAtom> FunctionalAtomTable =
            new Dictionary<Type, FunctionalAtom>();

        public static object GetInternal<V>(IDictionary<string, V> dictionary, string key)
        {
            return dictionary.TryGetValue(key, out var value) ? (object) value : null;
        }

        private static FunctionalAtom CreateFunctionalAtom(Type dictionaryType)
        {
            var atom = new FunctionalAtom();
            var valueType = dictionaryType.GetGenericArguments()[1];

            // Create the Get functional lambda
            if (true) {
                var eParam1 = Expression.Parameter(typeof(object), "o");
                var eParam2 = Expression.Parameter(typeof(string), "k");
                var rStaticMethod = typeof(MagicStringDictionaryExtensions)
                    .GetMethod("GetInternal", BindingFlags.Public | BindingFlags.Static)
                    .MakeGenericMethod(valueType);
                var eDictionary = Expression.Convert(eParam1, dictionaryType);
                var eInvoke = Expression.Call(rStaticMethod, eDictionary, eParam2);
                var eLambda = Expression.Lambda<Func<object, string, object>>(eInvoke, eParam1, eParam2);
                atom.Get = eLambda.Compile();
            }

            // Create the ContainsKey functional lambda
            if (true) {
                var eParam1 = Expression.Parameter(typeof(object), "o");
                var eParam2 = Expression.Parameter(typeof(string), "k");
                var rMethod = dictionaryType.GetMethod("ContainsKey", BindingFlags.Public | BindingFlags.Instance);
                var eDictionary = Expression.Convert(eParam1, dictionaryType);
                var eInvoke = Expression.Call(eDictionary, rMethod, eParam2);
                var eLambda = Expression.Lambda<Func<object, string, bool>>(eInvoke, eParam1, eParam2);
                atom.ContainsKey = eLambda.Compile();
            }

            return atom;
        }

        private static FunctionalAtom GetFunctionalAtom(Type dictionaryType)
        {
            using (FunctionalAtomTableLock.Acquire()) {
                if (!FunctionalAtomTable.TryGetValue(dictionaryType, out var functionalAtom)) {
                    functionalAtom = CreateFunctionalAtom(dictionaryType);
                }

                return functionalAtom;
            }
        }

        public static object SDGet(this object dict, string key)
        {
            var functionalAtom = GetFunctionalAtom(dict.GetType());
            return functionalAtom.Get.Invoke(dict, key);
        }

        public static bool SDContainsKey(this object dict, string key)
        {
            var functionalAtom = GetFunctionalAtom(dict.GetType());
            return functionalAtom.ContainsKey.Invoke(dict, key);
        }

        struct FunctionalAtom
        {
            public Func<object, string, object> Get;
            public Func<object, string, bool> ContainsKey;
        }
    }
}
