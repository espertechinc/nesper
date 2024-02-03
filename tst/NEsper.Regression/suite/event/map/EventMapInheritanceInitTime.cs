///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.map
{
    public class EventMapInheritanceInitTime : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.OBSERVEROPS);
        }

        public void Run(RegressionEnvironment env)
        {
            SupportEventPropUtil.AssertPropsEquals(
                env.Runtime.EventTypeService.GetEventTypePreconfigured("SubAEvent").PropertyDescriptors.ToArray(),
                new SupportEventPropDesc("base", typeof(string)),
                new SupportEventPropDesc("sub1", typeof(string)),
                new SupportEventPropDesc("suba", typeof(string)));

            RunAssertionMapInheritance(env, new RegressionPath());
        }

        internal static void RunAssertionMapInheritance(
            RegressionEnvironment env,
            RegressionPath path)
        {
            var listeners = new SupportUpdateListener[5];
            string[] statements = {
                "select base as vbase, sub1? as v1, sub2? as v2, suba? as va, subb? as vb from RootEvent", // 0
                "select base as vbase, sub1 as v1, sub2? as v2, suba? as va, subb? as vb from Sub1Event", // 1
                "select base as vbase, sub1? as v1, sub2 as v2, suba? as va, subb? as vb from Sub2Event", // 2
                "select base as vbase, sub1 as v1, sub2? as v2, suba as va, subb? as vb from SubAEvent", // 3
                "select base as vbase, sub1? as v1, sub2 as v2, suba? as va, subb as vb from SubBEvent" // 4
            };
            for (var i = 0; i < statements.Length; i++) {
                env.CompileDeploy("@name('s" + i + "') " + statements[i], path);
                listeners[i] = new SupportUpdateListener();
                env.Statement("s" + i).AddListener(listeners[i]);
            }

            var fields = new[] { "vbase", "v1", "v2", "va", "vb" };

            env.SendEventMap(EventMapCore.MakeMap("base=a,sub1=b,sub2=x,suba=c,subb=y"), "SubAEvent");
            EPAssertionUtil.AssertProps(
                listeners[0].AssertOneGetNewAndReset(),
                fields,
                new object[] { "a", "b", "x", "c", "y" });
            ClassicAssert.IsFalse(listeners[2].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(
                listeners[1].AssertOneGetNewAndReset(),
                fields,
                new object[] { "a", "b", "x", "c", "y" });
            EPAssertionUtil.AssertProps(
                listeners[3].AssertOneGetNewAndReset(),
                fields,
                new object[] { "a", "b", "x", "c", "y" });

            env.SendEventMap(EventMapCore.MakeMap("base=f1,sub1=f2,sub2=f3,suba=f4,subb=f5"), "SubAEvent");
            EPAssertionUtil.AssertProps(
                listeners[0].AssertOneGetNewAndReset(),
                fields,
                new object[] { "f1", "f2", "f3", "f4", "f5" });
            ClassicAssert.IsFalse(listeners[2].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(
                listeners[1].AssertOneGetNewAndReset(),
                fields,
                new object[] { "f1", "f2", "f3", "f4", "f5" });
            EPAssertionUtil.AssertProps(
                listeners[3].AssertOneGetNewAndReset(),
                fields,
                new object[] { "f1", "f2", "f3", "f4", "f5" });

            env.SendEventMap(EventMapCore.MakeMap("base=XBASE,sub1=X1,sub2=X2,subb=XY"), "SubBEvent");
            object[] values = { "XBASE", "X1", "X2", null, "XY" };
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            ClassicAssert.IsFalse(listeners[3].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, values);
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNewAndReset(), fields, values);
            EPAssertionUtil.AssertProps(listeners[4].AssertOneGetNewAndReset(), fields, values);

            env.SendEventMap(EventMapCore.MakeMap("base=YBASE,sub1=Y1"), "Sub1Event");
            values = new object[] { "YBASE", "Y1", null, null, null };
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            ClassicAssert.IsFalse(listeners[2].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, values);

            env.SendEventMap(EventMapCore.MakeMap("base=YBASE,sub2=Y2"), "Sub2Event");
            values = new object[] { "YBASE", null, "Y2", null, null };
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            ClassicAssert.IsFalse(listeners[1].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNewAndReset(), fields, values);

            env.SendEventMap(EventMapCore.MakeMap("base=ZBASE"), "RootEvent");
            values = new object[] { "ZBASE", null, null, null, null };
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            ClassicAssert.IsFalse(
                listeners[1].IsInvoked || listeners[2].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);

            // try property not available
            env.TryInvalidCompile(
                path,
                "select suba from Sub1Event",
                "Failed to validate select-clause expression 'suba': Property named 'suba' is not valid in any stream (did you mean 'sub1'?) [select suba from Sub1Event]");

            env.UndeployAll();
        }
    }
} // end of namespace