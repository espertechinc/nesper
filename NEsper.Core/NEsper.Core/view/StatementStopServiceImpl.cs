///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.view
{
    /// <summary>Provides subscription list for statement stop callbacks. </summary>
    public class StatementStopServiceImpl : StatementStopService
    {
        public event StatementStopCallback StatementStopped;

        public void FireStatementStopped()
        {
            if (StatementStopped != null)
            {
                StatementStopped.Invoke();
            }
        }
    }
}
