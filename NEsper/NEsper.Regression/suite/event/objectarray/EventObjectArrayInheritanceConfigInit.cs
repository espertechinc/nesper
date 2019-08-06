///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.@event.objectarray
{
    public class EventObjectArrayInheritanceConfigInit : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            RunObjectArrInheritanceAssertion(env, new RegressionPath());
        }

        internal static void RunObjectArrInheritanceAssertion(
            RegressionEnvironment env,
            RegressionPath path)
        {
            var listeners = new SupportUpdateListener[5];
            string[] statements = {
                "select base as vbase, sub1? as v1, sub2? as v2, suba? as va, subb? as vb from RootEvent", // 0
                "select base as vbase, sub1 as v1, sub2? as v2, suba? as va, subb? as vb from Sub1Event", // 1
                "select base as vbase, sub1? as v1, sub2 as v2, suba? as va, subb? as vb from Sub2Event", // 2
                "select base as vbase, sub1 as v1, sub2? as v2, suba as va, subb? as vb from SubAEvent", // 3
                "select base as vbase, sub1? as v1, sub2? as v2, suba? as va, subb as vb from SubBEvent" // 4
            };
            for (var i = 0; i < statements.Length; i++) {
                env.CompileDeploy("@Name('s" + i + "') " + statements[i], path);
                listeners[i] = new SupportUpdateListener();
                env.Statement("s" + i).AddListener(listeners[i]);
            }

            var fields = "vbase,v1,v2,va,vb".SplitCsv();

            var type = env.Runtime.EventTypeService.GetEventTypePreconfigured("SubAEvent");
            Assert.AreEqual("base", type.PropertyDescriptors[0].PropertyName);
            Assert.AreEqual("sub1", type.PropertyDescriptors[1].PropertyName);
            Assert.AreEqual("suba", type.PropertyDescriptors[2].PropertyName);
            Assert.AreEqual(3, type.PropertyDescriptors.Length);

            type = env.Runtime.EventTypeService.GetEventTypePreconfigured("SubBEvent");
            Assert.AreEqual("[base, sub1, suba, subb]", type.PropertyNames.RenderAny());
            Assert.AreEqual(4, type.PropertyDescriptors.Length);

            type = env.Runtime.EventTypeService.GetEventTypePreconfigured("Sub1Event");
            Assert.AreEqual("[base, sub1]", type.PropertyNames.RenderAny());
            Assert.AreEqual(2, type.PropertyDescriptors.Length);

            type = env.Runtime.EventTypeService.GetEventTypePreconfigured("Sub2Event");
            Assert.AreEqual("[base, sub2]", type.PropertyNames.RenderAny());
            Assert.AreEqual(2, type.PropertyDescriptors.Length);

            env.SendEventObjectArray(new object[] {"a", "b", "x"}, "SubAEvent"); // base, sub1, suba
            EPAssertionUtil.AssertProps(
                listeners[0].AssertOneGetNewAndReset(),
                fields,
                new object[] {"a", "b", null, "x", null});
            Assert.IsFalse(listeners[2].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(
                listeners[1].AssertOneGetNewAndReset(),
                fields,
                new object[] {"a", "b", null, "x", null});
            EPAssertionUtil.AssertProps(
                listeners[3].AssertOneGetNewAndReset(),
                fields,
                new object[] {"a", "b", null, "x", null});

            env.SendEventObjectArray(new object[] {"f1", "f2", "f4"}, "SubAEvent");
            EPAssertionUtil.AssertProps(
                listeners[0].AssertOneGetNewAndReset(),
                fields,
                new object[] {"f1", "f2", null, "f4", null});
            Assert.IsFalse(listeners[2].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(
                listeners[1].AssertOneGetNewAndReset(),
                fields,
                new object[] {"f1", "f2", null, "f4", null});
            EPAssertionUtil.AssertProps(
                listeners[3].AssertOneGetNewAndReset(),
                fields,
                new object[] {"f1", "f2", null, "f4", null});

            env.SendEventObjectArray(new object[] {"XBASE", "X1", "X2", "XY"}, "SubBEvent");
            object[] values = {"XBASE", "X1", null, "X2", "XY"};
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            Assert.IsFalse(listeners[2].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, values);
            EPAssertionUtil.AssertProps(listeners[3].AssertOneGetNewAndReset(), fields, values);
            EPAssertionUtil.AssertProps(listeners[4].AssertOneGetNewAndReset(), fields, values);

            env.SendEventObjectArray(new object[] {"YBASE", "Y1"}, "Sub1Event");
            values = new object[] {"YBASE", "Y1", null, null, null};
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            Assert.IsFalse(listeners[2].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, values);

            env.SendEventObjectArray(new object[] {"YBASE", "Y2"}, "Sub2Event");
            values = new object[] {"YBASE", null, "Y2", null, null};
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            Assert.IsFalse(listeners[1].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNewAndReset(), fields, values);

            env.SendEventObjectArray(new object[] {"ZBASE"}, "RootEvent");
            values = new object[] {"ZBASE", null, null, null, null};
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            Assert.IsFalse(
                listeners[1].IsInvoked || listeners[2].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);

            // try property not available
            TryInvalidCompile(
                env,
                path,
                "select suba from Sub1Event",
                "Failed to validate select-clause expression 'suba': Property named 'suba' is not valid in any stream (did you mean 'sub1'?)");

            env.UndeployAll();
        }
    }
} // end of namespace