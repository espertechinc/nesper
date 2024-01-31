///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;
using NUnit.Framework.Legacy;

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
                env.CompileDeploy("@name('s" + i + "') " + statements[i], path);
                listeners[i] = new SupportUpdateListener();
                env.Statement("s" + i).AddListener(listeners[i]);
            }

            var fields = new[] { "vbase", "v1", "v2", "va", "vb" };

            var type = env.Runtime.EventTypeService.GetEventTypePreconfigured("SubAEvent");
            ClassicAssert.AreEqual("base", type.PropertyDescriptors[0].PropertyName);
            ClassicAssert.AreEqual("sub1", type.PropertyDescriptors[1].PropertyName);
            ClassicAssert.AreEqual("suba", type.PropertyDescriptors[2].PropertyName);
            ClassicAssert.AreEqual(3, type.PropertyDescriptors.Count);

            type = env.Runtime.EventTypeService.GetEventTypePreconfigured("SubBEvent");
            ClassicAssert.AreEqual("[\"base\", \"sub1\", \"suba\", \"subb\"]", type.PropertyNames.RenderAny());
            ClassicAssert.AreEqual(4, type.PropertyDescriptors.Count);

            type = env.Runtime.EventTypeService.GetEventTypePreconfigured("Sub1Event");
            ClassicAssert.AreEqual("[\"base\", \"sub1\"]", type.PropertyNames.RenderAny());
            ClassicAssert.AreEqual(2, type.PropertyDescriptors.Count);

            type = env.Runtime.EventTypeService.GetEventTypePreconfigured("Sub2Event");
            ClassicAssert.AreEqual("[\"base\", \"sub2\"]", type.PropertyNames.RenderAny());
            ClassicAssert.AreEqual(2, type.PropertyDescriptors.Count);

            env.SendEventObjectArray(new object[] { "a", "b", "x" }, "SubAEvent"); // base, sub1, suba
            EPAssertionUtil.AssertProps(
                listeners[0].AssertOneGetNewAndReset(),
                fields,
                new object[] { "a", "b", null, "x", null });
            ClassicAssert.IsFalse(listeners[2].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(
                listeners[1].AssertOneGetNewAndReset(),
                fields,
                new object[] { "a", "b", null, "x", null });
            EPAssertionUtil.AssertProps(
                listeners[3].AssertOneGetNewAndReset(),
                fields,
                new object[] { "a", "b", null, "x", null });

            env.SendEventObjectArray(new object[] { "f1", "f2", "f4" }, "SubAEvent");
            EPAssertionUtil.AssertProps(
                listeners[0].AssertOneGetNewAndReset(),
                fields,
                new object[] { "f1", "f2", null, "f4", null });
            ClassicAssert.IsFalse(listeners[2].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(
                listeners[1].AssertOneGetNewAndReset(),
                fields,
                new object[] { "f1", "f2", null, "f4", null });
            EPAssertionUtil.AssertProps(
                listeners[3].AssertOneGetNewAndReset(),
                fields,
                new object[] { "f1", "f2", null, "f4", null });

            env.SendEventObjectArray(new object[] { "XBASE", "X1", "X2", "XY" }, "SubBEvent");
            object[] values = { "XBASE", "X1", null, "X2", "XY" };
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            ClassicAssert.IsFalse(listeners[2].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, values);
            EPAssertionUtil.AssertProps(listeners[3].AssertOneGetNewAndReset(), fields, values);
            EPAssertionUtil.AssertProps(listeners[4].AssertOneGetNewAndReset(), fields, values);

            env.SendEventObjectArray(new object[] { "YBASE", "Y1" }, "Sub1Event");
            values = new object[] { "YBASE", "Y1", null, null, null };
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            ClassicAssert.IsFalse(listeners[2].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, values);

            env.SendEventObjectArray(new object[] { "YBASE", "Y2" }, "Sub2Event");
            values = new object[] { "YBASE", null, "Y2", null, null };
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            ClassicAssert.IsFalse(listeners[1].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNewAndReset(), fields, values);

            env.SendEventObjectArray(new object[] { "ZBASE" }, "RootEvent");
            values = new object[] { "ZBASE", null, null, null, null };
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            ClassicAssert.IsFalse(
                listeners[1].IsInvoked || listeners[2].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);

            // try property not available
            env.TryInvalidCompile(
                path,
                "select suba from Sub1Event",
                "Failed to validate select-clause expression 'suba': Property named 'suba' is not valid in any stream (did you mean 'sub1'?)");

            env.UndeployAll();
        }

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.OBSERVEROPS);
        }
    }
} // end of namespace