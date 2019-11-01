using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using XLR8.CGLib;

namespace com.espertech.esper.compat.collections
{
    public static class DictionaryExtensions
    {
        public static IDictionary<TK, TV> AsSyncDictionary<TK, TV>(this IDictionary<TK, TV> dictionary)
            where TK : class
        {
            if (dictionary is ConcurrentDictionary<TK, TV>)
            {
                return dictionary;
            }

            return new ConcurrentDictionary<TK, TV>(dictionary);
        }

        public static IDictionary<TK, TV> WithDebugSupport<TK, TV>(this IDictionary<TK, TV> dictionary)
            where TK : class
        {
            if (dictionary is DebugDictionary<TK, TV>)
            {
                return dictionary;
            }

            return new DebugDictionary<TK, TV>(dictionary);
        }


        public static IDictionary<TK, TV> WithNullKeySupport<TK, TV>(this IDictionary<TK, TV> dictionary)
            where TK : class
        {
            if (dictionary is NullableDictionary<TK, TV>)
            {
                return dictionary;
            }

            return new NullableDictionary<TK, TV>(dictionary);
        }

        public static IDictionary<TK?, TV> WithValueTypeSupport<TK, TV>(this IDictionary<TK?, TV> dictionary)
            where TK : struct
        {
            if (dictionary is NullableValueTypeDictionary<TK, TV>)
            {
                return dictionary;
            }

            return new NullableValueTypeDictionary<TK, TV>(dictionary);
        }

        private static readonly IDictionary<Type, FastMethod> SafeDictionaryMethodTable =
            new Dictionary<Type, FastMethod>(ReferenceEqualityComparer<Type>.Default);

        public static IDictionary<TK, TV> WithSafeSupport<TK, TV>(this IDictionary<TK, TV> dictionary)
        {
            FastMethod fastMethod;

            var type = typeof(IDictionary<TK, TV>);

            if (typeof(TK).IsNullable())
            {
                var keyType = Nullable.GetUnderlyingType(typeof (TK));
                
                lock (SafeDictionaryMethodTable)
                {
                    if (!SafeDictionaryMethodTable.TryGetValue(type, out fastMethod))
                    {
                        var slowMethod = typeof(DictionaryExtensions)
                            .GetMethod("WithValueTypeSupport")
                            .MakeGenericMethod(keyType, typeof(TV));
                        fastMethod = FastClass.CreateMethod(slowMethod);
                        SafeDictionaryMethodTable[type] = fastMethod;
                    }
                }

                return (IDictionary<TK, TV>)fastMethod.InvokeStatic(dictionary, null);
            }

            if (typeof(TK).IsValueType)
                return dictionary; // this dictionary does not have the possibility of null entries

            lock (SafeDictionaryMethodTable)
            {
                if (!SafeDictionaryMethodTable.TryGetValue(type, out fastMethod))
                {
                    var slowMethod = typeof (DictionaryExtensions)
                        .GetMethod("WithNullKeySupport")
                        .MakeGenericMethod(typeof (TK), typeof (TV));
                    fastMethod = FastClass.CreateMethod(slowMethod);
                    SafeDictionaryMethodTable[type] = fastMethod;
                }
            }

            return (IDictionary<TK,TV>) fastMethod.InvokeStatic(dictionary, null);
        }

        /// <summary>
        /// Transforms the specified dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns></returns>
        public static IDictionary<TK, TVExt> TransformLeft<TK, TVInt, TVExt>(
            this IDictionary<TK, TVInt> dictionary)
            where TVExt : TVInt
        {
            return new TransformDictionary<TK, TVExt, TK, TVInt>(
                dictionary,
                ki => ki,
                ke => ke,
                vi => (TVExt) vi,
                ve => (TVInt) ve);
        }

