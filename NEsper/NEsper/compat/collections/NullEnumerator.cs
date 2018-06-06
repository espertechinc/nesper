///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    public class NullEnumerator<T> : IEnumerator<T>
    {

        ///<summary>
        ///Gets the element in the collection at the current position of the enumerator.
        ///</summary>
        ///
        ///<returns>
        ///The element in the collection at the current position of the enumerator.
        ///</returns>
        ///
        T IEnumerator<T>.Current => throw new InvalidOperationException();

        ///<summary>
        ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///</summary>
        ///<filterpriority>2</filterpriority>
        public void Dispose()
        {
        }

        ///<summary>
        ///Advances the enumerator to the next element of the collection.
        ///</summary>
        ///
        ///<returns>
        ///true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        ///</returns>
        ///
        ///<exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public bool MoveNext()
        {
            return false;
        }

        ///<summary>
        ///Sets the enumerator to its initial position, which is before the first element in the collection.
        ///</summary>
        ///
        ///<exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public void Reset()
        {
            throw new NotSupportedException();
        }

        ///<summary>
        ///Gets the current element in the collection.
        ///</summary>
        ///
        ///<returns>
        ///The current element in the collection.
        ///</returns>
        ///
        ///<exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception><filterpriority>2</filterpriority>
        public object Current => throw new InvalidOperationException();
    }
}
