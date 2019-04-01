///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientPatternGuardPlugIn : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddPlugInPatternGuard("myplugin", "count_to", typeof(MyCountToPatternGuardFactory));
            configuration.AddEventType("Bean", typeof(SupportBean));
            configuration.AddPlugInPatternGuard("namespace", "name", typeof(string));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionGuard(epService);
            RunAssertionGuardVariable(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionGuard(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientPatternGuardPlugIn))) {
                return;
            }
    
            string stmtText = "select * from pattern [(every Bean) where myplugin:count_to(10)]";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            for (int i = 0; i < 10; i++) {
                epService.EPRuntime.SendEvent(new SupportBean());
                Assert.IsTrue(listener.IsInvoked);
                listener.Reset();
            }
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(listener.IsInvoked);
    
            statement.Dispose();
        }
    
        private void RunAssertionGuardVariable(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientPatternGuardPlugIn))) {
                return;
            }
    
            epService.EPAdministrator.CreateEPL("create variable int COUNT_TO = 3");
            string stmtText = "select * from pattern [(every Bean) where myplugin:count_to(COUNT_TO)]";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            for (int i = 0; i < 3; i++) {
                epService.EPRuntime.SendEvent(new SupportBean());
                Assert.IsTrue(listener.IsInvoked);
                listener.Reset();
            }
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(listener.IsInvoked);
    
            statement.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientPatternGuardPlugIn))) {
                return;
            }
    
            try {
                string stmtText = "select * from pattern [every " + typeof(SupportBean).FullName + " where namespace:name(10)]";
                epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            } catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex,
                    "Failed to resolve pattern guard '"
                    + Name.Clean<SupportBean>()
                    + " where namespace:name(10)': Error invoking pattern object factory constructor for object 'name', no invocation access for Container.CreateInstance");
            }
        }
    }
} // end of namespace
