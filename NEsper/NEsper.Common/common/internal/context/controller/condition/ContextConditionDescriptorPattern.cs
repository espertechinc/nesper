///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.context.controller.condition
{
    public class ContextConditionDescriptorPattern : ContextConditionDescriptor
    {
        public EvalRootFactoryNode Pattern { get; set; }

        public PatternContext PatternContext { get; set; }

        public bool IsInclusive { get; set; }

        public string[] TaggedEvents { get; set; }

        public string[] ArrayEvents { get; set; }

        public bool IsImmediate { get; set; }

        public void AddFilterSpecActivatable(IList<FilterSpecActivatable> activatables)
        {
            EvalFactoryNodeVisitor visitor = new ProxyEvalFactoryNodeVisitor {
                ProcFilterFactoryNode = filter => activatables.Add(filter.FilterSpec)
            };
            Pattern.Accept(visitor);
        }
    }
} // end of namespace