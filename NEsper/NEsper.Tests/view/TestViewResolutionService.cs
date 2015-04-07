///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.spec;
using com.espertech.esper.support.view;
using com.espertech.esper.view.stat;

using com.espertech.esper.compat.logging;

using NUnit.Framework;



namespace com.espertech.esper.view
{
    [TestFixture]
    public class TestViewResolutionService 
    {
        private ViewResolutionService service;
    
        [SetUp]
        public void SetUp()
        {
            PluggableObjectRegistryImpl registry = new PluggableObjectRegistryImpl(new PluggableObjectCollection[] {ViewEnumHelper.BuiltinViews});
            service = new ViewResolutionServiceImpl(registry, null, null);
        }
    
        [Test]
        public void TestInitializeFromConfig()
        {
            service = CreateService(new String[] {"a", "b"}, new String[] {"v1", "v2"},
                    new String[] {typeof(SupportViewFactoryOne).FullName, typeof(SupportViewFactoryTwo).FullName});
    
            ViewFactory factory = service.Create("a", "v1");
            Assert.IsTrue(factory is SupportViewFactoryOne);
    
            factory = service.Create("b", "v2");
            Assert.IsTrue(factory is SupportViewFactoryTwo);
    
            TryInvalid("a", "v3");
            TryInvalid("c", "v1");
    
            try
            {
                service = CreateService(new String[] {"a"}, new String[] {"v1"}, new String[] {"abc"});
                Assert.Fail();
            }
            catch (ConfigurationException ex)
            {
                // expected
            }
        }
    
        private void TryInvalid(String @namespace, String name)
        {
            try
            {
                service.Create(@namespace, name);
                Assert.Fail();
            }
            catch (ViewProcessingException ex)
            {
                // expected
            }
        }
        
        [Test]
        public void TestCreate()
        {
            ViewFactory viewFactory = service.Create(ViewEnum.UNIVARIATE_STATISTICS.GetNamespace(), ViewEnum.UNIVARIATE_STATISTICS.GetName());
            Assert.IsTrue(viewFactory is UnivariateStatisticsViewFactory);
        }
    
        [Test]
        public void TestInvalidViewName()
        {
            try
            {
                service.Create("dummy", "bumblebee");
                Assert.IsFalse(true);
            }
            catch (ViewProcessingException ex)
            {
                log.Debug(".testInvalidViewName Expected exception caught, msg=" + ex.Message);
            }
        }
    
        private ViewResolutionService CreateService(String[] namespaces, String[] names, String[] classNames)
        {
            List<ConfigurationPlugInView> configs = new List<ConfigurationPlugInView>();
            for (int i = 0; i < namespaces.Length; i++)
            {
                ConfigurationPlugInView config = new ConfigurationPlugInView();
                config.Namespace = namespaces[i];
                config.Name = names[i];
                config.FactoryClassName = classNames[i];
                configs.Add(config);
            }
    
            PluggableObjectCollection desc = new PluggableObjectCollection();
            desc.AddViews(configs, Collections.GetEmptyList<ConfigurationPlugInVirtualDataWindow>());
            PluggableObjectRegistryImpl registry = new PluggableObjectRegistryImpl(new PluggableObjectCollection[] {desc});
            return new ViewResolutionServiceImpl(registry, null, null);
        }
    
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
