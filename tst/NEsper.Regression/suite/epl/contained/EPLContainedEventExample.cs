///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.epl.contained
{
    public class EPLContainedEventExample
    {
        public static IList<RegressionExecution> Executions(IResourceManager resourceManager)
        {
            var execs = new List<RegressionExecution>();

            WithExample(resourceManager, execs);
            WithSolutionPattern(execs);
            WithJoinSelfJoin(resourceManager, execs);
            WithJoinSelfLeftOuterJoin(resourceManager, execs);
            WithJoinSelfFullOuterJoin(resourceManager, execs);
            WithSolutionPatternFinancial(execs);

            return execs;
        }

        public static IList<RegressionExecution> WithSolutionPatternFinancial(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedSolutionPatternFinancial());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinSelfFullOuterJoin(
            IResourceManager resourceManager,
            IList<RegressionExecution> execs = null)
        {
            using (var xmlStreamOne = resourceManager.GetResourceAsStream("regression/mediaOrderOne.xml")) {
                var eventDocOne = SupportXML.GetDocument(xmlStreamOne);

                using (var xmlStreamTwo = resourceManager.GetResourceAsStream("regression/mediaOrderTwo.xml")) {
                    var eventDocTwo = SupportXML.GetDocument(xmlStreamTwo);

                    execs = execs ?? new List<RegressionExecution>();
                    execs.Add(new EPLContainedJoinSelfFullOuterJoin(eventDocOne, eventDocTwo));
                    return execs;
                }
            }
        }

        public static IList<RegressionExecution> WithJoinSelfLeftOuterJoin(
            IResourceManager resourceManager,
            IList<RegressionExecution> execs = null)
        {
            using (var xmlStreamOne = resourceManager.GetResourceAsStream("regression/mediaOrderOne.xml")) {
                var eventDocOne = SupportXML.GetDocument(xmlStreamOne);

                using (var xmlStreamTwo = resourceManager.GetResourceAsStream("regression/mediaOrderTwo.xml")) {
                    var eventDocTwo = SupportXML.GetDocument(xmlStreamTwo);
                    execs = execs ?? new List<RegressionExecution>();
                    execs.Add(new EPLContainedJoinSelfLeftOuterJoin(eventDocOne, eventDocTwo));
                    return execs;
                }
            }
        }

        public static IList<RegressionExecution> WithJoinSelfJoin(
            IResourceManager resourceManager,
            IList<RegressionExecution> execs = null)
        {
            using (var xmlStreamOne = resourceManager.GetResourceAsStream("regression/mediaOrderOne.xml")) {
                var eventDocOne = SupportXML.GetDocument(xmlStreamOne);

                using (var xmlStreamTwo = resourceManager.GetResourceAsStream("regression/mediaOrderTwo.xml")) {
                    var eventDocTwo = SupportXML.GetDocument(xmlStreamTwo);

                    execs = execs ?? new List<RegressionExecution>();
                    execs.Add(new EPLContainedJoinSelfJoin(eventDocOne, eventDocTwo));
                    return execs;
                }
            }
        }

        public static IList<RegressionExecution> WithSolutionPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedSolutionPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithExample(
            IResourceManager resourceManager,
            IList<RegressionExecution> execs = null)
        {
            using (var xmlStreamOne = resourceManager.GetResourceAsStream("regression/mediaOrderOne.xml")) {
                var eventDocOne = SupportXML.GetDocument(xmlStreamOne);

                execs = execs ?? new List<RegressionExecution>();
                execs.Add(new EPLContainedExample(eventDocOne));
                return execs;
            }
        }

        internal class EPLContainedSolutionPatternFinancial : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Could have also used a mapping event however here we uses fire-and-forget to load the mapping instead:
                //   @public @buseventtype create schema MappingEvent(ForeignSymbol string, LocalSymbol string);
                //   on MappingEvent merge Mapping insert select ForeignSymbol, LocalSymbol;
                // The events are:
                //   MappingEvent={ForeignSymbol="ABC", LocalSymbol="123"}
                //   MappingEvent={ForeignSymbol="DEF", LocalSymbol="456"}
                //   MappingEvent={ForeignSymbol="GHI", LocalSymbol="789"}
                //   MappingEvent={ForeignSymbol="JKL", LocalSymbol="666"}
                //   ForeignSymbols={companies={{symbol='ABC', Value=500}, new object[] {symbol='DEF', Value=300}, new object[] {symbol='JKL', Value=400}}}
                //   LocalSymbols={companies={{symbol='123', Value=600}, new object[] {symbol='456', Value=100}, new object[] {symbol='789', Value=200}}}
                var path = new RegressionPath();
                var epl =
                    "create schema Symbol(Symbol string, Value double);\n" +
                    "@public @buseventtype create schema ForeignSymbols(companies Symbol[]);\n" +
                    "@public @buseventtype create schema LocalSymbols(companies Symbol[]);\n" +
                    "\n" +
                    "@public create table Mapping(ForeignSymbol string primary key, LocalSymbol string primary key);\n" +
                    "create index MappingIndexForeignSymbol on Mapping(ForeignSymbol);\n" +
                    "create index MappingIndexLocalSymbol on Mapping(LocalSymbol);\n" +
                    "\n" +
                    "insert into SymbolsPair select * from ForeignSymbols#lastevent as foreign, LocalSymbols#lastevent as local;\n" +
                    "on SymbolsPair\n" +
                    "  insert into SymbolsPairBeginEvent select null\n" +
                    "  insert into ForeignSymbolRow select * from [foreign.companies]\n" +
                    "  insert into LocalSymbolRow select * from [local.companies]\n" +
                    "  insert into SymbolsPairOutputEvent select null" +
                    "  insert into SymbolsPairEndEvent select null" +
                    "  output all;\n" +
                    "\n" +
                    "create context SymbolsPairContext start SymbolsPairBeginEvent end SymbolsPairEndEvent;\n" +
                    "context SymbolsPairContext create table Result(ForeignSymbol string primary key, LocalSymbol string primary key, Value double);\n" +
                    "\n" +
                    "context SymbolsPairContext on ForeignSymbolRow as fsr merge Result as result where result.ForeignSymbol = fsr.Symbol\n" +
                    "  when not matched then insert select fsr.Symbol as ForeignSymbol,\n" +
                    "    (select LocalSymbol from Mapping as mapping where mapping.ForeignSymbol = fsr.Symbol) as LocalSymbol, fsr.Value as Value\n" +
                    "  when matched and fsr.Value > result.Value then update set Value = fsr.Value;\n" +
                    "\n" +
                    "context SymbolsPairContext on LocalSymbolRow as lsr merge Result as result where result.LocalSymbol = lsr.Symbol\n" +
                    "  when not matched then insert select (select ForeignSymbol from Mapping as mapping where mapping.LocalSymbol = lsr.Symbol) as ForeignSymbol," +
                    "    lsr.Symbol as LocalSymbol, lsr.Value as Value\n" +
                    "  when matched and lsr.Value > result.Value then update set Value = lsr.Value;\n" +
                    "\n" +
                    "@name('out') context SymbolsPairContext on SymbolsPairOutputEvent select ForeignSymbol, LocalSymbol, Value from Result order by ForeignSymbol asc;\n";
                env.CompileDeploy(epl, path).AddListener("out");

                // load mapping table
                var compiledFAF = env.CompileFAF(
                    "insert into Mapping select ?::string as ForeignSymbol, ?::string as LocalSymbol",
                    path);
                var preparedFAF = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(compiledFAF);
                LoadMapping(env, preparedFAF, "ABC", "123");
                LoadMapping(env, preparedFAF, "DEF", "456");
                LoadMapping(env, preparedFAF, "GHI", "789");
                LoadMapping(env, preparedFAF, "JKL", "666");

                SendForeignSymbols(env, "ABC=500,DEF=300,JKL=400");
                SendLocalSymbols(env, "123=600,456=100,789=200");

                env.AssertPropsPerRowLastNew(
                    "out",
                    "ForeignSymbol,LocalSymbol,Value".SplitCsv(),
                    new object[][] {
                        new object[] { "ABC", "123", 600d }, new object[] { "DEF", "456", 300d },
                        new object[] { "GHI", "789", 200d }, new object[] { "JKL", "666", 400d }
                    });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }

            private void SendForeignSymbols(
                RegressionEnvironment env,
                string symbolCsv)
            {
                var companies = ParseSymbols(symbolCsv);
                env.SendEventMap(Collections.SingletonDataMap("companies", companies), "ForeignSymbols");
            }

            private void SendLocalSymbols(
                RegressionEnvironment env,
                string symbolCsv)
            {
                var companies = ParseSymbols(symbolCsv);
                env.SendEventMap(Collections.SingletonDataMap("companies", companies), "LocalSymbols");
            }

            private IDictionary<string, object>[] ParseSymbols(string symbolCsv)
            {
                var pairs = symbolCsv.SplitCsv();
                var companies = new IDictionary<string, object>[pairs.Length];
                for (var i = 0; i < pairs.Length; i++) {
                    var nameAndValue = pairs[i].Split("=");
                    var symbol = nameAndValue[0];
                    var value = double.Parse(nameAndValue[1]);
                    companies[i] = CollectionUtil.BuildMap("Symbol", symbol, "Value", value);
                }

                return companies;
            }

            private void LoadMapping(
                RegressionEnvironment env,
                EPFireAndForgetPreparedQueryParameterized preparedFAF,
                string foreignSymbol,
                string localSymbol)
            {
                preparedFAF.SetObject(1, foreignSymbol);
                preparedFAF.SetObject(2, localSymbol);
                env.Runtime.FireAndForgetService.ExecuteQuery(preparedFAF);
            }
        }

        internal class EPLContainedExample : RegressionExecution
        {
            private readonly XmlDocument eventDocOne;

            public EPLContainedExample(XmlDocument eventDocOne)
            {
                this.eventDocOne = eventDocOne;
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtTextOne = "@name('s1') select OrderId, Items.Item[0].ItemId from MediaOrder";
                env.CompileDeploy(stmtTextOne).AddListener("s1");

                var stmtTextTwo = "@name('s2') select * from MediaOrder[Books.Book]";
                env.CompileDeploy(stmtTextTwo).AddListener("s2");

                var stmtTextThree = "@name('s3') select * from MediaOrder(OrderId='PO200901')[Books.Book]";
                env.CompileDeploy(stmtTextThree).AddListener("s3");

                var stmtTextFour = "@name('s4') select count(*) from MediaOrder[Books.Book]#unique(BookId)";
                env.CompileDeploy(stmtTextFour).AddListener("s4");

                var stmtTextFive = "@name('s5') select * from MediaOrder[Books.Book][Review]";
                env.CompileDeploy(stmtTextFive).AddListener("s5");

                var stmtTextSix =
                    "@name('s6') select * from pattern [c=Cancel -> o=MediaOrder(OrderId = c.OrderId)[Books.Book]]";
                env.CompileDeploy(stmtTextSix).AddListener("s6");

                var stmtTextSeven =
                    "@name('s7') select * from MediaOrder[select OrderId, BookId from Books.Book][select * from Review]";
                env.CompileDeploy(stmtTextSeven).AddListener("s7");

                var stmtTextEight =
                    "@name('s8') select * from MediaOrder[select * from Books.Book][select ReviewId, Comment from Review]";
                env.CompileDeploy(stmtTextEight).AddListener("s8");

                var stmtTextNine =
                    "@name('s9') select * from MediaOrder[Books.Book as Book][select Book.*, ReviewId, Comment from Review]";
                env.CompileDeploy(stmtTextNine).AddListener("s9");

                var stmtTextTen =
                    "@name('s10') select * from MediaOrder[Books.Book as Book][select MediaOrder.*, BookId, ReviewId from Review] as MediaOrder";
                env.CompileDeploy(stmtTextTen).AddListener("s10");

                var path = new RegressionPath();
                var stmtTextElevenZero =
                    "@name('s11_0') @public insert into ReviewStream select * from MediaOrder[Books.Book as Book]\n" +
                    "    [select MediaOrder.* as MediaOrder, Book.* as Book, Review.* as Review from Review as Review] as MediaOrder";
                env.CompileDeploy(stmtTextElevenZero, path);
                var stmtTextElevenOne =
                    "@name('s11') select MediaOrder.OrderId, Book.BookId, Review.ReviewId from ReviewStream";
                env.CompileDeploy(stmtTextElevenOne, path).AddListener("s11");

                var stmtTextTwelve =
                    "@name('s12') select * from MediaOrder[Books.Book where Author = 'Orson Scott Card'][Review]";
                env.CompileDeploy(stmtTextTwelve).AddListener("s12");

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");

                env.AssertPropsNew(
                    "s1",
                    "OrderId,Items.Item[0].ItemId".SplitCsv(),
                    new object[] { "PO200901", "100001" });
                env.AssertPropsPerRowNewOnly(
                    "s2",
                    "BookId".SplitCsv(),
                    new object[][] { new object[] { "B001" }, new object[] { "B002" } });
                env.AssertPropsPerRowNewOnly(
                    "s3",
                    "BookId".SplitCsv(),
                    new object[][] { new object[] { "B001" }, new object[] { "B002" } });
                env.AssertPropsPerRowNewOnly("s4", "count(*)".SplitCsv(), new object[][] { new object[] { 2L } });
                env.AssertPropsPerRowNewOnly("s5", "ReviewId".SplitCsv(), new object[][] { new object[] { "1" } });
                env.AssertListenerNotInvoked("s6");
                env.AssertPropsPerRowNewOnly(
                    "s7",
                    "OrderId,BookId,ReviewId".SplitCsv(),
                    new object[][] { new object[] { "PO200901", "B001", "1" } });
                env.AssertPropsPerRowNewOnly(
                    "s8",
                    "ReviewId,BookId".SplitCsv(),
                    new object[][] { new object[] { "1", "B001" } });
                env.AssertPropsPerRowNewOnly(
                    "s9",
                    "ReviewId,BookId".SplitCsv(),
                    new object[][] { new object[] { "1", "B001" } });
                env.AssertPropsPerRowNewOnly(
                    "s10",
                    "ReviewId,BookId".SplitCsv(),
                    new object[][] { new object[] { "1", "B001" } });
                env.AssertPropsPerRowNewOnly(
                    "s11",
                    "MediaOrder.OrderId,Book.BookId,Review.ReviewId".SplitCsv(),
                    new object[][] { new object[] { "PO200901", "B001", "1" } });
                env.AssertPropsPerRowNewOnly("s12", "ReviewId".SplitCsv(), new object[][] { new object[] { "1" } });

                env.UndeployAll();
            }
        }

        internal class EPLContainedJoinSelfJoin : RegressionExecution
        {
            private readonly XmlDocument eventDocOne;
            private readonly XmlDocument eventDocTwo;

            public EPLContainedJoinSelfJoin(
                XmlDocument eventDocOne,
                XmlDocument eventDocTwo)
            {
                this.eventDocOne = eventDocOne;
                this.eventDocTwo = eventDocTwo;
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select Book.BookId,Item.ItemId from MediaOrder[Books.Book] as Book, MediaOrder[Items.Item] as Item where ProductId = BookId order by BookId, Item.ItemId asc";
                env.CompileDeploy(stmtText).AddListener("s0");

                var fieldsItems = "Book.BookId,Item.ItemId".SplitCsv();
                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                env.AssertListener(
                    "s0",
                    listener => {
                        PrintRows(env, listener.LastNewData);
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.LastNewData,
                            fieldsItems,
                            new object[][] { new object[] { "B001", "100001" } });
                    });

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                env.AssertPropsPerRowLastNew("s0", fieldsItems, new object[][] { new object[] { "B001", "100001" } });

                env.SendEventXMLDOM(eventDocTwo, "MediaOrder");
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fieldsItems,
                    new object[][] {
                        new object[] { "B005", "200002" }, new object[] { "B005", "200004" },
                        new object[] { "B006", "200001" }
                    });

                // count
                env.UndeployAll();
                var fieldsCount = "count(*)".SplitCsv();
                stmtText =
                    "@name('s0') select count(*) from MediaOrder[Books.Book] as Book, MediaOrder[Items.Item] as Item where ProductId = BookId order by BookId asc";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventXMLDOM(eventDocTwo, "MediaOrder");
                env.AssertPropsPerRowLastNew("s0", fieldsCount, new object[][] { new object[] { 3L } });

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                env.AssertPropsPerRowLastNew("s0", fieldsCount, new object[][] { new object[] { 4L } });

                // unidirectional count
                env.UndeployAll();
                stmtText =
                    "@name('s0') select count(*) from MediaOrder[Books.Book] as Book unidirectional, MediaOrder[Items.Item] as Item where ProductId = BookId order by BookId asc";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventXMLDOM(eventDocTwo, "MediaOrder");
                env.AssertPropsPerRowLastNew("s0", fieldsCount, new object[][] { new object[] { 3L } });

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                env.AssertPropsPerRowLastNew("s0", fieldsCount, new object[][] { new object[] { 1L } });

                env.UndeployAll();
            }
        }

        internal class EPLContainedJoinSelfLeftOuterJoin : RegressionExecution
        {
            private readonly XmlDocument eventDocOne;
            private readonly XmlDocument eventDocTwo;

            public EPLContainedJoinSelfLeftOuterJoin(
                XmlDocument eventDocOne,
                XmlDocument eventDocTwo)
            {
                this.eventDocOne = eventDocOne;
                this.eventDocTwo = eventDocTwo;
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select Book.BookId,Item.ItemId from MediaOrder[Books.Book] as Book left outer join MediaOrder[Items.Item] as Item on ProductId = BookId order by BookId, Item.ItemId asc";
                env.CompileDeploy(stmtText).AddListener("s0");

                var fieldsItems = "Book.BookId,Item.ItemId".SplitCsv();
                env.SendEventXMLDOM(eventDocTwo, "MediaOrder");
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fieldsItems,
                    new object[][] {
                        new object[] { "B005", "200002" }, new object[] { "B005", "200004" },
                        new object[] { "B006", "200001" }, new object[] { "B008", null }
                    });

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                env.AssertListener(
                    "s0",
                    listener => {
                        PrintRows(env, listener.LastNewData);
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.GetAndResetLastNewData(),
                            fieldsItems,
                            new object[][] { new object[] { "B001", "100001" }, new object[] { "B002", null } });
                    });

                // count
                env.UndeployAll();
                var fieldsCount = "count(*)".SplitCsv();
                stmtText =
                    "@name('s0') select count(*) from MediaOrder[Books.Book] as Book left outer join MediaOrder[Items.Item] as Item on ProductId = BookId";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventXMLDOM(eventDocTwo, "MediaOrder");
                env.AssertPropsPerRowLastNew("s0", fieldsCount, new object[][] { new object[] { 4L } });

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                env.AssertPropsPerRowLastNew("s0", fieldsCount, new object[][] { new object[] { 6L } });

                // unidirectional count
                env.UndeployAll();
                stmtText =
                    "@name('s0') select count(*) from MediaOrder[Books.Book] as Book unidirectional left outer join MediaOrder[Items.Item] as Item on ProductId = BookId";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventXMLDOM(eventDocTwo, "MediaOrder");
                env.AssertPropsPerRowLastNew("s0", fieldsCount, new object[][] { new object[] { 4L } });

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                env.AssertPropsPerRowLastNew("s0", fieldsCount, new object[][] { new object[] { 2L } });

                env.UndeployAll();
            }
        }

        internal class EPLContainedJoinSelfFullOuterJoin : RegressionExecution
        {
            private readonly XmlDocument eventDocOne;
            private readonly XmlDocument eventDocTwo;

            public EPLContainedJoinSelfFullOuterJoin(
                XmlDocument eventDocOne,
                XmlDocument eventDocTwo)
            {
                this.eventDocOne = eventDocOne;
                this.eventDocTwo = eventDocTwo;
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select OrderId, Book.BookId,Item.ItemId from MediaOrder[Books.Book] as Book full outer join MediaOrder[select OrderId, * from Items.Item] as Item on ProductId = BookId order by BookId, Item.ItemId asc";
                env.CompileDeploy(stmtText).AddListener("s0");

                var fieldsItems = "Book.BookId,Item.ItemId".SplitCsv();
                env.SendEventXMLDOM(eventDocTwo, "MediaOrder");
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fieldsItems,
                    new object[][] {
                        new object[] { null, "200003" }, new object[] { "B005", "200002" },
                        new object[] { "B005", "200004" }, new object[] { "B006", "200001" },
                        new object[] { "B008", null }
                    });

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                env.AssertListener(
                    "s0",
                    listener => {
                        PrintRows(env, listener.LastNewData);
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.GetAndResetLastNewData(),
                            fieldsItems,
                            new object[][] { new object[] { "B001", "100001" }, new object[] { "B002", null } });
                    });

                // count
                env.UndeployAll();
                var fieldsCount = "count(*)".SplitCsv();
                stmtText =
                    "@name('s0') select count(*) from MediaOrder[Books.Book] as Book full outer join MediaOrder[Items.Item] as Item on ProductId = BookId";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventXMLDOM(eventDocTwo, "MediaOrder");
                env.AssertPropsPerRowLastNew("s0", fieldsCount, new object[][] { new object[] { 5L } });

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                env.AssertPropsPerRowLastNew("s0", fieldsCount, new object[][] { new object[] { 7L } });

                // unidirectional count
                env.UndeployAll();
                stmtText =
                    "@name('s0') select count(*) from MediaOrder[Books.Book] as Book unidirectional full outer join MediaOrder[Items.Item] as Item on ProductId = BookId";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventXMLDOM(eventDocTwo, "MediaOrder");
                env.AssertPropsPerRowLastNew("s0", fieldsCount, new object[][] { new object[] { 4L } });

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                env.AssertPropsPerRowLastNew("s0", fieldsCount, new object[][] { new object[] { 2L } });

                env.UndeployAll();
            }
        }

        internal class EPLContainedSolutionPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "Category,SubEventType,AvgTime".SplitCsv();
                var stmtText =
                    "@name('s0') select Category, SubEventType, avg(ResponseTimeMillis) as AvgTime from SupportResponseEvent[select Category, * from SubEvents]#time(1 min) group by Category, SubEventType order by Category, SubEventType";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(
                    new SupportResponseEvent(
                        "svcOne",
                        new SupportResponseSubEvent[]
                            { new SupportResponseSubEvent(1000, "typeA"), new SupportResponseSubEvent(800, "typeB") }));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "svcOne", "typeA", 1000.0 }, new object[] { "svcOne", "typeB", 800.0 } });

                env.SendEventBean(
                    new SupportResponseEvent(
                        "svcOne",
                        new SupportResponseSubEvent[]
                            { new SupportResponseSubEvent(400, "typeB"), new SupportResponseSubEvent(500, "typeA") }));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "svcOne", "typeA", 750.0 }, new object[] { "svcOne", "typeB", 600.0 } });

                env.UndeployAll();
            }
        }

        private static void PrintRows(
            RegressionEnvironment env,
            EventBean[] rows)
        {
            var renderer = env.Runtime.RenderEventService.GetJSONRenderer(rows[0].EventType);
            for (var i = 0; i < rows.Length; i++) {
                // Console.WriteLine(renderer.render("event#" + i, rows[i]));
                renderer.Render("event#" + i, rows[i]);
            }
        }
    }
} // end of namespace