///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml;

namespace com.espertech.esper.compat.collections
{
    public static class GenericExtensions
    {
        public static bool IsNullable(this Type t)
        {
            return Nullable.GetUnderlyingType(t) != null;
        }

        public static bool CanNotBeNull(this Type t)
        {
            return !CanBeNull(t);
        }
        
        public static bool CanBeNull(this Type t)
        {
            if (t.IsNullable()) {
                return true;
            }
            
            return !t.IsValueType;
        }
        
        public static bool IsAssignableIndex(this Type t)
        {
            if (t == null)
                return false;
            if (t.IsArray)
                return false;
            if (t == typeof(XmlNode))
                return false;
            if (t == typeof(string))
                return true;
            if (t.IsGenericList())
                return true;
            if (t.IsImplementsInterface(typeof(System.Collections.IList)))
                return true;
            if (t.IsGenericEnumerable())
                return false;
            if (t.IsImplementsInterface(typeof(System.Collections.IEnumerable)))
                return false;
            if (t.IsArray)
                return false;

            return false;
        }

        public static Type GetIndexType(this Type t)
        {
            if (t == null)
                return null;
            if (t.IsArray)
                return t.GetElementType();
            if (t == typeof(XmlNode))
                return null;
            if (t == typeof(string))
                return typeof(char);
            if (t.IsGenericList())
                return FindGenericInterface(t, typeof (IList<>)).GetGenericArguments()[0];
            if (t.IsImplementsInterface(typeof(System.Collections.IList)))
                return typeof(object);
            if (t.IsGenericDictionary())
                return null;
            if (t.IsGenericEnumerable())
                return FindGenericInterface(t, typeof(IEnumerable<>)).GetGenericArguments()[0];
            if (t.IsImplementsInterface(typeof(System.Collections.IEnumerable)))
                return typeof(object);

            return null;
        }

        public static bool IsIndexed(this Type t)
        {
            if (t == null)
                return false;
            if (t.IsArray)
                return true;
            if (t == typeof(XmlNode))
                return false;
            if (t == typeof(string))
                return true;
            if (t.IsGenericList())
                return true;
            if (t.IsImplementsInterface(typeof(System.Collections.IList)))
                return true;
            if (t.IsGenericDictionary())
                return false;
            if (t.IsGenericEnumerable())
                return true;
            if (t.IsImplementsInterface(typeof(System.Collections.IEnumerable)))
                return true;

            return false;
        }

        public static bool IsMapped(this Type t)
        {
            if (t == null)
                return false;
            if (t.IsGenericDictionary())
                return true;
            return false;
        }

        public static Type GetDictionaryKeyType(this Type t)
        {
            if (t == null)
                return null;
            if (t.IsGenericDictionary())
                return FindGenericDictionaryInterface(t).GetGenericArguments()[0];
            return null;
        }

        public static Type GetDictionaryValueType(this Type t)
        {
            if (t == null)
                return null;
            if (t.IsGenericDictionary())
                return FindGenericDictionaryInterface(t).GetGenericArguments()[1];
            return null;
        }
        
        public static bool IsGenericDictionary(this Type t)
        {
            var dictType = FindGenericInterface(t, typeof(IDictionary<,>));
            return (dictType != null);
        }

        public static bool IsNotGenericDictionary(this Type t)
        {
            return !IsGenericDictionary(t);
        }

        private static readonly IDictionary<Type, bool> StringDictionaryResultCache = 
            new Dictionary<Type, bool>();

        public static bool IsGenericStringDictionary(this Type t)
        {
            if (t.IsValueType || (t == typeof(object)))
                return false;

            if (StringDictionaryResultCache.TryGetValue(t, out var result))
                return result;

            var dictType = FindGenericInterface(t, typeof (IDictionary<,>));
            if (dictType != null) {
                result = dictType.GetGenericArguments()[0] == typeof(string);
                StringDictionaryResultCache[t] = result;
                return result;
            }

            StringDictionaryResultCache[t] = false;
            return false;
        }

        public static bool IsGenericObjectDictionary(this Type t)
        {
            if (t.IsValueType || (t == typeof(object)))
                return false;

            var dictType = FindGenericInterface(t, typeof (IDictionary<,>));
            if (dictType != null) {
                return dictType.GetGenericArguments()[0] == typeof(object);
            }

            return false;
        }
        
        public static bool IsGenericSet(this Type t)
        {
            return FindGenericInterface(t, typeof (ISet<>)) != null;
        }

        public static bool IsGenericCollection(this Type t)
        {
            return FindGenericInterface(t, typeof (ICollection<>)) != null;
        }

        public static bool IsGenericList(this Type t)
        {
            return FindGenericInterface(t, typeof(IList<>)) != null;
        }

        public static Type GetListType(this Type t)
        {
            if (t == null)
                return null;
            if (t.IsGenericList())
                return FindGenericListInterface(t).GetGenericArguments()[0];
            return null;
        }

        
        public static bool IsGenericEnumerable(this Type t)
        {
            return FindGenericInterface(t, typeof(IEnumerable<>)) != null;
        }

        public static bool IsGenericEnumerator(this Type t)
        {
            return FindGenericInterface(t, typeof(IEnumerator<>)) != null;
        }

