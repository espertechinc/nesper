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
    public class TestEnumOrderBy
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("Bean", typeof(SupportBean_ST0_Container));
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
        public void TestOrderByEvents()
        {
            String[] fields = "val0,val1,val2,val3,val4,val5".Split(',');
            String eplFragment = "select " +
                    "contained.orderBy(x => p00) as val0," +
                    "contained.orderBy(x => 10 - p00) as val1," +
                    "contained.orderBy(x => 0) as val2," +
                    "contained.orderByDesc(x => p00) as val3," +
                    "contained.orderByDesc(x => 10 - p00) as val4," +
                    "contained.orderByDesc(x => 0) as val5" +
                    " from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields,
                new Type[]
                {
                    typeof (ICollection<SupportBean_ST0>),
                    typeof (ICollection<SupportBean_ST0>),
                    typeof (ICollection<SupportBean_ST0>),
                    typeof (ICollection<SupportBean_ST0>),
                    typeof (ICollection<SupportBean_ST0>),
                    typeof (ICollection<SupportBean_ST0>)
                });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2"));
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "E1,E2");
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "E2,E1");
            LambdaAssertionUtil.AssertST0Id(_listener, "val2", "E1,E2");
            LambdaAssertionUtil.AssertST0Id(_listener, "val3", "E2,E1");
            LambdaAssertionUtil.AssertST0Id(_listener, "val4", "E1,E2");
            LambdaAssertionUtil.AssertST0Id(_listener, "val5", "E1,E2");
            _listener.Reset();

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E3,1", "E2,2", "E4,1", "E1,2"));
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "E3,E4,E2,E1");
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "E2,E1,E3,E4");
            LambdaAssertionUtil.AssertST0Id(_listener, "val2", "E3,E2,E4,E1");
            LambdaAssertionUtil.AssertST0Id(_listener, "val3", "E2,E1,E3,E4");
            LambdaAssertionUtil.AssertST0Id(_listener, "val4", "E3,E4,E2,E1");
            LambdaAssertionUtil.AssertST0Id(_listener, "val5", "E3,E2,E4,E1");
            _listener.Reset();

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            foreach (String field in fields)
            {
                LambdaAssertionUtil.AssertST0Id(_listener, field, null);
            }
            _listener.Reset();

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            foreach (String field in fields)
            {
                LambdaAssertionUtil.AssertST0Id(_listener, field, "");
            }
            _listener.Reset();
        }

        [Test]
        public void TestOrderByScalar()
        {
            String[] fields = "val0,val1".Split(',');
            String eplFragment = "select " +
                    "Strvals.orderBy() as val0, " +
                    "Strvals.OrderByDesc() as val1 " +
                    "from SupportCollection";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields,
                new Type[]
                {
                    typeof(ICollection<string>),
                    typeof(ICollection<string>)
                });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E5,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", "E1", "E2", "E4", "E5");
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val1", "E5", "E4", "E2", "E1");
            _listener.Reset();

            LambdaAssertionUtil.AssertSingleAndEmptySupportColl(_epService, _listener, fields);
            stmtFragment.Dispose();

            // test scalar-coll with lambda
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractNum", typeof(TestEnumMinMax.MyService).FullName, "ExtractNum");
            String eplLambda = "select " +
                    "Strvals.orderBy(v => extractNum(v)) as val0, " +
                    "Strvals.orderByDesc(v => extractNum(v)) as val1 " +
                    "from SupportCollection";
            EPStatement stmtLambda = _epService.EPAdministrator.CreateEPL(eplLambda);
            stmtLambda.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtLambda.EventType, fields, new []{typeof(ICollection<string>), typeof(ICollection<string>)});

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E5,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", "E1", "E2", "E4", "E5");
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val1", "E5", "E4", "E2", "E1");
            _listener.Reset();

            LambdaAssertionUtil.AssertSingleAndEmptySupportColl(_epService, _listener, fields);
        }

        [Test]
        public void TestInvalid()
        {
            String epl;

            epl = "select contained.orderBy() from Bean";
            TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'contained.orderBy()': Invalid input for built-in enumeration method 'orderBy' and 0-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type '" + Name.Of<SupportBean_ST0>() + "' [select contained.orderBy() from Bean]");
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
