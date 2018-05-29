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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestDecimalSupport 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            _listener = new SupportUpdateListener();
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanNumeric", typeof(SupportBeanNumeric));
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
        }

        [Test]
        public void TestEquals()
        {
            // test equals BigDecimal
            EPStatement stmt =
                _epService.EPAdministrator.CreateEPL(
                    "select * from SupportBeanNumeric where DecimalOne = 1 or DecimalOne = IntOne or DecimalOne = DoubleOne");
            stmt.Events += _listener.Update;

            SendDecimalEvent(1);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            SendDecimalEvent(2);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, 0, 2m, 0, 0));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(3, 0, 2m, 0, 0));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(0, 0, 3m, 3d, 0));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(0, 0, 3.9999m, 4d, 0));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
        }

        [Test]
        public void TestRelOp()
        {
            // relational op tests handled by relational op unit test
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from SupportBeanNumeric where DecimalOne < 10");
            stmt.Events += _listener.Update;
    
            SendDecimalEvent(10);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            SendDecimalEvent(9);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            stmt.Dispose();
    
            stmt = _epService.EPAdministrator.CreateEPL(
                "select * from SupportBeanNumeric where DecimalOne < 10.0");
            stmt.Events += _listener.Update;
    
            SendDecimalEvent(11);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(9.999m));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            stmt.Dispose();
        }
    
        [Test]
        public void TestBetween()
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from SupportBeanNumeric where DecimalOne between 10 and 20");
            stmt.Events += _listener.Update;
    
            SendDecimalEvent(9);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            SendDecimalEvent(10);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            SendDecimalEvent(0);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            SendDecimalEvent(20);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            stmt.Dispose();
        }
    
        [Test]
        public void TestIn()
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from SupportBeanNumeric where DecimalOne in (10, 20d)");
            stmt.Events += _listener.Update;
    
            SendDecimalEvent(9);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            SendDecimalEvent(10);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(20.0m));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            SendDecimalEvent(0);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        [Test]
        public void TestMath()
        {
            using(var stmt = _epService.EPAdministrator.CreateEPL(
                        "select * from SupportBeanNumeric " +
                        "where DecimalOne=100 or DecimalOne+1=2 or DecimalOne+2d=5.0")) {
                stmt.Events += _listener.Update;

                SendDecimalEvent(49);
                Assert.IsFalse(_listener.GetAndClearIsInvoked());

                SendDecimalEvent(50);
                Assert.IsFalse(_listener.GetAndClearIsInvoked());

                SendDecimalEvent(1);
                Assert.IsTrue(_listener.GetAndClearIsInvoked());

                SendDecimalEvent(2);
                Assert.IsFalse(_listener.GetAndClearIsInvoked());

                SendDecimalEvent(3);
                Assert.IsTrue(_listener.GetAndClearIsInvoked());

                SendDecimalEvent(0);
                Assert.IsFalse(_listener.GetAndClearIsInvoked());
            }
    
            using(var stmt = _epService.EPAdministrator.CreateEPL(
                    "select DecimalOne as v1, DecimalOne+2 as v2, DecimalOne+3d as v3 " +
                    " from SupportBeanNumeric")) {
                stmt.Events += _listener.Update;
                _listener.Reset();

                Assert.AreEqual(typeof(decimal?), stmt.EventType.GetPropertyType("v1"));
                Assert.AreEqual(typeof(decimal?), stmt.EventType.GetPropertyType("v2"));
                Assert.AreEqual(typeof(decimal?), stmt.EventType.GetPropertyType("v3"));

                SendDecimalEvent(2);
                EventBean theEvent = _listener.AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(theEvent, "v1,v2,v3".Split(','),
                                               new object[] {2m, 4m, 5m});
            }

            // test aggregation-sum, multiplication and division all together; test for ESPER-340
            using(var stmt = _epService.EPAdministrator.CreateEPL("select (sum(DecimalTwo * DecimalOne)/sum(DecimalOne)) as avgRate from SupportBeanNumeric")) {
                stmt.Events += _listener.Update;
                _listener.Reset();
                Assert.AreEqual(typeof(decimal?), stmt.EventType.GetPropertyType("avgRate"));
                SendDecimalEvent(5, 5);
                var avgRate = _listener.AssertOneGetNewAndReset().Get("avgRate");
                Assert.IsTrue(avgRate is decimal?);
                Assert.AreEqual(5.0m, avgRate);
            }
        }
    
        [Test]
        public void TestAggregation()
        {
            String fields =
                    "sum(DecimalOne)," +
                    "avg(DecimalOne)," +
                    "median(DecimalOne)," +
                    "stddev(DecimalOne)," +
                    "avedev(DecimalOne)," +
                    "min(DecimalOne)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select " + fields + " from SupportBeanNumeric");
            stmt.Events += _listener.Update;
            _listener.Reset();
    
            String[] fieldList = fields.Split(',');
            SendDecimalEvent(2);
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(
                theEvent, fieldList,
                new object[] {
                    2m, // sum
                    2m, // avg
                    2d, // median
                    null,
                    0.0,
                    2m,
                });
        }
    
        [Test]
        public void TestMinMax()
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select max(DecimalOne, 10) as v3, max(10, 100d, DecimalOne) as v4 from SupportBeanNumeric");
            stmt.Events += _listener.Update;
            _listener.Reset();
    
            String[] fieldList = "v3,v4".Split(',');
    
            SendDecimalEvent(2);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldList,
                    new object[] {10m, 100m});
    
            SendDecimalEvent(300);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldList,
                    new object[] {300m, 300m});
    
            SendDecimalEvent(50);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldList,
                    new object[] {50m, 100m});
        }

        [Test]
        public void TestFilterEquals()
        {
            String[] fieldList = "DecimalOne".Split(',');

            EPStatement stmt =
                _epService.EPAdministrator.CreateEPL("select DecimalOne from SupportBeanNumeric(DecimalOne = 4)");
            stmt.Events += _listener.Update;

            SendDecimalEvent(2);
            Assert.IsFalse(_listener.IsInvoked);

            SendDecimalEvent(4);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldList, new object[] {4m});

            stmt.Dispose();
            stmt = _epService.EPAdministrator.CreateEPL("select DecimalOne from SupportBeanNumeric(DecimalOne = 4d)");
            stmt.Events += _listener.Update;

            SendDecimalEvent(4);
            Assert.IsTrue(_listener.IsInvoked);
            _listener.Reset();

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(4m));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldList, new object[] {4m});

            stmt.Dispose();
        }

        [Test]
        public void TestJoin()
        {
            String[] fieldList = "DecimalOne".Split(',');
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select DecimalOne from SupportBeanNumeric#keepall, SupportBean#keepall " +
                    "where DoublePrimitive = DecimalOne");
            stmt.Events += _listener.Update;
    
            SendSupportBean(2, 3);
            SendDecimalEvent(2);
            SendDecimalEvent(0);
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(3m));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldList, new object[] {3m});
        }
    
        [Test]
        public void TestCastAndUDF()
        {
            _epService.EPAdministrator.Configuration.AddImport(typeof(SupportStaticMethodLib).FullName);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select SupportStaticMethodLib.MyDecimalFunc(cast(3d, decimal)) as v2 from SupportBeanNumeric");
            stmt.Events += _listener.Update;
    
            String[] fieldList = "v2".Split(',');
            SendDecimalEvent(2);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldList, new object[] {3.0m});
        }

        private void SendDecimalEvent(decimal decim1, decimal decim2)
        {
            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(decim1, decim2));
        }

        private void SendDecimalEvent(double decim)
        {
            _epService.EPRuntime.SendEvent(new SupportBeanNumeric((decimal) decim));
        }
    
        private void SendSupportBean(int IntPrimitive, double DoublePrimitive)
        {
            SupportBean bean = new SupportBean();
            bean.IntPrimitive = IntPrimitive;
            bean.DoublePrimitive = DoublePrimitive;
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
