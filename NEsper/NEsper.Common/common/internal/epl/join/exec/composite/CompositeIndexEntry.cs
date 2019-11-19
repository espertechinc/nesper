///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.join.exec.composite
{
    /// <summary>
    /// An entry in a dictionary that can represent either an index or a collection, but not both.
    /// This is another carryover of improper use of type erasure in Java that needs to be corrected
    /// in Esper.
    /// </summary>
    public class CompositeIndexEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeIndexEntry"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        public CompositeIndexEntry(IDictionary<object, CompositeIndexEntry> index)
        {
            Index = index;
        }

        public CompositeIndexEntry(ICollection<EventBean> collection)
        {
            Collection = collection;
        }

        internal bool IsCollection => Collection != null;
        internal ICollection<EventBean> Collection { get; set; }

        internal bool IsIndex => Index != null;
        internal IDictionary<object, CompositeIndexEntry> Index { get; set; }

        internal ICollection<EventBean> AssertCollection()
        {
            if (!IsCollection) {
                throw new IllegalStateException("entry was not a collection");
            }

            return Collection;
        }

        internal IDictionary<object, CompositeIndexEntry> AssertIndex()
        {
            if (!IsIndex) {
                throw new IllegalStateException("entry was not an index");
            }

            return Index;
        }
    }
}