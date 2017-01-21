///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.view
{
	/// <summary>
	/// Provides statement resources with the means to register a callback and be informed when a statement stopped
	/// and resources for the statement must be release.
	/// </summary>
	public interface StatementStopService
	{
        /// <summary>
        /// Callback that is performed for a stop of a statement.
        /// </summary>
        event StatementStopCallback StatementStopped;

	    /// <summary>
	    /// Used by the engine to indicate a statement stopped, invoking any callbacks registered.
	    /// </summary>
	    void FireStatementStopped();
	}
} // End of namespace
