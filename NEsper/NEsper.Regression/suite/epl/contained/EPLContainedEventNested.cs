///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bookexample;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil;

namespace com.espertech.esper.regressionlib.suite.epl.contained
{
    public class EPLContainedEventNested
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLContainedNamedWindowFilter());
            execs.Add(new EPLContainedNamedWindowSubquery());
            execs.Add(new EPLContainedNamedWindowOnTrigger());
            execs.Add(new EPLContainedSimple());
            execs.Add(new EPLContainedWhere());
            execs.Add(new EPLContainedColumnSelect());
            execs.Add(new EPLContainedPatternSelect());
            execs.Add(new EPLContainedSubSelect());
            execs.Add(new EPLContainedUnderlyingSelect());
            execs.Add(new EPLContainedInvalid());
            return execs;
        }

        private static void TryAssertionColumnSelect(RegressionEnvironment env)
        {
            var fields = "orderId,bookId,reviewId".SplitCsv();

            env.SendEventBean(OrderBeanFactory.MakeEventOne());
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {
                    new object[] {"PO200901", "10020", 1}, new object[] {"PO200901", "10020", 2},
                    new object[] {"PO200901", "10021", 10}
                });
            env.Listener("s0").Reset();

            env.SendEventBean(OrderBeanFactory.MakeEventFour());
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {new object[] {"PO200904", "10031", 201}});
            env.Listener("s0").Reset();
        }

        internal class EPLContainedNamedWindowFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "reviewId".SplitCsv();

                var path = new RegressionPath();
                env.CompileDeploy("create window OrderWindowNWF#lastevent as OrderBean", path);
                env.CompileDeploy("insert into OrderWindowNWF select * from OrderBean", path);

                var stmtText =
                    "@Name('s0') select reviewId from OrderWindowNWF[books][reviews] bookReviews order by reviewId asc";
                env.CompileDeploy(stmtText, path).AddListener("s0");

                env.SendEventBean(OrderBeanFactory.MakeEventOne());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {1}, new object[] {2}, new object[] {10}});
                env.Listener("s0").Reset();

                env.SendEventBean(OrderBeanFactory.MakeEventFour());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {201}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class EPLContainedNamedWindowSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "theString,totalPrice".SplitCsv();

                var path = new RegressionPath();
                env.CompileDeploy("create window OrderWindowNWS#lastevent as OrderBean", path);
                env.CompileDeploy("insert into OrderWindowNWS select * from OrderBean", path);

                var stmtText =
                    "@Name('s0') select *, (select sum(price) from OrderWindowNWS[books]) as totalPrice from SupportBean";
                env.CompileDeploy(stmtText, path).AddListener("s0");

                env.SendEventBean(OrderBeanFactory.MakeEventOne());
                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"E1", 24d + 35d + 27d}});
                env.Listener("s0").Reset();

                env.SendEventBean(OrderBeanFactory.MakeEventFour());
                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"E2", 15d + 13d}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class EPLContainedNamedWindowOnTrigger : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "theString,intPrimitive".SplitCsv();
                var path = new RegressionPath();

                env.CompileDeploy("create window SupportBeanWindow#lastevent as SupportBean", path);
                env.CompileDeploy("insert into SupportBeanWindow select * from SupportBean", path);
                env.CompileDeploy("create window OrderWindowNWOT#lastevent as OrderBean", path);
                env.CompileDeploy("insert into OrderWindowNWOT select * from OrderBean", path);

                var stmtText =
                    "@Name('s0') on OrderWindowNWOT[books] owb select sbw.* from SupportBeanWindow sbw where theString = title";
                env.CompileDeploy(stmtText, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(OrderBeanFactory.MakeEventFour());
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("Foundation 2", 2));
                env.SendEventBean(OrderBeanFactory.MakeEventFour());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"Foundation 2", 2}});

                env.UndeployAll();
            }
        }

        internal class EPLContainedSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "reviewId".SplitCsv();

                var stmtText =
                    "@Name('s0') select reviewId from OrderBean[books][reviews] bookReviews order by reviewId asc";
                env.CompileDeploy(stmtText).AddListener("s0");
                AssertStatelessStmt(env, "s0", true);

                env.SendEventBean(OrderBeanFactory.MakeEventOne());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {1}, new object[] {2}, new object[] {10}});
                env.Listener("s0").Reset();

                env.SendEventBean(OrderBeanFactory.MakeEventFour());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {201}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class EPLContainedWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "reviewId".SplitCsv();

                // try where in root
                var stmtText =
                    "@Name('s0') select reviewId from OrderBean[books where title = 'Enders Game'][reviews] bookReviews order by reviewId asc";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(OrderBeanFactory.MakeEventOne());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {1}, new object[] {2}});
                env.Listener("s0").Reset();

                // try where in different levels
                env.UndeployAll();
                stmtText =
                    "@Name('s0') select reviewId from OrderBean[books where title = 'Enders Game'][reviews where reviewId in (1, 10)] bookReviews order by reviewId asc";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(OrderBeanFactory.MakeEventOne());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {1}});
                env.Listener("s0").Reset();

                // try where in combination
                env.UndeployAll();
                stmtText =
                    "@Name('s0') select reviewId from OrderBean[books as bc][reviews as rw where rw.reviewId in (1, 10) and bc.title = 'Enders Game'] bookReviews order by reviewId asc";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(OrderBeanFactory.MakeEventOne());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {1}});
                env.Listener("s0").Reset();
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLContainedColumnSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // columns supplied
                var stmtText =
                    "@Name('s0') select * from OrderBean[select bookId, orderdetail.orderId as orderId from books][select reviewId from reviews] bookReviews order by reviewId asc";
                env.CompileDeploy(stmtText).AddListener("s0");
                TryAssertionColumnSelect(env);
                env.UndeployAll();

                // stream wildcards identify fragments
                stmtText =
                    "@Name('s0') select orderFrag.orderdetail.orderId as orderId, bookFrag.bookId as bookId, reviewFrag.reviewId as reviewId " +
                    "from OrderBean[books as book][select myorder.* as orderFrag, book.* as bookFrag, review.* as reviewFrag from reviews as review] as myorder";
                env.CompileDeploy(stmtText).AddListener("s0");
                TryAssertionColumnSelect(env);
                env.UndeployAll();

                // one event type dedicated as underlying
                stmtText =
                    "@Name('s0') select orderdetail.orderId as orderId, bookFrag.bookId as bookId, reviewFrag.reviewId as reviewId " +
                    "from OrderBean[books as book][select myorder.*, book.* as bookFrag, review.* as reviewFrag from reviews as review] as myorder";
                env.CompileDeploy(stmtText).AddListener("s0");
                TryAssertionColumnSelect(env);
                env.UndeployAll();

                // wildcard unnamed as underlying
                stmtText = "@Name('s0') select orderFrag.orderdetail.orderId as orderId, bookId, reviewId " +
                           "from OrderBean[select * from books][select myorder.* as orderFrag, reviewId from reviews as review] as myorder";
                env.CompileDeploy(stmtText).AddListener("s0");
                TryAssertionColumnSelect(env);
                env.UndeployAll();

                // wildcard named as underlying
                stmtText =
                    "@Name('s0') select orderFrag.orderdetail.orderId as orderId, bookFrag.bookId as bookId, reviewFrag.reviewId as reviewId " +
                    "from OrderBean[select * from books as bookFrag][select myorder.* as orderFrag, review.* as reviewFrag from reviews as review] as myorder";
                env.CompileDeploy(stmtText).AddListener("s0");
                TryAssertionColumnSelect(env);
                env.UndeployAll();

                // object model
                stmtText = "@Name('s0') select orderFrag.orderdetail.orderId as orderId, bookId, reviewId " +
                           "from OrderBean[select * from books][select myorder.* as orderFrag, reviewId from reviews as review] as myorder";
                env.EplToModelCompileDeploy(stmtText).AddListener("s0");
                TryAssertionColumnSelect(env);
                env.UndeployAll();

                // with where-clause
                stmtText = "@Name('s0') select * from AccountEvent[select * from wallets where currency=\"USD\"]";
                var model = env.EplToModel(stmtText);
                Assert.AreEqual(stmtText, model.ToEPL());
            }
        }

        internal class EPLContainedPatternSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@Name('s0') select * from pattern [" +
                        "every r=OrderBean[books][reviews] => SupportBean(IntPrimitive = r[0].reviewId)]")
                    .AddListener("s0");

                env.SendEventBean(OrderBeanFactory.MakeEventOne());
                env.SendEventBean(OrderBeanFactory.MakeEventFour());

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.SendEventBean(new SupportBean("E2", -1));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.SendEventBean(new SupportBean("E2", 201));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }

        internal class EPLContainedSubSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@Name('s0') select TheString from SupportBean s0 where " +
                        "exists (select * from OrderBean[books][reviews]#unique(reviewId) where reviewId = s0.IntPrimitive)")
                    .AddListener("s0");

                env.SendEventBean(OrderBeanFactory.MakeEventOne());
                env.SendEventBean(OrderBeanFactory.MakeEventFour());

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.SendEventBean(new SupportBean("E2", -1));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.SendEventBean(new SupportBean("E2", 201));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }

        internal class EPLContainedUnderlyingSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "orderId,bookId,reviewId".SplitCsv();

                var stmtText =
                    "@Name('s0') select orderdetail.orderId as orderId, bookFrag.bookId as bookId, reviewFrag.reviewId as reviewId " +
                    "from OrderBean[books as book][select myorder.*, book.* as bookFrag, review.* as reviewFrag from reviews as review] as myorder";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(OrderBeanFactory.MakeEventOne());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"PO200901", "10020", 1}, new object[] {"PO200901", "10020", 2},
                        new object[] {"PO200901", "10021", 10}
                    });
                env.Listener("s0").Reset();

                env.SendEventBean(OrderBeanFactory.MakeEventFour());
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"PO200904", "10031", 201}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class EPLContainedInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(
                    env,
                    "select bookId from OrderBean[select count(*) from books]",
                    "Expression in a property-selection may not utilize an aggregation function [select bookId from OrderBean[select count(*) from books]]");

                TryInvalidCompile(
                    env,
                    "select bookId from OrderBean[select bookId, (select abc from review#lastevent) from books]",
                    "Expression in a property-selection may not utilize a subselect [select bookId from OrderBean[select bookId, (select abc from review#lastevent) from books]]");

                TryInvalidCompile(
                    env,
                    "select bookId from OrderBean[select prev(1, bookId) from books]",
                    "Failed to validate contained-event expression 'prev(1,bookId)': Previous function cannot be used in this context [select bookId from OrderBean[select prev(1, bookId) from books]]");

                TryInvalidCompile(
                    env,
                    "select bookId from OrderBean[select * from books][select * from reviews]",
                    "A column name must be supplied for all but one stream if multiple streams are selected via the stream.* notation [select bookId from OrderBean[select * from books][select * from reviews]]");

                TryInvalidCompile(
                    env,
                    "select bookId from OrderBean[select abc from books][reviews]",
                    "Failed to validate contained-event expression 'abc': Property named 'abc' is not valid in any stream [select bookId from OrderBean[select abc from books][reviews]]");

                TryInvalidCompile(
                    env,
                    "select bookId from OrderBean[books][reviews]",
                    "Failed to validate select-clause expression 'bookId': Property named 'bookId' is not valid in any stream [select bookId from OrderBean[books][reviews]]");

                TryInvalidCompile(
                    env,
                    "select orderId from OrderBean[books]",
                    "Failed to validate select-clause expression 'orderId': Property named 'orderId' is not valid in any stream [select orderId from OrderBean[books]]");

                TryInvalidCompile(
                    env,
                    "select * from OrderBean[books where abc=1]",
                    "Failed to validate contained-event expression 'abc=1': Property named 'abc' is not valid in any stream [select * from OrderBean[books where abc=1]]");

                TryInvalidCompile(
                    env,
                    "select * from OrderBean[abc]",
                    "Failed to validate contained-event expression 'abc': Property named 'abc' is not valid in any stream [select * from OrderBean[abc]]");
            }
        }
    }
} // end of namespace