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
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    public class ExecEPLForGroupDelivery : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionInvalid(epService);
            RunAssertionSubscriberOnly(epService);
            RunAssertionDiscreteDelivery(epService);
            RunAssertionGroupDelivery(epService);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            TryInvalid(epService, "select * from SupportBean for ",
                    "Incorrect syntax near end-of-input expecting an identifier but found end-of-input at line 1 column 30 [select * from SupportBean for ]");
    
            TryInvalid(epService, "select * from SupportBean for other_keyword",
                    "Error starting statement: Expected any of the [grouped_delivery, discrete_delivery] for-clause keywords after reserved keyword 'for' [select * from SupportBean for other_keyword]");
    
            TryInvalid(epService, "select * from SupportBean for grouped_delivery",
                    "Error starting statement: The for-clause with the grouped_delivery keyword requires one or more grouping expressions [select * from SupportBean for grouped_delivery]");
    
            TryInvalid(epService, "select * from SupportBean for grouped_delivery()",
                    "Error starting statement: The for-clause with the grouped_delivery keyword requires one or more grouping expressions [select * from SupportBean for grouped_delivery()]");
    
            TryInvalid(epService, "select * from SupportBean for grouped_delivery(dummy)",
                    "Error starting statement: Failed to validate for-clause expression 'dummy': Property named 'dummy' is not valid in any stream [select * from SupportBean for grouped_delivery(dummy)]");
    
            TryInvalid(epService, "select * from SupportBean for Discrete_delivery(dummy)",
                    "Error starting statement: The for-clause with the discrete_delivery keyword does not allow grouping expressions [select * from SupportBean for Discrete_delivery(dummy)]");
    
            TryInvalid(epService, "select * from SupportBean for discrete_delivery for grouped_delivery(IntPrimitive)",
                    "Incorrect syntax near 'for' (a reserved keyword) at line 1 column 48 ");
        }
    
        private void RunAssertionSubscriberOnly(EPServiceProvider epService) {
            var subscriber = new SupportSubscriberMRD();
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream TheString,IntPrimitive from SupportBean#time_batch(1) for discrete_delivery");
            stmt.Subscriber = subscriber;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            SendTimer(epService, 1000);
            Assert.AreEqual(3, subscriber.InsertStreamList.Count);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"E1", 1}, subscriber.InsertStreamList[0][0]);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"E2", 2}, subscriber.InsertStreamList[1][0]);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"E3", 1}, subscriber.InsertStreamList[2][0]);
    
            stmt.Dispose();
            subscriber.Reset();
            stmt = epService.EPAdministrator.CreateEPL("select irstream TheString,IntPrimitive from SupportBean#time_batch(1) for grouped_delivery(IntPrimitive)");
            stmt.Subscriber = subscriber;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            SendTimer(epService, 2000);
            Assert.AreEqual(2, subscriber.InsertStreamList.Count);
            Assert.AreEqual(2, subscriber.RemoveStreamList.Count);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"E1", 1}, subscriber.InsertStreamList[0][0]);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"E3", 1}, subscriber.InsertStreamList[0][1]);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"E2", 2}, subscriber.InsertStreamList[1][0]);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"E1", 1}, subscriber.RemoveStreamList[0][0]);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"E3", 1}, subscriber.RemoveStreamList[0][1]);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"E2", 2}, subscriber.RemoveStreamList[1][0]);
    
            stmt.Dispose();
        }
    
        private void RunAssertionDiscreteDelivery(EPServiceProvider epService) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean#time_batch(1) for discrete_delivery");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            SendTimer(epService, 1000);
            Assert.AreEqual(3, listener.NewDataList.Count);
            EPAssertionUtil.AssertPropsPerRow(listener.NewDataList[0], "TheString,IntPrimitive".Split(','), new object[][]{new object[] {"E1", 1}});
            EPAssertionUtil.AssertPropsPerRow(listener.NewDataList[1], "TheString,IntPrimitive".Split(','), new object[][]{new object[] {"E2", 2}});
            EPAssertionUtil.AssertPropsPerRow(listener.NewDataList[2], "TheString,IntPrimitive".Split(','), new object[][]{new object[] {"E3", 1}});
            listener.Reset();
    
            // test no-event delivery
            epService.EPAdministrator.Configuration.AddEventType("ObjectEvent", typeof(Object));
            string epl = "SELECT *  FROM ObjectEvent OUTPUT ALL EVERY 1 seconds for discrete_delivery";
            epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
            epService.EPRuntime.SendEvent(new Object());
            SendTimer(epService, 2000);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            SendTimer(epService, 3000);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionGroupDelivery(EPServiceProvider epService) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean#time_batch(1) for grouped_delivery (IntPrimitive)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            SendTimer(epService, 1000);
            Assert.AreEqual(2, listener.NewDataList.Count);
            Assert.AreEqual(2, listener.NewDataList[0].Length);
            EPAssertionUtil.AssertPropsPerRow(listener.NewDataList[0], "TheString,IntPrimitive".Split(','), new object[][]{new object[] {"E1", 1}, new object[] {"E3", 1}});
            Assert.AreEqual(1, listener.NewDataList[1].Length);
            EPAssertionUtil.AssertPropsPerRow(listener.NewDataList[1], "TheString,IntPrimitive".Split(','), new object[][]{new object[] {"E2", 2}});
    
            // test sorted
            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean#time_batch(1) order by IntPrimitive desc for grouped_delivery (IntPrimitive)");
            stmt.Events += listener.Update;
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            SendTimer(epService, 2000);
            Assert.AreEqual(2, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.NewDataList[0].Length);
            EPAssertionUtil.AssertPropsPerRow(listener.NewDataList[0], "TheString,IntPrimitive".Split(','), new object[][]{new object[] {"E2", 2}});
            Assert.AreEqual(2, listener.NewDataList[1].Length);
            EPAssertionUtil.AssertPropsPerRow(listener.NewDataList[1], "TheString,IntPrimitive".Split(','), new object[][]{new object[] {"E1", 1}, new object[] {"E3", 1}});
    
            // test multiple criteria
            stmt.Dispose();
            string stmtText = "select TheString, DoubleBoxed, enumValue from SupportBean#time_batch(1) order by TheString, DoubleBoxed, enumValue for grouped_delivery(DoubleBoxed, enumValue)";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
            listener.Reset();
    
            SendEvent(epService, "E1", 10d, SupportEnum.ENUM_VALUE_2); // A (1)
            SendEvent(epService, "E2", 11d, SupportEnum.ENUM_VALUE_1); // B (2)
            SendEvent(epService, "E3", 9d, SupportEnum.ENUM_VALUE_2);  // C (3)
            SendEvent(epService, "E4", 10d, SupportEnum.ENUM_VALUE_2); // A
            SendEvent(epService, "E5", 10d, SupportEnum.ENUM_VALUE_1); // D (4)
            SendEvent(epService, "E6", 10d, SupportEnum.ENUM_VALUE_1); // D
            SendEvent(epService, "E7", 11d, SupportEnum.ENUM_VALUE_1); // B
            SendEvent(epService, "E8", 10d, SupportEnum.ENUM_VALUE_1); // D
            SendTimer(epService, 3000);
            Assert.AreEqual(4, listener.NewDataList.Count);
            string[] fields = "TheString,DoubleBoxed,enumValue".Split(',');
            EPAssertionUtil.AssertPropsPerRow(listener.NewDataList[0], fields,
                    new object[][]{new object[] {"E1", 10d, SupportEnum.ENUM_VALUE_2}, new object[] {"E4", 10d, SupportEnum.ENUM_VALUE_2}});
            EPAssertionUtil.AssertPropsPerRow(listener.NewDataList[1], fields,
                    new object[][]{new object[] {"E2", 11d, SupportEnum.ENUM_VALUE_1}, new object[] {"E7", 11d, SupportEnum.ENUM_VALUE_1}});
            EPAssertionUtil.AssertPropsPerRow(listener.NewDataList[2], fields,
                    new object[][]{new object[] {"E3", 9d, SupportEnum.ENUM_VALUE_2}});
            EPAssertionUtil.AssertPropsPerRow(listener.NewDataList[3], fields,
                    new object[][]{new object[] {"E5", 10d, SupportEnum.ENUM_VALUE_1}, new object[] {"E6", 10d, SupportEnum.ENUM_VALUE_1}, new object[] {"E8", 10d, SupportEnum.ENUM_VALUE_1}});
    
            // test SODA
            stmt.Dispose();
            listener.Reset();
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1", 10d, SupportEnum.ENUM_VALUE_2); // A (1)
            SendEvent(epService, "E2", 11d, SupportEnum.ENUM_VALUE_1); // B (2)
            SendEvent(epService, "E3", 11d, SupportEnum.ENUM_VALUE_1); // B (2)
            SendTimer(epService, 4000);
            Assert.AreEqual(2, listener.NewDataList.Count);
            EPAssertionUtil.AssertPropsPerRow(listener.NewDataList[0], fields,
                    new object[][]{new object[] {"E1", 10d, SupportEnum.ENUM_VALUE_2}});
            EPAssertionUtil.AssertPropsPerRow(listener.NewDataList[1], fields,
                    new object[][]{new object[] {"E2", 11d, SupportEnum.ENUM_VALUE_1}, new object[] {"E3", 11d, SupportEnum.ENUM_VALUE_1}});
    
            stmt.Dispose();
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void SendEvent(EPServiceProvider epService, string theString, double? doubleBoxed, SupportEnum enumVal) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.DoubleBoxed = doubleBoxed;
            bean.EnumValue = enumVal;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
