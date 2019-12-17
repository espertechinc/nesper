///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.contained
{
    public class EPLContainedEventExample
    {
        public static IList<RegressionExecution> Executions(IResourceManager resourceManager)
        {
            using (var xmlStreamOne = resourceManager.GetResourceAsStream("regression/mediaOrderOne.xml")) {
                var eventDocOne = SupportXML.GetDocument(xmlStreamOne);

                using (var xmlStreamTwo = resourceManager.GetResourceAsStream("regression/mediaOrderTwo.xml")) {
                    var eventDocTwo = SupportXML.GetDocument(xmlStreamTwo);

                    IList<RegressionExecution> execs = new List<RegressionExecution>();
                    execs.Add(new EPLContainedExample(eventDocOne));
                    execs.Add(new EPLContainedSolutionPattern());
                    execs.Add(new EPLContainedJoinSelfJoin(eventDocOne, eventDocTwo));
                    execs.Add(new EPLContainedJoinSelfLeftOuterJoin(eventDocOne, eventDocTwo));
                    execs.Add(new EPLContainedJoinSelfFullOuterJoin(eventDocOne, eventDocTwo));

                    return execs;
                }
            }
        }

        private static void PrintRows(
            RegressionEnvironment env,
            EventBean[] rows)
        {
            var renderer = env.Runtime.RenderEventService.GetJSONRenderer(rows[0].EventType);
            for (var i = 0; i < rows.Length; i++) {
                // System.out.println(renderer.render("event#" + i, rows[i]));
                renderer.Render("event#" + i, rows[i]);
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
                var stmtTextOne = "@Name('s1') select OrderId, Items.Item[0].ItemId from MediaOrder";
                env.CompileDeploy(stmtTextOne).AddListener("s1");

                var stmtTextTwo = "@Name('s2') select * from MediaOrder[Books.Book]";
                env.CompileDeploy(stmtTextTwo).AddListener("s2");

                var stmtTextThree = "@Name('s3') select * from MediaOrder(OrderId='PO200901')[Books.Book]";
                env.CompileDeploy(stmtTextThree).AddListener("s3");

                var stmtTextFour = "@Name('s4') select count(*) from MediaOrder[Books.Book]#unique(BookId)";
                env.CompileDeploy(stmtTextFour).AddListener("s4");

                var stmtTextFive = "@Name('s5') select * from MediaOrder[Books.Book][Review]";
                env.CompileDeploy(stmtTextFive).AddListener("s5");

                var stmtTextSix =
                    "@Name('s6') select * from pattern [c=Cancel -> o=MediaOrder(OrderId = c.OrderId)[Books.Book]]";
                env.CompileDeploy(stmtTextSix).AddListener("s6");

                var stmtTextSeven =
                    "@Name('s7') select * from MediaOrder[select OrderId, BookId from Books.Book][select * from Review]";
                env.CompileDeploy(stmtTextSeven).AddListener("s7");

                var stmtTextEight =
                    "@Name('s8') select * from MediaOrder[select * from Books.Book][select ReviewId, Comment from Review]";
                env.CompileDeploy(stmtTextEight).AddListener("s8");

                var stmtTextNine =
                    "@Name('s9') select * from MediaOrder[Books.Book as Book][select Book.*, ReviewId, Comment from Review]";
                env.CompileDeploy(stmtTextNine).AddListener("s9");

                var stmtTextTen =
                    "@Name('s10') select * from MediaOrder[Books.Book as Book][select MediaOrder.*, BookId, ReviewId from Review] as MediaOrder";
                env.CompileDeploy(stmtTextTen).AddListener("s10");

                var path = new RegressionPath();
                var stmtTextElevenZero =
                    "@Name('s11_0') insert into ReviewStream select * from MediaOrder[Books.Book as Book]\n" +
                    "    [select MediaOrder.* as MediaOrder, Book.* as Book, Review.* as Review from Review as Review] as MediaOrder";
                env.CompileDeploy(stmtTextElevenZero, path);
                var stmtTextElevenOne =
                    "@Name('s11') select MediaOrder.OrderId, Book.BookId, Review.ReviewId from ReviewStream";
                env.CompileDeploy(stmtTextElevenOne, path).AddListener("s11");

                var stmtTextTwelve =
                    "@Name('s12') select * from MediaOrder[Books.Book where Author = 'Orson Scott Card'][Review]";
                env.CompileDeploy(stmtTextTwelve).AddListener("s12");

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");

                EPAssertionUtil.AssertProps(
                    env.Listener("s1").AssertOneGetNewAndReset(),
                    new [] { "OrderId","Items.Item[0].ItemId" },
                    new object[] {"PO200901", "100001"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s2").LastNewData,
                    new [] { "BookId" },
                    new[] {new object[] {"B001"}, new object[] {"B002"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s3").LastNewData,
                    new [] { "BookId" },
                    new[] {new object[] {"B001"}, new object[] {"B002"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s4").LastNewData,
                    new [] { "count(*)" },
                    new[] {new object[] {2L}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s5").LastNewData,
                    new [] { "ReviewId" },
                    new[] {new object[] {"1"}});
                Assert.IsFalse(env.Listener("s6").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s7").LastNewData,
                    new [] { "OrderId","BookId","ReviewId" },
                    new[] {new object[] {"PO200901", "B001", "1"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s8").LastNewData,
                    new [] { "ReviewId","BookId" },
                    new[] {new object[] {"1", "B001"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s9").LastNewData,
                    new [] { "ReviewId","BookId" },
                    new[] {new object[] {"1", "B001"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s10").LastNewData,
                    new [] { "ReviewId","BookId" },
                    new[] {new object[] {"1", "B001"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s11").LastNewData,
                    new [] { "MediaOrder.OrderId","Book.BookId","Review.ReviewId" },
                    new[] {new object[] {"PO200901", "B001", "1"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s12").LastNewData,
                    new [] { "ReviewId" },
                    new[] {new object[] {"1"}});

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
                    "@Name('s0') select Book.BookId,Item.ItemId from" +
                    " MediaOrder[Books.Book] as Book," +
                    " MediaOrder[Items.Item] as Item" +
                    " where ProductId = BookId" +
                    " order by BookId, Item.ItemId asc";
                env.CompileDeploy(stmtText).AddListener("s0");

                var fields = new [] { "Book.BookId","Item.ItemId" };
                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                PrintRows(env, env.Listener("s0").LastNewData);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"B001", "100001"}});

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"B001", "100001"}});

                env.SendEventXMLDOM(eventDocTwo, "MediaOrder");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"B005", "200002"}, new object[] {"B005", "200004"},
                        new object[] {"B006", "200001"}
                    });

                // count
                env.UndeployAll();
                fields = new [] { "count(*)" };
                stmtText =
                    "@Name('s0') select count(*) from MediaOrder[Books.Book] as Book, MediaOrder[Items.Item] as Item" +
                    " where ProductId = BookId" +
                    " order by BookId asc";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventXMLDOM(eventDocTwo, "MediaOrder");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {3L}});

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {4L}});

                // unidirectional count
                env.UndeployAll();
                stmtText =
                    "@Name('s0') select count(*)" +
                    " from MediaOrder[Books.Book] as Book unidirectional, MediaOrder[Items.Item] as Item" +
                    " where ProductId = BookId" +
                    " order by BookId asc";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventXMLDOM(eventDocTwo, "MediaOrder");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {3L}});

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {1L}});

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
                    "@Name('s0') select Book.BookId,Item.ItemId from MediaOrder[Books.Book] as Book" +
                    " left outer join MediaOrder[Items.Item] as Item on ProductId = BookId" +
                    " order by BookId, Item.ItemId asc";
                env.CompileDeploy(stmtText).AddListener("s0");

                var fields = new [] { "Book.BookId","Item.ItemId" };
                env.SendEventXMLDOM(eventDocTwo, "MediaOrder");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"B005", "200002"}, new object[] {"B005", "200004"},
                        new object[] {"B006", "200001"}, new object[] {"B008", null}
                    });

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                PrintRows(env, env.Listener("s0").LastNewData);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"B001", "100001"}, new object[] {"B002", null}});

                // count
                env.UndeployAll();
                fields = new [] { "count(*)" };
                stmtText =
                    "@Name('s0') select count(*) from MediaOrder[Books.Book] as Book" +
                    " left outer join MediaOrder[Items.Item] as Item on ProductId = BookId";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventXMLDOM(eventDocTwo, "MediaOrder");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {4L}});

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {6L}});

                // unidirectional count
                env.UndeployAll();
                stmtText =
                    "@Name('s0') select count(*) from MediaOrder[Books.Book] as Book unidirectional" +
                    " left outer join MediaOrder[Items.Item] as Item on ProductId = BookId";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventXMLDOM(eventDocTwo, "MediaOrder");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {4L}});

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {2L}});

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
                    "@Name('s0') select OrderId, Book.BookId,Item.ItemId from MediaOrder[Books.Book] as Book" +
                    " full outer join MediaOrder[select OrderId, * from Items.Item] as Item on ProductId = BookId" +
                    " order by BookId, Item.ItemId asc";
                env.CompileDeploy(stmtText).AddListener("s0");

                var fields = new [] { "Book.BookId","Item.ItemId" };
                env.SendEventXMLDOM(eventDocTwo, "MediaOrder");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {null, "200003"}, new object[] {"B005", "200002"}, new object[] {"B005", "200004"},
                        new object[] {"B006", "200001"}, new object[] {"B008", null}
                    });

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                PrintRows(env, env.Listener("s0").LastNewData);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"B001", "100001"}, new object[] {"B002", null}});

                // count
                env.UndeployAll();
                fields = new [] { "count(*)" };
                stmtText =
                    "@Name('s0') select count(*) from MediaOrder[Books.Book] as Book" +
                    " full outer join MediaOrder[Items.Item] as Item on ProductId = BookId";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventXMLDOM(eventDocTwo, "MediaOrder");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {5L}});

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {7L}});

                // unidirectional count
                env.UndeployAll();
                stmtText =
                    "@Name('s0') select count(*) from MediaOrder[Books.Book] as Book unidirectional" +
                    " full outer join MediaOrder[Items.Item] as Item on ProductId = BookId";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventXMLDOM(eventDocTwo, "MediaOrder");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {4L}});

                env.SendEventXMLDOM(eventDocOne, "MediaOrder");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {2L}});

                env.UndeployAll();
            }
        }

        internal class EPLContainedSolutionPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "Category","SubEventType","AvgTime" };
                var stmtText =
                    "@Name('s0') select Category, SubEventType, avg(ResponseTimeMillis) as AvgTime" +
                    " from SupportResponseEvent[select Category, * from SubEvents]#time(1 min)" +
                    " group by Category, SubEventType" +
                    " order by Category, SubEventType";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(
                    new SupportResponseEvent(
                        "svcOne",
                        new[] {new SupportResponseSubEvent(1000, "typeA"), new SupportResponseSubEvent(800, "typeB")}));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"svcOne", "typeA", 1000.0}, new object[] {"svcOne", "typeB", 800.0}});

                env.SendEventBean(
                    new SupportResponseEvent(
                        "svcOne",
                        new[] {new SupportResponseSubEvent(400, "typeB"), new SupportResponseSubEvent(500, "typeA")}));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"svcOne", "typeA", 750.0}, new object[] {"svcOne", "typeB", 600.0}});

                env.UndeployAll();
            }
        }
    }
} // end of namespace