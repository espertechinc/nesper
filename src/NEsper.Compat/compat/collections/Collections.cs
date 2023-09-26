///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace com.espertech.esper.compat.collections
{
    /// <summary>
    /// Provides additional functions that are useful when operating on
    /// collections.
    /// </summary>

	public static class Collections
	{
        public static readonly IDictionary<string, object> EmptyDataMap =
            new Dictionary<string, object>();

        public static void SortInPlace<T>(this IList<T> list)
        {
            if (list is List<T>)
            {
                ((List<T>) list).Sort();
            }
            else if (list is T[])
            {
                System.Array.Sort((T[]) list);
            }
            else
            {
                var replList = new List<T>(list);
                replList.Sort();
                for (var ii = list.Count - 1; ii >= 0; ii--)
                    list[ii] = replList[ii];
            }
        }

        public static void SortInPlace<T>(this IList<T> list, IComparer<T> comparer)
        {
            if (list is List<T>)
            {
                ((List<T>)list).Sort(comparer);
            }
            else if (list is T[])
            {
                System.Array.Sort((T[])list, comparer);
            }
            else
            {
                var replList = new List<T>(list);
                replList.Sort(comparer);
                for (var ii = list.Count - 1; ii >= 0; ii--)
                    list[ii] = replList[ii];
            }
        }

        public static void SortInPlace<T>(this IList<T> list, Func<T, T, int> comparerFunc)
        {
            SortInPlace(list, new ProxyComparer<T>(comparerFunc));
        }

        public static T[] Array<T>(params T[] items)
        {
            return items;
        }

        /// <summary>
        /// Creates a set from the inputs.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static ISet<T> Set<T>(params T[] items)
	    {
	        return new HashSet<T>(items);
	    }

        /// <summary>
        /// Instas the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static IList<T> List<T>(params T[] items)
        {
            return items;
        }

        /// <summary>
        /// Creates a singleton list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static IList<T> SingletonList<T>(T item)
        {
            return new T[] {item};
        }

        /// <summary>
        /// Creates a singleton set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static ISet<T> SingletonSet<T>(T item)
        {
            return new HashSet<T>
            {
                item
            };
        }

        public static IDictionary<string, object> GetEmptyDataMap()
        {
            return EmptyDataMap;
        }

        /// <summary>
        /// Returns an empty IDictionary for type K,V
        /// </summary>
        /// <returns></returns>
        public static IDictionary<K,V> GetEmptyMap<K,V>()
        {
            return EmptyDictionary<K, V>.Instance;
        }

        /// <summary>
        /// Returns an empty IList for type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IList<T> GetEmptyList<T>()
        {
            return EmptyList<T>.Instance;
        }

        /// <summary>
        /// Returns an empty collection for type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ISet<T> GetEmptySet<T>()
        {
            return EmptySet<T>.Instance;
        }

        public static SortedSet<T> GetEmptySortedSet<T>()
        {
            return new SortedSet<T>();
        }

        /// <summary>
        /// Returns an empty enumerator for type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerator<T> GetEmptyEnumerator<T>()
        {
            return EmptyList<T>.Instance.GetEnumerator();
        }

        /// <summary>
        /// Compares two collections of objects.  The objects must share the same generic
        /// parameter, but can be of different collections.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseObj"></param>
        /// <param name="compObj"></param>

        public static bool AreEqual<T>(T[] baseObj, T[] compObj)
        {
            var baseIsNull = baseObj == null;
            var compIsNull = compObj == null;

            if (baseIsNull && compIsNull)
            {
                return true;
            }

            if (baseIsNull || compIsNull)
            {
                return false;
            }

            if (baseObj.Length != compObj.Length)
            {
                return false;
            }

            var objCount = baseObj.Length;
            for( var ii = 0 ; ii < objCount ; ii++ )
            {
                if (!Equals(baseObj[ii], compObj[ii]))
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Compares two collections of objects.  The objects must share the same generic
        /// parameter, but can be of different collections.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseObj"></param>
        /// <param name="compObj"></param>

        public static bool AreEqual<T>(ICollection<T> baseObj, ICollection<T> compObj)
        {
            var baseIsNull = baseObj == null;
            var compIsNull = compObj == null;

            if ( baseIsNull && compIsNull )
            {
                return true;
            }

            if ( baseIsNull || compIsNull )
            {
                return false;
            }

            if (baseObj.Count != compObj.Count)
            {
                return false;
            }

            var baseEnum = baseObj.GetEnumerator();
            var compEnum = compObj.GetEnumerator();

            return AreEqual(baseEnum, compEnum);
        }
        
        /// <summary>
        /// Compares two collections of objects.  The objects must share the same generic
        /// parameter, but can be of different collections.
        /// </summary>
        /// <param name="baseEnum">The base enumerator.</param>
        /// <param name="compEnum">The comp enumerator.</param>
        /// <returns></returns>
        /// <typeparam name="T"></typeparam>

        public static bool AreEqual<T>(IEnumerator<T> baseEnum, IEnumerator<T> compEnum)
        {
            for (; ;)
            {
#if true
                var bitMask =
                    (baseEnum.MoveNext() ? 2 : 0) |
                    (compEnum.MoveNext() ? 1 : 0);
                switch(bitMask)
                {
                    case 0:
                        return true;
                    case 3:
                        if (!Equals(baseEnum.Current, compEnum.Current))
                        {
                            return false;
                        }
                        break;
                    default:
                        return false;
                }
#else
                bool baseTest = baseEnum.MoveNext();
                bool compTest = compEnum.MoveNext();

                if (baseTest && compTest)
                {
                    if (!Object.Equals(baseEnum.Current, compEnum.Current))
                    {
                        return false;
                    }
                }
                else if (!baseTest && !compTest)
                {
                    return true;
                }
                else
                {
                    // Both baseTest and compTest should both be returning
                    // false at this point.  Failure to do so indicates that
                    // one enumerator is returning more results than the
                    // other.

                    return false;
                }
#endif
            }
        }

		/// <summary>
		/// Converts all of the items in source to an array.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		
		public static T[] ToArray<T>( ICollection<T> source )
		{
            if ( source is T[] ) {
                return ((T[]) source);
            } else if ( source is List<T> ) {
				return ((List<T>) source).ToArray() ;
			}
			
			var array = new T[source.Count] ;
			source.CopyTo( array, 0 ) ;
			return array ;
		}

        /// <summary>
        /// Advances the enumerator and returns the next item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumObj"></param>
        /// <returns></returns>

        public static T Next<T>(IEnumerator<T> enumObj)
        {
            enumObj.MoveNext();
            return enumObj.Current;
        }
        
        /// <summary>
        /// Shuffles the list.
        /// </summary>
        /// <param name="list"></param>
        
        public static void Shuffle<T>( IList<T> list )
        {
        	Shuffle( list, list.Count * 2, new Random() ) ;
        }
        
        /// <summary>
        /// Shuffles the list.  User supplies the randomizer.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="random"></param>
        
        public static void Shuffle<T>( IList<T> list, Random random )
        {
        	Shuffle( list, list.Count * 2, random ) ;
        }
        
        /// <summary>
        /// Shuffles the list.  User supplies the randomizer.  Performs
        /// at least iteration swaps.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="iterations"></param>
        /// <param name="random"></param>
        
        public static void Shuffle<T>( IList<T> list, int iterations, Random random )
        {
        	var count = list.Count ;
        	
        	for( var ii = iterations ; ii >= 0 ; ii-- )
        	{
        		var index1 = random.Next( count ) ;
        		var index2 = random.Next( count ) ;
        		var temp = list[index1] ;
        		list[index1] = list[index2] ;
        		list[index2] = temp ;
        	}        	
        }

        public static int GetHashCode<T>(IEnumerable<T> enumerable) where T : class
        {
            return enumerable.Aggregate(0, (current, temp) => current ^ (temp != null ? temp.GetHashCode() : 0));
        }

#if false
        public static int GetHashCode( Array a )
        {
            int hashCode = 0;
            for( int ii = a.Length - 1 ; ii >= 0 ; ii-- ) {
                Object temp = a.GetValue(ii);
                int tempHash = temp != null ? temp.GetHashCode() : 0;
                hashCode = 31*hashCode + tempHash;
            }

            return hashCode;
        }
#endif

        public static IDictionary<string, object> SingletonDataMap(string key, object value)
        {
            return SingletonMap(key, value);
        }

        public static IDictionary<TK,TV> SingletonMap<TK,TV>(TK key, TV value)
        {
            var dictionary = new Dictionary<TK, TV>();
            dictionary[key] = value;
            return dictionary;
        }

	    public static IList<T> ReadonlyList<T>(IList<T> sourceList)
	    {
	        return new ReadOnlyList<T>(sourceList.ToArray());
	    }
        
        /// <summary>
        /// Returns true if the object provided matches the type required for the collection and exists in the collection.  One
        /// of the truly nice things about compilation is that it allows widening and narrowing to work properly.  However, it
        /// can affect the Contains method in ways that are not anticipated.  For example, an int will be implicitly upcast
        /// to a long, allowing an integer to be found in a collection of longs.  This method enforces type consistency by
        /// taking in an object, thus avoiding implicit conversion during compilation.  Jury is still out, as some (myself
        /// included) may personally prefer to have the implicit conversion.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool CheckedContains<T>(
            this ICollection<T> collection,
            object value)
        {
            if (value is T valueT) {
                var result = collection.Contains(valueT);
                return result;
            }

            return false;
        }
        /// <summary>
        /// Returns true if the object provided matches the type required for the collection and exists as a key in the dictionary.
        /// One of the truly nice things about compilation is that it allows widening and narrowing to work properly.  However, it
        /// can affect the Contains method in ways that are not anticipated.  For example, an int will be implicitly upcast
        /// to a long, allowing an integer to be found in a collection of longs.  This method enforces type consistency by
        /// taking in an object, thus avoiding implicit conversion during compilation.  Jury is still out, as some (myself
        /// included) may personally prefer to have the implicit conversion.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <typeparam name="TK">type for key</typeparam>
        /// <typeparam name="TV">type for value</typeparam>
        /// <returns></returns>
        public static bool CheckedContainsKey<TK,TV>(
            this IDictionary<TK,TV> dictionary,
            object key)
        {
            if (key is TK keyT) {
                var result = dictionary.ContainsKey(keyT);
                return result;
            }

            return false;
        }

        /// <summary>
        /// Returns a collection type with the given collection type.
        /// </summary>
        /// <param name="collectionType"></param>
        /// <returns></returns>
        public static Type CollectionOf(Type collectionType)
        {
            return typeof(ICollection<>).MakeGenericType(collectionType);
        }
    }
}
