///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprBigNumberSupport : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBeanNumeric));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionEquals(epService);
            RunAssertionRelOp(epService);
            RunAssertionBetween(epService);
            RunAssertionIn(epService);
            RunAssertionMath(epService);
            RunAssertionAggregation(epService);
            RunAssertionMinMax(epService);
            RunAssertionFilterEquals(epService);
            RunAssertionJoin(epService);
            RunAssertionCastAndUDF(epService);
        }
    
        private void RunAssertionEquals(EPServiceProvider epService) {
            // test equals decimal
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBeanNumeric where bigdec = 1 or bigdec = intOne or bigdec = doubleOne");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            SendBigNumEvent(epService, -1, 1);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            SendBigNumEvent(epService, -1, 2);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, 0, null, 2m, 0, 0));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(3, 0, null, new decimal(2), 0, 0));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(0, 0, null, new decimal(3d), 3d, 0));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(0, 0, null, new decimal(3.9999d), 4d, 0));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            // test equals BigInteger
            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL("select * from SupportBeanNumeric where bigdec = bigint or bigint = intOne or bigint = 1");
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(0, 0, new BigInteger(2), new decimal(2), 0, 0));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(0, 0, new BigInteger(3), new decimal(2), 0, 0));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, 0, new BigInteger(2), null, 0, 0));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(3, 0, new BigInteger(2), null, 0, 0));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(0, 0, new BigInteger(1), null, 0, 0));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(0, 0, new BigInteger(4), null, 0, 0));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        private void RunAssertionRelOp(EPServiceProvider epService) {
            // relational op tests handled by relational op unit test
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBeanNumeric where bigdec < 10 and bigint > 10");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            SendBigNumEvent(epService, 10, 10);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBigNumEvent(epService, 11, 9);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            stmt.Dispose();
    
            stmt = epService.EPAdministrator.CreateEPL("select * from SupportBeanNumeric where bigdec < 10.0");
            stmt.AddListener(listener);
    
            SendBigNumEvent(epService, 0, 11);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(null, new decimal(9.999)));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            stmt.Dispose();
    
            // test float
            stmt = epService.EPAdministrator.CreateEPL("select * from SupportBeanNumeric where floatOne < 10f and floatTwo > 10f");
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(true, 1f, 20f));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(true, 20f, 1f));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        private void RunAssertionBetween(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBeanNumeric where bigdec between 10 and 20 or bigint between 100 and 200");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            SendBigNumEvent(epService, 0, 9);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBigNumEvent(epService, 0, 10);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendBigNumEvent(epService, 99, 0);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBigNumEvent(epService, 100, 0);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        private void RunAssertionIn(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBeanNumeric where bigdec in (10, 20d) or bigint in (0x02, 3)");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            SendBigNumEvent(epService, 0, 9);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBigNumEvent(epService, 0, 10);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(null, new decimal(20d)));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendBigNumEvent(epService, 99, 0);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBigNumEvent(epService, 2, 0);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendBigNumEvent(epService, 3, 0);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        private void RunAssertionMath(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBeanNumeric " +
                    "where bigdec+bigint=100 or bigdec+1=2 or bigdec+2d=5.0 or bigint+5L=8 or bigint+5d=9.0");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            SendBigNumEvent(epService, 50, 49);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBigNumEvent(epService, 50, 50);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendBigNumEvent(epService, 0, 1);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendBigNumEvent(epService, 0, 2);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBigNumEvent(epService, 0, 3);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendBigNumEvent(epService, 0, 0);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBigNumEvent(epService, 3, 0);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendBigNumEvent(epService, 4, 0);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            stmt.Dispose();
    
            stmt = epService.EPAdministrator.CreateEPL(
                    "select bigdec+bigint as v1, bigdec+2 as v2, bigdec+3d as v3, bigint+5L as v4, bigint+5d as v5 " +
                            " from SupportBeanNumeric");
            stmt.AddListener(listener);
            listener.Reset();
    
            Assert.AreEqual(typeof(decimal), stmt.EventType.GetPropertyType("v1"));
            Assert.AreEqual(typeof(decimal), stmt.EventType.GetPropertyType("v2"));
            Assert.AreEqual(typeof(decimal), stmt.EventType.GetPropertyType("v3"));
            Assert.AreEqual(typeof(BigInteger), stmt.EventType.GetPropertyType("v4"));
            Assert.AreEqual(typeof(decimal), stmt.EventType.GetPropertyType("v5"));
    
            SendBigNumEvent(epService, 1, 2);
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, "v1,v2,v3,v4,v5".Split(','),
                    new Object[]{new decimal(3), new decimal(4), new decimal(5d), new BigInteger(6), new decimal(6d)});
    
            // test aggregation-sum, multiplication and division all together; test for ESPER-340
            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL(
                    "select (sum(bigdecTwo * bigdec)/sum(bigdec)) as avgRate from SupportBeanNumeric");
            stmt.AddListener(listener);
            listener.Reset();
            Assert.AreEqual(typeof(decimal), stmt.EventType.GetPropertyType("avgRate"));
            SendBigNumEvent(epService, 0, 5);
            Object avgRate = listener.AssertOneGetNewAndReset().Get("avgRate");
            Assert.IsTrue(avgRate is decimal);
            Assert.AreEqual(new decimal(5d), avgRate);
    
            stmt.Dispose();
        }
    
        private void RunAssertionAggregation(EPServiceProvider epService) {
            string fields = "sum(bigint),sum(bigdec)," +
                    "avg(bigint),avg(bigdec)," +
                    "median(bigint),median(bigdec)," +
                    "stddev(bigint),stddev(bigdec)," +
                    "avedev(bigint),avedev(bigdec)," +
                    "min(bigint),min(bigdec)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select " + fields + " from SupportBeanNumeric");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            listener.Reset();
    
            string[] fieldList = fields.Split(',');
            SendBigNumEvent(epService, 1, 2);
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, fieldList,
                    new Object[]{
                        new BigInteger(1), new decimal(2d),        // sum
                        new decimal(1), new decimal(2),               // avg
                        1d, 2d,               // median
                        null, null,
                        0.0, 0.0,
                        new BigInteger(1), new decimal(2),
                    });
    
            stmt.Dispose();
        }
    
        private void RunAssertionMinMax(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select min(bigint, 10) as v1, min(10, bigint) as v2, " +
                            "max(bigdec, 10) as v3, max(10, 100d, bigint, bigdec) as v4 from SupportBeanNumeric");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            listener.Reset();
    
            string[] fieldList = "v1,v2,v3,v4".Split(',');
    
            SendBigNumEvent(epService, 1, 2);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldList,
                    new Object[]{new BigInteger(1), new BigInteger(1), new decimal(10), new decimal(100d)});
    
            SendBigNumEvent(epService, 40, 300);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldList,
                    new Object[]{new BigInteger(10), new BigInteger(10), new decimal(300), new decimal(300)});
    
            SendBigNumEvent(epService, 250, 200);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldList,
                    new Object[]{new BigInteger(10), new BigInteger(10), new decimal(200), new decimal(250)});
    
            stmt.Dispose();
        }
    
        private void RunAssertionFilterEquals(EPServiceProvider epService) {
            string[] fieldList = "bigdec".Split(',');
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select bigdec from SupportBeanNumeric(bigdec = 4)");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            SendBigNumEvent(epService, 0, 2);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBigNumEvent(epService, 0, 4);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldList, new Object[]{new decimal(4)});
    
            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL("select bigdec from SupportBeanNumeric(bigdec = 4d)");
            stmt.AddListener(listener);
    
            SendBigNumEvent(epService, 0, 4);
            Assert.IsTrue(listener.IsInvoked);
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(new BigInteger(0), new decimal(4d)));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldList, new Object[]{new decimal(4d)});
    
            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL("select bigdec from SupportBeanNumeric(bigint = 4)");
            stmt.AddListener(listener);
    
            SendBigNumEvent(epService, 3, 4);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBigNumEvent(epService, 4, 3);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldList, new Object[]{new decimal(3)});
    
            stmt.Dispose();
        }
    
        private void RunAssertionJoin(EPServiceProvider epService) {
            string[] fieldList = "bigint,bigdec".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select bigint,bigdec from SupportBeanNumeric#Keepall(), SupportBean#keepall " +
                    "where intPrimitive = bigint and doublePrimitive = bigdec");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            SendSupportBean(epService, 2, 3);
            SendBigNumEvent(epService, 0, 2);
            SendBigNumEvent(epService, 2, 0);
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(new BigInteger(2), new decimal(3d)));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldList, new Object[]{new BigInteger(2), new decimal(3d)});
    
            stmt.Dispose();
        }
    
        private void RunAssertionCastAndUDF(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddImport(typeof(SupportStaticMethodLib).FullName);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select SupportStaticMethodLib.MyBigIntFunc(Cast(2, BigInteger)) as v1, SupportStaticMethodLib.MyBigDecFunc(Cast(3d, decimal)) as v2 from SupportBeanNumeric");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            string[] fieldList = "v1,v2".Split(',');
            SendBigNumEvent(epService, 0, 2);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldList, new Object[]
            {
                new BigInteger(2), new decimal(3.0)
            });
    
            stmt.Dispose();
        }
    
        private void SendBigNumEvent(EPServiceProvider epService, int bigInt, double decimalTwo) {
            var bean = new SupportBeanNumeric(new BigInteger(bigInt), new decimal(decimalTwo));
            bean.DecimalTwo = new decimal(decimalTwo);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBean(EPServiceProvider epService, int intPrimitive, double doublePrimitive) {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            bean.DoublePrimitive = doublePrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
