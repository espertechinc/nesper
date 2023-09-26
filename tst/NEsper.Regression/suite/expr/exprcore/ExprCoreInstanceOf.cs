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
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework;

using SupportMarkerInterface = com.espertech.esper.regressionlib.support.bean.SupportMarkerInterface;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreInstanceOf
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithInstanceofSimple(execs);
            WithInstanceofStringAndNullOM(execs);
            WithInstanceofStringAndNullCompile(execs);
            WithDynamicPropertyNativeTypes(execs);
            WithDynamicSuperTypeAndInterface(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithDynamicSuperTypeAndInterface(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreDynamicSuperTypeAndInterface());
            return execs;
        }

        public static IList<RegressionExecution> WithDynamicPropertyNativeTypes(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreDynamicPropertyNativeTypes());
            return execs;
        }

        public static IList<RegressionExecution> WithInstanceofStringAndNullCompile(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreInstanceofStringAndNullCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithInstanceofStringAndNullOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreInstanceofStringAndNullOM());
            return execs;
        }

        public static IList<RegressionExecution> WithInstanceofSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreInstanceofSimple());
            return execs;
        }

        private class ExprCoreInstanceofSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5,c6,c7".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean")
                    .WithExpression(fields[0], "instanceof(theString, string)")
                    .WithExpression(fields[1], "instanceof(intBoxed, int)")
                    .WithExpression(fields[2], "instanceof(floatBoxed, System.Single)")
                    .WithExpression(fields[3], "instanceof(theString, System.Single, char, byte)")
                    .WithExpression(fields[4], "instanceof(intPrimitive, System.Int32)")
                    .WithExpression(fields[5], "instanceof(intPrimitive, long)")
                    .WithExpression(fields[6], "instanceof(intPrimitive, long, long, System.Object)")
                    .WithExpression(fields[7], "instanceof(floatBoxed, long, float)");

                builder.WithStatementConsumer(
                    stmt => {
                        for (var i = 0; i < fields.Length; i++) {
                            Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType(fields[i]));
                        }
                    });

                var bean = new SupportBean("abc", 100);
                bean.FloatBoxed = 100F;
                builder.WithAssertion(bean).Expect(fields, true, false, true, false, true, false, true, true);

                bean = new SupportBean(null, 100);
                bean.FloatBoxed = null;
                builder.WithAssertion(bean).Expect(fields, false, false, false, false, true, false, true, false);

                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreInstanceofStringAndNullOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "select instanceof(theString,string) as t0, " +
                               "instanceof(theString,float,string,int) as t1 " +
                               "from SupportBean";

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create()
                    .Add(Expressions.InstanceOf("theString", "string"), "t0")
                    .Add(Expressions.InstanceOf(Expressions.Property("theString"), "float", "string", "int"), "t1");
                model.FromClause = FromClause.Create(FilterStream.Create(nameof(SupportBean)));
                model = env.CopyMayFail(model);
                Assert.AreEqual(stmtText, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0").Milestone(0);

                env.SendEventBean(new SupportBean("abc", 100));
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        Assert.IsTrue((bool)theEvent.Get("t0"));
                        Assert.IsTrue((bool)theEvent.Get("t1"));
                    });

                env.SendEventBean(new SupportBean(null, 100));
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        Assert.IsFalse((bool)theEvent.Get("t0"));
                        Assert.IsFalse((bool)theEvent.Get("t1"));
                    });

                env.UndeployAll();
            }
        }

        private class ExprCoreInstanceofStringAndNullCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select instanceof(theString,string) as t0, " +
                          "instanceof(theString,float,string,int) as t1 " +
                          "from SupportBean";
                env.EplToModelCompileDeploy(epl).AddListener("s0").Milestone(0);

                env.SendEventBean(new SupportBean("abc", 100));
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        Assert.IsTrue((bool)theEvent.Get("t0"));
                        Assert.IsTrue((bool)theEvent.Get("t1"));
                    });

                env.SendEventBean(new SupportBean(null, 100));
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        Assert.IsFalse((bool)theEvent.Get("t0"));
                        Assert.IsFalse((bool)theEvent.Get("t1"));
                    });

                env.UndeployAll();
            }
        }

        private class ExprCoreDynamicPropertyNativeTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select instanceof(item?, string) as t0, " +
                          " instanceof(item?, int) as t1, " +
                          " instanceof(item?, System.Single) as t2, " +
                          " instanceof(item?, System.Single, char, byte) as t3, " +
                          " instanceof(item?, System.Int32) as t4, " +
                          " instanceof(item?, long) as t5, " +
                          " instanceof(item?, long, long, System.Object) as t6, " +
                          " instanceof(item?, long, float) as t7 " +
                          " from SupportBeanDynRoot";

                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanDynRoot("abc"));
                env.AssertEventNew(
                    "s0",
                    @event => AssertResults(
                        @event,
                        new bool[] { true, false, false, false, false, false, false, false }));

                env.SendEventBean(new SupportBeanDynRoot(100f));
                env.AssertEventNew(
                    "s0",
                    @event => AssertResults(@event, new bool[] { false, false, true, true, false, false, true, true }));

                env.SendEventBean(new SupportBeanDynRoot(null));
                env.AssertEventNew(
                    "s0",
                    @event => AssertResults(
                        @event,
                        new bool[] { false, false, false, false, false, false, false, false }));

                env.SendEventBean(new SupportBeanDynRoot(10));
                env.AssertEventNew(
                    "s0",
                    @event => AssertResults(
                        @event,
                        new bool[] { false, true, false, false, true, false, true, false }));

                env.SendEventBean(new SupportBeanDynRoot(99L));
                env.AssertEventNew(
                    "s0",
                    @event => AssertResults(
                        @event,
                        new bool[] { false, false, false, false, false, true, true, true }));

                env.UndeployAll();
            }
        }

        private class ExprCoreDynamicSuperTypeAndInterface : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select instanceof(item?, " +
                          typeof(SupportMarkerInterface).FullName +
                          ") as t0, " +
                          " instanceof(item?, " +
                          typeof(ISupportA).FullName +
                          ") as t1, " +
                          " instanceof(item?, " +
                          typeof(ISupportBaseAB).FullName +
                          ") as t2, " +
                          " instanceof(item?, " +
                          typeof(ISupportBaseABImpl).FullName +
                          ") as t3, " +
                          " instanceof(item?, " +
                          typeof(ISupportA).FullName +
                          ", " +
                          typeof(ISupportB).FullName +
                          ") as t4, " +
                          " instanceof(item?, " +
                          typeof(ISupportBaseAB).FullName +
                          ", " +
                          typeof(ISupportB).FullName +
                          ") as t5, " +
                          " instanceof(item?, " +
                          typeof(ISupportAImplSuperG).FullName +
                          ", " +
                          typeof(ISupportB).FullName +
                          ") as t6, " +
                          " instanceof(item?, " +
                          typeof(ISupportAImplSuperGImplPlus).FullName +
                          ", " +
                          typeof(SupportBeanAtoFBase).FullName +
                          ") as t7 " +
                          " from SupportBeanDynRoot";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanDynRoot(new SupportBeanDynRoot("abc")));
                env.AssertEventNew(
                    "s0",
                    @event => AssertResults(
                        @event,
                        new bool[] { true, false, false, false, false, false, false, false }));

                env.SendEventBean(new SupportBeanDynRoot(new ISupportAImplSuperGImplPlus()));
                env.AssertEventNew(
                    "s0",
                    @event => AssertResults(@event, new bool[] { false, true, true, false, true, true, true, true }));

                env.SendEventBean(new SupportBeanDynRoot(new ISupportAImplSuperGImpl("", "", "")));
                env.AssertEventNew(
                    "s0",
                    @event => AssertResults(@event, new bool[] { false, true, true, false, true, true, true, false }));

                env.SendEventBean(new SupportBeanDynRoot(new ISupportBaseABImpl("")));
                env.AssertEventNew(
                    "s0",
                    @event => AssertResults(
                        @event,
                        new bool[] { false, false, true, true, false, true, false, false }));

                env.SendEventBean(new SupportBeanDynRoot(new ISupportBImpl("", "")));
                env.AssertEventNew(
                    "s0",
                    @event => AssertResults(@event, new bool[] { false, false, true, false, true, true, true, false }));

                env.SendEventBean(new SupportBeanDynRoot(new ISupportAImpl("", "")));
                env.AssertEventNew(
                    "s0",
                    @event => AssertResults(@event, new bool[] { false, true, true, false, true, true, false, false }));

                env.UndeployAll();
            }
        }

        private static void AssertResults(
            EventBean theEvent,
            bool[] result)
        {
            for (var i = 0; i < result.Length; i++) {
                Assert.AreEqual(result[i], theEvent.Get("t" + i), "failed for index " + i);
            }
        }
    }
} // end of namespace