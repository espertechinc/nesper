///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.client
{
    public interface EPManagedStatement : EPStatement
    {
        /// <summary>
        /// Clears the event handlers and statement aware event handlers.  Should be
        /// used with caution since this clears anyone who has registered a handler.
        /// </summary>
        void ClearEventHandlers();
    }
}
