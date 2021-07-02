///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.wordexample;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.util.CollectionUtil;
using static com.espertech.esper.regressionlib.support.bookexample.OrderBeanFactory;

namespace com.espertech.esper.regressionlib.suite.epl.contained
{
    public class EPLContainedEventSimple
    {
        private static readonly string NEWLINE = Environment.NewLine;

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithPropertyAccess(execs);
            WithNamedWindowPremptive(execs);
            WithUnidirectionalJoin(execs);
            WithUnidirectionalJoinCount(execs);
            WithJoinCount(execs);
            WithJoin(execs);
            WithAloneCount(execs);
            WithIRStreamArrayItem(execs);
            WithSplitWords(execs);
            WithArrayProperty(execs);
            WithWithSubqueryResult(execs);

            return execs;
        }

        public static IList<RegressionExecution> WithWithSubqueryResult(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedWithSubqueryResult());
            return execs;
        }

        public static IList<RegressionExecution> WithArrayProperty(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedArrayProperty());
            return execs;
        }

        public static IList<RegressionExecution> WithSplitWords(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedSplitWords());
            return execs;
        }

        public static IList<RegressionExecution> WithIRStreamArrayItem(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedIRStreamArrayItem());
            return execs;
        }

        public static IList<RegressionExecution> WithAloneCount(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedAloneCount());
            return execs;
        }

        public static IList<RegressionExecution> WithJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinCount(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedJoinCount());
            return execs;
        }

        public static IList<RegressionExecution> WithUnidirectionalJoinCount(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedUnidirectionalJoinCount());
            return execs;
        }

        public static IList<RegressionExecution> WithUnidirectionalJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedUnidirectionalJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowPremptive(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedNamedWindowPremptive());
            return execs;
        }

        public static IList<RegressionExecution> WithPropertyAccess(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedPropertyAccess());
            return execs;
        }

        internal class EPLContainedWithSubqueryResult : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl =
                    "@public @buseventtype create schema Person(personId string);\n" +
                    "@public @buseventtype create schema Room(roomId string);\n" +
                    "create window RoomWindow#keepall as Room;\n" +
                    "insert into RoomWindow select * from Room;\n" +
                    "insert into PersonAndRooms select personId, (select roomId from RoomWindow).selectFrom(v => new {roomId = v}) as rooms from Person;\n" +
                    "@Name('s0') select personId, roomId from PersonAndRooms[select personId, roomId from rooms@type(Room)];";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventMap(BuildMap("roomId", "r1"), "Room");
                env.SendEventMap(BuildMap("roomId", "r2"), "Room");
                env.SendEventMap(BuildMap("personId", "va"), "Person");
                EventBean[] events = env.Listener("s0").GetAndResetLastNewData();
                EPAssertionUtil.AssertPropsPerRow(
                    events,
                    "personId,roomId".SplitCsv(),
                    new object[][] {
                        new object[] {"va", "r1"},
                        new object[] {"va", "r2"}
                    });

                env.UndeployAll();
            }
        }

        // Assures that the events inserted into the named window are preemptive to events generated by contained-event syntax.
        // This example generates 3 contained-events: One for each book.
        // It then inserts them into a named window to determine the highest price among all.
        // The named window updates first becoming useful to subsequent events (versus last and not useful).
        internal class EPLContainedNamedWindowPremptive : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"BookId"};
                var path = new RegressionPath();

                var stmtText = "@Name('s0') insert into BookStream select * from OrderBean[Books]";
                env.CompileDeploy(stmtText, path).AddListener("s0");

                env.CompileDeploy("@Name('nw') create window MyWindow#lastevent as BookDesc", path);
                env.CompileDeploy(
                    "insert into MyWindow select * from BookStream bs where not exists (select * from MyWindow mw where mw.Price > bs.Price)",
                    path);

                env.SendEventBean(MakeEventOne());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"10020"},
                        new object[] {"10021"},
                        new object[] {"10022"}
                    });
                env.Listener("s0").Reset();

                // highest price (27 is the last value)
                var theEnumerator = env.GetEnumerator("nw");
                Assert.That(theEnumerator, Is.Not.Null);
                Assert.That(theEnumerator.MoveNext(), Is.True);

                var theEvent = theEnumerator.Current;
                Assert.AreEqual(35.0, theEvent.Get("Price"));

                env.UndeployAll();
            }
        }

        internal class EPLContainedUnidirectionalJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select * from " +
                               "OrderBean as orderEvent unidirectional, " +
                               "OrderBean[select * from Books] as Book, " +
                               "OrderBean[select * from OrderDetail.Items] as Item " +
                               "where Book.BookId=Item.ProductId " +
                               "order by Book.BookId, Item.Amount";
                var stmtTextFormatted = "@Name('s0')" +
                                        NEWLINE +
                                        "select *" +
                                        NEWLINE +
                                        "from OrderBean as orderEvent unidirectional," +
                                        NEWLINE +
                                        "OrderBean[select * from Books] as Book," +
                                        NEWLINE +
                                        "OrderBean[select * from OrderDetail.Items] as Item" +
                                        NEWLINE +
                                        "where Book.BookId=Item.ProductId" +
                                        NEWLINE +
                                        "order by Book.BookId, Item.Amount";
                env.CompileDeploy(stmtText).AddListener("s0");

                TryAssertionUnidirectionalJoin(env);

                env.UndeployAll();

                var model = env.EplToModel(stmtText);
                Assert.AreEqual(stmtText, model.ToEPL());
                Assert.AreEqual(stmtTextFormatted, model.ToEPL(new EPStatementFormatter(true)));
                env.CompileDeploy(model).AddListener("s0");

                TryAssertionUnidirectionalJoin(env);

                env.UndeployAll();
            }

            private void TryAssertionUnidirectionalJoin(RegressionEnvironment env)
            {
                var fields = new[] {"orderEvent.OrderDetail.OrderId", "Book.BookId", "Book.Title", "Item.Amount"};
                env.SendEventBean(MakeEventOne());
                Assert.AreEqual(3, env.Listener("s0").LastNewData.Length);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"PO200901", "10020", "Enders Game", 10},
                        new object[] {"PO200901", "10020", "Enders Game", 30},
                        new object[] {"PO200901", "10021", "Foundation 1", 25}
                    });
                env.Listener("s0").Reset();

                env.SendEventBean(MakeEventTwo());
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"PO200902", "10022", "Stranger in a Strange Land", 5}});
                env.Listener("s0").Reset();

                env.SendEventBean(MakeEventThree());
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"PO200903", "10021", "Foundation 1", 50}});
            }
        }

        internal class EPLContainedUnidirectionalJoinCount : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select count(*) from " +
                               "OrderBean OrderBean unidirectional, " +
                               "OrderBean[Books] as book, " +
                               "OrderBean[OrderDetail.Items] Item " +
                               "where book.BookId = Item.ProductId order by book.BookId asc, Item.Amount asc";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(MakeEventOne());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"count(*)"},
                    new object[] {3L});

                env.SendEventBean(MakeEventTwo());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"count(*)"},
                    new object[] {1L});

                env.SendEventBean(MakeEventThree());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"count(*)"},
                    new object[] {1L});

                env.SendEventBean(MakeEventFour());
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLContainedJoinCount : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"count(*)"};
                var stmtText = "@Name('s0') select count(*) from " +
                               "OrderBean[Books]#unique(BookId) book, " +
                               "OrderBean[OrderDetail.Items]#keepall Item " +
                               "where book.BookId = Item.ProductId";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(MakeEventOne());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {3L}});

                env.SendEventBean(MakeEventTwo());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"count(*)"},
                    new object[] {4L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {4L}});

                env.SendEventBean(MakeEventThree());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"count(*)"},
                    new object[] {5L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {5L}});

                env.SendEventBean(MakeEventFour());
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(MakeEventOne());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"count(*)"},
                    new object[] {8L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {8L}});

                env.UndeployAll();
            }
        }

        internal class EPLContainedJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"Book.BookId", "Item.ItemId", "Amount"};
                var stmtText = "@Name('s0') select Book.BookId,Item.ItemId,Amount from " +
                               "OrderBean[Books]#firstunique(BookId) Book, " +
                               "OrderBean[OrderDetail.Items]#keepall Item " +
                               "where Book.BookId = Item.ProductId " +
                               "order by Book.BookId, Item.ItemId";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(MakeEventOne());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"10020", "A001", 10}, new object[] {"10020", "A003", 30},
                        new object[] {"10021", "A002", 25}
                    });
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {
                        new object[] {"10020", "A001", 10}, new object[] {"10020", "A003", 30},
                        new object[] {"10021", "A002", 25}
                    });
                env.Listener("s0").Reset();

                env.SendEventBean(MakeEventTwo());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"10022", "B001", 5}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {
                        new object[] {"10020", "A001", 10}, new object[] {"10020", "A003", 30},
                        new object[] {"10021", "A002", 25}, new object[] {"10022", "B001", 5}
                    });
                env.Listener("s0").Reset();

                env.SendEventBean(MakeEventThree());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"10021", "C001", 50}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {
                        new object[] {"10020", "A001", 10}, new object[] {"10020", "A003", 30},
                        new object[] {"10021", "A002", 25}, new object[] {"10021", "C001", 50},
                        new object[] {"10022", "B001", 5}
                    });
                env.Listener("s0").Reset();

                env.SendEventBean(MakeEventFour());
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLContainedAloneCount : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"count(*)"};

                var stmtText = "@Name('s0') select count(*) from OrderBean[Books]";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(MakeEventOne());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {3L}});

                env.SendEventBean(MakeEventFour());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {5L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {5L}});

                env.UndeployAll();
            }
        }

        internal class EPLContainedPropertyAccess : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') @IterableUnbound select BookId from OrderBean[Books]").AddListener("s0");
                env.CompileDeploy(
                    "@Name('s1') @IterableUnbound select Books[0].Author as val from OrderBean(Books[0].BookId = '10020')");

                env.SendEventBean(MakeEventOne());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"BookId"},
                    new[] {new object[] {"10020"}, new object[] {"10021"}, new object[] {"10022"}});
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    new[] {"BookId"},
                    new[] {new object[] {"10020"}, new object[] {"10021"}, new object[] {"10022"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s1"),
                    new[] {"val"},
                    new[] {new object[] {"Orson Scott Card"}});

                env.SendEventBean(MakeEventFour());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"BookId"},
                    new[] {new object[] {"10031"}, new object[] {"10032"}});
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    new[] {"BookId"},
                    new[] {new object[] {"10031"}, new object[] {"10032"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s1"),
                    new[] {"val"},
                    new[] {new object[] {"Orson Scott Card"}});

                // add where clause
                env.UndeployAll();
                env.CompileDeploy("@Name('s0') select BookId from OrderBean[Books where Author='Orson Scott Card']")
                    .AddListener("s0");
                env.SendEventBean(MakeEventOne());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"BookId"},
                    new[] {new object[] {"10020"}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class EPLContainedIRStreamArrayItem : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') @IterableUnbound select irstream BookId from OrderBean[Books[0]]";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(MakeEventOne());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"BookId"},
                    new[] {new object[] {"10020"}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    new[] {"BookId"},
                    new[] {new object[] {"10020"}});

                env.SendEventBean(MakeEventFour());
                Assert.IsNull(env.Listener("s0").LastOldData);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"BookId"},
                    new[] {new object[] {"10031"}});
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    new[] {"BookId"},
                    new[] {new object[] {"10031"}});

                env.UndeployAll();
            }
        }

        internal class EPLContainedSplitWords : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') insert into WordStream select * from SentenceEvent[Words]";

                var fields = new[] {"Word"};
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(new SentenceEvent("I am testing this"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"I"}, new object[] {"am"}, new object[] {"testing"}, new object[] {"this"}});

                env.UndeployAll();
            }
        }

        internal class EPLContainedArrayProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create objectarray schema ContainedId(Id string)", path);
                env.CompileDeploy(
                        "@Name('s0') select * from SupportStringBeanWithArray[select TopId, * from ContainedIds @type(ContainedId)]",
                        path)
                    .AddListener("s0");
                env.SendEventBean(new SupportStringBeanWithArray("A", new[] {"one", "two", "three"}));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    new[] {"TopId", "Id"},
                    new[] {new object[] {"A", "one"}, new object[] {"A", "two"}, new object[] {"A", "three"}});
                env.UndeployAll();
            }
        }
    }
} // end of namespace