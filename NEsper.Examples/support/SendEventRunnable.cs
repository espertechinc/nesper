///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;

namespace NEsper.Examples.Support
{
    public class SendEventRunnable : IRunnable
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly object _eventToSend;
        private readonly EPRuntime _runtime;

        public SendEventRunnable(EPRuntime runtime, object eventToSend)
        {
            _runtime = runtime;
            _eventToSend = eventToSend;
        }

        public void Run()
        {
            try {
                _runtime.EventService.SendEventBean(
                    _eventToSend,
                    _eventToSend.GetType().FullName);
            }
            catch (Exception ex) {
                Log.Fatal(string.Empty, ex);
            }
        }
    }
}
