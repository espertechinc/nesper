///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.context;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.context
{
    public class SupportSelectorNested : ContextPartitionSelectorNested
    {
        public SupportSelectorNested(
            ContextPartitionSelector s0,
            ContextPartitionSelector s1) : this(new[] {s0, s1})
        {
        }

        public SupportSelectorNested(ContextPartitionSelector[] selectors)
        {
            Selectors = Collections.SingletonList(selectors);
        }

        public SupportSelectorNested(IList<ContextPartitionSelector[]> selectors)
        {
            Selectors = selectors;
        }

        public IList<ContextPartitionSelector[]> Selectors { get; }
    }
} // end of namespace