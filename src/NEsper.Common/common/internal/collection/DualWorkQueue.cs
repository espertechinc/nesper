///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.collection
{
    /// <summary>
    /// Work queue wherein items can be added to the front and to the back, wherein both front and back
    /// have a given order, with the idea that all items of the front queue get processed before any
    /// given single item of the back queue gets processed.
    /// </summary>
    public class DualWorkQueue<TV>
    {
        /// <summary>Ctor.</summary>
        public DualWorkQueue()
        {
            FrontQueue = new ArrayDeque<TV>();
            BackQueue = new ArrayDeque<TV>();
        }

        /// <summary>
        /// Items to be processed first, in the order to be processed.
        /// </summary>
        /// <value>front queue</value>
        public ArrayDeque<TV> FrontQueue { get; private set; }

        /// <summary>
        /// Items to be processed after front-queue is empty, in the order to be processed.
        /// </summary>
        /// <value>back queue</value>
        public ArrayDeque<TV> BackQueue { get; private set; }
    }
} // end of namespace