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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.output.view
{
	public class OutputProcessViewConditionDeltaSetImpl : OutputProcessViewConditionDeltaSet {
	    private readonly IList<UniformPair<EventBean[]>> viewEventsList;
	    private readonly IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet;

	    public OutputProcessViewConditionDeltaSetImpl(bool isJoin) {
	        if (isJoin) {
	            joinEventsSet = new List<UniformPair<ISet<MultiKey<EventBean>>>>();
	            viewEventsList = Collections.GetEmptyList<object>();
	        } else {
	            viewEventsList = new List<UniformPair<EventBean[]>>();
	            joinEventsSet = Collections.GetEmptyList<object>();
	        }
	    }

	    public int NumChangesetRows
	    {
	        get => Math.Max(viewEventsList.Count, joinEventsSet.Count);
	    }

	    public void AddView(UniformPair<EventBean[]> uniformPair) {
	        viewEventsList.Add(uniformPair);
	    }

	    public void AddJoin(UniformPair<ISet<MultiKey<EventBean>>> setUniformPair) {
	        joinEventsSet.Add(setUniformPair);
	    }

	    public void Clear() {
	        viewEventsList.Clear();
	        joinEventsSet.Clear();
	    }

	    public void Destroy() {
	        Clear();
	    }

	    public IList<UniformPair<ISet<MultiKey<EventBean>>>> JoinEventsSet
	    {
	        get => joinEventsSet;
	    }

	    public IList<UniformPair<EventBean[]>> ViewEventsSet
	    {
	        get => viewEventsList;
	    }
	}
} // end of namespace