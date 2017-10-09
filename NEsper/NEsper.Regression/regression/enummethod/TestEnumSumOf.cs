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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.enummethod
{
    [TestFixture]
    public class TestEnumSumOf
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("Bean", typeof(SupportBean_Container));
            config.AddEventType("SupportCollection", typeof(SupportCollection));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestSumEvents()
        {
            String[] fields = "val0,val1,val2,val3".Split(',');
            String eplFragment = "select " +
                    "Beans.sumOf(x => IntBoxed) as val0," +
                    "Beans.sumOf(x => DoubleBoxed) as val1," +
                    "Beans.sumOf(x => LongBoxed) as val2," +
                    "Beans.sumOf(x => DecimalBoxed) as val3 " +
                    "from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(int?), typeof(double?), typeof(long?), typeof(decimal?) });

            _epService.EPRuntime.SendEvent(new SupportBean_Container(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, null, null, null });

            _epService.EPRuntime.SendEvent(new SupportBean_Container(new SupportBean[0]));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, null, null, null });

            List<SupportBean> list = new List<SupportBean>();
            list.Add(Make(2, 3d, 4L, 5));
            _epService.EPRuntime.SendEvent(new SupportBean_Container(list));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 2, 3d, 4L, 5m });

            list.Add(Make(4, 6d, 8L, 10));
            _epService.EPRuntime.SendEvent(new SupportBean_Container(list));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 2 + 4, 3d + 6d, 4L + 8L, 5m + 10m });
        }

        [Test]
        public void TestSumOfScalar()
        {
            String[] fields = "val0,val1".Split(',');
            String eplFragment = "select " +
                    "Intvals.sumOf() as val0, " +
                    "Bdvals.sumOf() as val1 " +
                    "from SupportCollection";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(int?), typeof(decimal?) });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric("1,4,5"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 1 + 4 + 5, 1m + 4m + 5m });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric("3,4"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 3 + 4, 3m + 4m });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric("3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 3, 3m });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric(""));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, null });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, null });

            stmtFragment.Dispose();

            // test average with lambda
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractNum", typeof(TestEnumMinMax.MyService).FullName, "ExtractNum");
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractDecimal", typeof(TestEnumMinMax.MyService).FullName, "ExtractDecimal");

            // lambda with string-array input
            String[] fieldsLambda = "val0,val1".SplitCsv();
            String eplLambda = "select " +
                    "Strvals.sumOf(v => extractNum(v)) as val0, " +
                    "Strvals.sumOf(v => extractDecimal(v)) as val1 " +
                    "from SupportCollection";
            EPStatement stmtLambda = _epService.EPAdministrator.CreateEPL(eplLambda);
            stmtLambda.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtLambda.EventType, fieldsLambda, new []{typeof(int?), typeof(decimal?)});

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E5,E4"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsLambda, new Object[] { 2 + 1 + 5 + 4, new decimal(2 + 1 + 5 + 4) });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsLambda, new Object[] { 1, new decimal(1) });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsLambda, new Object[] { null, null });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsLambda, new Object[]{null, null});
        }

        private SupportBean Make(int? intBoxed, double? doubleBoxed, long? longBoxed, int decimalBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.DoubleBoxed = doubleBoxed;
            bean.LongBoxed = longBoxed;
            bean.DecimalBoxed = decimalBoxed;
            return bean;
        }

        [Test]
        public void TestInvalid()
        {
            String epl;

            epl = "select Beans.sumof() from Bean";
            TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'Beans.sumof()': Invalid input for built-in enumeration method 'sumof' and 0-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type '" + Name.Of<SupportBean>() + "' [select Beans.sumof() from Bean]");
        }

        private void TryInvalid(String epl, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }
    }
}
