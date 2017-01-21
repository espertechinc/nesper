///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.view.internals
{
	public class IntersectAsymetricViewLocalState
	{
	    private readonly EventBean[][] _oldEventsPerView;
	    private readonly ISet<EventBean> _removalEvents = new HashSet<EventBean>();
	    private readonly ArrayDeque<EventBean> _newEvents = new ArrayDeque<EventBean>();

	    private EventBean[] _newDataChildView;
	    private bool _hasRemovestreamData;
	    private bool _retainObserverEvents;
	    private bool _discardObserverEvents;
	    private ISet<EventBean> _oldEvents = new HashSet<EventBean>();

	    public IntersectAsymetricViewLocalState(EventBean[][] oldEventsPerView)
	    {
	        _oldEventsPerView = oldEventsPerView;
	    }

	    public EventBean[][] OldEventsPerView
	    {
	        get { return _oldEventsPerView; }
	    }

	    public ISet<EventBean> RemovalEvents
	    {
	        get { return _removalEvents; }
	    }

	    public ArrayDeque<EventBean> NewEvents
	    {
	        get { return _newEvents; }
	    }

	    public EventBean[] NewDataChildView
	    {
	        get { return _newDataChildView; }
	        set { _newDataChildView = value; }
	    }

	    public bool HasRemovestreamData
	    {
	        get { return _hasRemovestreamData; }
	        set { _hasRemovestreamData = value; }
	    }

	    public bool IsRetainObserverEvents
	    {
	        get { return _retainObserverEvents; }
	        set { _retainObserverEvents = value; }
	    }

	    public bool IsDiscardObserverEvents
	    {
	        get { return _discardObserverEvents; }
	        set { _discardObserverEvents = value; }
	    }

	    public ISet<EventBean> OldEvents
	    {
	        get { return _oldEvents; }
	        set { _oldEvents = value; }
	    }
	}
} // end of namespace
