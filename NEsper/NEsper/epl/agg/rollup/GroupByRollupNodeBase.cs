///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.epl.agg.rollup
{
    public abstract class GroupByRollupNodeBase
    {
        private readonly IList<GroupByRollupNodeBase> _childNodes = new List<GroupByRollupNodeBase>();

        public abstract IList<int[]> Evaluate(GroupByRollupEvalContext context);

        public IList<GroupByRollupNodeBase> ChildNodes
        {
            get { return _childNodes; }
        }

        public void Add(GroupByRollupNodeBase child)
        {
            _childNodes.Add(child);
        }
    }
}