        public static Type FindGenericListInterface(this Type t)
        {
            return FindGenericInterface(t, typeof (IList<>));
        }

        public static Type FindGenericInterface(this Type t, Type baseInterface)
        {
            if (t.IsInterface && t.IsGenericType)
            {
                var genericType = t.GetGenericTypeDefinition();
                if (genericType == baseInterface)
                {
                    return t;
                }
            }

            foreach (var iface in t.GetInterfaces())
            {
                var subFind = FindGenericInterface(iface, baseInterface);
                if (subFind != null)
                {
                    return subFind;
                }
            }

            return null;
        }

        public static Type FindGenericDictionaryInterface(this Type t)
        {
            return t.FindGenericInterface(typeof (IDictionary<,>));
        }

        public static Type FindGenericCollectionInterface(this Type t)
        {
            return t.FindGenericInterface(typeof (ICollection<>));
        }
        
        public static Type FindGenericEnumerationInterface(this Type t)
        {
            return t.FindGenericInterface(typeof (IEnumerable<>));
        }

        public static object FetchGenericKeyedValue<TV>(this object o, string key)
        {
            var dictionary = o as IDictionary<string, TV>;
            return dictionary.Get(key);
        }

        private static readonly IDictionary<Type, Func<object, string, object>> TypeFetchTable =
            new Dictionary<Type, Func<object, string, object>>();

        public static object FetchKeyedValue(object o, string key, object defaultValue)
        {
            var type = o.GetType();

            Func<object, string, object> typeFetchFunc;

            lock( TypeFetchTable ) {
                typeFetchFunc = TypeFetchTable.Get(type);
                if ( typeFetchFunc == null ) {
                    var genericDictionaryType = FindGenericDictionaryInterface(o.GetType());
                    if (genericDictionaryType == null)
                    {
                        typeFetchFunc = ((p1, p2) => defaultValue);
                    } 
                    else
                    {
                        var genericMethod = typeof(GenericExtensions).GetMethod("FetchGenericKeyedValue");
                        var specificMethod = genericMethod.MakeGenericMethod(
                            genericDictionaryType.GetGenericArguments()[1]);
                        typeFetchFunc = ((p1, p2) => specificMethod.Invoke(null, new[] {p1, p2}));
                    }

                    TypeFetchTable[type] = typeFetchFunc;
                }
            }

            return typeFetchFunc.Invoke(o, key);
        }

        private static readonly Dictionary<Type, Func<object, object, bool>> CollectionAccessorTable =
            new Dictionary<Type, Func<object, object, bool>>();

        public static Type GetCollectionItemType(this Type t)
        {
            if (t == null)
                return null;
            if (t.IsGenericCollection())
                return FindGenericCollectionInterface(t).GetGenericArguments()[0];
            return null;
        }
        
        public static Func<object, object, bool> CreateCollectionContainsAccessor(this Type t)
        {
            lock( CollectionAccessorTable ) {
                if (!CollectionAccessorTable.TryGetValue(t, out var accessor)) {
                    // Scan the object and make sure that it : the collection interface
                    var rawInterface = FindGenericInterface(t, typeof(ICollection<>));
                    if (rawInterface == null) {
                        accessor = null;
                    } else {
                        var containMethod = rawInterface.GetMethod("Contains");
                        var exprParam1 = Expression.Parameter(t, "collection");
                        var exprParam2 = Expression.Parameter(typeof (object), "obj");
                        var exprMethod = Expression.Call(containMethod, exprParam1, exprParam2);
                        var exprLambda = Expression.Lambda<Func<object, object, bool>>(
                            exprMethod,
                            exprParam1,
                            exprParam2);
                        accessor = exprLambda.Compile();
                    }

                    CollectionAccessorTable[t] = accessor;
                }

                return accessor;
            }

        }
        
        
        public static bool IsMappedType(Type type)
        {
            if (type == null)
                return false;
            if (type.IsGenericStringDictionary())
                return true;

            return false;
        }

        
        public static bool IsIndexedType(Type type)
        {
            if (type == null)
                return false;
            if (type == typeof(string))
                return true;
            if (type == typeof(XmlNode))
                return false;
            if (type.IsArray)
                return true;
            if (type.IsGenericDictionary())
                return false;
            if (type.IsGenericList())
                return true;
            if (type.IsGenericEnumerable() || type.IsImplementsInterface(typeof(System.Collections.IEnumerable)))
                return true;

            return false;
        }

        public static Type GetComponentType(Type type)
        {
            if (type == null)
                return null;
            if (type == typeof(string))
                return typeof(char);
            if (type == typeof(XmlNode))
                return null;
            if (type.IsArray)
                return type.GetElementType();
            if (type.IsGenericDictionary())
                return null;
            if (type.IsGenericList())
                return type.GetCollectionItemType();
            if (type.IsGenericEnumerable())
                return type.GetCollectionItemType();
            if (type.IsImplementsInterface(typeof(System.Collections.IEnumerable)))
                return typeof(object);

            return null;
        }

        public static bool IsGenericKeyValuePair(this Type t)
        {
            return (t.IsGenericType) &&
                   (t.GetGenericTypeDefinition() == typeof(KeyValuePair<,>).GetGenericTypeDefinition());
        }
    }
}
