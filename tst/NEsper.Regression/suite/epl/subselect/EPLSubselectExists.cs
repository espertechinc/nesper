///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectExists
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithExistsInSelect(execs);
            WithExistsInSelectOM(execs);
            WithExistsInSelectCompile(execs);
            WithExistsSceneOne(execs);
            WithExistsFiltered(execs);
            WithTwoExistsFiltered(execs);
            WithNotExistsOM(execs);
            WithNotExistsCompile(execs);
            WithNotExists(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithNotExists(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectNotExists());
            return execs;
        }

        public static IList<RegressionExecution> WithNotExistsCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectNotExistsCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithNotExistsOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectNotExistsOM());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoExistsFiltered(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectTwoExistsFiltered());
            return execs;
        }

        public static IList<RegressionExecution> WithExistsFiltered(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectExistsFiltered());
            return execs;
        }

        public static IList<RegressionExecution> WithExistsSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectExistsSceneOne());
            return execs;
        }

        public static IList<RegressionExecution> WithExistsInSelectCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectExistsInSelectCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithExistsInSelectOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectExistsInSelectOM());
            return execs;
        }

        public static IList<RegressionExecution> WithExistsInSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectExistsInSelect());
            return execs;
        }

        private class EPLSubselectExistsInSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select exists (select * from SupportBean_S1#length(1000)) as value from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                RunTestExistsInSelect(env);

                env.UndeployAll();
            }
        }

        private class EPLSubselectExistsInSelectOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var subquery = new EPStatementObjectModel();
                subquery.SelectClause = SelectClause.CreateWildcard();
                subquery.FromClause = FromClause.Create(
                    FilterStream.Create("SupportBean_S1").AddView(View.Create("length", Expressions.Constant(1000))));

                var model = new EPStatementObjectModel();
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_S0"));
                model.SelectClause = SelectClause.Create().Add(Expressions.SubqueryExists(subquery), "value");
                model = env.CopyMayFail(model);

                var stmtText = "select exists (select * from SupportBean_S1#length(1000)) as value from SupportBean_S0";
                Assert.AreEqual(stmtText, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                RunTestExistsInSelect(env);

                env.UndeployAll();
            }
        }

        private class EPLSubselectExistsInSelectCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select exists (select * from SupportBean_S1#length(1000)) as value from SupportBean_S0";
                env.EplToModelCompileDeploy(stmtText).AddListener("s0").Milestone(1);

                RunTestExistsInSelect(env);

                env.UndeployAll();
            }
        }

        private class EPLSubselectExistsSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select Id from SupportBean_S0 where exists (select * from SupportBean_S1#length(1000))";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(2));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S1(-1));
                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEqualsNew("s0", "Id", 2);

                env.SendEventBean(new SupportBean_S1(-2));
                env.SendEventBean(new SupportBean_S0(3));
                env.AssertEqualsNew("s0", "Id", 3);

                env.UndeployAll();
            }
        }

        private class EPLSubselectExistsFiltered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select Id from SupportBean_S0 as s0 where exists (select * from SupportBean_S1#length(1000) as s1 where s1.Id=s0.Id)";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(2));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S1(-1));
                env.SendEventBean(new SupportBean_S0(2));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S1(-2));
                env.SendEventBean(new SupportBean_S0(-2));
                env.AssertEqualsNew("s0", "Id", -2);

                env.SendEventBean(new SupportBean_S1(1));
                env.SendEventBean(new SupportBean_S1(2));
                env.SendEventBean(new SupportBean_S1(3));
                env.SendEventBean(new SupportBean_S0(3));
                env.AssertEqualsNew("s0", "Id", 3);

                env.UndeployAll();
            }
        }

        private class EPLSubselectTwoExistsFiltered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Id from SupportBean_S0 as s0 where " +
                               "exists (select * from SupportBean_S1#length(1000) as s1 where s1.Id=s0.Id) " +
                               "and " +
                               "exists (select * from SupportBean_S2#length(1000) as s2 where s2.Id=s0.Id) ";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(2));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S2(3));
                env.SendEventBean(new SupportBean_S0(3));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S1(3));
                env.SendEventBean(new SupportBean_S0(3));
                env.AssertEqualsNew("s0", "Id", 3);

                env.SendEventBean(new SupportBean_S1(1));
                env.SendEventBean(new SupportBean_S1(2));
                env.SendEventBean(new SupportBean_S2(1));
                env.SendEventBean(new SupportBean_S0(1));
                env.AssertEqualsNew("s0", "Id", 1);

                env.SendEventBean(new SupportBean_S0(2));
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class EPLSubselectNotExistsOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var subquery = new EPStatementObjectModel();
                subquery.SelectClause = SelectClause.CreateWildcard();
                subquery.FromClause = FromClause.Create(
                    FilterStream.Create("SupportBean_S1").AddView("length", Expressions.Constant(1000)));

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("Id");
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_S0"));
                model.WhereClause = Expressions.Not(Expressions.SubqueryExists(subquery));
                model = env.CopyMayFail(model);

                var stmtText =
                    "select Id from SupportBean_S0 where not exists (select * from SupportBean_S1#length(1000))";
                Assert.AreEqual(stmtText, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEqualsNew("s0", "Id", 2);

                env.SendEventBean(new SupportBean_S1(-1));
                env.SendEventBean(new SupportBean_S0(1));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S1(-2));
                env.SendEventBean(new SupportBean_S0(3));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class EPLSubselectNotExistsCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select Id from SupportBean_S0 where not exists (select * from SupportBean_S1#length(1000))";
                env.EplToModelCompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEqualsNew("s0", "Id", 2);

                env.SendEventBean(new SupportBean_S1(-1));
                env.SendEventBean(new SupportBean_S0(1));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S1(-2));
                env.SendEventBean(new SupportBean_S0(3));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class EPLSubselectNotExists : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select Id from SupportBean_S0 where not exists (select * from SupportBean_S1#length(1000))";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEqualsNew("s0", "Id", 2);

                env.SendEventBean(new SupportBean_S1(-1));
                env.SendEventBean(new SupportBean_S0(1));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S1(-2));
                env.SendEventBean(new SupportBean_S0(3));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private static void RunTestExistsInSelect(RegressionEnvironment env)
        {
            env.SendEventBean(new SupportBean_S0(2));
            env.AssertEqualsNew("s0", "value", false);

            env.SendEventBean(new SupportBean_S1(-1));
            env.SendEventBean(new SupportBean_S0(2));
            env.AssertEqualsNew("s0", "value", true);
        }
    }
} // end of namespace