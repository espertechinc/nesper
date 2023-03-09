///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.patternassert;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternComplexPropertyAccess
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
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

        private static void RunIndexedValueProp(RegressionEnvironment env)
        {
            object eventOne = new SupportBeanComplexProps(new[] {3});
            env.SendEventBean(eventOne);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            object theEvent = new SupportBeanComplexProps(new[] {6});
            env.SendEventBean(theEvent);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            object eventTwo = new SupportBeanComplexProps(new[] {3});
            env.SendEventBean(eventTwo);
            var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreSame(eventOne, eventBean.Get("a"));
            Assert.AreSame(eventTwo, eventBean.Get("b"));
        }

        internal class PatternComplexProperties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var events = EventCollectionFactory.GetSetSixComplexProperties();
                EventExpressionCase testCase;

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(Mapped('keyOne') = 'valueOne')");
                testCase.Add("e1", "s", events.GetEvent("e1"));
                PatternTestHarness.RunSingle(env, events, testCase, GetType());

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(Indexed[1] = 2)");
                testCase.Add("e1", "s", events.GetEvent("e1"));
                PatternTestHarness.RunSingle(env, events, testCase, GetType());

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(Indexed[0] = 2)");
                PatternTestHarness.RunSingle(env, events, testCase, GetType());

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(ArrayProperty[1] = 20)");
                testCase.Add("e1", "s", events.GetEvent("e1"));
                PatternTestHarness.RunSingle(env, events, testCase, GetType());

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(ArrayProperty[1] in (10:30))");
                testCase.Add("e1", "s", events.GetEvent("e1"));
                PatternTestHarness.RunSingle(env, events, testCase, GetType());

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(ArrayProperty[2] = 20)");
                PatternTestHarness.RunSingle(env, events, testCase, GetType());

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(Nested.NestedValue = 'NestedValue')");
                testCase.Add("e1", "s", events.GetEvent("e1"));
                PatternTestHarness.RunSingle(env, events, testCase, GetType());

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(Nested.NestedValue = 'dummy')");
                PatternTestHarness.RunSingle(env, events, testCase, GetType());

                testCase = new EventExpressionCase(
                    "s=SupportBeanComplexProps(Nested.NestedNested.NestedNestedValue = 'NestedNestedValue')");
                testCase.Add("e1", "s", events.GetEvent("e1"));
                PatternTestHarness.RunSingle(env, events, testCase, GetType());

                testCase = new EventExpressionCase(
                    "s=SupportBeanComplexProps(Nested.NestedNested.NestedNestedValue = 'x')");
                PatternTestHarness.RunSingle(env, events, testCase, GetType());

                testCase = new EventExpressionCase(
                    "s=SupportBeanCombinedProps(Indexed[1].Mapped('1mb').Value = '1ma1')");
                testCase.Add("e2", "s", events.GetEvent("e2"));
                PatternTestHarness.RunSingle(env, events, testCase, GetType());

                testCase = new EventExpressionCase("s=SupportBeanCombinedProps(Indexed[0].Mapped('1ma').Value = 'x')");
                PatternTestHarness.RunSingle(env, events, testCase, GetType());

                testCase = new EventExpressionCase("s=SupportBeanCombinedProps(Array[0].Mapped('0ma').Value = '0ma0')");
                testCase.Add("e2", "s", events.GetEvent("e2"));
                PatternTestHarness.RunSingle(env, events, testCase, GetType());

                testCase = new EventExpressionCase("s=SupportBeanCombinedProps(Array[2].Mapped('x').Value = 'x')");
                PatternTestHarness.RunSingle(env, events, testCase, GetType());

                testCase = new EventExpressionCase("s=SupportBeanCombinedProps(Array[879787].Mapped('x').Value = 'x')");
                PatternTestHarness.RunSingle(env, events, testCase, GetType());

                testCase = new EventExpressionCase("s=SupportBeanCombinedProps(Array[0].Mapped('xxx').Value = 'x')");
                PatternTestHarness.RunSingle(env, events, testCase, GetType());
            }
        }

        internal class PatternIndexedFilterProp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var pattern = "@Name('s0') select * from pattern[every a=SupportBeanComplexProps(Indexed[0]=3)]";
                env.CompileDeploy(pattern).AddListener("s0");

                object theEvent = new SupportBeanComplexProps(new[] {3, 4});
                env.SendEventBean(theEvent);
                Assert.AreSame(theEvent, env.Listener("s0").AssertOneGetNewAndReset().Get("a"));

                theEvent = new SupportBeanComplexProps(new[] {6});
                env.SendEventBean(theEvent);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                theEvent = new SupportBeanComplexProps(new[] {3});
                env.SendEventBean(theEvent);
                Assert.AreSame(theEvent, env.Listener("s0").AssertOneGetNewAndReset().Get("a"));

                env.UndeployAll();
            }
        }

        internal class PatternIndexedValueProp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var pattern =
                    "@Name('s0') select * from pattern[every a=SupportBeanComplexProps -> b=SupportBeanComplexProps(Indexed[0] = a.Indexed[0])]";
                env.CompileDeploy(pattern).AddListener("s0");
                RunIndexedValueProp(env);
                env.UndeployAll();
            }
        }

        internal class PatternIndexedValuePropOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var type = nameof(SupportBeanComplexProps);

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.CreateWildcard();
                PatternExpr pattern = Patterns.FollowedBy(
                    Patterns.EveryFilter(type, "a"),
                    Patterns.Filter(Filter.Create(type, Expressions.EqProperty("Indexed[0]", "a.Indexed[0]")), "b"));
                model.FromClause = FromClause.Create(PatternStream.Create(pattern));
                model = env.CopyMayFail(model);

                var patternText = "select * from pattern [every a=" +
                                  type +
                                  " -> b=" +
                                  type +
                                  "(Indexed[0]=a.Indexed[0])]";
                Assert.AreEqual(patternText, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");
                RunIndexedValueProp(env);
                env.UndeployAll();
            }
        }

        internal class PatternIndexedValuePropCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var patternText =
                    "@Name('s0') select * from pattern [every a=SupportBeanComplexProps -> b=SupportBeanComplexProps(Indexed[0]=a.Indexed[0])]";
                env.EplToModelCompileDeploy(patternText).AddListener("s0");
                RunIndexedValueProp(env);
                env.UndeployAll();
            }
        }
    }
} // end of namespace