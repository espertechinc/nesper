///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    ///     NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowOnUpdateWMultiDispatch : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var fields = "company,value,total".SplitCsv();

            var eplSchema = "create schema S2 ( company string, value double, total double)";
            var path = new RegressionPath();
            env.CompileDeployWBusPublicType(eplSchema, path);

            // ESPER-568
            env.CompileDeploy("@Name('create') create window S2Win#time(25 hour)#firstunique(company) as S2", path);
            env.CompileDeploy("insert into S2Win select * from S2#firstunique(company)", path);
            env.CompileDeploy("on S2 as a update S2Win as b set total = b.Value + a.Value", path);
            env.CompileDeploy("@Name('s0') select count(*) as cnt from S2Win", path).AddListener("s0");

            CreateSendEvent(env, "S2", "AComp", 3.0, 0.0);
            Assert.AreEqual(1L, env.Listener("s0").AssertOneGetNewAndReset().Get("cnt"));
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("create"),
                fields,
                new[] {new object[] {"AComp", 3.0, 0.0}});

            CreateSendEvent(env, "S2", "AComp", 6.0, 0.0);
            Assert.AreEqual(1L, env.Listener("s0").AssertOneGetNewAndReset().Get("cnt"));
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("create"),
                fields,
                new[] {new object[] {"AComp", 3.0, 9.0}});

            CreateSendEvent(env, "S2", "AComp", 5.0, 0.0);
            Assert.AreEqual(1L, env.Listener("s0").AssertOneGetNewAndReset().Get("cnt"));
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("create"),
                fields,
                new[] {new object[] {"AComp", 3.0, 8.0}});

            CreateSendEvent(env, "S2", "BComp", 4.0, 0.0);
            // this example does not have @priority thereby it is undefined whether there are two counts delivered or one
            if (env.Listener("s0").LastNewData.Length == 2) {
                Assert.AreEqual(1L, env.Listener("s0").LastNewData[0].Get("cnt"));
                Assert.AreEqual(2L, env.Listener("s0").LastNewData[1].Get("cnt"));
            }
            else {
                Assert.AreEqual(2L, env.Listener("s0").AssertOneGetNewAndReset().Get("cnt"));
            }

            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("create"),
                fields,
                new[] {new object[] {"AComp", 3.0, 7.0}, new object[] {"BComp", 4.0, 0.0}});

            env.UndeployAll();
        }

        private static void CreateSendEvent(
            RegressionEnvironment env,
            string typeName,
            string company,
            double value,
            double total)
        {
            IDictionary<string, object> map = new LinkedHashMap<string, object>();
            map.Put("company", company);
            map.Put("value", value);
            map.Put("total", total);
            if (EventRepresentationChoiceExtensions.GetEngineDefault(env.Configuration).IsObjectArrayEvent()) {
                env.SendEventObjectArray(map.Values.ToArray(), typeName);
            }
            else {
                env.SendEventMap(map, typeName);
            }
        }
    }
} // end of namespace