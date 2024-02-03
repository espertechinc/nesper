///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.collection
{
    public class TransformEventEnumerator : IEnumerator<EventBean>
    {
        private readonly IEnumerator<EventBean> _sourceEnumerator;
        private readonly TransformEventMethod _transformEventMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformEventEnumerator"/> class.
        /// </summary>
        /// <param name="sourceEnumerator">The source enumerator.</param>
        /// <param name="transformEventMethod">The transform event method.</param>
        public TransformEventEnumerator(
            IEnumerator<EventBean> sourceEnumerator,
            TransformEventMethod transformEventMethod)
        {
            _sourceEnumerator = sourceEnumerator;
            _transformEventMethod = transformEventMethod;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset()
        {
            _sourceEnumerator.Reset();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        public bool MoveNext()
        {
            return _sourceEnumerator.MoveNext();
        }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        object IEnumerator.Current => Current;

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        public EventBean Current => _transformEventMethod.Transform(_sourceEnumerator.Current);
    }
}