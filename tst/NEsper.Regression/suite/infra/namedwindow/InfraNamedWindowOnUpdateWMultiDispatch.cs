///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    /// NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowOnUpdateWMultiDispatch : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
        }

        public void Run(RegressionEnvironment env)
        {
            var fields = "Company,Value,Total".SplitCsv();

            var eplSchema = "@public @buseventtype create schema S2 ( Company string, Value double, Total double)";
            var path = new RegressionPath();
            env.CompileDeploy(eplSchema, path);

            // ESPER-568
            env.CompileDeploy(
                "@name('create') @public create window S2Win#time(25 hour)#firstunique(Company) as S2",
                path);
            env.CompileDeploy("insert into S2Win select * from S2#firstunique(Company)", path);
            env.CompileDeploy("on S2 as a update S2Win as b set Total = b.Value + a.Value", path);
            env.CompileDeploy("@name('s0') select count(*) as cnt from S2Win", path).AddListener("s0");

            CreateSendEvent(env, "S2", "AComp", 3.0, 0.0);
            AssertCount(env, 1L);
            env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "AComp", 3.0, 0.0 } });

            CreateSendEvent(env, "S2", "AComp", 6.0, 0.0);
            AssertCount(env, 1L);
            env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "AComp", 3.0, 9.0 } });

            CreateSendEvent(env, "S2", "AComp", 5.0, 0.0);
            AssertCount(env, 1L);
            env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "AComp", 3.0, 8.0 } });

            CreateSendEvent(env, "S2", "BComp", 4.0, 0.0);
            // this example does not have @priority thereby it is undefined whether there are two counts delivered or one
            env.AssertListener(
                "s0",
                listener => {
                    if (listener.LastNewData.Length == 2) {
                        ClassicAssert.AreEqual(1L, listener.LastNewData[0].Get("cnt"));
                        ClassicAssert.AreEqual(2L, listener.LastNewData[1].Get("cnt"));
                    }
                    else {
                        ClassicAssert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("cnt"));
                    }
                });
            env.AssertPropsPerRowIterator(
                "create",
                fields,
                new object[][] { new object[] { "AComp", 3.0, 7.0 }, new object[] { "BComp", 4.0, 0.0 } });

            env.UndeployAll();
        }

        private static void AssertCount(
            RegressionEnvironment env,
            long expected)
        {
            env.AssertEqualsNew("s0", "cnt", expected);
        }

        private static void CreateSendEvent(
            RegressionEnvironment env,
            string typeName,
            string company,
            double value,
            double total)
        {
            var map = new LinkedHashMap<string, object>();
            map.Put("Company", company);
            map.Put("Value", value);
            map.Put("Total", total);
            if (EventRepresentationChoiceExtensions.GetEngineDefault(env.Configuration).IsObjectArrayEvent()) {
                env.SendEventObjectArray(map.Values.ToArray(), typeName);
            }
            else {
                env.SendEventMap(map, typeName);
            }
        }
    }
} // end of namespace