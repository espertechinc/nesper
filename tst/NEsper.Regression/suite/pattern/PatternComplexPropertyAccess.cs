///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.patternassert;

using NUnit.Framework; // assertEquals

// assertSame

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternComplexPropertyAccess
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithComplexProperties(execs);
            WithIndexedFilterProp(execs);
            WithIndexedValueProp(execs);
            WithIndexedValuePropOM(execs);
            WithIndexedValuePropCompile(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithIndexedValuePropCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternIndexedValuePropCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithIndexedValuePropOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternIndexedValuePropOM());
            return execs;
        }

        public static IList<RegressionExecution> WithIndexedValueProp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternIndexedValueProp());
            return execs;
        }

        public static IList<RegressionExecution> WithIndexedFilterProp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternIndexedFilterProp());
            return execs;
        }

        public static IList<RegressionExecution> WithComplexProperties(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternComplexProperties());
            return execs;
        }

        private class PatternComplexProperties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                EventCollection events = EventCollectionFactory.GetSetSixComplexProperties();
                var testCaseList = new CaseList();
                EventExpressionCase testCase;

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(mapped('keyOne') = 'valueOne')");
                testCase.Add("e1", "s", events.GetEvent("e1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(indexed[1] = 2)");
                testCase.Add("e1", "s", events.GetEvent("e1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(indexed[0] = 2)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(ArrayProperty[1] = 20)");
                testCase.Add("e1", "s", events.GetEvent("e1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(ArrayProperty[1] in (10:30))");
                testCase.Add("e1", "s", events.GetEvent("e1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(ArrayProperty[2] = 20)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(nested.NestedValue = 'nestedValue')");
                testCase.Add("e1", "s", events.GetEvent("e1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(nested.NestedValue = 'dummy')");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "s=SupportBeanComplexProps(nested.NestedNested.NestedNestedValue = 'NestedNestedValue')");
                testCase.Add("e1", "s", events.GetEvent("e1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "s=SupportBeanComplexProps(nested.NestedNested.NestedNestedValue = 'x')");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "s=SupportBeanCombinedProps(indexed[1].mapped('1mb').Value = '1ma1')");
                testCase.Add("e2", "s", events.GetEvent("e2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanCombinedProps(indexed[0].mapped('1ma').Value = 'x')");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanCombinedProps(array[0].mapped('0ma').Value = '0ma0')");
                testCase.Add("e2", "s", events.GetEvent("e2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanCombinedProps(array[2].mapped('x').Value = 'x')");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanCombinedProps(array[879787].mapped('x').Value = 'x')");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanCombinedProps(array[0].mapped('xxx').Value = 'x')");
                testCaseList.AddTest(testCase);

                var util = new PatternTestHarness(events, testCaseList);
                util.RunTest(env);
            }
        }

        private class PatternIndexedFilterProp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var pattern = "@name('s0') select * from pattern[every a=SupportBeanComplexProps(indexed[0]=3)]";
                env.CompileDeploy(pattern).AddListener("s0");

                object theEventOne = new SupportBeanComplexProps(new int[] { 3, 4 });
                env.SendEventBean(theEventOne);
                env.AssertEventNew("s0", @event => Assert.AreSame(theEventOne, @event.Get("a")));

                env.SendEventBean(new SupportBeanComplexProps(new int[] { 6 }));
                env.AssertListenerNotInvoked("s0");

                object theEventTwo = new SupportBeanComplexProps(new int[] { 3 });
                env.SendEventBean(theEventTwo);
                env.AssertEventNew("s0", @event => Assert.AreSame(theEventTwo, @event.Get("a")));

                env.UndeployAll();
            }
        }

        private class PatternIndexedValueProp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var pattern =
                    "@name('s0') select * from pattern[every a=SupportBeanComplexProps -> b=SupportBeanComplexProps(indexed[0] = a.indexed[0])]";
                env.CompileDeploy(pattern).AddListener("s0");
                RunIndexedValueProp(env);
                env.UndeployAll();
            }
        }

        private class PatternIndexedValuePropOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var type = nameof(SupportBeanComplexProps);

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.CreateWildcard();
                PatternExpr pattern = Patterns.FollowedBy(
                    Patterns.EveryFilter(type, "a"),
                    Patterns.Filter(Filter.Create(type, Expressions.EqProperty("indexed[0]", "a.indexed[0]")), "b"));
                model.FromClause = FromClause.Create(PatternStream.Create(pattern));
                model = env.CopyMayFail(model);

                var patternText = "select * from pattern [every a=" +
                                  type +
                                  " -> b=" +
                                  type +
                                  "(indexed[0]=a.indexed[0])]";
                Assert.AreEqual(patternText, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");
                RunIndexedValueProp(env);
                env.UndeployAll();
            }
        }

        private class PatternIndexedValuePropCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var patternText =
                    "@name('s0') select * from pattern [every a=SupportBeanComplexProps -> b=SupportBeanComplexProps(indexed[0]=a.indexed[0])]";
                env.EplToModelCompileDeploy(patternText).AddListener("s0");
                RunIndexedValueProp(env);
                env.UndeployAll();
            }
        }

        private static void RunIndexedValueProp(RegressionEnvironment env)
        {
            object eventOne = new SupportBeanComplexProps(new int[] { 3 });
            env.SendEventBean(eventOne);
            env.AssertListenerNotInvoked("s0");

            object theEvent = new SupportBeanComplexProps(new int[] { 6 });
            env.SendEventBean(theEvent);
            env.AssertListenerNotInvoked("s0");

            object eventTwo = new SupportBeanComplexProps(new int[] { 3 });
            env.SendEventBean(eventTwo);
            env.AssertEventNew(
                "s0",
                @event => {
                    Assert.AreSame(eventOne, @event.Get("a"));
                    Assert.AreSame(eventTwo, @event.Get("b"));
                });
        }
    }
} // end of namespace