///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.events;

namespace com.espertech.esper.core.support
{
	public class SupportEventAdapterService
	{
	    private static EventAdapterService _eventAdapterService;

	    static SupportEventAdapterService()
	    {
	        _eventAdapterService = new EventAdapterServiceImpl(new EventTypeIdGeneratorImpl(), 5);
	    }

	    public static void Reset()
	    {
	        _eventAdapterService = new EventAdapterServiceImpl(new EventTypeIdGeneratorImpl(), 5);
	    }
	    public static EventAdapterService GetService()
	    {
	        return _eventAdapterService;
	    }
	}
} // end of namespace
