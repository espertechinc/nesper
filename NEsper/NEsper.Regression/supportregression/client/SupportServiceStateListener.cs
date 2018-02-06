///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.client
{
    public class SupportServiceStateListener
    {
        public SupportServiceStateListener()
        {
            DestroyedEvents = new List<EPServiceProvider>();
            InitializedEvents = new List<EPServiceProvider>();
        }

        public void OnEPServiceDestroyRequested(Object sender, ServiceProviderEventArgs serviceProviderEventArgs)
        {
            DestroyedEvents.Add(serviceProviderEventArgs.ServiceProvider);
        }
    
        public void OnEPServiceInitialized(Object sender, ServiceProviderEventArgs serviceProviderEventArgs)
        {
            InitializedEvents.Add(serviceProviderEventArgs.ServiceProvider);
        }
    
        public EPServiceProvider AssertOneGetAndResetDestroyedEvents()
        {
            Assert.AreEqual(1, DestroyedEvents.Count);
            EPServiceProvider item = DestroyedEvents[0];
            DestroyedEvents.Clear();
            return item;
        }
    
        public EPServiceProvider AssertOneGetAndResetInitializedEvents()
        {
            Assert.AreEqual(1, InitializedEvents.Count);
            EPServiceProvider item = InitializedEvents[0];
            InitializedEvents.Clear();
            return item;
        }

        public List<EPServiceProvider> DestroyedEvents { get; private set; }

        public List<EPServiceProvider> InitializedEvents { get; private set; }
    }
}
