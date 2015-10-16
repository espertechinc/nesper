///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestOrderByEventPerGroup
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _testListener = null;
        }

        [Test]
        public void TestNoHavingNoJoin()
        {
            String statementString = "select irstream Symbol, sum(Price) as mysum from " +
                                    typeof(SupportMarketDataBean).FullName + ".win:length(20) " +
                                    "group by Symbol " +
                                    "output every 6 events " +
                                    "order by sum(Price), Symbol";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);

            RunAssertionNoHaving(statement);
        }

        [Test]
        public void TestHavingNoJoin()
        {
            String statementString = "select irstream Symbol, sum(Price) as mysum from " +
                                        typeof(SupportMarketDataBean).FullName + ".win:length(20) " +
                                        "group by Symbol " +
                                        "having sum(Price) > 0 " +
                                        "output every 6 events " +
                                        "order by sum(Price), Symbol";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            RunAssertionHaving(statement);
        }

        [Test]
        public void TestNoHavingJoin()
        {
            String statementString = "select irstream Symbol, sum(Price) as mysum from " +
                                typeof(SupportMarketDataBean).FullName + ".win:length(20) as one, " +
                                typeof(SupportBeanString).FullName + ".win:length(100) as two " +
                                "where one.Symbol = two.TheString " +
                                "group by Symbol " +
                                "output every 6 events " +
                                "order by sum(Price), Symbol";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);

            _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));

            RunAssertionNoHaving(statement);
        }

        [Test]
        public void TestHavingJoin()
        {
            String statementString = "select irstream Symbol, sum(Price) as mysum from " +
                typeof(SupportMarketDataBean).FullName + ".win:length(20) as one, " +
                typeof(SupportBeanString).FullName + ".win:length(100) as two " +
                "where one.Symbol = two.TheString " +
                "group by Symbol " +
                "having sum(Price) > 0 " +
                "output every 6 events " +
                "order by sum(Price), Symbol";

            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));

            RunAssertionHaving(statement);
        }

        [Test]
        public void TestHavingJoinAlias()
        {
            String statementString = "select irstream Symbol, sum(Price) as mysum from " +
                typeof(SupportMarketDataBean).FullName + ".win:length(20) as one, " +
                typeof(SupportBeanString).FullName + ".win:length(100) as two " +
                "where one.Symbol = two.TheString " +
                "group by Symbol " +
                "having sum(Price) > 0 " +
                "output every 6 events " +
                "order by mysum, Symbol";

            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));

            RunAssertionHaving(statement);
        }

        [Test]
        public void TestLast()
        {
            String statementString = "select irstream Symbol, sum(Price) as mysum from " +
                                        typeof(SupportMarketDataBean).FullName + ".win:length(20) " +
                                        "group by Symbol " +
                                        "output last every 6 events " +
                                        "order by sum(Price), Symbol";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            RunAssertionLast(statement);
        }

        [Test]
        public void TestLastJoin()
        {
            String statementString = "select irstream Symbol, sum(Price) as mysum from " +
                                    typeof(SupportMarketDataBean).FullName + ".win:length(20) as one, " +
                                    typeof(SupportBeanString).FullName + ".win:length(100) as two " +
                                    "where one.Symbol = two.TheString " +
                                    "group by Symbol " +
                                    "output last every 6 events " +
                                    "order by sum(Price), Symbol";

            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);

            _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));

            RunAssertionLast(statement);
        }

        [Test]
        public void TestIteratorGroupByEventPerGroup()
        {
            String[] fields = new String[] { "Symbol", "sumPrice" };
            String statementString = "select Symbol, sum(Price) as sumPrice from " +
                        typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
                        typeof(SupportBeanString).FullName + ".win:length(100) as two " +
                        "where one.Symbol = two.TheString " +
                        "group by Symbol " +
                        "order by Symbol";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);

            _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));

            SendEvent("CAT", 50);
            SendEvent("IBM", 49);
            SendEvent("CAT", 15);
            SendEvent("IBM", 100);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields,
                    new Object[][] {
                                    new Object[]{"CAT", 65d},
                                    new Object[]{"IBM", 149d},
                                    });

            SendEvent("KGB", 75);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields,
                    new Object[][] {
                                    new Object[]{"CAT", 65d},
                                    new Object[]{"IBM", 149d},
                                    new Object[]{"KGB", 75d},
                                    });
        }

        private void SendEvent(String symbol, double price)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, price, 0L, null);
            _epService.EPRuntime.SendEvent(bean);
        }

        private void RunAssertionLast(EPStatement statement)
        {
            String[] fields = "Symbol,mysum".Split(',');
            _testListener = new SupportUpdateListener();
            statement.Events += _testListener.Update;

            SendEvent("IBM", 3);
            SendEvent("IBM", 4);
            SendEvent("CMU", 1);
            SendEvent("CMU", 2);
            SendEvent("CAT", 5);
            SendEvent("CAT", 6);

            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields,
                    new Object[][] { new Object[] { "CMU", 3.0 }, new Object[] { "IBM", 7.0 }, new Object[] { "CAT", 11.0 } });
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastOldData, fields,
                    new Object[][] { new Object[] { "CAT", null }, new Object[] { "CMU", null }, new Object[] { "IBM", null }, });

            SendEvent("IBM", 3);
            SendEvent("IBM", 4);
            SendEvent("CMU", 5);
            SendEvent("CMU", 5);
            SendEvent("DOG", 0);
            SendEvent("DOG", 1);

            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields,
                    new Object[][] { new Object[] { "DOG", 1.0 }, new Object[] { "CMU", 13.0 }, new Object[] { "IBM", 14.0 } });
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastOldData, fields,
                    new Object[][] { new Object[] { "DOG", null }, new Object[] { "CMU", 3.0 }, new Object[] { "IBM", 7.0 } });
        }

        private void RunAssertionNoHaving(EPStatement statement)
        {
            String[] fields = "Symbol,mysum".Split(',');

            _testListener = new SupportUpdateListener();
            statement.Events += _testListener.Update;
            SendEvent("IBM", 3);
            SendEvent("IBM", 4);
            SendEvent("CMU", 1);
            SendEvent("CMU", 2);
            SendEvent("CAT", 5);
            SendEvent("CAT", 6);
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields,
                    new Object[][] { new Object[] { "CMU", 1.0 }, new Object[] { "CMU", 3.0 }, new Object[] { "IBM", 3.0 }, new Object[] { "CAT", 5.0 }, new Object[] { "IBM", 7.0 }, new Object[] { "CAT", 11.0 } });
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastOldData, fields,
                    new Object[][] { new Object[] { "CAT", null }, new Object[] { "CMU", null }, new Object[] { "IBM", null }, new Object[] { "CMU", 1.0 }, new Object[] { "IBM", 3.0 }, new Object[] { "CAT", 5.0 } });
            _testListener.Reset();

            SendEvent("IBM", 3);
            SendEvent("IBM", 4);
            SendEvent("CMU", 5);
            SendEvent("CMU", 5);
            SendEvent("DOG", 0);
            SendEvent("DOG", 1);
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields,
                    new Object[][] { new Object[] { "DOG", 0.0 }, new Object[] { "DOG", 1.0 }, new Object[] { "CMU", 8.0 }, new Object[] { "IBM", 10.0 }, new Object[] { "CMU", 13.0 }, new Object[] { "IBM", 14.0 } });
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastOldData, fields,
                    new Object[][] { new Object[] { "DOG", null }, new Object[] { "DOG", 0.0 }, new Object[] { "CMU", 3.0 }, new Object[] { "IBM", 7.0 }, new Object[] { "CMU", 8.0 }, new Object[] { "IBM", 10.0 } });
        }

        private void RunAssertionHaving(EPStatement statement)
        {
            String[] fields = "Symbol,mysum".Split(',');
            _testListener = new SupportUpdateListener();
            statement.Events += _testListener.Update;
            SendEvent("IBM", 3);
            SendEvent("IBM", 4);
            SendEvent("CMU", 1);
            SendEvent("CMU", 2);
            SendEvent("CAT", 5);
            SendEvent("CAT", 6);

            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields,
                    new Object[][] { new Object[] { "CMU", 1.0 }, new Object[] { "CMU", 3.0 }, new Object[] { "IBM", 3.0 }, new Object[] { "CAT", 5.0 }, new Object[] { "IBM", 7.0 }, new Object[] { "CAT", 11.0 } });
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastOldData, fields,
                    new Object[][] { new Object[] { "CMU", 1.0 }, new Object[] { "IBM", 3.0 }, new Object[] { "CAT", 5.0 } });
            _testListener.Reset();

            SendEvent("IBM", 3);
            SendEvent("IBM", 4);
            SendEvent("CMU", 5);
            SendEvent("CMU", 5);
            SendEvent("DOG", 0);
            SendEvent("DOG", 1);
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields,
                    new Object[][] { new Object[] { "DOG", 1.0 }, new Object[] { "CMU", 8.0 }, new Object[] { "IBM", 10.0 }, new Object[] { "CMU", 13.0 }, new Object[] { "IBM", 14.0 } });
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastOldData, fields,
                    new Object[][] { new Object[] { "CMU", 3.0 }, new Object[] { "IBM", 7.0 }, new Object[] { "CMU", 8.0 }, new Object[] { "IBM", 10.0 } });
        }
    }
}
