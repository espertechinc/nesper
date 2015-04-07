///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    [TestFixture]
    public class TestMTContextMultiStmtStartEnd
    {
        [Test]
        public void fTestContextMultistmt()
        {
            RunAssertion(ConfigurationEngineDefaults.FilterServiceProfile.READMOSTLY);
            RunAssertion(ConfigurationEngineDefaults.FilterServiceProfile.READWRITE);
        }
    
        private void RunAssertion(ConfigurationEngineDefaults.FilterServiceProfile profile) 
        {
            Configuration configuration = new Configuration();
            configuration.EngineDefaults.ExecutionConfig.FilterServiceProfile = profile;
            string engineURI = this.GetType().Name + "_" + profile;
            EPServiceProvider engine = EPServiceProviderManager.GetProvider(engineURI, configuration);
            engine.EPAdministrator.Configuration.AddEventType(typeof(MyEvent));
    
            engine.EPAdministrator.CreateEPL("create context MyContext start @now end after 100 milliseconds");
            SupportUpdateListener[] listeners = new SupportUpdateListener[100];
            for (int i = 0; i < 100; i++) {
                listeners[i] = new SupportUpdateListener();
                EPStatement stmt = engine.EPAdministrator.CreateEPL("context MyContext select FieldOne, count(*) as cnt from MyEvent " +
                        "group by FieldOne output last when terminated");
                stmt.Events += listeners[i].Update;
            }
    
            int eventCount = 100000; // keep this divisible by 1000
            for (int i = 0; i < eventCount; i++) {
                string group = (eventCount % 1000).ToString();
                engine.EPRuntime.SendEvent(new MyEvent(i.ToString(), group));
            }
    
            Thread.Sleep(2000);
            engine.Dispose();
    
            AssertReceived(eventCount, listeners);
        }
    
        private void AssertReceived(int eventCount, SupportUpdateListener[] listeners)
        {
            foreach (SupportUpdateListener listener in listeners) {
                EventBean[] outputEvents = listener.GetNewDataListFlattened();
                long total = 0;
    
                foreach (EventBean @out in outputEvents) {
                    long cnt = @out.Get("cnt").AsLong();
                    total += cnt;
                }
    
                if (total != eventCount) {
                    Assert.Fail("Listener received " + total + " expected " + eventCount);
                }
            }
        }
    
        public class MyEvent
        {
            public MyEvent(string id, string fieldOne)
            {
                Id = id;
                FieldOne = fieldOne;
            }

            public string Id { get; private set; }

            public string FieldOne { get; private set; }
        }
    }
}
