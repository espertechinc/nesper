///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.core.support
{
	public class SupportEngineImportServiceFactory
    {
	    public static EngineImportServiceImpl Make()
        {
	        return new EngineImportServiceImpl(true, true, true, false, null, TimeZoneInfo.Local, ConfigurationEngineDefaults.ThreadingProfile.NORMAL);
	    }
	}
} // end of namespace
