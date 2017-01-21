///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;

namespace com.espertech.esper.epl.view
{
    public interface OutputProcessViewConditionDeltaSet
	{
	    int NumChangesetRows { get; }
	    void AddView(UniformPair<EventBean[]> events);
	    void AddJoin(UniformPair<ISet<MultiKey<EventBean>>> events);
	    void Clear();
	    IList<UniformPair<ISet<MultiKey<EventBean>>>> JoinEventsSet { get; }
	    IList<UniformPair<EventBean[]>> ViewEventsSet { get; }
	    void Destroy();
	}
} // end of namespace
