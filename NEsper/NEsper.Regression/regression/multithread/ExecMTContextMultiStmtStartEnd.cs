///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    public class ExecMTContextMultiStmtStartEnd : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            RunAssertion(ConfigurationEngineDefaults.FilterServiceProfile.READMOSTLY);
            RunAssertion(ConfigurationEngineDefaults.FilterServiceProfile.READWRITE);
        }
    
        private void RunAssertion(ConfigurationEngineDefaults.FilterServiceProfile profile) {
            var configuration = new Configuration(SupportContainer.Instance);
            configuration.EngineDefaults.Execution.FilterServiceProfile = profile;
            var engineURI = this.GetType().Name + "_" + profile;
            var engine = EPServiceProviderManager.GetProvider(
                SupportContainer.Instance, engineURI, configuration);
            engine.EPAdministrator.Configuration.AddEventType(typeof(MyEvent));
    
            engine.EPAdministrator.CreateEPL("create context MyContext start @now end after 100 milliseconds");
            var listeners = new SupportUpdateListener[100];
            for (var i = 0; i < 100; i++) {
                listeners[i] = new SupportUpdateListener();
                var stmt = engine.EPAdministrator.CreateEPL("context MyContext select FieldOne, count(*) as cnt from MyEvent " +
                        "group by FieldOne output last when terminated");
                stmt.Events += listeners[i].Update;
            }
    
            var eventCount = 100000; // keep this divisible by 1000
            for (var i = 0; i < eventCount; i++) {
                var group = Convert.ToString(eventCount % 1000);
                engine.EPRuntime.SendEvent(new MyEvent(Convert.ToString(i), group));
            }
    
            Thread.Sleep(2000);
            engine.Dispose();
    
            AssertReceived(eventCount, listeners);
        }
    
        private void AssertReceived(int eventCount, SupportUpdateListener[] listeners) {
            foreach (var listener in listeners) {
                var outputEvents = listener.GetNewDataListFlattened();
                long total = 0;
    
                foreach (var @out in outputEvents) {
                    var cnt = (long) @out.Get("cnt");
                    total += cnt;
                }
    
                if (total != eventCount) {
                    Assert.Fail("Listener received " + total + " expected " + eventCount);
                }
            }
        }
    
        public class MyEvent {
            private readonly string _id;
            private readonly string _fieldOne;
    
            public MyEvent(string id, string fieldOne) {
                this._id = id;
                this._fieldOne = fieldOne;
            }

            public string Id => _id;

            public string FieldOne => _fieldOne;
        }
    }
} // end of namespace
