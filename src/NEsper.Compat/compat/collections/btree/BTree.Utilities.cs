using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.compat.collections.btree
{
    public partial class BTree<TK, TV>
    {
        public delegate TK IndexAccessor(int index);

        /// <summary>
        /// Returns the position of the first value whose key is not less than k using
        /// linear search.
        /// </summary>
        /// <param name="k">input test key</param>
        /// <param name="s">start index</param>
        /// <param name="e">end index</param>
        /// <param name="comp">key comparator</param>
        /// <param name="keyAccessor">key accessor</param>
        /// <returns></returns>
        public static int LinearSearch(
            TK k,
            int s,
            int e,
            IComparer<TK> comp,
            IndexAccessor keyAccessor)
        {
            while (s < e) {
                var c = comp.Compare(keyAccessor(s), k);
                if (c == 0) {
                    return s;
                }
                else if (c > 0) {
                    break;
                }

                ++s;
            }

            return s;
        }

        /// <summary>
        /// Returns the position of the first value whose key is not less than k using
        /// binary search.
        /// </summary>
        /// <param name="k">input test key</param>
        /// <param name="s">start index</param>
        /// <param name="e">end index</param>
        /// <param name="comp">key comparator</param>
        /// <param name="keyAccessor">key accessor</param>
        /// <returns></returns>
        public static int BinarySearch(
            TK k,
            int s,
            int e,
            IComparer<TK> comp,
            IndexAccessor keyAccessor)
        {
            while (s != e) {
                var mid = (s + e) / 2;
                var c = comp.Compare(keyAccessor(mid), k);
                if (c < 0) {
                    s = mid + 1;
                }
                else if (c > 0) {
                    e = mid;
                }
                else {
                    // Need to return the first value whose key is not less than k, which
                    // requires continuing the binary search. Note that we are guaranteed
                    // that the result is an exact match because if "key(mid-1) < k" the
                    // call to BinarySearchCompareTo() will return "mid".
                    return BinarySearch(k, s, mid, comp, keyAccessor);
                }
            }

            return s;
        }
        
        /// <summary>
        /// Dumps the btree to the TextWriter.
        /// </summary>
        public void Dump(TextWriter textWriter)
        {
            Root?.Dump(textWriter, 0);
        }

        public string Dump()
        {
            using (var stringWriter = new StringWriter()) {
                Dump(stringWriter);
                return stringWriter.ToString();
            }
        }

        /// <summary>
        /// Performs a swap of the values contained by two references.  Needs better concurrency controls.
        /// </summary>
        /// <param name="lvalue"></param>
        /// <param name="rvalue"></param>
        /// <typeparam name="T"></typeparam>
        public static void RefSwap<T>(
            ref T lvalue,
            ref T rvalue)
        {
            var tvalue = rvalue;
            rvalue = lvalue;
            lvalue = tvalue;
        }

        /// <summary>
        /// Performs an assertion on a condition.
        /// </summary>
        /// <param name="assertion"></param>
        internal static void Assert(bool assertion)
        {
            if (!assertion) {
                throw new IllegalStateException();
            }
        }
    }
}