        /// <summary>
        /// Transforms the specified dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns></returns>
        public static IDictionary<TK, TVExt> TransformRight<TK, TVInt, TVExt>(
            this IDictionary<TK, TVInt> dictionary)
            where TVInt : TVExt
        {
            return new TransformDictionary<TK, TVExt, TK, TVInt>(
                dictionary,
                ki => ki,
                ke => ke,
                vi => (TVExt) vi,
                ve => (TVInt) ve);
        }

        /// <summary>
        /// Transforms the specified dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns></returns>
        public static IDictionary<TKExt, TV> TransformUp<TKInt, TKExt, TV>(
            this IDictionary<TKInt, TV> dictionary)
            where TKExt : TKInt
        {
            return new TransformDictionary<TKExt, TV, TKInt, TV>(
                dictionary,
                ki => (TKExt) ki,
                ke => (TKInt) ke,
                vi => vi,
                ve => ve);
        }

        public static IDictionary<TKExt, TV> TransformDown<TKInt, TKExt, TV>(
            this IDictionary<TKInt, TV> dictionary)
            where TKInt : TKExt
        {
            return new TransformDictionary<TKExt, TV, TKInt, TV>(
                dictionary,
                ki => (TKExt) ki,
                ke => (TKInt) ke,
                vi => vi,
                ve => ve);
        }

        /// <summary>
        /// Transforms the specified dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="keyIntToExt">The key out.</param>
        /// <param name="keyExtToInt">The key in.</param>
        /// <returns></returns>
        public static IDictionary<TKExt, TV> Transform<TKExt, TKInt, TV>(
            this IDictionary<TKInt, TV> dictionary,
            Func<TKInt, TKExt> keyIntToExt,
            Func<TKExt, TKInt> keyExtToInt) {
            return new TransformDictionary<TKExt, TV, TKInt, TV>(
                dictionary,
                keyIntToExt,
                keyExtToInt,
                vi => vi,
                ve => ve);
        }

        /// <summary>
        /// Transforms the specified dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="keyIntToExt">The key out.</param>
        /// <param name="valueIntToExt">The value out.</param>
        /// <param name="keyExtToInt">The key in.</param>
        /// <param name="valueExtToInt">The value in.</param>
        /// <returns></returns>
        public static IDictionary<TKExt, TVExt> Transform<TKExt, TVExt, TKInt, TVInt>(
            this IDictionary<TKInt, TVInt> dictionary,
            Func<TKInt, TKExt> keyIntToExt,
            Func<TVInt, TVExt> valueIntToExt,
            Func<TKExt, TKInt> keyExtToInt,
            Func<TVExt, TVInt> valueExtToInt)
        {
            return new TransformDictionary<TKExt, TVExt, TKInt, TVInt>(
                dictionary,
                keyIntToExt,
                keyExtToInt,
                valueIntToExt,
                valueExtToInt);
        }

        /// <summary>
        /// Removes the item from the dictionary that is associated with
        /// the specified key.  Returns the value that was found at that
        /// location and removed or the defaultValue.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">Search key into the dictionary</param>
        /// <param name="value">The value removed from the dictionary (if found).</param>
        /// <returns></returns>

        public static bool TryRemove<K, V>(this IDictionary<K, V> dictionary, K key, out V value)
        {
            if (dictionary.TryGetValue(key, out value)) {
                return dictionary.Remove(key);
            }

            return false;
        }

        /// <summary>
        /// Removes the item from the dictionary that is associated with
        /// the specified key.  The item if found is returned; if not,
        /// default(V) is returned.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>

        public static V Delete<K, V>(this IDictionary<K, V> dictionary, K key)
        {
            return dictionary.TryRemove(key, out V tempItem)
                    ? tempItem
                    : default(V);
        }

        /// <summary>
        /// Fetches the value associated with the specified key.
        /// If no value can be found, then the defaultValue is
        /// returned.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>

        public static V Get<K, V>(this IDictionary<K, V> dictionary, K key, V defaultValue = default(V))
        {
            if (!dictionary.TryGetValue(key, out V returnValue))
            {
                returnValue = defaultValue;
            }

            return returnValue;
        }

