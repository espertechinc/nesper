///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.epl.join.rep
{
    /// <summary>A utility class for an iterator that has one element.</summary>
    public class SingleCursorIterator : IEnumerator<Cursor> {
        private Cursor cursor;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="cursor">is the single element.</param>
        public SingleCursorIterator(Cursor cursor) {
            this.cursor = cursor;
        }
    
        public bool HasNext() {
            return cursor != null;
        }
    
        public Cursor Next() {
            if (cursor == null) {
                throw new NoSuchElementException();
            }
            Cursor c = cursor;
            this.cursor = null;
            return c;
        }
    
        public void Remove() {
            throw new UnsupportedOperationException();
        }
    }
    
} // end of namespace
