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
    public class LookaheadEnumerator<T> : IEnumerator<T>
    {
        private IEnumerator<T> _baseEnum;
        private T _next;
        private bool _hasNext;

        /// <summary>
        /// Gets or sets a value indicating whether this instance has a value after
        /// the current value.
        /// </summary>
        public bool HasNext()
        {
            return _hasNext;
        }

        /// <summary>
        /// Gets the next item.
        /// </summary>
        /// <value>The next.</value>
        public T Next()
        {
            if (! HasNext())
                throw new ArgumentOutOfRangeException();

            var temp = _next;

            if (_baseEnum.MoveNext())
            {
                _hasNext = true;
                _next = _baseEnum.Current;
            }
            else
            {
                _hasNext = false;
                _next = default(T);
            }

            return temp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LookaheadEnumerator&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="baseEnum">The base enum.</param>
        public LookaheadEnumerator(IEnumerable<T> baseEnum)
            : this(baseEnum.GetEnumerator())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LookaheadEnumerator&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="baseEnum">The base enum.</param>
        public LookaheadEnumerator(IEnumerator<T> baseEnum)
        {
            _baseEnum = baseEnum;
            if ( baseEnum.MoveNext() ) {
                _hasNext = true;
                _next = baseEnum.Current;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _baseEnum.Dispose();
            _baseEnum = null;
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">
        /// The collection was modified after the enumerator was created.
        /// </exception>
        public bool MoveNext()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">
        /// The collection was modified after the enumerator was created.
        /// </exception>
        public void Reset()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the current element in the collection.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The current element in the collection.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">
        /// The enumerator is positioned before the first element of the collection or after the last element.
        /// </exception>
        object IEnumerator.Current => Current;

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The element in the collection at the current position of the enumerator.
        /// </returns>
        public T Current
        {
            get ; private set;
        }
    }
}
