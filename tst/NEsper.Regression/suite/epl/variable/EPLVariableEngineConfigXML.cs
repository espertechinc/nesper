///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.variable
{
    public class EPLVariableEngineConfigXML : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var stmtTextSet =
                "@name('set') on SupportBean set p_1 = TheString, p_2 = BoolBoxed, p_3 = IntBoxed, p_4 = IntBoxed";
            env.CompileDeploy(stmtTextSet).AddListener("set");
            string[] fieldsVar = { "p_1", "p_2", "p_3", "p_4" };
            env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { null, true, 10L, 11.1d } });

            env.AssertStatement(
                "set",
                statement => {
                    var typeSet = statement.EventType;

                    Assert.AreEqual(typeof(string), typeSet.GetPropertyType("p_1"));
                    Assert.AreEqual(typeof(bool?), typeSet.GetPropertyType("p_2"));
                    Assert.AreEqual(typeof(long?), typeSet.GetPropertyType("p_3"));
                    Assert.AreEqual(typeof(double?), typeSet.GetPropertyType("p_4"));
                    Array.Sort(typeSet.PropertyNames);
                    CollectionAssert.AreEquivalent(fieldsVar, typeSet.PropertyNames);
                });

            var bean = new SupportBean();
            bean.TheString = "text";
            bean.BoolBoxed = false;
            bean.IntBoxed = 200;
            env.SendEventBean(bean);
            env.AssertPropsNew("set", fieldsVar, new object[] { "text", false, 200L, 200d });
            env.AssertPropsPerRowIterator(
                "set",
                fieldsVar,
                new object[][] { new object[] { "text", false, 200L, 200d } });

            bean = new SupportBean(); // leave all fields null
            env.SendEventBean(bean);
            env.AssertPropsNew("set", fieldsVar, new object[] { null, null, null, null });
            env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { null, null, null, null } });

            env.UndeployAll();
        }
    }
} // end of namespace