///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewInheritAndInterface : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionOverridingSubclass(epService);
            RunAssertionImplementationClass(epService);
        }
    
        private void RunAssertionOverridingSubclass(EPServiceProvider epService) {
            string epl = "select val as value from " +
                    typeof(SupportOverrideOne).FullName + "#length(10)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            epService.EPRuntime.SendEvent(new SupportOverrideOneA("valA", "valOne", "valBase"));
            EventBean theEvent = testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("valA", theEvent.Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportOverrideBase("x"));
            Assert.IsFalse(testListener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportOverrideOneB("valB", "valTwo", "valBase2"));
            theEvent = testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("valB", theEvent.Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportOverrideOne("valThree", "valBase3"));
            theEvent = testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("valThree", theEvent.Get("value"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionImplementationClass(EPServiceProvider epService) {
            string[] epls = {
                    "select baseAB from " + typeof(ISupportBaseAB).FullName + "#length(10)",
                    "select baseAB, a from " + typeof(ISupportA).FullName + "#length(10)",
                    "select baseAB, b from " + typeof(ISupportB).FullName + "#length(10)",
                    "select c from " + typeof(ISupportC).FullName + "#length(10)",
                    "select baseAB, a, g from " + typeof(ISupportAImplSuperG).FullName + "#length(10)",
                    "select baseAB, a, b, g, c from " + typeof(ISupportAImplSuperGImplPlus).FullName + "#length(10)",
            };
    
            string[][] expected = {
                new string[]{"baseAB"},
                new string[]{"baseAB", "a"},
                new string[]{"baseAB", "b"},
                new string[]{"c"},
                new string[]{"baseAB", "a", "g"},
                new string[]{"baseAB", "a", "b", "g", "c"}
            };
    
            EPStatement[] stmts = new EPStatement[epls.Length];
            var listeners = new SupportUpdateListener[epls.Length];
            for (int i = 0; i < epls.Length; i++) {
                stmts[i] = epService.EPAdministrator.CreateEPL(epls[i]);
                listeners[i] = new SupportUpdateListener();
                stmts[i].Events += listeners[i].Update;
            }
    
            epService.EPRuntime.SendEvent(new ISupportAImplSuperGImplPlus("g", "a", "baseAB", "b", "c"));
            for (int i = 0; i < listeners.Length; i++) {
                Assert.IsTrue(listeners[i].IsInvoked);
                EventBean theEvent = listeners[i].GetAndResetLastNewData()[0];
    
                for (int j = 0; j < expected[i].Length; j++) {
                    Assert.IsTrue(theEvent.EventType.IsProperty(expected[i][j]), "failed property valid check for stmt=" + epls[i]);
                    Assert.AreEqual(expected[i][j], theEvent.Get(expected[i][j]), "failed property check for stmt=" + epls[i]);
                }
            }
        }
    }
} // end of namespace
