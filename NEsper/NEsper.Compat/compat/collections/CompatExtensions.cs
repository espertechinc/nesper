///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using com.espertech.esper.compat.attributes;
using com.espertech.esper.compat.magic;

namespace com.espertech.esper.compat.collections
{
    public static class CompatExtensions
    {
        public static LookaheadEnumerator<T> WithLookahead<T>(this IEnumerator<T> en)
        {
            return new LookaheadEnumerator<T>(en);
        }

        public static LookaheadEnumerator<T> EnumerateWithLookahead<T>(this IEnumerable<T> en)
        {
            return new LookaheadEnumerator<T>(en);
        }

        public static ICollection<TExt> TransformDowncast<TInt, TExt>(this ICollection<TInt> collection)
            where TExt : TInt
        {
            return TransformInto(
                collection,
                e => (TInt) e,
                i => (TExt) i);
        }

        public static ICollection<TExt> TransformUpcast<TInt, TExt>(this ICollection<TInt> collection)
            where TInt : TExt
        {
            return TransformInto(
                collection,
                e => (TInt) e,
                i => (TExt) i);
        }

        public static ICollection<TExt> TransformInto<TInt, TExt>(
            this ICollection<TInt> collection,
            Func<TExt, TInt> transformExtInt,
            Func<TInt, TExt> transformIntExt)
        {
            return new TransformCollection<TInt, TExt>(collection, transformExtInt, transformIntExt);
        }

        public static ReadOnlyCollection<T> AsReadOnlyCollection<T>(this ICollection<T> enumerable)
        {
            return new ReadOnlyCollection<T>(enumerable);
        }

        public static ReadOnlyList<T> AsReadOnlyList<T>(this IList<T> enumerable)
        {
            return new ReadOnlyList<T>(enumerable);
        }

        public static T[] MaterializeArray<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return new T[0];
            }

