///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.fromclausemethod
{
    public class ExecFromClauseMethodCacheExpiry : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var methodConfig = new ConfigurationMethodRef();
            methodConfig.SetExpiryTimeCache(1, 10);
            configuration.AddMethodRef(typeof(SupportStaticMethodInvocations).FullName, methodConfig);
            configuration.AddImport(typeof(SupportStaticMethodInvocations).Namespace);
        }
    
        public override void Run(EPServiceProvider epService) {
            string joinStatement = "select id, p00, TheString from " +
                    typeof(SupportBean).FullName + "()#length(100) as s1, " +
                    " method:SupportStaticMethodInvocations.FetchObjectLog(TheString, IntPrimitive)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(joinStatement);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // set sleep off
            SupportStaticMethodInvocations.GetInvocationSizeAndReset();
    
            SendTimer(epService, 1000);
            var fields = new string[]{"id", "p00", "TheString"};
            SendBeanEvent(epService, "E1", 1);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, "|E1|", "E1"});
    
            SendTimer(epService, 1500);
            SendBeanEvent(epService, "E2", 2);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, "|E2|", "E2"});
    
            SendTimer(epService, 2000);
            SendBeanEvent(epService, "E3", 3);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3, "|E3|", "E3"});
            Assert.AreEqual(3, SupportStaticMethodInvocations.GetInvocationSizeAndReset());
    
            // should be cached
            SendBeanEvent(epService, "E3", 3);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3, "|E3|", "E3"});
            Assert.AreEqual(0, SupportStaticMethodInvocations.GetInvocationSizeAndReset());
    
            SendTimer(epService, 2100);
            // should not be cached
            SendBeanEvent(epService, "E4", 4);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{4, "|E4|", "E4"});
            Assert.AreEqual(1, SupportStaticMethodInvocations.GetInvocationSizeAndReset());
    
            // should be cached
            SendBeanEvent(epService, "E2", 2);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, "|E2|", "E2"});
            Assert.AreEqual(0, SupportStaticMethodInvocations.GetInvocationSizeAndReset());
    
            // should not be cached
            SendBeanEvent(epService, "E1", 1);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, "|E1|", "E1"});
            Assert.AreEqual(1, SupportStaticMethodInvocations.GetInvocationSizeAndReset());
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void SendBeanEvent(EPServiceProvider epService, string theString, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
