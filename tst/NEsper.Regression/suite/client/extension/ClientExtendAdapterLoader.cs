///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.extension
{
    public class ClientExtendAdapterLoader : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Assure destroy order ESPER-489
            Assert.AreEqual(2, SupportPluginLoader.Names.Count);
            Assert.AreEqual(2, SupportPluginLoader.PostInitializes.Count);
            Assert.AreEqual("MyLoader", SupportPluginLoader.Names[0]);
            Assert.AreEqual("MyLoader2", SupportPluginLoader.Names[1]);
            Assert.AreEqual("val", SupportPluginLoader.Props[0].Get("name"));
            Assert.AreEqual("val2", SupportPluginLoader.Props[1].Get("name2"));

            var loader = GetFromEnv(env, "plugin-loader/MyLoader");
            Assert.IsTrue(loader is SupportPluginLoader);
            loader = GetFromEnv(env, "plugin-loader/MyLoader2");
            Assert.IsTrue(loader is SupportPluginLoader);

            SupportPluginLoader.PostInitializes.Clear();
            SupportPluginLoader.Names.Clear();
            env.Runtime.Initialize();
            Assert.AreEqual(2, SupportPluginLoader.PostInitializes.Count);
            Assert.AreEqual(2, SupportPluginLoader.Names.Count);

            env.Runtime.Destroy();
            Assert.AreEqual(2, SupportPluginLoader.Destroys.Count);
            Assert.AreEqual("val2", SupportPluginLoader.Destroys[0].Get("name2"));
            Assert.AreEqual("val", SupportPluginLoader.Destroys[1].Get("name"));

            SupportPluginLoader.Reset();
        }

        private object GetFromEnv(
            RegressionEnvironment env,
            string name)
        {
            try {
                return env.Runtime.Context.Lookup(name);
            }
            catch (Exception t) {
                Assert.Fail(t.Message);
                throw new EPException(t);
            }
        }
    }
} // end of namespace