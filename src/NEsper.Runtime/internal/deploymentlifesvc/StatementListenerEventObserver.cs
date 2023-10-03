///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.@internal.deploymentlifesvc
{
	/// <summary>
	///     Observer statement management events.
	/// </summary>
	internal interface StatementListenerEventObserver
    {
	    /// <summary>
	    ///     Observer statement state changes.
	    /// </summary>
	    /// <param name="theEvent">indicates statement changed</param>
	    void Observe(StatementListenerEvent theEvent);
    }
} // end of namespace