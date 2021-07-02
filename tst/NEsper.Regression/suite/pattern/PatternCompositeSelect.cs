///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternCompositeSelect
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithFollowedByFilter(execs);
            WithFragment(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithFragment(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new PatternFragment());
            return execs;
        }

        public static IList<RegressionExecution> WithFollowedByFilter(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new PatternFollowedByFilter());
            return execs;
        }

        internal class PatternFollowedByFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('insert') insert into StreamOne select * from pattern [a=SupportBean_A -> b=SupportBean_B];\n" +
                    "@Name('s0') select *, 1 as code from StreamOne;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_B("B1"));
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();

                var values = new object[env.Statement("s0").EventType.PropertyNames.Length];
                var count = 0;
                foreach (var name in env.Statement("s0").EventType.PropertyNames) {
                    values[count++] = theEvent.Get(name);
                }

                SupportEventPropUtil.AssertPropsEquals(
                    env.Statement("insert").EventType.PropertyDescriptors.ToArray(),
                    new SupportEventPropDesc("a", typeof(SupportBean_A)).WithFragment(),
                    new SupportEventPropDesc("b", typeof(SupportBean_B)).WithFragment());
                
                env.UndeployAll();
            }
        }

        internal class PatternFragment : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTxtOne = "@Name('s0') select * from pattern [[2] a=SupportBean_A -> b=SupportBean_B]";
                env.CompileDeploy(stmtTxtOne).AddListener("s0");

                SupportEventPropUtil.AssertPropsEquals(
                    env.Statement("s0").EventType.PropertyDescriptors.ToArray(),
                    new SupportEventPropDesc("a", typeof(SupportBean_A[])).WithIndexed().WithComponentType(typeof(SupportBean_A)).WithFragment(),
                    new SupportEventPropDesc("b", typeof(SupportBean_B)).WithFragment());

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_A("A2"));
                env.SendEventBean(new SupportBean_B("B1"));

                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.That(theEvent.Underlying, Is.InstanceOf<IDictionary<string, object>>());

                // test fragment B type and event
                var typeFragB = theEvent.EventType.GetFragmentType("b");
                Assert.IsFalse(typeFragB.IsIndexed);
                Assert.AreEqual("SupportBean_B", typeFragB.FragmentType.Name);
                Assert.AreEqual(typeof(string), typeFragB.FragmentType.GetPropertyType("Id"));

                var eventFragB = (EventBean) theEvent.GetFragment("b");
                Assert.AreEqual("SupportBean_B", eventFragB.EventType.Name);

                // test fragment A type and event
                var typeFragA = theEvent.EventType.GetFragmentType("a");
                Assert.IsTrue(typeFragA.IsIndexed);
                Assert.AreEqual("SupportBean_A", typeFragA.FragmentType.Name);
                Assert.AreEqual(typeof(string), typeFragA.FragmentType.GetPropertyType("Id"));

                Assert.IsTrue(theEvent.GetFragment("a") is EventBean[]);
                var eventFragA1 = (EventBean) theEvent.GetFragment("a[0]");
                Assert.AreEqual("SupportBean_A", eventFragA1.EventType.Name);
                Assert.AreEqual("A1", eventFragA1.Get("Id"));
                var eventFragA2 = (EventBean) theEvent.GetFragment("a[1]");
                Assert.AreEqual("SupportBean_A", eventFragA2.EventType.Name);
                Assert.AreEqual("A2", eventFragA2.Get("Id"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace