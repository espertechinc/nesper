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

namespace com.espertech.esper.compat.collections
{
    /// <summary>
    /// An enumerable that leverages a function.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProxyEnumerable<T> : IEnumerable<T>
    {
        /// <summary>
        /// Gets or sets the proc enumerator.
        /// </summary>
        /// <value>
        /// The proc enumerator.
        /// </value>
        public Func<IEnumerator<T>> ProcEnumerator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyEnumerable{T}"/> class.
        /// </summary>
        public ProxyEnumerable()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyEnumerable{T}"/> class.
        /// </summary>
        /// <param name="procEnumerator">The proc enumerator.</param>
        public ProxyEnumerable(Func<IEnumerator<T>> procEnumerator)
        {
            ProcEnumerator = procEnumerator;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return ProcEnumerator.Invoke();
        }
    }
}
