///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanMappedIndexedPropertyExpression : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // test bean-type
            var path = new RegressionPath();
            var eplBeans = "select " +
                           "Mapped(TheString) as val0, " +
                           "Indexed(IntPrimitive) as val1 " +
                           "from SupportBeanComplexProps#lastevent, SupportBean sb unidirectional";
            RunAssertionBean(env, path, eplBeans);

            // test bean-type prefixed
            var eplBeansPrefixed = "select " +
                                   "sbcp.Mapped(TheString) as val0, " +
                                   "sbcp.Indexed(IntPrimitive) as val1 " +
                                   "from SupportBeanComplexProps#lastevent sbcp, SupportBean sb unidirectional";
            RunAssertionBean(env, path, eplBeansPrefixed);

            // test wrap
            env.CompileDeploy("insert into SecondStream select 'a' as val0, * from SupportBeanComplexProps", path);

            var eplWrap = "select " +
                          "Mapped(TheString) as val0," +
                          "Indexed(IntPrimitive) as val1 " +
                          "from SecondStream #lastevent, SupportBean unidirectional";
            RunAssertionBean(env, path, eplWrap);

            var eplWrapPrefixed = "select " +
                                  "sbcp.Mapped(TheString) as val0," +
                                  "sbcp.Indexed(IntPrimitive) as val1 " +
                                  "from SecondStream #lastevent sbcp, SupportBean unidirectional";
            RunAssertionBean(env, path, eplWrapPrefixed);
            
            // test Map-type
            var eplMap = "select " +
                         "Mapped(TheString) as val0," +
                         "Indexed(IntPrimitive) as val1 " +
                         "from MapEvent#lastevent, SupportBean unidirectional";
            RunAssertionMap(env, eplMap);

            var eplMapPrefixed = "select " +
                                 "sbcp.Mapped(TheString) as val0," +
                                 "sbcp.Indexed(IntPrimitive) as val1 " +
                                 "from MapEvent#lastevent sbcp, SupportBean unidirectional";
            RunAssertionMap(env, eplMapPrefixed);

            // test insert-int
            env.CompileDeploy("@name('s0') select name,value,properties(name) = value as ok from InputEvent")
                .AddListener("s0");

            env.SendEventMap(
                MakeMapEvent("name", "value1", Collections.SingletonDataMap("name", "xxxx")),
                "InputEvent");
            Assert.IsFalse((bool) env.Listener("s0").AssertOneGetNewAndReset().Get("ok"));

            env.SendEventMap(
                MakeMapEvent("name", "value1", Collections.SingletonDataMap("name", "value1")),
                "InputEvent");
            Assert.IsTrue((bool) env.Listener("s0").AssertOneGetNewAndReset().Get("ok"));

            env.UndeployAll();

            // test Object-array-type
            var eplObjectArray = "select " +
                                 "Mapped(TheString) as val0," +
                                 "Indexed(IntPrimitive) as val1 " +
                                 "from ObjectArrayEvent#lastevent, SupportBean unidirectional";
            RunAssertionObjectArray(env, eplObjectArray);

            var eplObjectArrayPrefixed = "select " +
                                         "sbcp.Mapped(TheString) as val0," +
                                         "sbcp.Indexed(IntPrimitive) as val1 " +
                                         "from ObjectArrayEvent#lastevent sbcp, SupportBean unidirectional";
            RunAssertionObjectArray(env, eplObjectArrayPrefixed);
        }

        private void RunAssertionMap(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy("@name('s0') " + epl).AddListener("s0");

            env.SendEventMap(MakeMapEvent(), "MapEvent");
            env.SendEventBean(new SupportBean("keyOne", 1));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new [] { "val0", "val1" },
                new object[] {"valueOne", 2});
            env.UndeployModuleContaining("s0");
        }

        private void RunAssertionObjectArray(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy("@name('s0') " + epl).AddListener("s0");

            env.SendEventObjectArray(
                new object[] {Collections.SingletonMap("keyOne", "valueOne"), new[] {1, 2}},
                "ObjectArrayEvent");
            env.SendEventBean(new SupportBean("keyOne", 1));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new [] { "val0", "val1" },
                new object[] {"valueOne", 2});
            env.UndeployModuleContaining("s0");
        }

        private void RunAssertionBean(
            RegressionEnvironment env,
            RegressionPath path,
            string epl)
        {
            env.CompileDeploy("@name('s0') " + epl, path).AddListener("s0");

            env.SendEventBean(SupportBeanComplexProps.MakeDefaultBean());
            env.SendEventBean(new SupportBean("keyOne", 1));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new [] { "val0", "val1" },
                new object[] {"valueOne", 2});
            env.UndeployModuleContaining("s0");
        }

        private IDictionary<string, object> MakeMapEvent()
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Put("Mapped", Collections.SingletonMap("keyOne", "valueOne"));
            map.Put("Indexed", new[] {1, 2});
            return map;
        }

        private IDictionary<string, object> MakeMapEvent(
            string name,
            string value,
            IDictionary<string, object> properties)
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Put("name", name);
            map.Put("value", value);
            map.Put("properties", properties);
            return map;
        }
    }
} // end of namespace