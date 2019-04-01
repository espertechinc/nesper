///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client
{
    [Serializable]
    public sealed class UpdateEventArgs : EventArgs
    {
        public static readonly UpdateEventArgs EmptyArgs = new UpdateEventArgs(null, null, null, null);

        [NonSerialized]
        private EPStatement _statement;
        [NonSerialized]
        private EPServiceProvider _serviceProvider;

        public EPServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
            private set { _serviceProvider = value; }
        }

        public EPStatement Statement
        {
            get { return _statement; }
            private set { _statement = value; }
        }

        public EventBean[] NewEvents { get; private set; }
        public EventBean[] OldEvents { get; private set; }

        public UpdateEventArgs(EPServiceProvider serviceProvider,
                               EPStatement statement,
                               EventBean[] newEvents,
                               EventBean[] oldEvents)
        {
            ServiceProvider = serviceProvider;
            Statement = statement;
            NewEvents = newEvents;
            OldEvents = oldEvents;
        }
    }


    /// <summary>
    /// Defines a delegate that is notified of new and old events.
    /// </summary>
    /// <param name="sender">The event sender</param>
    /// <param name="e">The Update event arguments</param>

    public delegate void UpdateEventHandler(Object sender, UpdateEventArgs e);
}
