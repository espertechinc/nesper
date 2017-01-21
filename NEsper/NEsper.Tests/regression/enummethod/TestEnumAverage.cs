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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.bean.lambda;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.enummethod
{
    [TestFixture]
    public class TestEnumAverage
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
        public void TestAverageEvents()
        {
            String[] fields = "val0,val1,val2,val3".Split(',');
            String eplFragment = "select " +
                    "Beans.average(x => IntBoxed) as val0," +
                    "Beans.average(x => DoubleBoxed) as val1," +
                    "Beans.average(x => LongBoxed) as val2," +
                    "Beans.average(x => DecimalBoxed) as val3 " +
                    "from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(double?), typeof(double?), typeof(double?), typeof(decimal?) });

            _epService.EPRuntime.SendEvent(new SupportBean_Container(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, null, null, null });

            _epService.EPRuntime.SendEvent(new SupportBean_Container(new SupportBean[0]));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, null, null, null });

            List<SupportBean> list = new List<SupportBean>();
            list.Add(Make(2, 3d, 4l, 5));
            _epService.EPRuntime.SendEvent(new SupportBean_Container(list));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 2d, 3d, 4d, 5.0m });

            list.Add(Make(4, 6d, 8l, 10));
            _epService.EPRuntime.SendEvent(new SupportBean_Container(list));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { (2 + 4) / 2d, (3d + 6d) / 2d, (4L + 8L) / 2d, (5 + 10) / 2m });
        }

        [Test]
        public void TestAverageScalar()
        {
            String[] fields = "val0,val1".Split(',');
            String eplFragment = "select " +
                    "Intvals.average() as val0," +
                    "Bdvals.average() as val1 " +
                    "from SupportCollection";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(double?), typeof(decimal?) });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric("1,2,3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 2d, 2m });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric("1,null,3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 2d, 2m });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric("4"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 4d, 4m });

            stmtFragment.Dispose();

            // test average with lambda
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractNum", typeof(TestEnumMinMax.MyService).FullName, "ExtractNum");
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractDecimal", typeof(TestEnumMinMax.MyService).FullName, "ExtractDecimal");

            String[] fieldsLambda = "val0,val1".SplitCsv();
            String eplLambda = "select " +
                    "Strvals.average(v => extractNum(v)) as val0, " +
                    "Strvals.average(v => extractDecimal(v)) as val1 " +
                    "from SupportCollection";
            EPStatement stmtLambda = _epService.EPAdministrator.CreateEPL(eplLambda);
            stmtLambda.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtLambda.EventType, fieldsLambda, new []{typeof(double?), typeof(decimal?)});

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E5,E4"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsLambda, new Object[]{(2+1+5+4)/4d, new decimal((2+1+5+4)/4d)});

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsLambda, new Object[]{1d, new decimal(1)});

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsLambda, new Object[]{null, null});

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsLambda, new Object[]{null, null});
        }

        [Test]
        public void TestInvalid()
        {
            String epl;

            epl = "select Strvals.average() from SupportCollection";
            TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'Strvals.average()': Invalid input for built-in enumeration method 'average' and 0-parameter footprint, expecting collection of numeric values as input, received collection of String [select Strvals.average() from SupportCollection]");

            epl = "select Beans.average() from Bean";
            TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'Beans.average()': Invalid input for built-in enumeration method 'average' and 0-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type 'com.espertech.esper.support.bean.SupportBean' [select Beans.average() from Bean]");
        }

        private static SupportBean Make(int? intBoxed, double? doubleBoxed, long? longBoxed, int bigDecimal)
        {
            SupportBean bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.DoubleBoxed = doubleBoxed;
            bean.LongBoxed = longBoxed;
            bean.DecimalBoxed = bigDecimal;
            return bean;
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
