///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestOrderBySimple
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private EPServiceProvider _epService;
        private List<double> _prices;
        private List<string> _symbols;
        private SupportUpdateListener _testListener;
        private List<long> _volumes;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();

            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _symbols = new List<string>();
            _prices = new List<double>();
            _volumes = new List<long>();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _testListener = null;
            _prices = null;
            _symbols = null;
            _volumes = null;
        }

        [Test]
        public void TestOrderByMultiDelivery()
        {
            // test for QWY-933597 or ESPER-409
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            _epService.EPAdministrator.Configuration.AddEventType(
                "SupportBean", typeof (SupportBean));

            // try pattern
            var listener = new SupportUpdateListener();
            String stmtText =
                "select a.TheString from pattern [every a=SupportBean(TheString like 'A%') -> b=SupportBean(TheString like 'B%')] order by a.TheString desc";
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(stmtText);

            stmtOne.Events += listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("A2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("B", 3));

            EventBean[] received = listener.GetNewDataListFlattened();

            Assert.AreEqual(2, received.Length);
            EPAssertionUtil.AssertPropsPerRow(received, "a.TheString".Split(','), new Object[][]{ new Object[]{ "A2" }, new Object[]{ "A1" }});

            // try pattern with output limit
            var listenerThree = new SupportUpdateListener();
            String stmtTextThree = "select a.TheString from pattern [every a=SupportBean(TheString like 'A%') -> b=SupportBean(TheString like 'B%')] "
                                   + "output every 2 events order by a.TheString desc";
            EPStatement stmtThree = _epService.EPAdministrator.CreateEPL(
                stmtTextThree);

            stmtThree.Events += listenerThree.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("A2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("A3", 3));
            _epService.EPRuntime.SendEvent(new SupportBean("B", 3));

            EventBean[] receivedThree = listenerThree.GetNewDataListFlattened();

            Assert.AreEqual(2, receivedThree.Length);
            EPAssertionUtil.AssertPropsPerRow(receivedThree, "a.TheString".Split(','), new Object[][] { new Object[]{ "A2" }, new Object[]{ "A1" }});

            // try grouped time window
            String stmtTextTwo =
                "select rstream TheString from SupportBean.std:groupwin(TheString).win:time(10) order by TheString desc";
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(
                stmtTextTwo);
            var listenerTwo = new SupportUpdateListener();

            stmtTwo.Events += listenerTwo.Update;

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            _epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("A2", 1));

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(11000));
            EventBean[] receivedTwo = listenerTwo.GetNewDataListFlattened();

            Assert.AreEqual(2, receivedTwo.Length);
            EPAssertionUtil.AssertPropsPerRow(
                receivedTwo, "TheString".Split(','),
                new Object[][] {
                    new Object[] { "A2" },
                    new Object[] { "A1" }
                });
        }

#if false
        [Test]
        public void TestCollatorSortLocale() {
            String[] items = "p�ch�,p�che".Split(',');
            String[] sortedFrench = "p�che,p�ch�".Split(',');
            String[] sortedUS = "p�ch�,p�che".Split(',');
    
            Assert.AreEqual(1, "p�che".CompareTo("p�ch�"));
            Assert.AreEqual(-1, "p�ch�".CompareTo("p�che"));
            Locale.Default = Locale.FRENCH;
            Assert.AreEqual(1, "p�che".CompareTo("p�ch�"));
            Assert.AreEqual(-1, Collator.Instance.Compare("p�che", "p�ch�"));
            Assert.AreEqual(-1, "p�ch�".CompareTo("p�che"));
            Assert.AreEqual(1, Collator.Instance.Compare("p�ch�", "p�che"));
            Assert.IsFalse("p�ch�".Equals("p�che"));
    
            /*
             Collections.Sort(items);
             Console.WriteLine("Sorted default" + items);
    
             Collections.Sort(items, new Comparator<String>() {
             Collator collator = Collator.GetInstance(Locale.FRANCE);
             public int Compare(String o1, String o2)
             {
             return collator.Compare(o1, o2);
             }
             });
             Console.WriteLine("Sorted FR" + items);
             */
    
            Configuration config = SupportConfigFactory.GetConfiguration();
    
            config.EngineDefaults.LanguageConfig.IsSortUsingCollator = true;
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            epService.EPAdministrator.Configuration.AddEventType(
                    "SupportBean", typeof(SupportBean).FullName);
    
            // test order by
            String stmtText = "select TheString from SupportBean.win:keepall() order by TheString asc";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtText);
    
            epService.EPRuntime.SendEvent(new SupportBean("p�ch�", 1));
            epService.EPRuntime.SendEvent(new SupportBean("p�che", 1));
            EPAssertionUtil.AssertPropsPerRow(stmtOne.GetEnumerator(),
                    "TheString".Split(','), new Object[][] {
                new Object[] {
                    sortedFrench[0]
                }
                        ,
                new Object[] {
                    sortedFrench[1]
                }
            }
                    );
    
            // test sort view
            SupportUpdateListener listener = new SupportUpdateListener();
    
            stmtText = "select irstream TheString from SupportBean.ext:sort(2, TheString asc)";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtText);
    
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("p�ch�", 1));
            epService.EPRuntime.SendEvent(new SupportBean("p�che", 1));
            epService.EPRuntime.SendEvent(new SupportBean("abc", 1));
    
            Assert.AreEqual("p�ch�", listener.LastOldData[0].Get("TheString"));
            Locale.Default = Locale.US;
        }
