///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.supportregression.util
{
    public class PrintUpdateListener
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly String _listenerName;

        public PrintUpdateListener()
        {
            _listenerName = "";
        }

        public PrintUpdateListener(String listenerName)
        {
            this._listenerName = listenerName;
        }

        public void Update(Object sender, UpdateEventArgs e)
        {
            var newEvents = e.NewEvents;
            if (newEvents == null)
            {
                Log.Info("Update received no new events");
                return;
            }

            for (int i = 0; i < newEvents.Length; i++)
            {
                Log.Info(".Update " + _listenerName + " Event#" + i + " : " + DumpProperties(newEvents[i]));
            }
        }
    
        private static string DumpProperties(EventBean newEvent)
        {
            StringBuilder buf = new StringBuilder();
            foreach (String name in newEvent.EventType.PropertyNames)
            {
                buf.Append(' ');
                buf.Append(name);
                buf.Append("=");
                buf.Append(newEvent.Get(name));
            }
            return buf.ToString();
        }

        public static void Print(String title, EventBean[] events)
        {
            for (int i = 0; i < events.Length; i++)
            {
                Log.Info(".print " + title + " Event#" + i + " : " + DumpProperties(events[i]));
            }
        }
    }
}
