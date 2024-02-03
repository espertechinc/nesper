///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.client.extension
{
    public class ClientExtendAdapterLoader : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Assure destroy order ESPER-489
            ClassicAssert.AreEqual(2, SupportPluginLoader.Names.Count);
            ClassicAssert.AreEqual(2, SupportPluginLoader.PostInitializes.Count);
            ClassicAssert.AreEqual("MyLoader", SupportPluginLoader.Names[0]);
            ClassicAssert.AreEqual("MyLoader2", SupportPluginLoader.Names[1]);
            ClassicAssert.AreEqual("val", SupportPluginLoader.Props[0].Get("name"));
            ClassicAssert.AreEqual("val2", SupportPluginLoader.Props[1].Get("name2"));

            var loader = GetFromEnv(env, "plugin-loader/MyLoader");
            ClassicAssert.IsTrue(loader is SupportPluginLoader);
            loader = GetFromEnv(env, "plugin-loader/MyLoader2");
            ClassicAssert.IsTrue(loader is SupportPluginLoader);

            SupportPluginLoader.PostInitializes.Clear();
            SupportPluginLoader.Names.Clear();
            env.Runtime.Initialize();
            ClassicAssert.AreEqual(2, SupportPluginLoader.PostInitializes.Count);
            ClassicAssert.AreEqual(2, SupportPluginLoader.Names.Count);

            env.Runtime.Destroy();
            ClassicAssert.AreEqual(2, SupportPluginLoader.Destroys.Count);
            ClassicAssert.AreEqual("val2", SupportPluginLoader.Destroys[0].Get("name2"));
            ClassicAssert.AreEqual("val", SupportPluginLoader.Destroys[1].Get("name"));

            SupportPluginLoader.Reset();
        }

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.STATICHOOK);
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