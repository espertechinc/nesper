///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.context;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.context
{
    public class SupportSelectorNested : ContextPartitionSelectorNested {
        private readonly IList<ContextPartitionSelector[]> _selectors;
    
        public SupportSelectorNested(ContextPartitionSelector s0, ContextPartitionSelector s1)
            : this(new ContextPartitionSelector[]{s0, s1})
        {
        }
    
        public SupportSelectorNested(ContextPartitionSelector[] selectors)
        {
            this._selectors = new List<ContextPartitionSelector[]>();
            this._selectors.Add(selectors);
        }
    
        public SupportSelectorNested(IList<ContextPartitionSelector[]> selectors) {
            this._selectors = selectors;
        }

        public IList<ContextPartitionSelector[]> Selectors
        {
            get { return _selectors; }
        }
    }
} // end of namespace
