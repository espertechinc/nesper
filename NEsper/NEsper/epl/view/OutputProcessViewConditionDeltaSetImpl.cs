///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.view
{
	public class OutputProcessViewConditionDeltaSetImpl : OutputProcessViewConditionDeltaSet
	{
	    private readonly IList<UniformPair<EventBean[]>> _viewEventsList;
		private readonly IList<UniformPair<ISet<MultiKey<EventBean>>>> _joinEventsSet;

	    public OutputProcessViewConditionDeltaSetImpl(bool isJoin)
        {
	        if (isJoin)
            {
	            _joinEventsSet = new List<UniformPair<ISet<MultiKey<EventBean>>>>();
	            _viewEventsList = Collections.GetEmptyList<UniformPair<EventBean[]>>();
	        }
	        else
            {
	            _viewEventsList = new List<UniformPair<EventBean[]>>();
	            _joinEventsSet = Collections.GetEmptyList<UniformPair<ISet<MultiKey<EventBean>>>>();
	        }
	    }

	    public int NumChangesetRows => Math.Max(_viewEventsList.Count, _joinEventsSet.Count);

	    public void AddView(UniformPair<EventBean[]> uniformPair)
        {
	        _viewEventsList.Add(uniformPair);
	    }

	    public void AddJoin(UniformPair<ISet<MultiKey<EventBean>>> setUniformPair)
        {
	        _joinEventsSet.Add(setUniformPair);
	    }

	    public void Clear()
        {
	        _viewEventsList.Clear();
	        _joinEventsSet.Clear();
	    }

	    public void Destroy()
        {
	        Clear();
	    }

	    public IList<UniformPair<ISet<MultiKey<EventBean>>>> JoinEventsSet => _joinEventsSet;

	    public IList<UniformPair<EventBean[]>> ViewEventsSet => _viewEventsList;
	}
} // end of namespace
