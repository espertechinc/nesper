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

namespace com.espertech.esper.regression.epl.join
{
    public class ExecJoinPropertyAccess : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionRegularJoin(epService);
            RunAssertionOuterJoin(epService);
        }
    
        private void RunAssertionRegularJoin(EPServiceProvider epService) {
            SupportBeanCombinedProps combined = SupportBeanCombinedProps.MakeDefaultBean();
            SupportBeanComplexProps complex = SupportBeanComplexProps.MakeDefaultBean();
            Assert.AreEqual("0ma0", combined.GetIndexed(0).GetMapped("0ma").Value);
    
            string epl = "select nested.nested, s1.indexed[0], nested.indexed[1] from " +
                    typeof(SupportBeanComplexProps).FullName + "#length(3) nested, " +
                    typeof(SupportBeanCombinedProps).FullName + "#length(3) s1" +
                    " where Mapped('keyOne') = indexed[2].Mapped('2ma').value and" +
                    " indexed[0].Mapped('0ma').value = '0ma0'";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            epService.EPRuntime.SendEvent(combined);
            epService.EPRuntime.SendEvent(complex);
    
            EventBean theEvent = testListener.GetAndResetLastNewData()[0];
            Assert.AreSame(complex.Nested, theEvent.Get("nested.nested"));
            Assert.AreSame(combined.GetIndexed(0), theEvent.Get("s1.indexed[0]"));
            Assert.AreEqual(complex.GetIndexed(1), theEvent.Get("nested.indexed[1]"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionOuterJoin(EPServiceProvider epService) {
            string epl = "select * from " +
                    typeof(SupportBeanComplexProps).FullName + "#length(3) s0" +
                    " left outer join " +
                    typeof(SupportBeanCombinedProps).FullName + "#length(3) s1" +
                    " on Mapped('keyOne') = indexed[2].Mapped('2ma').value";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            SupportBeanCombinedProps combined = SupportBeanCombinedProps.MakeDefaultBean();
            epService.EPRuntime.SendEvent(combined);
            SupportBeanComplexProps complex = SupportBeanComplexProps.MakeDefaultBean();
            epService.EPRuntime.SendEvent(complex);
    
            // double check that outer join criteria match
            Assert.AreEqual(complex.GetMapped("keyOne"), combined.GetIndexed(2).GetMapped("2ma").Value);
    
            EventBean theEvent = testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("Simple", theEvent.Get("s0.SimpleProperty"));
            Assert.AreSame(complex, theEvent.Get("s0"));
            Assert.AreSame(combined, theEvent.Get("s1"));
    
            stmt.Dispose();
        }
    }
} // end of namespace
