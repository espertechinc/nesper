///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.wordexample;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.util.CollectionUtil; // buildMap
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

        private class EPLContainedWithSubqueryResult : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create schema Person(personId string);\n" +
                    "@public @buseventtype create schema Room(roomId string);\n" +
                    "create window RoomWindow#keepall as Room;\n" +
                    "insert into RoomWindow select * from Room;\n" +
                    "insert into PersonAndRooms select personId, (select roomId from RoomWindow).selectFrom(v => new {roomId = v}) as rooms from Person;\n" +
                    "@name('s0') select personId, roomId from PersonAndRooms[select personId, roomId from rooms@type(Room)];";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventMap(BuildMap("roomId", "r1"), "Room");
                env.SendEventMap(BuildMap("roomId", "r2"), "Room");
                env.SendEventMap(BuildMap("personId", "va"), "Person");
                env.AssertPropsPerRowLastNew(
                    "s0",
                    "personId,roomId".SplitCsv(),
                    new object[][] { new object[] { "va", "r1" }, new object[] { "va", "r2" } });

                env.UndeployAll();
            }
        }

        // Assures that the events inserted into the named window are preemptive to events generated by contained-event syntax.
        // This example generates 3 contained-events: One for each book.
        // It then inserts them into a named window to determine the highest price among all.
        // The named window updates first becoming useful to subsequent events (versus last and not useful).
        private class EPLContainedNamedWindowPremptive : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "bookId".SplitCsv();
                var path = new RegressionPath();

                var stmtText = "@name('s0') @public insert into BookStream select * from OrderBean[books]";
                env.CompileDeploy(stmtText, path).AddListener("s0");

                env.CompileDeploy("@name('nw') @public create window MyWindow#lastevent as BookDesc", path);
                env.CompileDeploy(
                    "insert into MyWindow select * from BookStream bs where not exists (select * from MyWindow mw where mw.price > bs.price)",
                    path);

                env.SendEventBean(MakeEventOne());
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "10020" }, new object[] { "10021" }, new object[] { "10022" } });

                // higest price (27 is the last value)
                env.AssertIterator("nw", iterator => Assert.AreEqual(35.0, iterator.Advance().Get("price")));

                env.UndeployAll();
            }
        }

        private class EPLContainedUnidirectionalJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select * from " +
                               "OrderBean as orderEvent unidirectional, " +
                               "OrderBean[select * from books] as book, " +
                               "OrderBean[select * from orderdetail.items] as item " +
                               "where book.bookId=item.productId " +
                               "order by book.bookId, item.amount";
                var stmtTextFormatted = "@name('s0')" +
                                        NEWLINE +
                                        "select *" +
                                        NEWLINE +
                                        "from OrderBean as orderEvent unidirectional," +
                                        NEWLINE +
                                        "OrderBean[select * from books] as book," +
                                        NEWLINE +
                                        "OrderBean[select * from orderdetail.items] as item" +
                                        NEWLINE +
                                        "where book.bookId=item.productId" +
                                        NEWLINE +
                                        "order by book.bookId, item.amount";
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
                var fields = "orderEvent.orderdetail.orderId,book.bookId,book.title,item.amount".SplitCsv();
                env.SendEventBean(MakeEventOne());
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "PO200901", "10020", "Enders Game", 10 },
                        new object[] { "PO200901", "10020", "Enders Game", 30 },
                        new object[] { "PO200901", "10021", "Foundation 1", 25 }
                    });

                env.SendEventBean(MakeEventTwo());
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "PO200902", "10022", "Stranger in a Strange Land", 5 } });

                env.SendEventBean(MakeEventThree());
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "PO200903", "10021", "Foundation 1", 50 } });
            }
        }

        private class EPLContainedUnidirectionalJoinCount : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select count(*) from " +
                               "OrderBean OrderBean unidirectional, " +
                               "OrderBean[books] as book, " +
                               "OrderBean[orderdetail.items] item " +
                               "where book.bookId = item.productId order by book.bookId asc, item.amount asc";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(MakeEventOne());
                env.AssertPropsNew("s0", "count(*)".SplitCsv(), new object[] { 3L });

                env.SendEventBean(MakeEventTwo());
                env.AssertPropsNew("s0", "count(*)".SplitCsv(), new object[] { 1L });

                env.SendEventBean(MakeEventThree());
                env.AssertPropsNew("s0", "count(*)".SplitCsv(), new object[] { 1L });

                env.SendEventBean(MakeEventFour());
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class EPLContainedJoinCount : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "count(*)".SplitCsv();
                var stmtText = "@name('s0') select count(*) from " +
                               "OrderBean[books]#unique(bookId) book, " +
                               "OrderBean[orderdetail.items]#keepall item " +
                               "where book.bookId = item.productId";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(MakeEventOne());
                env.AssertPropsNew("s0", fields, new object[] { 3L });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 3L } });

                env.SendEventBean(MakeEventTwo());
                env.AssertPropsNew("s0", "count(*)".SplitCsv(), new object[] { 4L });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 4L } });

                env.SendEventBean(MakeEventThree());
                env.AssertPropsNew("s0", "count(*)".SplitCsv(), new object[] { 5L });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 5L } });

                env.SendEventBean(MakeEventFour());
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(MakeEventOne());
                env.AssertPropsNew("s0", "count(*)".SplitCsv(), new object[] { 8L });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 8L } });

                env.UndeployAll();
            }
        }

        private class EPLContainedJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "book.bookId,item.itemId,amount".SplitCsv();
                var stmtText = "@name('s0') select book.bookId,item.itemId,amount from " +
                               "OrderBean[books]#firstunique(bookId) book, " +
                               "OrderBean[orderdetail.items]#keepall item " +
                               "where book.bookId = item.productId " +
                               "order by book.bookId, item.itemId";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(MakeEventOne());
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "10020", "A001", 10 }, new object[] { "10020", "A003", 30 },
                        new object[] { "10021", "A002", 25 }
                    });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "10020", "A001", 10 }, new object[] { "10020", "A003", 30 },
                        new object[] { "10021", "A002", 25 }
                    });

                env.SendEventBean(MakeEventTwo());
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "10022", "B001", 5 } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "10020", "A001", 10 }, new object[] { "10020", "A003", 30 },
                        new object[] { "10021", "A002", 25 }, new object[] { "10022", "B001", 5 }
                    });

                env.SendEventBean(MakeEventThree());
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "10021", "C001", 50 } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "10020", "A001", 10 }, new object[] { "10020", "A003", 30 },
                        new object[] { "10021", "A002", 25 }, new object[] { "10021", "C001", 50 },
                        new object[] { "10022", "B001", 5 }
                    });

                env.SendEventBean(MakeEventFour());
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class EPLContainedAloneCount : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "count(*)".SplitCsv();

                var stmtText = "@name('s0') select count(*) from OrderBean[books]";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(MakeEventOne());
                env.AssertPropsNew("s0", fields, new object[] { 3L });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 3L } });

                env.SendEventBean(MakeEventFour());
                env.AssertPropsNew("s0", fields, new object[] { 5L });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 5L } });

                env.UndeployAll();
            }
        }

        private class EPLContainedPropertyAccess : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') @IterableUnbound select bookId from OrderBean[books]").AddListener("s0");
                env.CompileDeploy(
                    "@name('s1') @IterableUnbound select books[0].author as val from OrderBean(books[0].bookId = '10020')");

                env.SendEventBean(MakeEventOne());
                env.AssertPropsPerRowLastNew(
                    "s0",
                    "bookId".SplitCsv(),
                    new object[][] { new object[] { "10020" }, new object[] { "10021" }, new object[] { "10022" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    "bookId".SplitCsv(),
                    new object[][] { new object[] { "10020" }, new object[] { "10021" }, new object[] { "10022" } });
                env.AssertPropsPerRowIterator(
                    "s1",
                    "val".SplitCsv(),
                    new object[][] { new object[] { "Orson Scott Card" } });

                env.SendEventBean(MakeEventFour());
                env.AssertPropsPerRowLastNew(
                    "s0",
                    "bookId".SplitCsv(),
                    new object[][] { new object[] { "10031" }, new object[] { "10032" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    "bookId".SplitCsv(),
                    new object[][] { new object[] { "10031" }, new object[] { "10032" } });
                env.AssertPropsPerRowIterator(
                    "s1",
                    "val".SplitCsv(),
                    new object[][] { new object[] { "Orson Scott Card" } });

                // add where clause
                env.UndeployAll();
                env.CompileDeploy("@name('s0') select bookId from OrderBean[books where author='Orson Scott Card']")
                    .AddListener("s0");
                env.SendEventBean(MakeEventOne());
                env.AssertPropsPerRowLastNew("s0", "bookId".SplitCsv(), new object[][] { new object[] { "10020" } });

                env.UndeployAll();
            }
        }

        private class EPLContainedIRStreamArrayItem : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') @IterableUnbound select irstream bookId from OrderBean[books[0]]";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(MakeEventOne());
                env.AssertPropsPerRowLastNew("s0", "bookId".SplitCsv(), new object[][] { new object[] { "10020" } });
                env.AssertPropsPerRowIterator("s0", "bookId".SplitCsv(), new object[][] { new object[] { "10020" } });

                env.SendEventBean(MakeEventFour());
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.IsNull(listener.LastOldData);
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.LastNewData,
                            "bookId".SplitCsv(),
                            new object[][] { new object[] { "10031" } });
                        listener.Reset();
                    });
                env.AssertPropsPerRowIterator("s0", "bookId".SplitCsv(), new object[][] { new object[] { "10031" } });

                env.UndeployAll();
            }
        }

        private class EPLContainedSplitWords : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') insert into WordStream select * from SentenceEvent[words]";

                var fields = "word".SplitCsv();
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(new SentenceEvent("I am testing this"));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "I" }, new object[] { "am" }, new object[] { "testing" }, new object[] { "this" }
                    });

                env.UndeployAll();
            }
        }

        private class EPLContainedArrayProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create objectarray schema ContainedId(id string)", path);
                env.CompileDeploy(
                        "@name('s0') select * from SupportStringBeanWithArray[select topId, * from containedIds @type(ContainedId)]",
                        path)
                    .AddListener("s0");
                env.SendEventBean(new SupportStringBeanWithArray("A", "one,two,three".SplitCsv()));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    "topId,id".SplitCsv(),
                    new object[][]
                        { new object[] { "A", "one" }, new object[] { "A", "two" }, new object[] { "A", "three" } });
                env.UndeployAll();
            }
        }
    }
} // end of namespace