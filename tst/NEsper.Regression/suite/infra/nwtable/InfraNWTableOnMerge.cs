///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.bookexample;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client.scopetest;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.common.@internal.support.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableOnMerge
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithOnMergeSimpleInsert(execs);
            WithOnMergeMatchNoMatch(execs);
            WithUpdateNestedEvent(execs);
            WithOnMergeInsertStream(execs);

            foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                WithInsertOtherStream(rep, execs);
            }

            WithMultiactionDeleteUpdate(execs);
            WithUpdateOrderOfFields(execs);
            WithSubqueryNotMatched(execs);
            WithPatternMultimatch(execs);
            WithNoWhereClause(execs);
            WithMultipleInsert(execs);
            WithFlow(execs);

            foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                if (!rep.IsAvroOrJsonEvent()) {
                    WithInnerTypeAndVariable(rep, execs);
                }
            }

            WithInvalid(execs);

            foreach (var namedWindow in new bool[] { true, false }) {
                WithInsertOnly(namedWindow, execs);
            }

            WithDeleteThenUpdate(execs);
            WithPropertyEvalUpdate(execs);
            WithPropertyEvalInsertNoMatch(execs);
            WithSetArrayElementWithIndex(execs);
            WithSetArrayElementWithIndexInvalid(execs);

            return execs;
        }

        public static IList<RegressionExecution> WithSetArrayElementWithIndexInvalid(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSetArrayElementWithIndexInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithSetArrayElementWithIndex(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSetArrayElementWithIndex(false, false));
            execs.Add(new InfraSetArrayElementWithIndex(true, true));
            return execs;
        }

        public static IList<RegressionExecution> WithPropertyEvalInsertNoMatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraPropertyEvalInsertNoMatch(true));
            execs.Add(new InfraPropertyEvalInsertNoMatch(false));
            return execs;
        }

        public static IList<RegressionExecution> WithPropertyEvalUpdate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraPropertyEvalUpdate(true));
            execs.Add(new InfraPropertyEvalUpdate(false));
            return execs;
        }

        public static IList<RegressionExecution> WithDeleteThenUpdate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraDeleteThenUpdate(true));
            execs.Add(new InfraDeleteThenUpdate(false));
            return execs;
        }

        public static IList<RegressionExecution> WithInsertOnly(
            bool namedWindow,
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInsertOnly(namedWindow, true, false, false));
            execs.Add(new InfraInsertOnly(namedWindow, false, false, false));
            execs.Add(new InfraInsertOnly(namedWindow, false, false, true));
            execs.Add(new InfraInsertOnly(namedWindow, false, true, false));
            execs.Add(new InfraInsertOnly(namedWindow, false, true, true));
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInvalid(true));
            execs.Add(new InfraInvalid(false));
            return execs;
        }

        public static IList<RegressionExecution> WithInnerTypeAndVariable(
            EventRepresentationChoice rep,
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInnerTypeAndVariable(true, rep));
            execs.Add(new InfraInnerTypeAndVariable(false, rep));
            return execs;
        }

        public static IList<RegressionExecution> WithFlow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFlow(true));
            execs.Add(new InfraFlow(false));
            return execs;
        }

        public static IList<RegressionExecution> WithMultipleInsert(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraMultipleInsert(true));
            execs.Add(new InfraMultipleInsert(false));
            return execs;
        }

        public static IList<RegressionExecution> WithNoWhereClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNoWhereClause(true));
            execs.Add(new InfraNoWhereClause(false));
            return execs;
        }

        public static IList<RegressionExecution> WithPatternMultimatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraPatternMultimatch(true));
            execs.Add(new InfraPatternMultimatch(false));
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryNotMatched(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSubqueryNotMatched(true));
            execs.Add(new InfraSubqueryNotMatched(false));
            return execs;
        }

        public static IList<RegressionExecution> WithUpdateOrderOfFields(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraUpdateOrderOfFields(true));
            execs.Add(new InfraUpdateOrderOfFields(false));
            return execs;
        }

        public static IList<RegressionExecution> WithMultiactionDeleteUpdate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraMultiactionDeleteUpdate(true));
            execs.Add(new InfraMultiactionDeleteUpdate(false));
            return execs;
        }

        public static IList<RegressionExecution> WithInsertOtherStream(
            EventRepresentationChoice rep,
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInsertOtherStream(true, rep));
            execs.Add(new InfraInsertOtherStream(false, rep));
            return execs;
        }

        public static IList<RegressionExecution> WithOnMergeInsertStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraOnMergeInsertStream(true));
            execs.Add(new InfraOnMergeInsertStream(false));
            return execs;
        }

        public static IList<RegressionExecution> WithUpdateNestedEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraUpdateNestedEvent(true));
            execs.Add(new InfraUpdateNestedEvent(false));
            return execs;
        }

        public static IList<RegressionExecution> WithOnMergeMatchNoMatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraOnMergeMatchNoMatch(true));
            execs.Add(new InfraOnMergeMatchNoMatch(false));
            return execs;
        }

        public static IList<RegressionExecution> WithOnMergeSimpleInsert(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraOnMergeSimpleInsert(true));
            execs.Add(new InfraOnMergeSimpleInsert(false));
            return execs;
        }

        internal class InfraSetArrayElementWithIndex : RegressionExecution
        {
            private readonly bool soda;
            private readonly bool namedWindow;

            public InfraSetArrayElementWithIndex(
                bool soda,
                bool namedWindow)
            {
                this.soda = soda;
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                // parenthesis are not required by due to precedence the SODA may output them
                RunAssertionSetWithIndex(
                    env,
                    namedWindow,
                    soda,
                    "(thearray[cnt])=1, (thearray[IntPrimitive])=2",
                    0,
                    1,
                    2,
                    0);
                RunAssertionSetWithIndex(env, namedWindow, soda, "cnt=cnt+1, (thearray[cnt])=1", 1, 0, 1, 0);
                RunAssertionSetWithIndex(
                    env,
                    namedWindow,
                    soda,
                    "cnt=cnt+1, (thearray[cnt])=3, cnt=cnt+1, (thearray[cnt])=4",
                    2,
                    0,
                    3,
                    4);
                RunAssertionSetWithIndex(env, namedWindow, soda, "cnt=cnt+1, (thearray[initial.cnt])=3", 1, 3, 0, 0);
            }

            private static void RunAssertionSetWithIndex(
                RegressionEnvironment env,
                bool namedWindow,
                bool soda,
                string setter,
                int cntExpected,
                params double[] thearrayExpected)
            {
                var path = new RegressionPath();
                var eplCreate = namedWindow
                    ? "@name('create') @public create window MyInfra#keepall(cnt int, thearray double[primitive]);\n"
                    : "@name('create') @public create table MyInfra(cnt int, thearray double[primitive]);\n";
                eplCreate +=
                    "@priority(1) on SupportBean merge MyInfra when not matched then insert select 0 as cnt, new double[3] as thearray;\n";
                env.CompileDeploy(eplCreate, path);

                var epl = "on SupportBean update MyInfra set " + setter;
                env.CompileDeploy(soda, epl, path);

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertIterator(
                    "create",
                    iterator => EPAssertionUtil.AssertProps(
                        iterator.Advance(),
                        "cnt,thearray".SplitCsv(),
                        new object[] { cntExpected, thearrayExpected }));

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "soda=" +
                       soda +
                       ", namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        internal class InfraSetArrayElementWithIndexInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplTable =
                    "@name('create') @public create table MyInfra(doublearray double[primitive], intarray int[primitive], notAnArray int)";
                env.Compile(eplTable, path);

                // invalid property
                env.TryInvalidCompile(
                    path,
                    "on SupportBean update MyInfra set c1[0]=1",
                    "Failed to validate assignment expression 'c1[0]=1': Property 'c1[0]' is not available for write access");
                env.TryInvalidCompile(
                    path,
                    "on SupportBean update MyInfra set c('a')=1",
                    "Failed to validate assignment expression 'c('a')=1': Property 'c('a')' is not available for write access");

                // index expression is not Integer
                env.TryInvalidCompile(
                    path,
                    "on SupportBean update MyInfra set doublearray[null]=1",
                    "Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'null' returns 'null' for expression 'doublearray'");

                // type incompatible cannot assign
                env.TryInvalidCompile(
                    path,
                    "on SupportBean update MyInfra set intarray[IntPrimitive]='x'",
                    "Failed to validate assignment expression 'intarray[IntPrimitive]=\"x\"': Invalid assignment to property 'intarray' component type 'int' from expression returning 'String'");
                env.TryInvalidCompile(
                    path,
                    "on SupportBean update MyInfra set intarray[IntPrimitive]=1L",
                    "Failed to validate assignment expression 'intarray[IntPrimitive]=1': Invalid assignment to property 'intarray' component type 'int' from expression returning 'long'");

                // not-an-array
                env.TryInvalidCompile(
                    path,
                    "on SupportBean update MyInfra set notAnArray[IntPrimitive]=1",
                    "Failed to validate assignment expression 'notAnArray[IntPrimitive]=1': Property 'notAnArray' is not an array");

                // not found
                env.TryInvalidCompile(
                    path,
                    "on SupportBean update MyInfra set dummy[IntPrimitive]=1",
                    "Failed to validate assignment expression 'dummy[IntPrimitive]=1': Property 'dummy' could not be found");

                // property found in updating-event
                env.TryInvalidCompile(
                    path,
                    "create schema UpdateEvent(dummy int[primitive]);\n" +
                    "on UpdateEvent update MyInfra set dummy[10]=1;\n",
                    "Failed to validate assignment expression 'dummy[10]=1': Property 'dummy[10]' is not available for write access");

                env.TryInvalidCompile(
                    path,
                    "create schema UpdateEvent(dummy int[primitive], position int);\n" +
                    "on UpdateEvent update MyInfra set dummy[position]=1;\n",
                    "Failed to validate assignment expression 'dummy[position]=1': Property 'dummy' could not be found");

                path.Clear();

                // runtime-behavior for index-overflow and null-array and null-index and
                var epl = "@name('create') create table MyInfra(doublearray double[primitive]);\n" +
                          "@priority(1) on SupportBean merge MyInfra when not matched then insert select new double[3] as doublearray;\n" +
                          "on SupportBean update MyInfra set doublearray[IntBoxed]=DoubleBoxed;\n";
                env.CompileDeploy(epl);

                // index returned is too large
                try {
                    var sb = new SupportBean();
                    sb.IntBoxed = 10;
                    sb.DoubleBoxed = 10d;
                    env.SendEventBean(sb);
                    Assert.Fail();
                }
                catch (Exception ex) {
                    Assert.IsTrue(ex.Message.Contains("Array length 3 less than index 10 for property 'doublearray'"));
                }

                // index returned null
                var sbIndexNull = new SupportBean();
                sbIndexNull.DoubleBoxed = 10d;
                env.SendEventBean(sbIndexNull);

                // rhs returned null for array-of-primitive
                var sbRHSNull = new SupportBean();
                sbRHSNull.IntBoxed = 1;
                env.SendEventBean(sbRHSNull);

                env.UndeployAll();
            }
        }

        internal class InfraPropertyEvalInsertNoMatch : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraPropertyEvalInsertNoMatch(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = "c1,c2".SplitCsv();
                var path = new RegressionPath();

                var stmtTextCreateOne = namedWindow
                    ? "@name('create') @public create window MyInfra#keepall() as (c1 string, c2 string)"
                    : "@name('create') @public create table MyInfra(c1 string primary key, c2 string)";
                env.CompileDeploy(stmtTextCreateOne, path);

                var epl = "@name('merge') on OrderBean[books] " +
                          "merge MyInfra mw " +
                          "insert select BookId as c1, title as c2 ";
                env.CompileDeploy(epl, path).AddListener("merge");

                env.SendEventBean(OrderBeanFactory.MakeEventOne());
                env.AssertPropsPerRowLastNew(
                    "merge",
                    fields,
                    new object[][] {
                        new object[] { "10020", "Enders Game" },
                        new object[] { "10021", "Foundation 1" }, new object[] { "10022", "Stranger in a Strange Land" }
                    });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        internal class InfraPropertyEvalUpdate : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraPropertyEvalUpdate(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "p0", "p1" };
                var path = new RegressionPath();
                var stmtTextCreateOne = namedWindow
                    ? "@name('create') @public create window MyInfra#keepall() as (p0 string, p1 int)"
                    : "@name('create') @public create table MyInfra(p0 string primary key, p1 int)";
                env.CompileDeploy(stmtTextCreateOne, path);
                env.CompileDeploy(
                    "on SupportBean_Container[beans] merge MyInfra where TheString=p0 " +
                    "when matched then update set p1 = IntPrimitive",
                    path);

                env.CompileExecuteFAFNoResult("insert into MyInfra select 'A' as p0, 1 as p1", path);

                var b1 = new SupportBean("A", 20);
                var b2 = new SupportBean("A", 30);
                var container = new SupportBean_Container(Arrays.AsList(b1, b2));
                env.SendEventBean(container);

                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "A", 30 } });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        internal class InfraDeleteThenUpdate : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraDeleteThenUpdate(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// There is no guarantee whether the delete or the update wins
                /// </summary>
                var fields = new string[] { "p0", "p1" };
                var path = new RegressionPath();

                // create window
                var stmtTextCreateOne = namedWindow
                    ? "@name('create') @public create window MyInfra#keepall() as (p0 string, p1 int)"
                    : "@name('create') @public create table MyInfra(p0 string primary key, p1 int)";
                env.CompileDeploy(stmtTextCreateOne, path);

                // create merge
                var stmtTextMerge =
                    "@name('merge') on SupportBean sb merge MyInfra where TheString = p0 when matched " +
                    "then delete " +
                    "then update set p1 = IntPrimitive";
                env.CompileDeploy(stmtTextMerge, path).AddListener("merge");

                env.CompileExecuteFAFNoResult("insert into MyInfra select 'A' as p0, 1 as p1", path);

                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "A", 1 } });

                env.SendEventBean(new SupportBean("A", 10));

                if (namedWindow) {
                    env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "A", 10 } });
                }
                else {
                    env.AssertIterator("create", iterator => Assert.IsFalse(iterator.MoveNext()));
                }

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        internal class InfraOnMergeSimpleInsert : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraOnMergeSimpleInsert(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "p0", "p1" };
                var path = new RegressionPath();

                // create window
                var stmtTextCreateOne = namedWindow
                    ? "@name('create') @public create window MyInfra#keepall() as (p0 string, p1 int)"
                    : "@name('create') @public create table MyInfra(p0 string primary key, p1 int)";
                env.CompileDeploy(stmtTextCreateOne, path);

                // create merge
                var stmtTextMerge =
                    "@name('merge') on SupportBean sb merge MyInfra insert select TheString as p0, IntPrimitive as p1";
                env.CompileDeploy(stmtTextMerge, path).AddListener("merge");
                env.AssertStatement(
                    "merge",
                    statement => Assert.AreEqual(
                        StatementType.ON_MERGE,
                        statement.GetProperty(StatementProperty.STATEMENTTYPE)));

                env.Milestone(0);

                // populate some data
                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("merge", fields, new object[] { "E1", 1 });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1", 1 } });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsPerRowLastNew("merge", fields, new object[][] { new object[] { "E2", 2 } });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        internal class InfraOnMergeMatchNoMatch : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraOnMergeMatchNoMatch(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString", "IntPrimitive" };
                var path = new RegressionPath();

                // create window
                var stmtTextCreateOne = namedWindow
                    ? "@name('create') @public create window MyInfra.win:keepall() as SupportBean"
                    : "@name('create') @public create table MyInfra(TheString string primary key, IntPrimitive int)";
                env.CompileDeploy(stmtTextCreateOne, path).AddListener("create");

                // create merge
                var stmtTextMerge = namedWindow
                    ? "@name('merge') on SupportBean sb merge MyInfra mw where sb.TheString = mw.TheString " +
                      "when matched and sb.IntPrimitive < 0 then delete " +
                      "when not matched and IntPrimitive > 0 then insert select *" +
                      "when matched and sb.IntPrimitive > 0 then update set IntPrimitive = sb.IntPrimitive + mw.IntPrimitive"
                    : "@name('merge') on SupportBean sb merge MyInfra mw where sb.TheString = mw.TheString " +
                      "when matched and sb.IntPrimitive < 0 then delete " +
                      "when not matched and IntPrimitive > 0 then insert select TheString, IntPrimitive " +
                      "when matched and sb.IntPrimitive > 0 then update set IntPrimitive = sb.IntPrimitive + mw.IntPrimitive";
                env.CompileDeploy(stmtTextMerge, path).AddListener("merge");

                env.Milestone(0);

                // populate some data
                env.SendEventBean(new SupportBean("E1", 0));
                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsNew("merge", fields, new object[] { "E2", 2 });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E2", 2 } });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 10));
                env.AssertPropsIRPair("merge", fields, new object[] { "E2", 12 }, new object[] { "E2", 2 });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E2", 12 } });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E2", -1));
                env.AssertPropsOld("merge", fields, new object[] { "E2", 12 });
                env.AssertPropsPerRowIterator("create", fields, null);

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E3", 3));
                env.SendEventBean(new SupportBean("E3", 4));
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E3", 7 } });

                env.UndeployAll();

                env.Milestone(4);
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        internal class InfraInsertOnly : RegressionExecution
        {
            private readonly bool namedWindow;
            private readonly bool useEquivalent;
            private readonly bool soda;
            private readonly bool useColumnNames;

            public InfraInsertOnly(
                bool namedWindow,
                bool useEquivalent,
                bool soda,
                bool useColumnNames)
            {
                this.namedWindow = namedWindow;
                this.useEquivalent = useEquivalent;
                this.soda = soda;
                this.useColumnNames = useColumnNames;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = "p0,p1,".SplitCsv();
                var path = new RegressionPath();
                var createEPL = namedWindow
                    ? "@name('Window') @public create window InsertOnlyInfra#unique(p0) as (p0 string, p1 int)"
                    : "@name('Window') @public create table InsertOnlyInfra (p0 string primary key, p1 int)";
                env.CompileDeploy(createEPL, path);

                string epl;
                if (useEquivalent) {
                    epl =
                        "@name('on') on SupportBean merge InsertOnlyInfra where 1=2 when not matched then insert select TheString as p0, IntPrimitive as p1";
                }
                else if (useColumnNames) {
                    epl =
                        "@name('on') on SupportBean as provider merge InsertOnlyInfra insert(p0, p1) select provider.TheString, IntPrimitive";
                }
                else {
                    epl =
                        "@name('on') on SupportBean merge InsertOnlyInfra insert select TheString as p0, IntPrimitive as p1";
                }

                env.CompileDeploy(soda, epl, path);
                env.AssertThat(
                    () => {
                        var windowType = env.Statement("Window").EventType;
                        var onType = env.Statement("on").EventType;
                        Assert.AreSame(windowType, onType);
                    });
                env.AddListener("on");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsPerRowIteratorAnyOrder("Window", fields, new object[][] { new object[] { "E1", 1 } });
                env.AssertThat(
                    () => {
                        var onEvent = env.Listener("on").AssertOneGetNewAndReset();
                        Assert.AreEqual("E1", onEvent.Get("p0"));
                        Assert.AreSame(onEvent.EventType, env.Statement("on").EventType);
                    });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "Window",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });
                env.AssertEqualsNew("on", "p0", "E2");

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       ", useEquivalent=" +
                       useEquivalent +
                       ", soda=" +
                       soda +
                       ", useColumnNames=" +
                       useColumnNames +
                       '}';
            }
        }

        internal class InfraFlow : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraFlow(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = "TheString,IntPrimitive,IntBoxed".SplitCsv();
                var milestone = new AtomicLong();
                var path = new RegressionPath();
                var createEPL = namedWindow
                    ? "@name('Window') @public create window MyMergeInfra#unique(TheString) as SupportBean"
                    : "@name('Window') @public create table MyMergeInfra (TheString string primary key, IntPrimitive int, IntBoxed int)";
                env.CompileDeploy(createEPL, path).AddListener("Window");

                env.CompileDeploy(
                    "@name('Insert') insert into MyMergeInfra select TheString, IntPrimitive, IntBoxed from SupportBean(BoolPrimitive)",
                    path);
                env.CompileDeploy("@name('Delete') on SupportBean_A delete from MyMergeInfra", path);

                var epl = "@name('Merge') on SupportBean(BoolPrimitive=false) as up " +
                          "merge MyMergeInfra as mv " +
                          "where mv.TheString=up.TheString " +
                          "when matched and up.IntPrimitive<0 then " +
                          "delete " +
                          "when matched and up.IntPrimitive=0 then " +
                          "update set IntPrimitive=0, IntBoxed=0 " +
                          "when matched then " +
                          "update set IntPrimitive=up.IntPrimitive, IntBoxed=up.IntBoxed+mv.IntBoxed " +
                          "when not matched then " +
                          "insert select " +
                          (namedWindow ? "*" : "TheString, IntPrimitive, IntBoxed");
                env.CompileDeploy(epl, path).AddListener("Merge");

                RunAssertionFlow(env, namedWindow, fields, milestone);

                env.UndeployModuleContaining("Merge");
                env.SendEventBean(new SupportBean_A("A1"));
                env.ListenerReset("Window");

                env.EplToModelCompileDeploy(epl, path).AddListener("Merge");

                RunAssertionFlow(env, namedWindow, fields, milestone);

                // test stream wildcard
                env.SendEventBean(new SupportBean_A("A2"));
                env.UndeployModuleContaining("Merge");
                epl = "@name('Merge') on SupportBean(BoolPrimitive = false) as up " +
                      "merge MyMergeInfra as mv " +
                      "where mv.TheString = up.TheString " +
                      "when not matched then " +
                      "insert select " +
                      (namedWindow ? "up.*" : "TheString, IntPrimitive, IntBoxed");
                env.CompileDeploy(epl, path).AddListener("Merge");

                SendSupportBeanEvent(env, false, "E99", 2, 3); // insert via merge
                env.AssertPropsPerRowIteratorAnyOrder(
                    "Window",
                    fields,
                    new object[][] { new object[] { "E99", 2, 3 } });

                // Test ambiguous columns.
                epl = "create schema TypeOne (Id long, mylong long, mystring long);\n";
                epl += namedWindow
                    ? "@public create window MyInfraTwo#unique(Id) as select * from TypeOne;\n"
                    : "@public create table MyInfraTwo (Id long, mylong long, mystring long);\n";
                // The "and not matched" should not complain if "mystring" is ambiguous.
                // The "insert" should not complain as column names have been provided.
                epl += "on TypeOne as t1 merge MyInfraTwo nm where nm.Id = t1.Id\n" +
                       "  when not matched and mystring = 0 then insert select *\n" +
                       "  when not matched then insert (Id, mylong, mystring) select 0L, 0L, 0L\n";
                env.CompileDeploy(epl);

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        private static void RunAssertionFlow(
            RegressionEnvironment env,
            bool namedWindow,
            string[] fields,
            AtomicLong milestone)
        {
            env.ListenerReset("Window");
            env.ListenerReset("Merge");

            SendSupportBeanEvent(env, true, "E1", 10, 200); // insert via insert-into
            if (namedWindow) {
                env.AssertPropsNew("Window", fields, new object[] { "E1", 10, 200 });
            }
            else {
                env.AssertListenerNotInvoked("Window");
            }

            env.AssertPropsPerRowIteratorAnyOrder("Window", fields, new object[][] { new object[] { "E1", 10, 200 } });
            env.AssertListenerNotInvoked("Merge");

            SendSupportBeanEvent(env, false, "E1", 11, 201); // update via merge
            if (namedWindow) {
                env.AssertPropsIRPair("Window", fields, new object[] { "E1", 11, 401 }, new object[] { "E1", 10, 200 });
            }

            env.AssertPropsPerRowIteratorAnyOrder("Window", fields, new object[][] { new object[] { "E1", 11, 401 } });
            env.AssertPropsIRPair("Merge", fields, new object[] { "E1", 11, 401 }, new object[] { "E1", 10, 200 });

            env.MilestoneInc(milestone);

            SendSupportBeanEvent(env, false, "E2", 13, 300); // insert via merge
            if (namedWindow) {
                env.AssertPropsNew("Window", fields, new object[] { "E2", 13, 300 });
            }

            env.AssertPropsPerRowIteratorAnyOrder(
                "Window",
                fields,
                new object[][] { new object[] { "E1", 11, 401 }, new object[] { "E2", 13, 300 } });
            env.AssertPropsNew("Merge", fields, new object[] { "E2", 13, 300 });

            SendSupportBeanEvent(env, false, "E2", 14, 301); // update via merge
            if (namedWindow) {
                env.AssertPropsIRPair("Window", fields, new object[] { "E2", 14, 601 }, new object[] { "E2", 13, 300 });
            }

            env.AssertPropsPerRowIteratorAnyOrder(
                "Window",
                fields,
                new object[][] { new object[] { "E1", 11, 401 }, new object[] { "E2", 14, 601 } });
            env.AssertPropsIRPair("Merge", fields, new object[] { "E2", 14, 601 }, new object[] { "E2", 13, 300 });

            env.MilestoneInc(milestone);

            SendSupportBeanEvent(env, false, "E2", 15, 302); // update via merge
            if (namedWindow) {
                env.AssertPropsIRPair("Window", fields, new object[] { "E2", 15, 903 }, new object[] { "E2", 14, 601 });
            }

            env.AssertPropsPerRowIteratorAnyOrder(
                "Window",
                fields,
                new object[][] { new object[] { "E1", 11, 401 }, new object[] { "E2", 15, 903 } });
            env.AssertPropsIRPair("Merge", fields, new object[] { "E2", 15, 903 }, new object[] { "E2", 14, 601 });

            SendSupportBeanEvent(env, false, "E3", 40, 400); // insert via merge
            if (namedWindow) {
                env.AssertPropsNew("Window", fields, new object[] { "E3", 40, 400 });
            }

            env.AssertPropsPerRowIteratorAnyOrder(
                "Window",
                fields,
                new object[][]
                    { new object[] { "E1", 11, 401 }, new object[] { "E2", 15, 903 }, new object[] { "E3", 40, 400 } });
            env.AssertPropsNew("Merge", fields, new object[] { "E3", 40, 400 });

            env.MilestoneInc(milestone);

            SendSupportBeanEvent(env, false, "E3", 0, 1000); // reset E3 via merge
            if (namedWindow) {
                env.AssertPropsIRPair("Window", fields, new object[] { "E3", 0, 0 }, new object[] { "E3", 40, 400 });
            }

            env.AssertPropsPerRowIteratorAnyOrder(
                "Window",
                fields,
                new object[][]
                    { new object[] { "E1", 11, 401 }, new object[] { "E2", 15, 903 }, new object[] { "E3", 0, 0 } });
            env.AssertPropsIRPair("Merge", fields, new object[] { "E3", 0, 0 }, new object[] { "E3", 40, 400 });

            SendSupportBeanEvent(env, false, "E2", -1, 1000); // delete E2 via merge
            if (namedWindow) {
                env.AssertPropsOld("Window", fields, new object[] { "E2", 15, 903 });
            }

            env.AssertPropsOld("Merge", fields, new object[] { "E2", 15, 903 });
            env.AssertPropsPerRowIteratorAnyOrder(
                "Window",
                fields,
                new object[][] { new object[] { "E1", 11, 401 }, new object[] { "E3", 0, 0 } });

            env.MilestoneInc(milestone);

            SendSupportBeanEvent(env, false, "E1", -1, 1000); // delete E1 via merge
            if (namedWindow) {
                env.AssertPropsOld("Window", fields, new object[] { "E1", 11, 401 });
            }

            env.AssertPropsOld("Merge", fields, new object[] { "E1", 11, 401 });
            env.AssertPropsPerRowIteratorAnyOrder("Window", fields, new object[][] { new object[] { "E3", 0, 0 } });
        }

        internal class InfraMultipleInsert : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraMultipleInsert(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var rep = EventRepresentationChoiceExtensions.GetEngineDefault(env.Configuration);
                var fields = "col1,col2".SplitCsv();

                var epl = "@public @buseventtype create schema MyEvent as (in1 string, in2 int);\n" +
                          "create schema MySchema as (col1 string, col2 int);\n";
                epl += namedWindow
                    ? "@public create window MyInfraMI#keepall as MySchema;\n"
                    : "@public create table MyInfraMI (col1 string primary key, col2 int);\n";
                epl += "@name('Merge') on MyEvent " +
                       "merge MyInfraMI " +
                       "where col1=in1 " +
                       "when not matched and in1 like \"A%\" then " +
                       "insert(col1, col2) select in1, in2 " +
                       "when not matched and in1 like \"B%\" then " +
                       "insert select in1 as col1, in2 as col2 " +
                       "when not matched and in1 like \"C%\" then " +
                       "insert select \"Z\" as col1, -1 as col2 " +
                       "when not matched and in1 like \"D%\" then " +
                       "insert select \"x\"||in1||\"x\" as col1, in2*-1 as col2;\n";
                env.CompileDeploy(epl, new RegressionPath()).AddListener("Merge");

                SendMyEvent(env, rep, "E1", 0);
                env.AssertListenerNotInvoked("Merge");

                env.Milestone(0);

                SendMyEvent(env, rep, "A1", 1);
                env.AssertPropsNew("Merge", fields, new object[] { "A1", 1 });

                SendMyEvent(env, rep, "B1", 2);
                env.AssertPropsNew("Merge", fields, new object[] { "B1", 2 });

                SendMyEvent(env, rep, "C1", 3);
                env.AssertPropsNew("Merge", fields, new object[] { "Z", -1 });

                env.Milestone(1);

                SendMyEvent(env, rep, "D1", 4);
                env.AssertPropsNew("Merge", fields, new object[] { "xD1x", -4 });

                SendMyEvent(env, rep, "B1", 2);
                env.AssertListenerNotInvoked("Merge");

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        internal class InfraNoWhereClause : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraNoWhereClause(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = "col1,col2".SplitCsv();
                var rep = EventRepresentationChoiceExtensions.GetEngineDefault(env.Configuration);

                var epl = "@public @buseventtype create schema MyEvent as (in1 string, in2 int);\n" +
                          "create schema MySchema as (col1 string, col2 int);\n";
                epl += namedWindow
                    ? "@name('create') @public create window MyInfraNWC#keepall as MySchema;\n"
                    : "@name('create') @public create table MyInfraNWC (col1 string, col2 int);\n";
                epl += "on SupportBean_A delete from MyInfraNWC;\n";
                epl += "on MyEvent me " +
                       "merge MyInfraNWC mw " +
                       "when not matched and me.in1 like \"A%\" then " +
                       "insert(col1, col2) select me.in1, me.in2 " +
                       "when not matched and me.in1 like \"B%\" then " +
                       "insert select me.in1 as col1, me.in2 as col2 " +
                       "when matched and me.in1 like \"C%\" then " +
                       "update set col1='Z', col2=-1 " +
                       "when not matched then " +
                       "insert select \"x\" || me.in1 || \"x\" as col1, me.in2 * -1 as col2;\n";
                env.CompileDeploy(epl, new RegressionPath());

                SendMyEvent(env, rep, "E1", 2);
                env.AssertPropsPerRowIteratorAnyOrder("create", fields, new object[][] { new object[] { "xE1x", -2 } });

                SendMyEvent(env, rep, "A1", 3); // matched : no where clause
                env.AssertPropsPerRowIteratorAnyOrder("create", fields, new object[][] { new object[] { "xE1x", -2 } });

                env.SendEventBean(new SupportBean_A("Ax1"));
                env.AssertPropsPerRowIteratorAnyOrder("create", fields, null);

                env.Milestone(0);

                SendMyEvent(env, rep, "A1", 4);
                env.AssertPropsPerRowIteratorAnyOrder("create", fields, new object[][] { new object[] { "A1", 4 } });

                SendMyEvent(env, rep, "B1", 5); // matched : no where clause
                env.AssertPropsPerRowIteratorAnyOrder("create", fields, new object[][] { new object[] { "A1", 4 } });

                env.Milestone(1);

                env.SendEventBean(new SupportBean_A("Ax1"));
                env.AssertPropsPerRowIteratorAnyOrder("create", fields, null);

                env.Milestone(2);

                SendMyEvent(env, rep, "B1", 5);
                env.AssertPropsPerRowIteratorAnyOrder("create", fields, new object[][] { new object[] { "B1", 5 } });

                SendMyEvent(env, rep, "C", 6);
                env.AssertPropsPerRowIteratorAnyOrder("create", fields, new object[][] { new object[] { "Z", -1 } });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        internal class InfraInvalid : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraInvalid(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = namedWindow
                    ? "@public create window MergeInfra#unique(TheString) as SupportBean;\n"
                    : "@public create table MergeInfra as (TheString string, IntPrimitive int, BoolPrimitive bool);\n";
                epl += "create schema ABCSchema as (val int);\n";
                epl += namedWindow
                    ? "@public create window ABCInfra#keepall as ABCSchema;\n"
                    : "@public create table ABCInfra (val int);\n";
                env.CompileDeploy(epl, path);

                epl =
                    "on SupportBean_A merge MergeInfra as windowevent where Id = TheString when not matched and exists(select * from MergeInfra mw where mw.TheString = windowevent.TheString) is not null then insert into ABC select '1'";
                env.TryInvalidCompile(
                    path,
                    epl,
                    "On-Merge not-matched filter expression may not use properties that are provided by the named window event [on SupportBean_A merge MergeInfra as windowevent where Id = TheString when not matched and exists(select * from MergeInfra mw where mw.TheString = windowevent.TheString) is not null then insert into ABC select '1']");

                epl = "on SupportBean_A as up merge ABCInfra as mv when not matched then insert (col) select 1";
                if (namedWindow) {
                    env.TryInvalidCompile(
                        path,
                        epl,
                        "Validation failed in when-not-matched (clause 1): Event type named 'ABCInfra' has already been declared with differing column name or type information: Type by name 'ABCInfra' in property 'col' property name not found in target");
                }
                else {
                    env.TryInvalidCompile(
                        path,
                        epl,
                        "Validation failed in when-not-matched (clause 1): Column 'col' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [");
                }

                epl =
                    "on SupportBean_A as up merge MergeInfra as mv where mv.BoolPrimitive=true when not matched then update set IntPrimitive = 1";
                env.TryInvalidCompile(
                    path,
                    epl,
                    "Incorrect syntax near 'update' (a reserved keyword) expecting 'insert' but found 'update' at line 1 column 9");

                if (namedWindow) {
                    epl =
                        "on SupportBean_A as up merge MergeInfra as mv where mv.TheString=Id when matched then insert select *";
                    env.TryInvalidCompile(
                        path,
                        epl,
                        "Validation failed in when-not-matched (clause 1): Expression-returned event type 'SupportBean_A' with underlying type '" +
                        typeof(SupportBean_A).FullName +
                        "' cannot be converted to target event type 'MergeInfra' with underlying type '" +
                        typeof(SupportBean).FullName +
                        "' [on SupportBean_A as up merge MergeInfra as mv where mv.TheString=Id when matched then insert select *]");
                }

                epl = "on SupportBean as up merge MergeInfra as mv";
                env.TryInvalidCompile(path, epl, "Unexpected end-of-input at line 1 column 4");

                epl = "on SupportBean as up merge MergeInfra as mv where a=b when matched";
                env.TryInvalidCompile(
                    path,
                    epl,
                    "Incorrect syntax near end-of-input ('matched' is a reserved keyword) expecting 'then' but found end-of-input at line 1 column 66 [");

                epl = "on SupportBean as up merge MergeInfra as mv where a=b when matched and then delete";
                env.TryInvalidCompile(
                    path,
                    epl,
                    "Incorrect syntax near 'then' (a reserved keyword) at line 1 column 71 [on SupportBean as up merge MergeInfra as mv where a=b when matched and then delete]");

                epl =
                    "on SupportBean as up merge MergeInfra as mv where BoolPrimitive=true when not matched then insert select *";
                env.TryInvalidCompile(
                    path,
                    epl,
                    "Failed to validate where-clause expression 'BoolPrimitive=true': Property named 'BoolPrimitive' is ambiguous as is valid for more then one stream [on SupportBean as up merge MergeInfra as mv where BoolPrimitive=true when not matched then insert select *]");

                epl =
                    "on SupportBean_A as up merge MergeInfra as mv where mv.BoolPrimitive=true when not matched then insert select IntPrimitive";
                env.TryInvalidCompile(
                    path,
                    epl,
                    "Failed to validate select-clause expression 'IntPrimitive': Property named 'IntPrimitive' is not valid in any stream [on SupportBean_A as up merge MergeInfra as mv where mv.BoolPrimitive=true when not matched then insert select IntPrimitive]");

                epl =
                    "on SupportBean_A as up merge MergeInfra as mv where mv.BoolPrimitive=true when not matched then insert select * where TheString = 'A'";
                env.TryInvalidCompile(
                    path,
                    epl,
                    "Failed to validate match where-clause expression 'TheString=\"A\"': Property named 'TheString' is not valid in any stream [on SupportBean_A as up merge MergeInfra as mv where mv.BoolPrimitive=true when not matched then insert select * where TheString = 'A']");

                epl = "@public create variable int myvariable;\n" +
                      "on SupportBean_A merge MergeInfra when matched then update set myvariable = 1;\n";
                env.TryInvalidCompile(path, epl, "Left-hand-side does not allow variables for variable 'myvariable'");

                epl = "on SupportBean_A merge MergeInfra when matched then update set TheString[1][2] = 1;\n";
                env.TryInvalidCompile(path, epl, "Unrecognized left-hand-side assignment 'TheString[1][2]'");

                env.UndeployAll();

                // invalid assignment: wrong event type
                path.Clear();
                env.CompileDeploy("@public create map schema Composite as (c0 int)", path);
                env.CompileDeploy("@public create window AInfra#keepall as (c Composite)", path);
                env.CompileDeploy("@public create map schema SomeOther as (c1 int)", path);
                env.CompileDeploy("@public create map schema MyEvent as (so SomeOther)", path);

                env.TryInvalidCompile(
                    path,
                    "on MyEvent as me update AInfra set c = me.so",
                    "Failed to validate assignment expression 'c=me.so': Invalid assignment to property 'c' event type 'Composite' from event type 'SomeOther' [on MyEvent as me update AInfra set c = me.so]");

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        internal class InfraInnerTypeAndVariable : RegressionExecution
        {
            private readonly bool namedWindow;
            private readonly EventRepresentationChoice eventRepresentationEnum;

            public InfraInnerTypeAndVariable(
                bool namedWindow,
                EventRepresentationChoice eventRepresentationEnum)
            {
                this.namedWindow = namedWindow;
                this.eventRepresentationEnum = eventRepresentationEnum;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var schema =
                    eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMyInnerSchema)) +
                    " @public create schema MyInnerSchema(in1 string, in2 int);\n" +
                    eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMyEventSchema)) +
                    " @public @buseventtype @public create schema MyEventSchema(col1 string, col2 MyInnerSchema)";
                env.CompileDeploy(schema, path);

                var eplCreate = namedWindow
                    ? eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMyInfraITV)) +
                      " @public create window MyInfraITV#keepall as (c1 string, c2 MyInnerSchema)"
                    : "@public create table MyInfraITV as (c1 string primary key, c2 MyInnerSchema)";
                env.CompileDeploy(eplCreate, path);
                env.CompileDeploy("@name('createvar') @public create variable boolean myvar", path);

                var epl = "@name('Merge') on MyEventSchema me " +
                          "merge MyInfraITV mw " +
                          "where me.col1 = mw.c1 " +
                          " when not matched and myvar then " +
                          "  insert select col1 as c1, col2 as c2 " +
                          " when not matched and myvar = false then " +
                          "  insert select 'A' as c1, null as c2 " +
                          " when not matched and myvar is null then " +
                          "  insert select 'B' as c1, me.col2 as c2 " +
                          " when matched then " +
                          "  delete";
                env.CompileDeploy(epl, path).AddListener("Merge");
                var fields = "c1,c2.in1,c2.in2".SplitCsv();

                SendMyInnerSchemaEvent(env, eventRepresentationEnum, "X1", "Y1", 10);
                env.AssertPropsNew("Merge", fields, new object[] { "B", "Y1", 10 });

                SendMyInnerSchemaEvent(env, eventRepresentationEnum, "B", "0", 0); // delete
                env.AssertPropsOld("Merge", fields, new object[] { "B", "Y1", 10 });

                env.Milestone(0);

                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("createvar"), "myvar", true);
                SendMyInnerSchemaEvent(env, eventRepresentationEnum, "X2", "Y2", 11);
                env.AssertPropsNew("Merge", fields, new object[] { "X2", "Y2", 11 });

                env.Milestone(1);

                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("createvar"), "myvar", false);
                SendMyInnerSchemaEvent(env, eventRepresentationEnum, "X3", "Y3", 12);
                env.AssertPropsNew("Merge", fields, new object[] { "A", null, null });

                env.UndeployModuleContaining("Merge");
                env.CompileDeploy(epl, path);

                var subscriber = new SupportSubscriberMRD();
                env.Statement("Merge").SetSubscriber(subscriber);
                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("createvar"), "myvar", true);

                SendMyInnerSchemaEvent(env, eventRepresentationEnum, "X4", "Y4", 11);
                var result = subscriber.InsertStreamList[0];
                if (eventRepresentationEnum.IsObjectArrayEvent() || !namedWindow) {
                    var row = (object[])result[0][0];
                    Assert.AreEqual("X4", row[0]);
                    var theEvent = (EventBean)row[1];
                    Assert.AreEqual("Y4", theEvent.Get("in1"));
                }
                else if (eventRepresentationEnum.IsMapEvent()) {
                    var map = (IDictionary<string, object>)result[0][0];
                    Assert.AreEqual("X4", map.Get("c1"));
                    var theEvent = (EventBean)map.Get("c2");
                    Assert.AreEqual("Y4", theEvent.Get("in1"));
                }
                else if (eventRepresentationEnum.IsAvroEvent()) {
                    var avro = (GenericRecord)result[0][0];
                    Assert.AreEqual("X4", avro.Get("c1"));
                    var theEvent = (GenericRecord)avro.Get("c2");
                    Assert.AreEqual("Y4", theEvent.Get("in1"));
                }

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       ", eventRepresentationEnum=" +
                       eventRepresentationEnum +
                       '}';
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        internal class InfraPatternMultimatch : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraPatternMultimatch(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = "c1,c2".SplitCsv();
                var path = new RegressionPath();

                var eplCreate = namedWindow
                    ? "@name('create') @public create window MyInfraPM#keepall as (c1 string, c2 string)"
                    : "@name('create') @public create table MyInfraPM as (c1 string primary key, c2 string primary key)";
                env.CompileDeploy(eplCreate, path);

                var epl =
                    "@name('Merge') on pattern[every a=SupportBean(TheString like 'A%') -> b=SupportBean(TheString like 'B%', IntPrimitive = a.IntPrimitive)] me " +
                    "merge MyInfraPM mw " +
                    "where me.a.TheString = mw.c1 and me.b.TheString = mw.c2 " +
                    "when not matched then " +
                    "insert select me.a.TheString as c1, me.b.TheString as c2 ";
                env.CompileDeploy(epl, path);

                env.SendEventBean(new SupportBean("A1", 1));
                env.SendEventBean(new SupportBean("A2", 1));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("B1", 1));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "A1", "B1" }, new object[] { "A2", "B1" } });

                env.SendEventBean(new SupportBean("A3", 2));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("A4", 2));
                env.SendEventBean(new SupportBean("B2", 2));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] {
                        new object[] { "A1", "B1" }, new object[] { "A2", "B1" }, new object[] { "A3", "B2" },
                        new object[] { "A4", "B2" }
                    });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        internal class InfraOnMergeInsertStream : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraOnMergeInsertStream(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = "create schema WinOMISSchema as (v1 string, v2 int);\n";
                epl += namedWindow
                    ? "@name('Create') create window WinOMIS#keepall as WinOMISSchema;\n"
                    : "@name('Create') create table WinOMIS as (v1 string primary key, v2 int);\n";
                epl += "on SupportBean_ST0 as st0 merge WinOMIS as win where win.v1=st0.key0 " +
                       "when not matched " +
                       "then insert into StreamOne select * " +
                       "then insert into StreamTwo select st0.Id as Id, st0.key0 as key0 " +
                       "then insert into StreamThree(Id, key0) select st0.Id, st0.key0 " +
                       "then insert into StreamFour select Id, key0 where key0=\"K2\" " +
                       "then insert into WinOMIS select key0 as v1, P00 as v2;\n";
                epl += "@name('s1') select * from StreamOne;\n";
                epl += "@name('s2') select * from StreamTwo;\n";
                epl += "@name('s3') select * from StreamThree;\n";
                epl += "@name('s4') select * from StreamFour;\n";
                env.CompileDeploy(epl).AddListener("s1").AddListener("s2").AddListener("s3").AddListener("s4");

                env.SendEventBean(new SupportBean_ST0("ID1", "K1", 1));
                env.AssertPropsNew("s1", "Id,key0".SplitCsv(), new object[] { "ID1", "K1" });
                env.AssertPropsNew("s2", "Id,key0".SplitCsv(), new object[] { "ID1", "K1" });
                env.AssertPropsNew("s3", "Id,key0".SplitCsv(), new object[] { "ID1", "K1" });
                env.AssertListenerNotInvoked("s4");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_ST0("ID1", "K2", 2));
                env.AssertPropsNew("s4", "Id,key0".SplitCsv(), new object[] { "ID1", "K2" });
                env.AssertPropsPerRowIterator(
                    "Create",
                    "v1,v2".SplitCsv(),
                    new object[][] { new object[] { "K1", 1 }, new object[] { "K2", 2 } });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        internal class InfraMultiactionDeleteUpdate : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraMultiactionDeleteUpdate(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplCreate = namedWindow
                    ? "@name('Create') @public create window WinMDU#keepall as SupportBean"
                    : "@name('Create') @public create table WinMDU (TheString string primary key, IntPrimitive int)";
                env.CompileDeploy(eplCreate, path);
                env.CompileDeploy("insert into WinMDU select TheString, IntPrimitive from SupportBean", path);

                var epl = "@name('merge') on SupportBean_ST0 as st0 merge WinMDU as win where st0.key0=win.TheString " +
                          "when matched " +
                          "then delete where IntPrimitive<0 " +
                          "then update set IntPrimitive=st0.P00 where IntPrimitive=3000 or P00=3000 " +
                          "then update set IntPrimitive=999 where IntPrimitive=1000 " +
                          "then delete where IntPrimitive=1000 " +
                          "then update set IntPrimitive=1999 where IntPrimitive=2000 " +
                          "then delete where IntPrimitive=2000";
                env.CompileDeploy(epl, path);
                var fields = "TheString,IntPrimitive".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean_ST0("ST0", "E1", 0));
                env.AssertPropsPerRowIterator("Create", fields, new object[][] { new object[] { "E1", 1 } });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", -1));
                env.SendEventBean(new SupportBean_ST0("ST0", "E2", 0));
                env.AssertPropsPerRowIterator("Create", fields, new object[][] { new object[] { "E1", 1 } });

                env.SendEventBean(new SupportBean("E3", 3000));
                env.SendEventBean(new SupportBean_ST0("ST0", "E3", 3));
                env.AssertPropsPerRowIterator(
                    "Create",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 } });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E4", 4));
                env.SendEventBean(new SupportBean_ST0("ST0", "E4", 3000));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "Create",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 }, new object[] { "E4", 3000 } });

                env.SendEventBean(new SupportBean("E5", 1000));
                env.SendEventBean(new SupportBean_ST0("ST0", "E5", 0));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "Create",
                    fields,
                    new object[][] {
                        new object[] { "E1", 1 }, new object[] { "E3", 3 }, new object[] { "E4", 3000 },
                        new object[] { "E5", 999 }
                    });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E6", 2000));
                env.SendEventBean(new SupportBean_ST0("ST0", "E6", 0));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "Create",
                    fields,
                    new object[][] {
                        new object[] { "E1", 1 }, new object[] { "E3", 3 }, new object[] { "E4", 3000 },
                        new object[] { "E5", 999 }, new object[] { "E6", 1999 }
                    });
                env.UndeployModuleContaining("merge");

                env.EplToModelCompileDeploy(epl, path);

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        internal class InfraSubqueryNotMatched : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSubqueryNotMatched(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplCreateOne = namedWindow
                    ? "@name('Create') @public create window InfraOne#unique(string) (string string, IntPrimitive int)"
                    : "@name('Create') @public create table InfraOne (string string primary key, IntPrimitive int)";
                env.CompileDeploy(eplCreateOne, path);
                SupportAdminUtil.AssertStatelessStmt(env, "Create", false);

                var eplCreateTwo = namedWindow
                    ? "@public create window InfraTwo#unique(val0) (val0 string, val1 int)"
                    : "@public create table InfraTwo (val0 string primary key, val1 int primary key)";
                env.CompileDeploy(eplCreateTwo, path);
                env.CompileDeploy("insert into InfraTwo select 'W2' as val0, Id as val1 from SupportBean_S0", path);

                var epl = "on SupportBean sb merge InfraOne w1 " +
                          "where sb.TheString = w1.string " +
                          "when not matched then insert select 'Y' as string, (select val1 from InfraTwo as w2 where w2.val0 = sb.TheString) as IntPrimitive";
                env.CompileDeploy(epl, path);

                env.SendEventBean(new SupportBean_S0(50)); // InfraTwo now has a row {W2, 1}
                env.SendEventBean(new SupportBean("W2", 1));
                env.AssertPropsPerRowIterator(
                    "Create",
                    "string,IntPrimitive".SplitCsv(),
                    new object[][] { new object[] { "Y", 50 } });

                if (namedWindow) {
                    env.SendEventBean(new SupportBean_S0(51)); // InfraTwo now has a row {W2, 1}
                    env.SendEventBean(new SupportBean("W2", 2));
                    env.AssertPropsPerRowIterator(
                        "Create",
                        "string,IntPrimitive".SplitCsv(),
                        new object[][] { new object[] { "Y", 51 } });
                }

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        internal class InfraUpdateOrderOfFields : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraUpdateOrderOfFields(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = namedWindow
                    ? "@public create window MyInfraUOF#keepall as SupportBean;\n"
                    : "@public create table MyInfraUOF(TheString string primary key, IntPrimitive int, IntBoxed int, DoublePrimitive double);\n";
                epl +=
                    "insert into MyInfraUOF select TheString, IntPrimitive, IntBoxed, DoublePrimitive from SupportBean;\n";
                epl += "@name('Merge') on SupportBean_S0 as sb " +
                       "merge MyInfraUOF as mywin where mywin.TheString = sb.P00 when matched then " +
                       "update set IntPrimitive=Id, IntBoxed=mywin.IntPrimitive, DoublePrimitive=initial.IntPrimitive;\n";

                env.CompileDeploy(epl).AddListener("Merge");
                var fields = "IntPrimitive,IntBoxed,DoublePrimitive".SplitCsv();

                env.SendEventBean(MakeSupportBean("E1", 1, 2));
                env.SendEventBean(new SupportBean_S0(5, "E1"));
                env.AssertPropsPerRowLastNew("Merge", fields, new object[][] { new object[] { 5, 5, 1.0 } });

                env.Milestone(0);

                env.SendEventBean(MakeSupportBean("E2", 10, 20));
                env.SendEventBean(new SupportBean_S0(6, "E2"));
                env.AssertPropsPerRowLastNew("Merge", fields, new object[][] { new object[] { 6, 6, 10.0 } });

                env.SendEventBean(new SupportBean_S0(7, "E1"));
                env.AssertPropsPerRowLastNew("Merge", fields, new object[][] { new object[] { 7, 7, 5.0 } });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        internal class InfraInsertOtherStream : RegressionExecution
        {
            private readonly bool namedWindow;
            private readonly EventRepresentationChoice eventRepresentationEnum;

            public InfraInsertOtherStream(
                bool namedWindow,
                EventRepresentationChoice eventRepresentationEnum)
            {
                this.namedWindow = namedWindow;
                this.eventRepresentationEnum = eventRepresentationEnum;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMyEvent)) +
                          " @public @buseventtype @public create schema MyEvent as (name string, value double);\n" +
                          (namedWindow
                              ? eventRepresentationEnum.GetAnnotationTextWJsonProvided(
                                    typeof(MyLocalJsonProvidedMyEvent)) +
                                " @public create window MyInfraIOS#unique(name) as MyEvent;\n"
                              : "@public create table MyInfraIOS (name string primary key, value double primary key);\n"
                          ) +
                          "insert into MyInfraIOS select * from MyEvent;\n" +
                          eventRepresentationEnum.GetAnnotationTextWJsonProvided(
                              typeof(MyLocalJsonProvidedInputEvent)) +
                          " create schema InputEvent as (col1 string, col2 double);\n" +
                          "\n" +
                          "on MyEvent as eme\n" +
                          "  merge MyInfraIOS as MyInfraIOS where MyInfraIOS.name = eme.name\n" +
                          "   when matched then\n" +
                          "      insert into OtherStreamOne select eme.name as event_name, MyInfraIOS.value as status\n" +
                          "   when not matched then\n" +
                          "      insert into OtherStreamOne select eme.name as event_name, 0d as status;\n" +
                          "@name('s0') select * from OtherStreamOne;\n";
                env.CompileDeploy(epl, new RegressionPath()).AddListener("s0");

                MakeSendNameValueEvent(env, eventRepresentationEnum, "MyEvent", "name1", 10d);
                env.AssertPropsNew(
                    "s0",
                    "event_name,status".SplitCsv(),
                    new object[] { "name1", namedWindow ? 0d : 10d });

                // for named windows we can send same-value keys now
                if (namedWindow) {
                    MakeSendNameValueEvent(env, eventRepresentationEnum, "MyEvent", "name1", 11d);
                    env.AssertPropsNew("s0", "event_name,status".SplitCsv(), new object[] { "name1", 10d });

                    MakeSendNameValueEvent(env, eventRepresentationEnum, "MyEvent", "name1", 12d);
                    env.AssertPropsNew("s0", "event_name,status".SplitCsv(), new object[] { "name1", 11d });
                }

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       ", eventRepresentationEnum=" +
                       eventRepresentationEnum +
                       '}';
            }

            private static void MakeSendNameValueEvent(
                RegressionEnvironment env,
                EventRepresentationChoice eventRepresentationEnum,
                string typeName,
                string name,
                double value)
            {
                if (eventRepresentationEnum.IsObjectArrayEvent()) {
                    env.SendEventObjectArray(new object[] { name, value }, typeName);
                }
                else if (eventRepresentationEnum.IsMapEvent()) {
                    IDictionary<string, object> theEvent = new Dictionary<string, object>();
                    theEvent.Put("name", name);
                    theEvent.Put("value", value);
                    env.SendEventMap(theEvent, typeName);
                }
                else if (eventRepresentationEnum.IsAvroEvent()) {
                    var record = new GenericRecord(env.RuntimeAvroSchemaPreconfigured(typeName).AsRecordSchema());
                    record.Put("name", name);
                    record.Put("value", value);
                    env.SendEventAvro(record, typeName);
                }
                else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                    env.SendEventJson(
                        new JObject(new JProperty("name", name), new JProperty("value", value)).ToString(),
                        typeName);
                }
                else {
                    Assert.Fail();
                }
            }
        }

        internal class InfraUpdateNestedEvent : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraUpdateNestedEvent(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                RunUpdateNestedEvent(env, namedWindow, "map", milestone);
                RunUpdateNestedEvent(env, namedWindow, "objectarray", milestone);
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        private static void RunUpdateNestedEvent(
            RegressionEnvironment env,
            bool namedWindow,
            string metaType,
            AtomicLong milestone)
        {
            var eplTypes =
                "@public create " +
                metaType +
                " schema Composite as (c0 int);\n" +
                "@buseventtype @public create " +
                metaType +
                " schema AInfraType as (k string, cflat Composite, carr Composite[]);\n" +
                (namedWindow
                    ? "@public create window AInfra#lastevent as AInfraType;\n"
                    : "@public create table AInfra (k string, cflat Composite, carr Composite[]);\n") +
                "insert into AInfra select TheString as k, null as cflat, null as carr from SupportBean;\n" +
                "@public @buseventtype create " +
                metaType +
                " schema MyEvent as (cf Composite, ca Composite[]);\n" +
                "on MyEvent e merge AInfra when matched then update set cflat = e.cf, carr = e.ca";
            var path = new RegressionPath();
            env.CompileDeploy(eplTypes, path);

            env.SendEventBean(new SupportBean("E1", 1));

            if (metaType.Equals("map")) {
                env.SendEventMap(MakeNestedMapEvent(), "MyEvent");
            }
            else {
                env.SendEventObjectArray(MakeNestedOAEvent(), "MyEvent");
            }

            env.MilestoneInc(milestone);

            env.AssertThat(
                () => {
                    var result = env.CompileExecuteFAF(
                        "select cflat.c0 as cf0, carr[0].c0 as ca0, carr[1].c0 as ca1 from AInfra",
                        path);
                    EPAssertionUtil.AssertProps(result.Array[0], "cf0,ca0,ca1".SplitCsv(), new object[] { 1, 1, 2 });
                });

            env.UndeployAll();
        }

        private static IDictionary<string, object> MakeNestedMapEvent()
        {
            var cf1 = Collections.SingletonDataMap("c0", 1);
            var cf2 = Collections.SingletonDataMap("c0", 2);
            IDictionary<string, object> myEvent = new Dictionary<string, object>();
            myEvent.Put("cf", cf1);
            myEvent.Put("ca", new IDictionary<string, object>[] { cf1, cf2 });
            return myEvent;
        }

        private static object[] MakeNestedOAEvent()
        {
            var cf1 = new object[] { 1 };
            var cf2 = new object[] { 2 };
            return new object[] { cf1, new object[] { cf1, cf2 } };
        }

        private static SupportBean MakeSupportBean(
            string theString,
            int intPrimitive,
            double doublePrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.DoublePrimitive = doublePrimitive;
            return sb;
        }

        private static void SendMyInnerSchemaEvent(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            string col1,
            string col2in1,
            int col2in2)
        {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] { col1, new object[] { col2in1, col2in2 } }, "MyEventSchema");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                IDictionary<string, object> inner = new Dictionary<string, object>();
                inner.Put("in1", col2in1);
                inner.Put("in2", col2in2);
                IDictionary<string, object> theEvent = new Dictionary<string, object>();
                theEvent.Put("col1", col1);
                theEvent.Put("col2", inner);
                env.SendEventMap(theEvent, "MyEventSchema");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var schema = env.RuntimeAvroSchemaPreconfigured("MyEventSchema").AsRecordSchema();
                var innerSchema = schema.GetField("col2").Schema.AsRecordSchema();
                var innerRecord = new GenericRecord(innerSchema);
                innerRecord.Put("in1", col2in1);
                innerRecord.Put("in2", col2in2);
                var record = new GenericRecord(schema);
                record.Put("col1", col1);
                record.Put("col2", innerRecord);
                env.SendEventAvro(record, "MyEventSchema");
            }
            else if (eventRepresentationEnum.IsJsonEvent()) {
                var inner = new JObject(new JProperty("in1", col2in1), new JProperty("in2", col2in2));
                var outer = new JObject(new JProperty("col1", col1), new JProperty("col2", inner));
                env.SendEventJson(outer.ToString(), "MyEventSchema");
            }
            else {
                Assert.Fail();
            }
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            bool boolPrimitive,
            string theString,
            int intPrimitive,
            int? intBoxed)
        {
            var theEvent = new SupportBean(theString, intPrimitive);
            theEvent.IntBoxed = intBoxed;
            theEvent.BoolPrimitive = boolPrimitive;
            env.SendEventBean(theEvent);
        }

        private static void SendMyEvent(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            string in1,
            int in2)
        {
            IDictionary<string, object> theEvent = new LinkedHashMap<string, object>();
            theEvent.Put("in1", in1);
            theEvent.Put("in2", in2);
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(theEvent.Values.ToArray(), "MyEvent");
            }
            else {
                env.SendEventMap(theEvent, "MyEvent");
            }
        }

        [Serializable]
        internal class MyLocalJsonProvidedMyEvent
        {
            public string name;
            public double value;
        }

        [Serializable]
        internal class MyLocalJsonProvidedInputEvent
        {
            public string col1;
            public double col2;
        }

        [Serializable]
        internal class MyLocalJsonProvidedMyInnerSchema
        {
            public string in1;
            public int in2;
        }

        [Serializable]
        internal class MyLocalJsonProvidedMyEventSchema
        {
            public string col1;
            public MyLocalJsonProvidedMyInnerSchema col2;
        }

        [Serializable]
        internal class MyLocalJsonProvidedMyInfraITV
        {
            public string c1;
            public MyLocalJsonProvidedMyInnerSchema c2;
        }
    }
} // end of namespace