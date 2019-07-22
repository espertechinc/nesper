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
            execs.Add(new PatternComplexProperties());
            execs.Add(new PatternIndexedFilterProp());
            execs.Add(new PatternIndexedValueProp());
            execs.Add(new PatternIndexedValuePropOM());
            execs.Add(new PatternIndexedValuePropCompile());
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

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(arrayProperty[1] = 20)");
                testCase.Add("e1", "s", events.GetEvent("e1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(arrayProperty[1] in (10:30))");
                testCase.Add("e1", "s", events.GetEvent("e1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(arrayProperty[2] = 20)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(nested.NestedValue = 'NestedValue')");
                testCase.Add("e1", "s", events.GetEvent("e1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanComplexProps(nested.NestedValue = 'dummy')");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "s=SupportBeanComplexProps(nested.nestedNested.nestedNestedValue = 'nestedNestedValue')");
                testCase.Add("e1", "s", events.GetEvent("e1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "s=SupportBeanComplexProps(nested.nestedNested.nestedNestedValue = 'x')");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "s=SupportBeanCombinedProps(indexed[1].mapped('1mb').value = '1ma1')");
                testCase.Add("e2", "s", events.GetEvent("e2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanCombinedProps(indexed[0].mapped('1ma').value = 'x')");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanCombinedProps(array[0].mapped('0ma').value = '0ma0')");
                testCase.Add("e2", "s", events.GetEvent("e2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanCombinedProps(array[2].mapped('x').value = 'x')");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanCombinedProps(array[879787].mapped('x').value = 'x')");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("s=SupportBeanCombinedProps(array[0].mapped('xxx').value = 'x')");
                testCaseList.AddTest(testCase);

                var util = new PatternTestHarness(events, testCaseList, GetType());
                util.RunTest(env);
            }
        }

        internal class PatternIndexedFilterProp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var pattern = "@Name('s0') select * from pattern[every a=SupportBeanComplexProps(indexed[0]=3)]";
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
                    "@Name('s0') select * from pattern[every a=SupportBeanComplexProps -> b=SupportBeanComplexProps(indexed[0] = a.indexed[0])]";
                env.CompileDeploy(pattern).AddListener("s0");
                RunIndexedValueProp(env);
                env.UndeployAll();
            }
        }

        internal class PatternIndexedValuePropOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var type = typeof(SupportBeanComplexProps).Name;

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

        internal class PatternIndexedValuePropCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var patternText =
                    "@Name('s0') select * from pattern [every a=SupportBeanComplexProps -> b=SupportBeanComplexProps(indexed[0]=a.indexed[0])]";
                env.EplToModelCompileDeploy(patternText).AddListener("s0");
                RunIndexedValueProp(env);
                env.UndeployAll();
            }
        }
    }
} // end of namespace