        public static V? GetBoxed<K, V>(this IDictionary<K, V> dictionary, K key)
            where V : struct
        {
            if (!dictionary.TryGetValue(key, out V returnValue)) {
                return null;
            }

            return returnValue;
        }

        /// <summary>
        /// Sets the given key in the dictionary.  If the key
        /// already exists, then it is remapped to thenew value.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>

        public static void Put<K, V>(this IDictionary<K, V> dictionary, K key, V value = default(V))
        {
            dictionary[key] = value;
        }

        /// <summary>
        /// Sets the given key in the dictionary.  If the key
        /// already exists, then it is remapped to the new value.
        /// If a value was previously mapped it is returned.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>

        public static V Push<K, V>(this IDictionary<K, V> dictionary, K key, V value)
        {
            dictionary.TryGetValue(key, out V temp);
            dictionary[key] = value;
            return temp;
        }

        public static bool TryPush<K, V>(this IDictionary<K, V> dictionary, K key, V value, out V outPreviousValue)
        {
            bool rvalue = dictionary.TryGetValue(key, out outPreviousValue);
            dictionary[key] = value;
            return rvalue;
        }

        public static V PutIfAbsent<K, V>(this IDictionary<K, V> dictionary, K key, V value)
        {
            if (!dictionary.TryGetValue(key, out V temp)) {
                dictionary[key] = value;
            }
            return temp;
        }

        public static bool TryPutIfAbsent<K, V>(this IDictionary<K, V> dictionary, K key, V value, out V outValue)
        {
            if (!dictionary.TryGetValue(key, out outValue))
            {
                dictionary[key] = value;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Puts all values from the source dictionary into
        /// this dictionary.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="source">The source.</param>

        public static void PutAll<K, V>(this IDictionary<K, V> dictionary, IEnumerable<KeyValuePair<K, V>> source)
        {
            foreach (KeyValuePair<K, V> kvPair in source)
            {
                dictionary[kvPair.Key] = kvPair.Value;
            }
        }

        /// <summary>
        /// Puts all values from the source dictionary into this dictionary.  This variation
        /// of the method allows the values to be transformed from one type to another.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="source">The source.</param>
        /// <param name="transformer">The transformer.</param>

        public static void PutAll<K, V, T>(this IDictionary<K, V> dictionary, IEnumerable<KeyValuePair<K, T>> source, Transformer<T, V> transformer)
        {
            foreach (KeyValuePair<K, T> kvPair in source)
            {
                dictionary[kvPair.Key] = transformer(kvPair.Value);
            }
        }

        /// <summary>
        /// Returns the first value in the enumeration of values
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns></returns>

        public static V FirstValue<K, V>(this IDictionary<K, V> dictionary)
        {
            IEnumerator<KeyValuePair<K, V>> kvPairEnum = dictionary.GetEnumerator();
            kvPairEnum.MoveNext();
            return kvPairEnum.Current.Value;
        }

        public static SortedDictionary<TK, TV> Invert<TK, TV>(this SortedDictionary<TK, TV> dictionary)
        {
            var comparer = dictionary.Comparer;
            var inverted = new StandardComparer<TK>(
                (a, b) => -comparer.Compare(a, b));
            return new SortedDictionary<TK, TV>(dictionary, inverted);
        }

        public static IDictionary<K, V> AsBasicDictionary<K, V>(this object anyEntity)
        {
            var asRawDictionary = anyEntity as Dictionary<K, V>;
            if (asRawDictionary != null)
                return asRawDictionary;

            var asFuzzyDictionary = anyEntity as IDictionary<K, V>;
            if (asFuzzyDictionary != null)
                return new Dictionary<K, V>(asFuzzyDictionary);

            throw new ArgumentException("unable to translate dictionary", "anyEntity");
        }
    }
}
