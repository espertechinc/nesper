///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    public sealed class CopyOnWriteArraySet<T> : CopyOnWriteList<T>
    {
        #region ICollection<T> Members

        /// <summary>
        /// Adds all of the items in the source.
        /// </summary>
        /// <param name="source">The source.</param>
        public void AddAll(IEnumerable<T> source)
        {
            List<T> tempList = new List<T>();

            using( WriteLock.Acquire() )
            {
                foreach (T item in source)
                {
                    if (! Contains(item))
                    {
                        tempList.Add(item);
                    }
                }

                if (tempList.Count != 0)
                {
                    AddRange(tempList);
                }
            }
        }

        /// <summary>
        /// Returns the first item in the set
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        public T First => this[0];

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty => Count == 0;

        #endregion

        #region ICollection<T> Members

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public override void Add(T item)
        {
            using (WriteLock.Acquire())
            {
                if (!Contains( item ))
                {
                    base.Add(item);
                }
            }
        }

        #endregion
    }
}
