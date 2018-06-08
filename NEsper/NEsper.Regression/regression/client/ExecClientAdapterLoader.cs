///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.plugin;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientAdapterLoader : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionAdapterLoader();
            RunAssertionDestroyObtainTwice();
        }
    
        private void RunAssertionAdapterLoader() {
            SupportPluginLoader.Reset();
            // Assure destroy order ESPER-489
    
            var props = new Properties();
            props.Put("name", "val");
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddPluginLoader("MyLoader", typeof(SupportPluginLoader).FullName, props);
    
            props = new Properties();
            props.Put("name2", "val2");
            config.AddPluginLoader("MyLoader2", typeof(SupportPluginLoader).FullName, props);
    
            EPServiceProvider service = EPServiceProviderManager.GetProvider(
                SupportContainer.Instance, "ExecClientAdapterLoader", config);
            Assert.AreEqual(2, SupportPluginLoader.Names.Count);
            Assert.AreEqual(2, SupportPluginLoader.PostInitializes.Count);
            Assert.AreEqual("MyLoader", SupportPluginLoader.Names[0]);
            Assert.AreEqual("MyLoader2", SupportPluginLoader.Names[1]);
            Assert.AreEqual("val", SupportPluginLoader.Props[0].Get("name"));
            Assert.AreEqual("val2", SupportPluginLoader.Props[1].Get("name2"));
    
            Object loader = service.Directory.Lookup("plugin-loader/MyLoader");
            Assert.IsTrue(loader is SupportPluginLoader);
            loader = service.Directory.Lookup("plugin-loader/MyLoader2");
            Assert.IsTrue(loader is SupportPluginLoader);
    
            SupportPluginLoader.PostInitializes.Clear();
            SupportPluginLoader.Names.Clear();
            service.Initialize();
            Assert.AreEqual(2, SupportPluginLoader.PostInitializes.Count);
            Assert.AreEqual(2, SupportPluginLoader.Names.Count);
    
            service.Dispose();
            Assert.AreEqual(2, SupportPluginLoader.Destroys.Count);
            Assert.AreEqual("val2", SupportPluginLoader.Destroys[0].Get("name2"));
            Assert.AreEqual("val", SupportPluginLoader.Destroys[1].Get("name"));
        }
    
        private void RunAssertionDestroyObtainTwice() {
            SupportPluginLoader.Reset();
    
            Configuration cf = SupportConfigFactory.GetConfiguration();
            cf.AddPluginLoader("AP", typeof(SupportPluginLoader).FullName, null);
            EPServiceProviderManager.GetProvider(SupportContainer.Instance, "ExecClientAdapterLoader", cf);
            EPServiceProvider ep = EPServiceProviderManager.GetProvider("ExecClientAdapterLoader");
            ep.Dispose();
            Assert.AreEqual(1, SupportPluginLoader.Destroys.Count);
        }
    }
} // end of namespace
