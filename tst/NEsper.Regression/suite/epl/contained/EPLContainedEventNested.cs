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
            WithNamedWindowFilter(execs);
            WithNamedWindowSubquery(execs);
            WithNamedWindowOnTrigger(execs);
            WithSimple(execs);
            WithWhere(execs);
            WithColumnSelect(execs);
            WithPatternSelect(execs);
            WithSubSelect(execs);
            WithUnderlyingSelect(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithUnderlyingSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedUnderlyingSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithSubSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedSubSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedPatternSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithColumnSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedColumnSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithWhere(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedWhere());
            return execs;
        }

        public static IList<RegressionExecution> WithSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedSimple());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowOnTrigger(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedNamedWindowOnTrigger());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowSubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedNamedWindowSubquery());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedNamedWindowFilter());
            return execs;
        }

        private static void TryAssertionColumnSelect(RegressionEnvironment env)
        {
            var fields = new[] {"OrderId", "BookId", "ReviewId"};

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
                var fields = new[] {"ReviewId"};

                var path = new RegressionPath();
                env.CompileDeploy("create window OrderWindowNWF#lastevent as OrderBean", path);
                env.CompileDeploy("insert into OrderWindowNWF select * from OrderBean", path);

                var stmtText =
                    "@Name('s0') select ReviewId from OrderWindowNWF[Books][Reviews] bookReviews order by ReviewId asc";
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
                var fields = new[] {"TheString", "TotalPrice"};

                var path = new RegressionPath();
                env.CompileDeploy("create window OrderWindowNWS#lastevent as OrderBean", path);
                env.CompileDeploy("insert into OrderWindowNWS select * from OrderBean", path);

                var stmtText =
                    "@Name('s0') select *, (select sum(Price) from OrderWindowNWS[Books]) as TotalPrice from SupportBean";
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
                var fields = new[] {"TheString", "IntPrimitive"};
                var path = new RegressionPath();

                env.CompileDeploy("create window SupportBeanWindow#lastevent as SupportBean", path);
                env.CompileDeploy("insert into SupportBeanWindow select * from SupportBean", path);
                env.CompileDeploy("create window OrderWindowNWOT#lastevent as OrderBean", path);
                env.CompileDeploy("insert into OrderWindowNWOT select * from OrderBean", path);

                var stmtText =
                    "@Name('s0') on OrderWindowNWOT[Books] owb select sbw.* from SupportBeanWindow sbw where TheString = Title";
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
                var fields = new[] {"ReviewId"};

                var stmtText =
                    "@Name('s0') select ReviewId from OrderBean[Books][Reviews] bookReviews order by ReviewId asc";
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
                var fields = new[] {"ReviewId"};

                // try where in root
                var stmtText =
                    "@Name('s0') select ReviewId from OrderBean[Books where Title = 'Enders Game'][Reviews] bookReviews order by ReviewId asc";
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
                    "@Name('s0') select ReviewId from OrderBean[Books where Title = 'Enders Game'][Reviews where ReviewId in (1, 10)] bookReviews order by ReviewId asc";
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
                    "@Name('s0') select ReviewId from OrderBean[Books as bc][Reviews as rw where rw.ReviewId in (1, 10) and bc.Title = 'Enders Game'] bookReviews order by ReviewId asc";
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
                    "@Name('s0') select * from OrderBean[select BookId, OrderDetail.OrderId as OrderId from Books][select ReviewId from Reviews] bookReviews order by ReviewId asc";
                env.CompileDeploy(stmtText).AddListener("s0");
                TryAssertionColumnSelect(env);
                env.UndeployAll();

                // stream wildcards identify fragments
                stmtText =
                    "@Name('s0') select OrderFrag.OrderDetail.OrderId as OrderId, BookFrag.BookId as BookId, ReviewFrag.ReviewId as ReviewId " +
                    "from OrderBean[Books as Book][select myorder.* as OrderFrag, Book.* as BookFrag, review.* as ReviewFrag from Reviews as review] as myorder";
                env.CompileDeploy(stmtText).AddListener("s0");
                TryAssertionColumnSelect(env);
                env.UndeployAll();

                // one event type dedicated as underlying
                stmtText =
                    "@Name('s0') select OrderDetail.OrderId as OrderId, BookFrag.BookId as BookId, reviewFrag.ReviewId as ReviewId " +
                    "from OrderBean[Books as Book][select myorder.*, Book.* as BookFrag, review.* as reviewFrag from Reviews as review] as myorder";
                env.CompileDeploy(stmtText).AddListener("s0");
                TryAssertionColumnSelect(env);
                env.UndeployAll();

                // wildcard unnamed as underlying
                stmtText = "@Name('s0') select OrderFrag.OrderDetail.OrderId as OrderId, BookId, ReviewId " +
                           "from OrderBean[select * from Books][select myorder.* as OrderFrag, ReviewId from Reviews as review] as myorder";
                env.CompileDeploy(stmtText).AddListener("s0");
                TryAssertionColumnSelect(env);
                env.UndeployAll();

                // wildcard named as underlying
                stmtText =
                    "@Name('s0') select OrderFrag.OrderDetail.OrderId as OrderId, BookFrag.BookId as BookId, reviewFrag.ReviewId as ReviewId " +
                    "from OrderBean[select * from Books as BookFrag][select myorder.* as OrderFrag, review.* as reviewFrag from Reviews as review] as myorder";
                env.CompileDeploy(stmtText).AddListener("s0");
                TryAssertionColumnSelect(env);
                env.UndeployAll();

                // object model
                stmtText = "@Name('s0') select OrderFrag.OrderDetail.OrderId as OrderId, BookId, ReviewId " +
                           "from OrderBean[select * from Books][select myorder.* as OrderFrag, ReviewId from Reviews as review] as myorder";
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
                        "every r=OrderBean[Books][Reviews] -> SupportBean(IntPrimitive = r[0].ReviewId)]")
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
                        "@Name('s0') select TheString from SupportBean S0 where " +
                        "exists (select * from OrderBean[Books][Reviews]#unique(ReviewId) where ReviewId = S0.IntPrimitive)")
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
                var fields = new[] {"OrderId", "BookId", "ReviewId"};

                var stmtText =
                    "@Name('s0') select OrderDetail.OrderId as OrderId, BookFrag.BookId as BookId, reviewFrag.ReviewId as ReviewId " +
                    "from OrderBean[Books as Book][select myorder.*, Book.* as BookFrag, Review.* as reviewFrag from Reviews as Review] as myorder";
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
                    "select BookId from OrderBean[select count(*) from Books]",
                    "Expression in a property-selection may not utilize an aggregation function [select BookId from OrderBean[select count(*) from Books]]");

                TryInvalidCompile(
                    env,
                    "select BookId from OrderBean[select BookId, (select abc from Review#lastevent) from Books]",
                    "Expression in a property-selection may not utilize a subselect [select BookId from OrderBean[select BookId, (select abc from Review#lastevent) from Books]]");

                TryInvalidCompile(
                    env,
                    "select BookId from OrderBean[select prev(1, BookId) from Books]",
                    "Failed to validate Contained-event expression 'prev(1,BookId)': Previous function cannot be used in this context [select BookId from OrderBean[select prev(1, BookId) from Books]]");

                TryInvalidCompile(
                    env,
                    "select BookId from OrderBean[select * from Books][select * from Reviews]",
                    "A column name must be supplied for all but one stream if multiple streams are selected via the stream.* notation [select BookId from OrderBean[select * from Books][select * from Reviews]]");

                TryInvalidCompile(
                    env,
                    "select BookId from OrderBean[select abc from Books][Reviews]",
                    "Failed to validate Contained-event expression 'abc': Property named 'abc' is not valid in any stream [select BookId from OrderBean[select abc from Books][Reviews]]");

                TryInvalidCompile(
                    env,
                    "select BookId from OrderBean[Books][Reviews]",
                    "Failed to validate select-clause expression 'BookId': Property named 'BookId' is not valid in any stream [select BookId from OrderBean[Books][Reviews]]");

                TryInvalidCompile(
                    env,
                    "select OrderId from OrderBean[Books]",
                    "Failed to validate select-clause expression 'OrderId': Property named 'OrderId' is not valid in any stream [select OrderId from OrderBean[Books]]");

                TryInvalidCompile(
                    env,
                    "select * from OrderBean[Books where abc=1]",
                    "Failed to validate Contained-event expression 'abc=1': Property named 'abc' is not valid in any stream [select * from OrderBean[Books where abc=1]]");

                TryInvalidCompile(
                    env,
                    "select * from OrderBean[abc]",
                    "Failed to validate Contained-event expression 'abc': Property named 'abc' is not valid in any stream [select * from OrderBean[abc]]");
            }
        }
    }
} // end of namespace