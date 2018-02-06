///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.util
{
	/// <summary>
	/// Stop callback that performs no action.
	/// </summary>
	public class StopCallbackNoAction : StopCallback
	{
        public readonly static StopCallbackNoAction INSTANCE = new StopCallbackNoAction();

        /// <summary>
        /// Stops this instance.
        /// </summary>
	    public void Stop()
	    {
	    }
	}
} // end of namespace