            return enumerable.ToArray();
        }

        public static T[] ToArrayOrNull<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return null;
            }

            var tempArray = enumerable.ToArray();
            if (tempArray.Length == 0)
            {
                return null;
            }

            return tempArray;
        }

        public static T[] ToListOrNull<T>(this IEnumerable<T> enumerable, bool nullWhenEmpty = true)
        {
            if (enumerable == null)
            {
                return null;
            }

            var tempArray = enumerable.ToArray();
            if (tempArray.Length == 0 && nullWhenEmpty)
            {
                return null;
            }

            return tempArray;
        }

        public static bool IsGreaterThan<T>(this IComparable<T> self, T that)
        {
            return self.CompareTo(that) > 0;
        }

        public static bool IsGreaterThanOrEqual<T>(this IComparable<T> self, T that)
        {
            return self.CompareTo(that) >= 0;
        }

        public static bool IsLessThan<T>(this IComparable<T> self, T that)
        {
            return self.CompareTo(that) < 0;
        }

        public static bool IsLessThanOrEqual<T>(this IComparable<T> self, T that)
        {
            return self.CompareTo(that) <= 0;
        }

        public static T[] ToPopArray<T>(this Stack<T> stack)
        {
            var array = new T[stack.Count];
            for (var ii = 0; stack.Count != 0; ii++)
            {
                array[ii] = stack.Pop();
            }

            return array;
        }

        public static bool Contains<T>(this IList<T> list, Predicate<T> predicate)
        {
            for (var ii = 0; ii < list.Count; ii++)
            {
                if (predicate.Invoke(list[ii]))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Contains<T>(this T[] array, T value)
        {
            return Array.IndexOf(array, value) != -1;
        }

        public static HashSet<T> AsHashSet<T>(params T[] values)
        {
            return new HashSet<T>(values);
        }

        public static HashSet<T> AsHashSet<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return null;
            }

            if (enumerable is HashSet<T> set)
            {
                return set;
            }

            return new HashSet<T>(enumerable);
        }

        public static SynchronizedCollection<T> AsSyncCollection<T>(this ICollection<T> unsyncCollection)
        {
            return new SynchronizedCollection<T>(unsyncCollection);
        }

        public static SynchronizedList<T> AsSyncList<T>(this IList<T> unsyncList)
        {
            return new SynchronizedList<T>(unsyncList);
        }

        public static SynchronizedSet<T> AsSyncSet<T>(this ISet<T> unsyncSet)
        {
            return new SynchronizedSet<T>(unsyncSet);
        }

        public static LinkedList<T> AsLinkedList<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable is LinkedList<T> list)
            {
                return list;
            }

            return new LinkedList<T>(enumerable);
        }

        public static IList<T> AsList<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable is IList<T> list)
            {
                return list;
            }

            return new List<T>(enumerable);
        }

        public static IList<T> AsList<T>(params T[] array)
        {
            return array;
        }

        public static IList<T> AsMutableList<T>(this IEnumerable<T> enumerable)
        {
            return new List<T>(enumerable);
        }

        public static IList<T> AsSortedList<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return null;
            }

            if (!(enumerable is List<T> asList))
            {
                asList = new List<T>(enumerable);
            }

            asList.Sort();

            return asList;
        }

        public static void ForEach<T>(this LinkedList<T> listThis, Action<T> action)
        {
            if (listThis != null)
            {
                var node = listThis.First;
                while (node != null)
                {
                    action.Invoke(node.Value);
                    node = node.Next;
                }
            }
        }

        public static void ForEach<T>(this IList<T> arrayThis, Action<T> action)
        {
            if (arrayThis != null)
            {
                var length = arrayThis.Count;
                for (var ii = 0; ii < length; ii++)
                {
                    action.Invoke(arrayThis[ii]);
                }
            }
        }

        public static void ForEvery<T>(this T[] arrayThis, Action<T> action)
        {
            if (arrayThis != null)
            {
                var length = arrayThis.Length;
                for (var ii = 0; ii < length; ii++)
                {
                    action.Invoke(arrayThis[ii]);
                }
            }
        }

        public static void For<T>(this IEnumerable<T> enumThis, Action<T> action)
        {
            unchecked
            {
                if (enumThis == null)
                {
                    return;
                }

                if (enumThis is ChainedArrayList<T> chainedArrayList)
                {
                    chainedArrayList.ForEach(action);
                }
                else if (enumThis is List<T>)
                {
                    var arrayThis = (List<T>) enumThis;
                    arrayThis.ForEach(action);
                }
                else if (enumThis is IList<T>)
                {
                    var arrayThis = (IList<T>) enumThis;
                    var length = arrayThis.Count;
                    for (var ii = 0; ii < length; ii++)
                    {
                        action.Invoke(arrayThis[ii]);
                    }
                }
                else
                {
                    foreach (var item in enumThis)
                    {
                        action.Invoke(item);
                    }
                }
            }
        }

        public static IEnumerable Is<T, TX>(this IEnumerable<T> enumThis)
        {
            return enumThis.Where(item => item is TX);
        }

        public static IList<T> Fill<T>(this IList<T> listThis, T value)
        {
            for (var ii = 0; ii < listThis.Count; ii++)
            {
                listThis[ii] = value;
            }

            return listThis;
        }

        public static T[] Fill<T>(this T[] arrayThis, T value)
        {
            for (var ii = arrayThis.Length - 1; ii >= 0; ii--)
            {
                arrayThis[ii] = value;
            }

            return arrayThis;
        }

        public static T[] Fill<T>(this T[] arrayThis, Func<T> generator)
        {
            for (var ii = arrayThis.Length - 1; ii >= 0; ii--)
            {
                arrayThis[ii] = generator.Invoke();
            }

            return arrayThis;
        }

        public static T[] Fill<T>(this T[] arrayThis, Func<int, T> generator)
        {
            for (var ii = 0; ii < arrayThis.Length; ii++)
            {
                arrayThis[ii] = generator.Invoke(ii);
            }

            return arrayThis;
        }

        public static bool AreEqual(this Array arrayThis, Array arrayThat)
        {
            var arrayThisLength = arrayThis.Length;
            var arrayThatLength = arrayThat.Length;
            if (arrayThisLength != arrayThatLength)
            {
                return false;
            }

            for (var ii = 0; ii < arrayThisLength; ii++)
            {
                if (!Equals(arrayThis.GetValue(ii), arrayThat.GetValue(ii)))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsEqual<T>(this T[] arrayThis, T[] arrayThat)
        {
            var arrayThisLength = arrayThis.Length;
            var arrayThatLength = arrayThat.Length;
            if (arrayThisLength != arrayThatLength)
            {
                return false;
            }

            for (var ii = 0; ii < arrayThisLength; ii++)
            {
                if (!Equals(arrayThis[ii], arrayThat[ii]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsEqual<T>(this IEnumerator<T> enumThis, IEnumerator<T> enumThat)
        {
            while (true)
            {
                var testThis = enumThis.MoveNext();
                var testThat = enumThat.MoveNext();
                if (testThis && testThat)
                {
                    if (!Equals(enumThis.Current, enumThat.Current))
                    {
                        return false;
                    }
                }
                else if (!testThis && !testThat)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        ///     Gets the item at the nth index of the enumerable.
        /// </summary>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public static object AtIndex(this IEnumerable enumerable, int index)
        {
            return AtIndex(enumerable, index, failIndex => null);
        }

        /// <summary>
        ///     Gets the item at the nth index of the enumerable.
        /// </summary>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="index">The index.</param>
        /// <param name="itemNotFound">The item not found.</param>
        /// <returns></returns>
        public static object AtIndex(this IEnumerable enumerable, int index, Func<int, object> itemNotFound)
        {
            if (enumerable is IList asList)
            {
                return index < asList.Count ? asList[index] : itemNotFound(index);
            }

            var enumerator = enumerable.GetEnumerator();
            if (index == 0)
            {
                return enumerator.MoveNext() ? enumerator.Current : itemNotFound(index);
            }

            for (var myIndex = 1; myIndex <= index; myIndex++)
            {
                if (!enumerator.MoveNext())
                {
                    return itemNotFound(index);
                }
            }

            return enumerator.Current;
        }

        /// <summary>
        ///     Returns true if all items in the itemEnum are contained in referenceCollection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="referenceCollection"></param>
        /// <param name="itemEnum"></param>
        /// <returns></returns>
        public static bool ContainsAll<T>(this ICollection<T> referenceCollection, IEnumerable<T> itemEnum)
        {
            foreach (var item in itemEnum)
            {
                if (!referenceCollection.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Advances the specified enumerator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerator">The enumerator.</param>
        /// <returns></returns>
        public static T Advance<T>(this IEnumerator<T> enumerator)
        {
            enumerator.MoveNext();
            return enumerator.Current;
        }

        public static bool DeepUnorderedEquals<T>(this IList<T> pthis, IList<T> pthat)
        {
            if (pthis.Count != pthat.Count)
            {
                return false;
            }

            for (var ii = pthis.Count - 1; ii >= 0; ii--)
            {
                if (!pthat.Contains(pthis[ii]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Does a deep equality test on a set of lists.  Order is assumed to be the same.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pthis">The pthis.</param>
        /// <param name="pthat">The pthat.</param>
        /// <returns></returns>
        public static bool DeepEquals<T>(this IList<T> pthis, IList<T> pthat)
        {
            if (pthis == null && pthat == null)
            {
                return true;
            }

            if (pthis == null)
            {
                return pthat.Count == 0;
            }

            if (pthat == null)
            {
                return pthis.Count == 0;
            }

            if (pthis.Count != pthat.Count)
            {
                return false;
            }

            var thisEnum = pthis.GetEnumerator();
            var thatEnum = pthat.GetEnumerator();

            while (true)
            {
                var thisHasMore = thisEnum.MoveNext();
                var thatHasMore = thatEnum.MoveNext();
                if (!thisHasMore || !thatHasMore)
                {
                    return !thisHasMore && !thatHasMore;
                }

                if (!Equals(thisEnum.Current, thatEnum.Current))
                {
                    return false;
                }
            }
        }

        /// <summary>
        ///     Determines whether the specified collection in the parameter is null or
        ///     empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter">The parameter.</param>
        /// <returns>
        ///     <c>true</c> if [is empty or null] [the specified parameter]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmptyOrNull<T>(this ICollection<T> parameter)
        {
            return
                parameter == null ||
                parameter.Count == 0;
        }

        /// <summary>
        ///     Determines whether the specified collection in the parameter is empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter">The parameter.</param>
        /// <returns>
        ///     <c>true</c> if the specified parameter is empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmpty<T>(this ICollection<T> parameter)
        {
            return parameter.Count == 0;
        }

        /// <summary>
        ///     Determines whether the specified collection in the parameter is empty.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>
        ///     <c>true</c> if the specified parameter is empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmptyCollection(this ICollection parameter)
        {
            return parameter.Count == 0;
        }

        /// <summary>
        ///     Determines whether the specified collection in the parameter is not empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter">The parameter.</param>
        /// <returns>
        ///     <c>true</c> if the specified parameter is empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNotEmpty<T>(this ICollection<T> parameter)
        {
            return parameter.Count != 0;
        }

        /// <summary>
        ///     Adds all of the items in the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pthis">The pthis.</param>
        /// <param name="source">The source.</param>
        public static void AddAll<T>(this ICollection<T> pthis, IEnumerable<T> source)
        {
            if (source is IList<T> asList)
            {
                var asListCount = asList.Count;
                for (var ii = 0; ii < asListCount; ii++)
                {
                    pthis.Add(asList[ii]);
                }
            }
            else if (source != null)
            {
                foreach (var value in source)
                {
                    pthis.Add(value);
                }
            }
        }

        public static IEnumerable<Tuple<TA, TB>> Merge<TA, TB>(this IEnumerable<TA> enumA, IEnumerable<TB> enumB)
        {
            if (enumA != null && enumB != null)
            {
                var enA = enumA.GetEnumerator();
                var enB = enumB.GetEnumerator();
                while (enA.MoveNext() && enB.MoveNext())
                {
                    yield return new Tuple<TA, TB>
                    {
                        A = enA.Current,
                        B = enB.Current
                    };
                }
            }
        }

        public static bool HasFirst<T>(this IEnumerable<T> pthis)
        {
            if (pthis == null)
            {
                throw new ArgumentNullException(nameof(pthis));
            }

            var tableEnum = pthis.GetEnumerator();
            return tableEnum != null && tableEnum.MoveNext();
        }

        /// <summary>
        ///     Returns the second item in the set
        /// </summary>
        /// <returns></returns>
        public static T Second<T>(this IEnumerable<T> pthis)
        {
            if (pthis == null)
            {
                throw new ArgumentNullException(nameof(pthis));
            }

            var tableEnum = pthis.GetEnumerator();
            tableEnum.MoveNext();
            tableEnum.MoveNext();
            return tableEnum.Current;
        }

        public static void RemoveWhere<T>(this LinkedList<T> list, Func<T, bool> where)
        {
            for (var curr = list.First; curr != null;)
            {
                var next = curr.Next;
                if (where.Invoke(curr.Value))
                {
                    list.Remove(curr);
                }

                curr = next;
            }
        }

        public static void RemoveWhere<T>(this IList<T> list, Func<T, bool> where)
        {
            for (var ii = 0; ii < list.Count;)
            {
                var testItem = where.Invoke(list[ii]);
                if (testItem)
                {
                    list.RemoveAt(ii);
                }
                else
                {
                    ii++;
                }
            }
        }

        public static int RemoveWhere<T>(this IList<T> list, Func<T, bool> where, Action<T> collector)
        {
            var count = 0;

            for (var ii = 0; ii < list.Count;)
            {
                var testItem = where.Invoke(list[ii]);
                if (testItem)
                {
                    count++;
                    collector.Invoke(list[ii]);
                    list.RemoveAt(ii);
                }
                else
                {
                    ii++;
                }
            }

            return count;
        }

        /// <summary>
        ///     Removes all items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="items">The items.</param>
        public static void RemoveAll<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                while (collection.Remove(item))
                {
                }
            }
        }

        /// <summary>
        ///     Retains all items in the passed enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="items">The items.</param>
        public static void RetainAll<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items.Where(item => !collection.Contains(item)))
            {
                collection.Remove(item);
            }
        }

        /// <summary>
        ///     Primitive reversal of a collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        public static void Reverse<T>(this ICollection<T> collection)
        {
            if (collection is List<T> tempList)
            {
                tempList.Reverse();
                return;
            }

            tempList = new List<T>();
            tempList.AddRange(collection);
            tempList.Reverse();

            collection.Clear();

            foreach (var item in tempList)
            {
                collection.Add(item);
            }
        }

        public static string Render<TK, TV>(this IEnumerable<KeyValuePair<TK, TV>> source, MagicMarker magicMarker)
        {
            var fieldDelimiter = string.Empty;

            using (var textWriter = new StringWriter()) {
                textWriter.Write('[');

                if (source != null) {
                    foreach (var current in source) {
                        textWriter.Write(fieldDelimiter);
                        textWriter.Write(RenderAny(current.Key));
                        textWriter.Write('=');
                        if (ReferenceEquals(current.Value, null)) {
                            textWriter.Write("null");
                        }
                        else if (current.Value.GetType().IsGenericDictionary()) {
                            textWriter.Write(magicMarker.GetDictionaryFactory(current.Value.GetType()).Invoke(current.Value));
                        }
                        else if (current.Value is string) {
                            textWriter.Write(RenderAny(current.Value));
                        }
                        else if (current.Value is IEnumerable) {
                            Render((IEnumerable) current.Value, textWriter);
                        }
                        else {
                            textWriter.Write(RenderAny(current.Value));
                        }

                        fieldDelimiter = ", ";
                    }
                }

                textWriter.Write(']');
                return textWriter.ToString();
            }
        }

        /// <summary>
        ///     Renders an enumerable source
        /// </summary>
        /// <param name="source">the object to render.</param>
        /// <param name="textWriter">the destination to write to.</param>
        /// <returns></returns>
        public static void Render(this IEnumerable source, TextWriter textWriter)
        {
            var fieldDelimiter = string.Empty;

            if (source != null)
            {
                textWriter.Write('[');

                var sourceEnum = source.GetEnumerator();
                while (sourceEnum.MoveNext())
                {
                    textWriter.Write(fieldDelimiter);
                    textWriter.Write(RenderAny(sourceEnum.Current));
                    fieldDelimiter = ", ";
                }

                textWriter.Write(']');
            }
            else
            {
                textWriter.Write("null");
            }
        }

        /// <summary>
        ///     Renders an enumerable source
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="renderEngine">The render engine.</param>
        /// <returns></returns>
        public static string Render(this IEnumerable source, Func<object, string> renderEngine)
        {
            var fieldDelimiter = string.Empty;

            var builder = new StringBuilder();
            builder.Append('[');

            var sourceEnum = source.GetEnumerator();
            while (sourceEnum.MoveNext())
            {
                builder.Append(fieldDelimiter);
                builder.Append(renderEngine(sourceEnum.Current));
                fieldDelimiter = ", ";
            }

            builder.Append(']');
            return builder.ToString();
        }

        /// <summary>
        ///     Removes the item at the specified index from the list and
        ///     returns the item.
        /// </summary>
        /// <typeparam name="TV"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public static TV DeleteAt<TV>(this IList<TV> list, int index)
        {
            var tempItem = list[index];
            list.RemoveAt(index);
            return tempItem;
        }

        /// <summary>
        ///     Removes the item at the front of the list and returns it.
        /// </summary>
        /// <typeparam name="TV"></typeparam>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        public static TV PopFront<TV>(this LinkedList<TV> list)
        {
            var tempItem = list.First.Value;
            list.RemoveFirst();
            return tempItem;
        }

        public static TV Poll<TV>(this LinkedList<TV> list, TV defaultValue)
        {
            if (list.First == null)
            {
                return defaultValue;
            }

            var tempItem = list.First.Value;
            list.RemoveFirst();
            return tempItem;
        }

        public static TV Poll<TV>(this LinkedList<TV> list)
        {
            return Poll(list, default(TV));
        }

        public static TV Poll<TV>(this IList<TV> list, TV defaultValue)
        {
            if (list.Count == 0)
            {
                return defaultValue;
            }

            var tempItem = list[0];
            list.RemoveAt(0);
            return tempItem;
        }

        public static TV Poll<TV>(this IList<TV> list)
        {
            return Poll(list, default(TV));
        }

        public static LinkedListNode<TV> FirstNode<TV>(this LinkedList<TV> list, Func<TV, bool> whereClause)
        {
            for (var curr = list.First; curr != null; curr = curr.Next)
            {
                if (whereClause(curr.Value))
                {
                    return curr;
                }
            }

            return null;
        }

        public static string[] ToUpper(this string[] inArray)
        {
            var outArray = new string[inArray.Length];
            for (var ii = 0; ii < inArray.Length; ii++)
            {
                var inItem = inArray[ii];
                if (inItem != null)
                {
                    outArray[ii] = inItem.ToUpper();
                }
            }

            return outArray;
        }

        public static Type[] GetParameterTypes(this MethodInfo method)
        {
            return method.GetParameters().Select(p => p.ParameterType).ToArray();
        }

        public static Type[] GetParameterTypes(this ConstructorInfo ctor)
        {
            return ctor.GetParameters().Select(p => p.ParameterType).ToArray();
        }

        public static IEnumerable<int> XRange(int lowerBound, int upperBound)
        {
            for (var ii = lowerBound; ii < upperBound; ii++)
            {
                yield return ii;
            }
        }

        public static bool CanUnwrap<T>(this object value)
        {
            if (value == null)
            {
                return true;
            }

            if (value is ICollection<T>)
            {
                return true;
            }

            if (value is IEnumerable<object>)
            {
                return true;
            }

            if (value is IEnumerable)
            {
                return true;
            }

            if (value is IEnumerator)
            {
                return true;
            }

            return false;
        }

        public static T[] UnwrapIntoArray<T>(this object value, bool includeNullValues = true)
        {
            if (value == null)
            {
                return null;
            }

            if (value is T[] array)
            {
                return array;
            }

            return Unwrap<T>(value, includeNullValues).ToArray();
        }

        public static IList<T> UnwrapIntoList<T>(this object value, bool includeNullValues = true)
        {
            if (value == null)
            {
                return null;
            }

            if (value is IList<T> list)
            {
                return list;
            }

            return Unwrap<T>(value, includeNullValues).ToList();
        }

        public static ICollection<T> UnwrapSafe<T>(this object value, bool includeNullValues = false)
        {
            if (value == null)
            {
                return null;
            }

            if (value is ICollection<T> collection)
            {
                return collection;
            }

            return UnwrapEnumerable<T>(value, includeNullValues).ToArray();
        }

        public static ICollection<T> Unwrap<T>(this object value, bool includeNullValues = false)
        {
            if (value == null)
            {
                return null;
            }

            if (value is ICollection<T> collection)
            {
                return collection;
            }

            return UnwrapEnumerable<T>(value, includeNullValues).ToArray();
        }

        public static IEnumerable<T> UnwrapEnumerable<T>(
            this object value,
            bool includeNullValues = false)
        {
            if (value == null)
            {
                return null;
            }

            if (value is IEnumerable<T> enumerableT)
            {
                return enumerableT;
            }

            if (value is IEnumerable<object>)
            {
                var expression = (IEnumerable<object>) value;
                expression = includeNullValues
                    ? expression.Where(o => o == null || o is T)
                    : expression.Where(o => o is T);

                return expression.Cast<T>();
            }

            if (value is IEnumerable enumerable)
            {
                var expression = enumerable.Cast<object>();
                expression = includeNullValues
                    ? expression.Where(o => o == null || o is T)
                    : expression.Where(o => o is T);

                return expression.Cast<T>();
            }

            if (value is IEnumerator enumerator)
            {
                var result = new List<T>();
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (includeNullValues)
                    {
                        if (current is T currentAsT)
                        {
                            result.Add(currentAsT);
                        }
                    }
                    else if (current is T)
                    {
                        result.Add((T) current);
                    }
                }

                return result;
            }

            throw new ArgumentException("invalid value");
        }

        public static IDictionary<object, object> UnwrapDictionary(this object value, MagicMarker magicMarker)
        {
            if (value == null)
            {
                return null;
            }

            if (value is IDictionary<object, object> stringDictionary)
            {
                return stringDictionary;
            }

            var valueType = value.GetType();
            if (valueType.IsGenericDictionary())
            {
                return magicMarker.GetDictionaryFactory(valueType).Invoke(value);
            }

            if (value is IEnumerable<KeyValuePair<object, object>> enumerables)
            {
                var valueDataMap = new Dictionary<object, object>();
                foreach (var valueKeyValuePair in enumerables)
                {
                    valueDataMap[valueKeyValuePair.Key] = valueKeyValuePair.Value;
                }

                return valueDataMap;
            }

            if (value is KeyValuePair<object, object> keyValuePair)
            {
                var valueDataMap = new Dictionary<object, object>
                {
                    [keyValuePair.Key] = keyValuePair.Value
                };
                return valueDataMap;
            }

            throw new ArgumentException("unable to convert input to string dictionary");
        }

        public static IDictionary<string, object> UnwrapStringDictionary(
            this object value)
        {
            return UnwrapStringDictionary(value, MagicMarker.SingletonInstance);
        }
        
        public static IDictionary<string, object> UnwrapStringDictionary(this object value, MagicMarker magicMarker)
        {
            if (value == null)
            {
                return null;
            }

            if (value is IDictionary<string, object> stringDictionary)
            {
                return stringDictionary;
            }

            var valueType = value.GetType();
            if (valueType.IsGenericStringDictionary())
            {
                return magicMarker.GetStringDictionaryFactory(valueType).Invoke(value);
            }

            if (value is IEnumerable<KeyValuePair<string, object>> pairs)
            {
                var valueDataMap = new Dictionary<string, object>();
                foreach (var valueKeyValuePair in pairs)
                {
                    valueDataMap[valueKeyValuePair.Key] = valueKeyValuePair.Value;
                }

                return valueDataMap;
            }

            if (value is KeyValuePair<string, object> valueKeyValuePair1)
            {
                return new Dictionary<string, object>
                {
                    [valueKeyValuePair1.Key] = valueKeyValuePair1.Value
                };
            }

            // use this sparingly since its more expensive... we may need to write
            // a more generalized method if this becomes commonplace.

            var dictType = valueType.FindGenericInterface(typeof(IDictionary<,>));
            if (dictType != null)
            {
                var magicDictionary = magicMarker.GetDictionaryFactory(valueType).Invoke(value);
                return magicDictionary.Transform(
                    Convert.ToString,
                    ke => ke);
            }

            throw new ArgumentException("unable to convert input to string dictionary");
        }

        /// <summary>
        /// Transparent cast for the lazy
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
        public static IDictionary<string, object> AsDataMap(this object o)
        {
            return o as IDictionary<string, object>;
        }

        [Obsolete]
        public static IDictionary<string, object> AsStringDictionary(this object value, MagicMarker magicMarker)
        {
#if DEPRECATED
            if (value == null)
                return null;
            if (value is IDictionary<string, object>)
                return (IDictionary<string, object>)value;
            if (value.GetType().IsGenericDictionary())
                return magicMarker.GetStringDictionary(value);

            throw new ArgumentException("invalid value for string dictionary", nameof(value));
#else
            return UnwrapStringDictionary(value, magicMarker);
#endif
        }

        public static void RenderAny(
            this object value,
            TextWriter textWriter)
        {
            if (value == null)
            {
                textWriter.Write("null");
                return;
            }

            if (value is string s) {
                textWriter.Write(s);
                return;
            }

            if (value is decimal) {
                var text = value.ToString();
                if (text.IndexOf('.') == -1)
                {
                    text += ".0";
                }

                textWriter.Write(text);
                textWriter.Write('m');
                return;
            }

            if (value is double)
            {
                var text = value.ToString();
                if (text.IndexOf('.') == -1)
                {
                    text += ".0";
                }

                textWriter.Write(text);
                return; // + 'd'
            }

            if (value is float)
            {
                var text = value.ToString();
                if (text.IndexOf('.') == -1)
                {
                    text += ".0";
                }

                textWriter.Write(text);
                textWriter.Write('f');
                return;
            }

            if (value is long)
            {
                textWriter.Write(value.ToString());
                textWriter.Write('L');
                return;
            }

            if (value is DateTimeOffset dateTimeOffset)
            {
                var dateOnly = dateTimeOffset.Date;
                if (dateTimeOffset == dateOnly) {
                    textWriter.Write(dateTimeOffset.ToString("yyyy-MM-dd z"));
                    return;
                }

                if (dateTimeOffset.Millisecond == 0) {
                    textWriter.Write(dateTimeOffset.ToString("yyyy-MM-dd hh:mm:ss z"));
                    return;
                }

                textWriter.Write(dateTimeOffset.ToString("yyyy-MM-dd hh:mm:ss.ffff z"));
                return;
            }

            if (value is DateTime dateTime)
            {
                var dateOnly = dateTime.Date;
                if (dateTime == dateOnly)
                {
                    textWriter.Write(dateTime.ToString("yyyy-MM-dd"));
                    return;
                }

                if (dateTime.Millisecond == 0)
                {
                    textWriter.Write(dateTime.ToString("yyyy-MM-dd hh:mm:ss"));
                    return;
                }

                textWriter.Write(dateTime.ToString("yyyy-MM-dd hh:mm:ss.ffff"));
                return;
            }

            if (value is Array array)
            {
                Render(array, textWriter);
                return;
            }

            if (value.GetType().GetCustomAttributes(typeof(RenderWithToStringAttribute), true).Length > 0) {
                textWriter.Write(value.ToString());
                return;
            }

            if (value is IEnumerable enumerable)
            {
                Render(enumerable, textWriter);
                return;
            }

            textWriter.Write(value.ToString());
        }

        public static string RenderAny(this object value)
        {
            using (var writer = new StringWriter()) {
                RenderAny(value, writer);
                return writer.ToString();
            }
        }

        /// <summary>
        ///     Renders the array as a string.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns></returns>
        public static string Render(this Array array)
        {
            using (var stringWriter = new StringWriter()) {
                Render(array, stringWriter, ", ", "[]");
                return stringWriter.ToString();
            }
        }

        /// <summary>
        ///     Renders the array as a string.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="textWriter">the destination to write to.</param>
        /// <returns></returns>
        public static void Render(this Array array, TextWriter textWriter)
        {
            Render(array, textWriter, ", ", "[]");
        }

        /// <summary>
        ///     Renders the array as a string
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="textWriter">Destination for the content.</param>
        /// <param name="itemSeparator">The item separator.</param>
        /// <param name="firstAndLast">The first and last.</param>
        /// <returns></returns>
        public static void Render(this Array array, TextWriter textWriter, string itemSeparator, string firstAndLast)
        {
            var fieldDelimiter = string.Empty;

            textWriter.Write(firstAndLast[0]);

            if (array != null)
            {
                var length = array.Length;
                for (var ii = 0; ii < length; ii++)
                {
                    textWriter.Write(fieldDelimiter);
                    textWriter.Write(RenderAny(array.GetValue(ii)));
                    fieldDelimiter = itemSeparator;
                }
            }

            textWriter.Write(firstAndLast[1]);
        }

        /// <summary>
        ///     Renders the array as a string
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="itemSeparator">The item separator.</param>
        /// <param name="firstAndLast">The first and last.</param>
        /// <returns></returns>
        public static string Render(this Array array, string itemSeparator, string firstAndLast)
        {
            using (var stringWriter = new StringWriter()) {
                Render(array, stringWriter, itemSeparator, firstAndLast);
                return stringWriter.ToString();
            }
        }

        public static string FormatInt(this int? value)
        {
            return value?.ToString(CultureInfo.CurrentCulture) ?? "null";
        }

        /// <summary>
        ///     Returns a view into a subsection of the list.  Since the resulting list is a shallow view, care
        ///     should be taken to ensure that the underlying list is not adversely modified while using the
        ///     view.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="fromIndex">From index.</param>
        /// <param name="toIndex">To index.</param>
        /// <returns></returns>
        public static IList<T> SubList<T>(this IList<T> list, int fromIndex, int toIndex)
        {
            if (toIndex >= list.Count)
            {
                toIndex = list.Count;
            }

            return new SubList<T>(list, fromIndex, toIndex);
        }

        /// <summary>
        ///     Appends one or more items to the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="more">The more.</param>
        /// <returns></returns>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, params T[] more)
        {
            foreach (var item in source)
            {
                yield return item;
            }

            foreach (var item in more)
            {
                yield return item;
            }
        }

        public static int Hash(IEnumerable @values)
        {
            var result = 0;
            if (values != null)
            {
                foreach (var item in values)
                {
                    int itemHash = item?.GetHashCode() ?? 0;
                    result *= 397;
                    result ^= itemHash;
                }
            }

            return result;
        }

        public static int Hash<T>(T[] @objects)
        {
            var result = 0;
            if (objects != null)
            {
                for (int ii = 0; ii < @objects.Length; ii++)
                {
                    var item = @objects[ii];
                    int itemHash = item?.GetHashCode() ?? 0;
                    result *= 397;
                    result ^= itemHash;
                }
            }

            return result;
        }

        public static int HashAll<T>(params T[] @objects)
        {
            return Hash(@objects);
        }
    }
}