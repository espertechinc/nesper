///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.view.window
{
	/// <summary>
	/// Provides random-access into window contents by event and index as a combination.
	/// </summary>
	public class RelativeAccessByEventNIndexGetterImpl 
        : IStreamRelativeAccessUpdateObserver
        , RelativeAccessByEventNIndexGetter
	{
	    private readonly IDictionary<EventBean, RelativeAccessByEventNIndex> _accessorByEvent = new Dictionary<EventBean, RelativeAccessByEventNIndex>();
	    private readonly IDictionary<RelativeAccessByEventNIndex, EventBean[]> _eventsByAccessor  = new Dictionary<RelativeAccessByEventNIndex, EventBean[]>();

	    public void Updated(RelativeAccessByEventNIndex iStreamRelativeAccess, EventBean[] newData)
	    {
	        // remove data posted from the last update
	        EventBean[] lastNewData = _eventsByAccessor.Get(iStreamRelativeAccess);
	        if (lastNewData != null)
	        {
	            for (int i = 0; i < lastNewData.Length; i++)
	            {
	                _accessorByEvent.Remove(lastNewData[i]);
	            }
	        }

	        if (newData == null)
	        {
	            return;
	        }

	        // hold accessor per event for querying
	        for (int i = 0; i < newData.Length; i++)
	        {
	            _accessorByEvent.Put(newData[i], iStreamRelativeAccess);
	        }

	        // save new data for access to later removal
	        _eventsByAccessor.Put(iStreamRelativeAccess, newData);
	    }

	    /// <summary>
	    /// Returns the access into window contents given an event.
	    /// </summary>
	    /// <param name="theEvent">to which the method returns relative access from</param>
	    /// <returns>buffer</returns>
	    public RelativeAccessByEventNIndex GetAccessor(EventBean theEvent)
	    {
	        RelativeAccessByEventNIndex iStreamRelativeAccess = _accessorByEvent.Get(theEvent);
	        if (iStreamRelativeAccess == null)
	        {
	            return null;
	        }
	        return iStreamRelativeAccess;
	    }
	}
} // end of namespace