#endif

        [Test]
        public void TestIterator()
        {
            String statementString = "select symbol, TheString, price from "
                                     + typeof (SupportMarketDataBean).FullName
                                     + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                                     + ".win:length(100) as two "
                                     + "where one.symbol = two.TheString " + "order by price";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(
                statementString);

            SendJoinEvents();
            SendEvent("CAT", 50);
            SendEvent("IBM", 49);
            SendEvent("CAT", 15);
            SendEvent("IBM", 100);
            EPAssertionUtil.AssertPropsPerRow(
                statement.GetEnumerator(), 
                new String[] { "symbol", "TheString", "price" },
                new Object[][] {
                    new Object[] { "CAT", "CAT", 15d },
                    new Object[] { "IBM", "IBM", 49d },
                    new Object[] { "CAT", "CAT", 50d },
                    new Object[] { "IBM", "IBM", 100d },
                });

            SendEvent("KGB", 75);
            EPAssertionUtil.AssertPropsPerRow(
                statement.GetEnumerator(),
                new String[]{ "symbol", "TheString", "price" },
                new Object[][]{
                    new Object[] { "CAT", "CAT", 15d },
                    new Object[] { "IBM", "IBM", 49d },
                    new Object[] { "CAT", "CAT", 50d },
                    new Object[] { "KGB", "KGB", 75d },
                    new Object[] { "IBM", "IBM", 100d },
                });
        }

        [Test]
        public void TestAcrossJoin()
        {
            String statementString = "select symbol, TheString from "
                                     + typeof (SupportMarketDataBean).FullName
                                     + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                                     + ".win:length(100) as two "
                                     + "where one.symbol = two.TheString " + "output every 6 events "
                                     + "order by price";

            CreateAndSend(statementString);
            SendJoinEvents();
            OrderValuesByPriceJoin();
            AssertValues(_symbols, "symbol");
            AssertValues(_symbols, "TheString");
            AssertOnlyProperties(new String[] {"symbol", "TheString"});
            ClearValues();

            statementString = "select symbol from "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                              + ".win:length(100) as two "
                              + "where one.symbol = two.TheString " + "output every 6 events "
                              + "order by TheString, price";
            CreateAndSend(statementString);
            SendJoinEvents();
            OrderValuesBySymbolPrice();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[] {"symbol"});
            ClearValues();
        }

        [Test]
        public void TestDescending_OM()
        {
            String stmtText = "select symbol from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                              + "output every 6 events " + "order by price desc";

            var model = new EPStatementObjectModel();

            model.SelectClause = SelectClause.Create("symbol");
            model.FromClause = FromClause.Create(
                FilterStream.Create(typeof (SupportMarketDataBean).FullName).AddView(
                    "win", "length", Expressions.Constant(5)));
            model.OutputLimitClause = OutputLimitClause.Create(6);
            model.OrderByClause = OrderByClause.Create().Add("price", true);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(stmtText, model.ToEPL());

            _testListener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.Create(model);

            statement.Events += _testListener.Update;
            SendEvent("IBM", 2);
            SendEvent("KGB", 1);
            SendEvent("CMU", 3);
            SendEvent("IBM", 6);
            SendEvent("CAT", 6);
            SendEvent("CAT", 5);

            OrderValuesByPriceDesc();
            AssertValues(_symbols, "symbol");
            ClearValues();
        }

        [Test]
        public void TestDescending()
        {
            String statementString = "select symbol from "
                                     + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                                     + "output every 6 events " + "order by price desc";

            CreateAndSend(statementString);
            OrderValuesByPriceDesc();
            AssertValues(_symbols, "symbol");
            ClearValues();

            statementString = "select symbol from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                              + "output every 6 events " + "order by price desc, symbol asc";
            CreateAndSend(statementString);
            OrderValuesByPrice();
            _symbols.Reverse();
            AssertValues(_symbols, "symbol");
            ClearValues();

            statementString = "select symbol from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                              + "output every 6 events " + "order by price asc";
            CreateAndSend(statementString);
            OrderValuesByPrice();
            AssertValues(_symbols, "symbol");
            ClearValues();

            statementString = "select symbol, volume from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                              + "output every 6 events " + "order by symbol desc";
            CreateAndSend(statementString);
            OrderValuesBySymbol();
            _symbols.Reverse();
            AssertValues(_symbols, "symbol");
            AssertValues(_volumes, "volume");
            ClearValues();

            statementString = "select symbol, price from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                              + "output every 6 events " + "order by symbol desc, price desc";
            CreateAndSend(statementString);
            OrderValuesBySymbolPrice();
            _symbols.Reverse();
            _prices.Reverse();
            AssertValues(_symbols, "symbol");
            AssertValues(_prices, "price");
            ClearValues();

            statementString = "select symbol, price from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                              + "output every 6 events " + "order by symbol, price";
            CreateAndSend(statementString);
            OrderValuesBySymbolPrice();
            AssertValues(_symbols, "symbol");
            AssertValues(_prices, "price");
            ClearValues();
        }

        [Test]
        public void TestExpressions()
        {
            String statementString = "select symbol from "
                                     + typeof (SupportMarketDataBean).FullName + ".win:length(10) "
                                     + "output every 6 events " + "order by (price * 6) + 5";

            CreateAndSend(statementString);
            OrderValuesByPrice();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[] {"symbol"});
            ClearValues();

            _epService.Initialize();

            statementString = "select symbol, price from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(10) "
                              + "output every 6 events " + "order by (price * 6) + 5, price";
            CreateAndSend(statementString);
            OrderValuesByPrice();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[] {"symbol", "price"});
            ClearValues();

            _epService.Initialize();

            statementString = "select symbol, 1+volume*23 from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(10) "
                              + "output every 6 events "
                              + "order by (price * 6) + 5, price, volume";
            CreateAndSend(statementString);
            OrderValuesByPrice();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[] {"symbol", "1+volume*23"});
            ClearValues();

            _epService.Initialize();

            statementString = "select symbol from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(10) "
                              + "output every 6 events " + "order by volume*price, symbol";
            CreateAndSend(statementString);
            OrderValuesBySymbol();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[] {"symbol"});
            ClearValues();
        }

        [Test]
        public void TestAliasesSimple()
        {
            String statementString = "select symbol as mySymbol from "
                                     + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                                     + "output every 6 events " + "order by mySymbol";

            CreateAndSend(statementString);
            OrderValuesBySymbol();
            AssertValues(_symbols, "mySymbol");
            AssertOnlyProperties(new String[] {"mySymbol"});
            ClearValues();

            statementString = "select symbol as mySymbol, price as myPrice from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                              + "output every 6 events " + "order by myPrice";
            CreateAndSend(statementString);
            OrderValuesByPrice();
            AssertValues(_symbols, "mySymbol");
            AssertValues(_prices, "myPrice");
            AssertOnlyProperties(new String[]{ "mySymbol", "myPrice" });
            ClearValues();

            statementString = "select symbol, price as myPrice from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(10) "
                              + "output every 6 events " + "order by (myPrice * 6) + 5, price";
            CreateAndSend(statementString);
            OrderValuesByPrice();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[]{ "symbol", "myPrice" });
            ClearValues();

            statementString = "select symbol, 1+volume*23 as myVol from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(10) "
                              + "output every 6 events "
                              + "order by (price * 6) + 5, price, myVol";
            CreateAndSend(statementString);
            OrderValuesByPrice();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[]{ "symbol", "myVol" });
            ClearValues();
        }

        [Test]
        public void TestExpressionsJoin()
        {
            String statementString = "select symbol from "
                                     + typeof (SupportMarketDataBean).FullName
                                     + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                                     + ".win:length(100) as two "
                                     + "where one.symbol = two.TheString " + "output every 6 events "
                                     + "order by (price * 6) + 5";

            CreateAndSend(statementString);
            SendJoinEvents();
            OrderValuesByPriceJoin();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[]{ "symbol" });
            ClearValues();

            _epService.Initialize();

            statementString = "select symbol, price from "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                              + ".win:length(100) as two "
                              + "where one.symbol = two.TheString " + "output every 6 events "
                              + "order by (price * 6) + 5, price";
            CreateAndSend(statementString);
            SendJoinEvents();
            OrderValuesByPriceJoin();
            AssertValues(_prices, "price");
            AssertOnlyProperties(new String[]{ "symbol", "price" });
            ClearValues();

            _epService.Initialize();

            statementString = "select symbol, 1+volume*23 from "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                              + ".win:length(100) as two "
                              + "where one.symbol = two.TheString " + "output every 6 events "
                              + "order by (price * 6) + 5, price, volume";
            CreateAndSend(statementString);
            SendJoinEvents();
            OrderValuesByPriceJoin();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[]{ "symbol", "1+volume*23" });
            ClearValues();

            _epService.Initialize();

            statementString = "select symbol from "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                              + ".win:length(100) as two "
                              + "where one.symbol = two.TheString " + "output every 6 events "
                              + "order by volume*price, symbol";
            CreateAndSend(statementString);
            SendJoinEvents();
            OrderValuesBySymbol();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[]
            {
                "symbol"
            }
                );
            ClearValues();
        }

        [Test]
        public void TestInvalid()
        {
            String statementString = "select symbol from "
                                     + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                                     + "output every 6 events " + "order by sum(price)";

            try
            {
                CreateAndSend(statementString);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                // expected
            }

            statementString = "select sum(price) from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                              + "output every 6 events " + "order by sum(price + 6)";
            try
            {
                CreateAndSend(statementString);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                // expected
            }

            statementString = "select sum(price + 6) from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                              + "output every 6 events " + "order by sum(price)";
            try
            {
                CreateAndSend(statementString);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                // expected
            }
        }

        [Test]
        public void TestInvalidJoin()
        {
            String statementString = "select symbol from "
                                     + typeof (SupportMarketDataBean).FullName
                                     + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                                     + ".win:length(100) as two "
                                     + "where one.symbol = two.TheString " + "output every 6 events "
                                     + "order by sum(price)";

            try
            {
                CreateAndSend(statementString);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                // expected
            }

            statementString = "select sum(price) from "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                              + ".win:length(100) as two "
                              + "where one.symbol = two.TheString " + "output every 6 events "
                              + "order by sum(price + 6)";
            try
            {
                CreateAndSend(statementString);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                // expected
            }

            statementString = "select sum(price + 6) from "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                              + ".win:length(100) as two "
                              + "where one.symbol = two.TheString " + "output every 6 events "
                              + "order by sum(price)";
            try
            {
                CreateAndSend(statementString);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                // expected
            }
        }

        [Test]
        public void TestMultipleKeys()
        {
            String statementString = "select symbol from "
                                     + typeof (SupportMarketDataBean).FullName + ".win:length(10) "
                                     + "output every 6 events " + "order by symbol, price";

            CreateAndSend(statementString);
            OrderValuesBySymbolPrice();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[]{ "symbol" });
            ClearValues();

            statementString = "select symbol from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(10) "
                              + "output every 6 events " + "order by price, symbol, volume";
            CreateAndSend(statementString);
            OrderValuesByPriceSymbol();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[]{ "symbol" });
            ClearValues();

            statementString = "select symbol, volume*2 from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(10) "
                              + "output every 6 events " + "order by price, volume";
            CreateAndSend(statementString);
            OrderValuesByPrice();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[]{ "symbol", "volume*2" });
            ClearValues();
        }

        [Test]
        public void TestAliases()
        {
            String statementString = "select symbol as mySymbol from "
                                     + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                                     + "output every 6 events " + "order by mySymbol";

            CreateAndSend(statementString);
            OrderValuesBySymbol();
            AssertValues(_symbols, "mySymbol");
            AssertOnlyProperties(new String[]{ "mySymbol" });
            ClearValues();

            statementString = "select symbol as mySymbol, price as myPrice from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                              + "output every 6 events " + "order by myPrice";
            CreateAndSend(statementString);
            OrderValuesByPrice();
            AssertValues(_symbols, "mySymbol");
            AssertValues(_prices, "myPrice");
            AssertOnlyProperties(new String[]{ "mySymbol", "myPrice" });
            ClearValues();

            statementString = "select symbol, price as myPrice from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(10) "
                              + "output every 6 events " + "order by (myPrice * 6) + 5, price";
            CreateAndSend(statementString);
            OrderValuesByPrice();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[]{ "symbol", "myPrice" });
            ClearValues();

            statementString = "select symbol, 1+volume*23 as myVol from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(10) "
                              + "output every 6 events "
                              + "order by (price * 6) + 5, price, myVol";
            CreateAndSend(statementString);
            OrderValuesByPrice();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[]{ "symbol", "myVol" });
            ClearValues();

            statementString = "select symbol as mySymbol from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                              + "order by price, mySymbol";
            CreateAndSend(statementString);
            _symbols.Add("CAT");
            AssertValues(_symbols, "mySymbol");
            ClearValues();
            SendEvent("FOX", 10);
            _symbols.Add("FOX");
            AssertValues(_symbols, "mySymbol");
            ClearValues();
        }

        [Test]
        public void TestMultipleKeysJoin()
        {
            String statementString = "select symbol from "
                                     + typeof (SupportMarketDataBean).FullName
                                     + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                                     + ".win:length(100) as two "
                                     + "where one.symbol = two.TheString " + "output every 6 events "
                                     + "order by symbol, price";

            CreateAndSend(statementString);
            SendJoinEvents();
            OrderValuesBySymbolPrice();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[]{ "symbol" });
            ClearValues();

            statementString = "select symbol from "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                              + ".win:length(100) as two "
                              + "where one.symbol = two.TheString " + "output every 6 events "
                              + "order by price, symbol, volume";
            CreateAndSend(statementString);
            SendJoinEvents();
            OrderValuesByPriceSymbol();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[]{ "symbol" });
            ClearValues();

            statementString = "select symbol, volume*2 from "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                              + ".win:length(100) as two "
                              + "where one.symbol = two.TheString " + "output every 6 events "
                              + "order by price, volume";
            CreateAndSend(statementString);
            SendJoinEvents();
            OrderValuesByPriceJoin();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[]{ "symbol", "volume*2" });
            ClearValues();
        }

        [Test]
        public void TestSimple()
        {
            String statementString = "select symbol from "
                                     + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                                     + "output every 6 events " + "order by price";

            CreateAndSend(statementString);
            OrderValuesByPrice();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[]{ "symbol" });
            ClearValues();

            statementString = "select symbol, price from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                              + "output every 6 events " + "order by price";
            CreateAndSend(statementString);
            OrderValuesByPrice();
            AssertValues(_symbols, "symbol");
            AssertValues(_prices, "price");
            AssertOnlyProperties(new String[]{ "symbol", "price" });
            ClearValues();

            statementString = "select symbol, volume from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                              + "output every 6 events " + "order by price";
            CreateAndSend(statementString);
            OrderValuesByPrice();
            AssertValues(_symbols, "symbol");
            AssertValues(_volumes, "volume");
            AssertOnlyProperties(new String[]{ "symbol", "volume" });
            ClearValues();

            statementString = "select symbol, volume*2 from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                              + "output every 6 events " + "order by price";
            CreateAndSend(statementString);
            OrderValuesByPrice();
            AssertValues(_symbols, "symbol");
            AssertValues(_volumes, "volume*2");
            AssertOnlyProperties(new String[]{ "symbol", "volume*2" });
            ClearValues();

            statementString = "select symbol, volume from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                              + "output every 6 events " + "order by symbol";
            CreateAndSend(statementString);
            OrderValuesBySymbol();
            AssertValues(_symbols, "symbol");
            AssertValues(_volumes, "volume");
            AssertOnlyProperties(new String[]{ "symbol", "volume" });
            ClearValues();

            statementString = "select price from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                              + "output every 6 events " + "order by symbol";
            CreateAndSend(statementString);
            OrderValuesBySymbol();
            AssertValues(_prices, "price");
            AssertOnlyProperties(new String[]{ "price" });
            ClearValues();
        }

        [Test]
        public void TestSimpleJoin()
        {
            String statementString = "select symbol from "
                                     + typeof (SupportMarketDataBean).FullName
                                     + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                                     + ".win:length(100) as two "
                                     + "where one.symbol = two.TheString " + "output every 6 events "
                                     + "order by price";

            CreateAndSend(statementString);
            SendJoinEvents();
            OrderValuesByPriceJoin();
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[]{ "symbol" });
            ClearValues();

            statementString = "select symbol, price from "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                              + ".win:length(100) as two "
                              + "where one.symbol = two.TheString " + "output every 6 events "
                              + "order by price";
            CreateAndSend(statementString);
            SendJoinEvents();
            OrderValuesByPriceJoin();
            AssertValues(_symbols, "symbol");
            AssertValues(_prices, "price");
            AssertOnlyProperties(new String[]{ "symbol", "price" });
            ClearValues();

            statementString = "select symbol, volume from "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                              + ".win:length(100) as two "
                              + "where one.symbol = two.TheString " + "output every 6 events "
                              + "order by price";
            CreateAndSend(statementString);
            SendJoinEvents();
            OrderValuesByPriceJoin();
            AssertValues(_symbols, "symbol");
            AssertValues(_volumes, "volume");
            AssertOnlyProperties(new String[]{ "symbol", "volume" });
            ClearValues();

            statementString = "select symbol, volume*2 from "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                              + ".win:length(100) as two "
                              + "where one.symbol = two.TheString " + "output every 6 events "
                              + "order by price";
            CreateAndSend(statementString);
            SendJoinEvents();
            OrderValuesByPriceJoin();
            AssertValues(_symbols, "symbol");
            AssertValues(_volumes, "volume*2");
            AssertOnlyProperties(new String[]{ "symbol", "volume*2" });
            ClearValues();

            statementString = "select symbol, volume from "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                              + ".win:length(100) as two "
                              + "where one.symbol = two.TheString " + "output every 6 events "
                              + "order by symbol";
            CreateAndSend(statementString);
            SendJoinEvents();
            OrderValuesBySymbol();
            AssertValues(_symbols, "symbol");
            AssertValues(_volumes, "volume");
            AssertOnlyProperties(new String[] {"symbol", "volume"});
            ClearValues();

            statementString = "select price from "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                              + ".win:length(100) as two "
                              + "where one.symbol = two.TheString " + "output every 6 events "
                              + "order by symbol, price";
            CreateAndSend(statementString);
            SendJoinEvents();
            OrderValuesBySymbolJoin();
            AssertValues(_prices, "price");
            AssertOnlyProperties(new String[] {"price"});
            ClearValues();
        }

        [Test]
        public void TestWildcard()
        {
            String statementString = "select * from "
                                     + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                                     + "output every 6 events " + "order by Price";

            CreateAndSend(statementString);
            OrderValuesByPrice();
            AssertValues(_symbols, "Symbol");
            AssertValues(_prices, "Price");
            AssertValues(_volumes, "Volume");
            AssertOnlyProperties(new String[] { "Symbol", "Id", "Volume", "Price", "Feed" });
            ClearValues();

            statementString = "select * from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                              + "output every 6 events " + "order by Symbol";
            CreateAndSend(statementString);
            OrderValuesBySymbol();
            AssertValues(_symbols, "Symbol");
            AssertValues(_prices, "Price");
            AssertValues(_volumes, "Volume");
            AssertOnlyProperties(new String[] { "Symbol", "Volume", "Price", "Feed", "Id" });
            ClearValues();
        }

        [Test]
        public void TestWildcardJoin()
        {
            String statementString = "select * from "
                                     + typeof (SupportMarketDataBean).FullName
                                     + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                                     + ".win:length(100) as two "
                                     + "where one.symbol = two.TheString " + "output every 6 events "
                                     + "order by price";

            CreateAndSend(statementString);
            SendJoinEvents();
            OrderValuesByPriceJoin();
            AssertSymbolsJoinWildCard();
            ClearValues();

            _epService.Initialize();

            statementString = "select * from "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                              + ".win:length(100) as two "
                              + "where one.symbol = two.TheString " + "output every 6 events "
                              + "order by symbol, price";
            CreateAndSend(statementString);
            SendJoinEvents();
            OrderValuesBySymbolJoin();
            AssertSymbolsJoinWildCard();
            ClearValues();
        }

        [Test]
        public void TestNoOutputClauseView()
        {
            String statementString = "select symbol from "
                                     + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                                     + "order by price";

            CreateAndSend(statementString);
            _symbols.Add("CAT");
            AssertValues(_symbols, "symbol");
            ClearValues();
            SendEvent("FOX", 10);
            _symbols.Add("FOX");
            AssertValues(_symbols, "symbol");
            ClearValues();

            _epService.Initialize();

            // Set start time
            SendTimeEvent(0);

            statementString = "select symbol from "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:time_batch(1 sec) " + "order by price";
            CreateAndSend(statementString);
            OrderValuesByPrice();
            SendTimeEvent(1000);
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[] {"symbol"});
            ClearValues();
        }

        [Test]
        public void TestNoOutputClauseJoin()
        {
            String statementString = "select symbol from "
                                     + typeof (SupportMarketDataBean).FullName
                                     + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                                     + ".win:length(100) as two "
                                     + "where one.symbol = two.TheString " + "order by price";

            CreateAndSend(statementString);
            SendJoinEvents();
            _symbols.Add("KGB");
            AssertValues(_symbols, "symbol");
            ClearValues();
            SendEvent("DOG", 10);
            _symbols.Add("DOG");
            AssertValues(_symbols, "symbol");
            ClearValues();

            _epService.Initialize();

            // Set start time
            SendTimeEvent(0);

            statementString = "select symbol from "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:time_batch(1) as one, "
                              + typeof (SupportBeanString).FullName + ".win:length(100) as two "
                              + "where one.symbol = two.TheString " + "order by price, symbol";
            CreateAndSend(statementString);
            SendJoinEvents();
            OrderValuesByPriceJoin();
            SendTimeEvent(1000);
            AssertValues(_symbols, "symbol");
            AssertOnlyProperties(new String[] {"symbol"});
            ClearValues();
        }

        private void AssertOnlyProperties(IList<String> requiredProperties)
        {
            EventBean[] events = _testListener.LastNewData;

            if (events == null || events.Length == 0)
            {
                return;
            }
            EventType type = events[0].EventType;
            var actualProperties = new List<String>(type.PropertyNames);

            Log.Debug(".assertOnlyProperties actualProperties==" + actualProperties);
            Assert.IsTrue(actualProperties.ContainsAll(requiredProperties));
            actualProperties.RemoveAll(requiredProperties);
            Assert.IsTrue(actualProperties.IsEmpty());
        }

        private void AssertSymbolsJoinWildCard()
        {
            EventBean[] events = _testListener.LastNewData;

            Log.Debug(".assertValues event type = " + events[0].EventType);
            Log.Debug(".assertValues values: " + _symbols);
            Log.Debug(".assertValues events.Length==" + events.Length);
            for (int i = 0; i < events.Length; i++)
            {
                var theEvent = (SupportMarketDataBean) events[i].Get(
                    "one");

                Assert.AreEqual(_symbols[i], theEvent.Symbol);
            }
        }

        private void AssertValues<T>(IList<T> values, string valueName)
        {
            EventBean[] events = _testListener.LastNewData;

            Assert.AreEqual(values.Count, events.Length);
            Log.Debug(".assertValues values: " + values);
            for (int i = 0; i < events.Length; i++)
            {
                Log.Debug(
                    ".assertValues events[" + i + "]=="
                    + events[i].Get(valueName));
                Assert.AreEqual(values[i], events[i].Get(valueName));
            }
        }

        private void ClearValues()
        {
            _prices.Clear();
            _volumes.Clear();
            _symbols.Clear();
        }

        private void CreateAndSend(String statementString)
        {
            _testListener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL(
                statementString);

            statement.Events += _testListener.Update;
            SendEvent("IBM", 2);
            SendEvent("KGB", 1);
            SendEvent("CMU", 3);
            SendEvent("IBM", 6);
            SendEvent("CAT", 6);
            SendEvent("CAT", 5);
        }

        private void OrderValuesByPrice()
        {
            _symbols.Insert(0, "KGB");
            _symbols.Insert(1, "IBM");
            _symbols.Insert(2, "CMU");
            _symbols.Insert(3, "CAT");
            _symbols.Insert(4, "IBM");
            _symbols.Insert(5, "CAT");
            _prices.Insert(0, 1d);
            _prices.Insert(1, 2d);
            _prices.Insert(2, 3d);
            _prices.Insert(3, 5d);
            _prices.Insert(4, 6d);
            _prices.Insert(5, 6d);
            _volumes.Insert(0, 0l);
            _volumes.Insert(1, 0l);
            _volumes.Insert(2, 0l);
            _volumes.Insert(3, 0l);
            _volumes.Insert(4, 0l);
            _volumes.Insert(5, 0l);
        }

        private void OrderValuesByPriceDesc()
        {
            _symbols.Insert(0, "IBM");
            _symbols.Insert(1, "CAT");
            _symbols.Insert(2, "CAT");
            _symbols.Insert(3, "CMU");
            _symbols.Insert(4, "IBM");
            _symbols.Insert(5, "KGB");
            _prices.Insert(0, 6d);
            _prices.Insert(1, 6d);
            _prices.Insert(2, 5d);
            _prices.Insert(3, 3d);
            _prices.Insert(4, 2d);
            _prices.Insert(5, 1d);
            _volumes.Insert(0, 0l);
            _volumes.Insert(1, 0l);
            _volumes.Insert(2, 0l);
            _volumes.Insert(3, 0l);
            _volumes.Insert(4, 0l);
            _volumes.Insert(5, 0l);
        }

        private void OrderValuesByPriceJoin()
        {
            _symbols.Insert(0, "KGB");
            _symbols.Insert(1, "IBM");
            _symbols.Insert(2, "CMU");
            _symbols.Insert(3, "CAT");
            _symbols.Insert(4, "CAT");
            _symbols.Insert(5, "IBM");
            _prices.Insert(0, 1d);
            _prices.Insert(1, 2d);
            _prices.Insert(2, 3d);
            _prices.Insert(3, 5d);
            _prices.Insert(4, 6d);
            _prices.Insert(5, 6d);
            _volumes.Insert(0, 0l);
            _volumes.Insert(1, 0l);
            _volumes.Insert(2, 0l);
            _volumes.Insert(3, 0l);
            _volumes.Insert(4, 0l);
            _volumes.Insert(5, 0l);
        }

        private void OrderValuesByPriceSymbol()
        {
            _symbols.Insert(0, "KGB");
            _symbols.Insert(1, "IBM");
            _symbols.Insert(2, "CMU");
            _symbols.Insert(3, "CAT");
            _symbols.Insert(4, "CAT");
            _symbols.Insert(5, "IBM");
            _prices.Insert(0, 1d);
            _prices.Insert(1, 2d);
            _prices.Insert(2, 3d);
            _prices.Insert(3, 5d);
            _prices.Insert(4, 6d);
            _prices.Insert(5, 6d);
            _volumes.Insert(0, 0l);
            _volumes.Insert(1, 0l);
            _volumes.Insert(2, 0l);
            _volumes.Insert(3, 0l);
            _volumes.Insert(4, 0l);
            _volumes.Insert(5, 0l);
        }

        private void OrderValuesBySymbol()
        {
            _symbols.Insert(0, "CAT");
            _symbols.Insert(1, "CAT");
            _symbols.Insert(2, "CMU");
            _symbols.Insert(3, "IBM");
            _symbols.Insert(4, "IBM");
            _symbols.Insert(5, "KGB");
            _prices.Insert(0, 6d);
            _prices.Insert(1, 5d);
            _prices.Insert(2, 3d);
            _prices.Insert(3, 2d);
            _prices.Insert(4, 6d);
            _prices.Insert(5, 1d);
            _volumes.Insert(0, 0l);
            _volumes.Insert(1, 0l);
            _volumes.Insert(2, 0l);
            _volumes.Insert(3, 0l);
            _volumes.Insert(4, 0l);
            _volumes.Insert(5, 0l);
        }

        private void OrderValuesBySymbolJoin()
        {
            _symbols.Insert(0, "CAT");
            _symbols.Insert(1, "CAT");
            _symbols.Insert(2, "CMU");
            _symbols.Insert(3, "IBM");
            _symbols.Insert(4, "IBM");
            _symbols.Insert(5, "KGB");
            _prices.Insert(0, 5d);
            _prices.Insert(1, 6d);
            _prices.Insert(2, 3d);
            _prices.Insert(3, 2d);
            _prices.Insert(4, 6d);
            _prices.Insert(5, 1d);
            _volumes.Insert(0, 0l);
            _volumes.Insert(1, 0l);
            _volumes.Insert(2, 0l);
            _volumes.Insert(3, 0l);
            _volumes.Insert(4, 0l);
            _volumes.Insert(5, 0l);
        }

        private void OrderValuesBySymbolPrice()
        {
            _symbols.Insert(0, "CAT");
            _symbols.Insert(1, "CAT");
            _symbols.Insert(2, "CMU");
            _symbols.Insert(3, "IBM");
            _symbols.Insert(4, "IBM");
            _symbols.Insert(5, "KGB");
            _prices.Insert(0, 5d);
            _prices.Insert(1, 6d);
            _prices.Insert(2, 3d);
            _prices.Insert(3, 2d);
            _prices.Insert(4, 6d);
            _prices.Insert(5, 1d);
            _volumes.Insert(0, 0l);
            _volumes.Insert(1, 0l);
            _volumes.Insert(2, 0l);
            _volumes.Insert(3, 0l);
            _volumes.Insert(4, 0l);
            _volumes.Insert(5, 0l);
        }

        private void SendEvent(String symbol, double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L,
                                                 null);

            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendTimeEvent(int millis)
        {
            var theEvent = new CurrentTimeEvent(millis);

            _epService.EPRuntime.SendEvent(theEvent);
        }

        private void SendJoinEvents()
        {
            _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));
        }
    }
}
