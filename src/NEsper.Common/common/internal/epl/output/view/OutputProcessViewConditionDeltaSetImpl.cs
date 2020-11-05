///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    public class OutputProcessViewConditionDeltaSetImpl : OutputProcessViewConditionDeltaSet
    {
        public OutputProcessViewConditionDeltaSetImpl(bool isJoin)
        {
            if (isJoin) {
                JoinEventsSet = new List<UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>>();
                ViewEventsSet = new EmptyList<UniformPair<EventBean[]>>();
            }
            else {
                ViewEventsSet = new List<UniformPair<EventBean[]>>();
                JoinEventsSet = new EmptyList<UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>>();
            }
        }

        public int NumChangesetRows => Math.Max(ViewEventsSet.Count, JoinEventsSet.Count);

        public void AddView(UniformPair<EventBean[]> uniformPair)
        {
            ViewEventsSet.Add(uniformPair);
        }

        public void AddJoin(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>> setUniformPair)
        {
            JoinEventsSet.Add(setUniformPair);
        }

        public void Clear()
        {
            ViewEventsSet.Clear();
            JoinEventsSet.Clear();
        }

        public void Destroy()
        {
            Clear();
        }

        public IList<UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>> JoinEventsSet { get; }

        public IList<UniformPair<EventBean[]>> ViewEventsSet { get; }
    }
} // end of namespace