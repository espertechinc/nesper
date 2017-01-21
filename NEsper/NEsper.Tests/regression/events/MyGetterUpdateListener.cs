///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.client;

namespace com.espertech.esper.regression.events
{
    public class MyGetterUpdateListener
    {
        private readonly EventPropertyGetter symbolGetter;
        private readonly EventPropertyGetter volumeGetter;
    
        private String lastSymbol;
        private long? lastVolume;
        private EPStatement statement;
        private EPServiceProvider serviceProvider;
    
        public void Update(Object sender, UpdateEventArgs e)
        {
            statement = e.Statement;
            serviceProvider = e.ServiceProvider;
            lastSymbol = (string) symbolGetter.Get(e.NewEvents[0]);
            lastVolume = (long?) volumeGetter.Get(e.NewEvents[0]);
        }
    
        public MyGetterUpdateListener(EventType eventType)
        {
            symbolGetter = eventType.GetGetter("Symbol");
            volumeGetter = eventType.GetGetter("Volume");
        }

        public string LastSymbol
        {
            get { return lastSymbol; }
        }

        public long? LastVolume
        {
            get { return lastVolume; }
        }

        public EPStatement Statement
        {
            get { return statement; }
        }

        public EPServiceProvider ServiceProvider
        {
            get { return serviceProvider; }
        }
    }
}
