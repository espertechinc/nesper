///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.spec;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.supportunit.view;
using com.espertech.esper.view.stat;

using NUnit.Framework;

namespace com.espertech.esper.view
{
    [TestFixture]
    public class TestViewResolutionService 
    {
        private ViewResolutionService _service;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            var registry = new PluggableObjectRegistryImpl(new[] {ViewEnumHelper.BuiltinViews});
            _service = new ViewResolutionServiceImpl(registry, null, null);
        }
    
        [Test]
        public void TestInitializeFromConfig()
        {
            _service = CreateService(
                new[] {"a", "b"},
                new[] {"v1", "v2"},
                new[] {typeof(SupportViewFactoryOne).FullName, typeof(SupportViewFactoryTwo).FullName});
    
            var factory = _service.Create(_container, "a", "v1");
            Assert.IsTrue(factory is SupportViewFactoryOne);
    
            factory = _service.Create(_container, "b", "v2");
            Assert.IsTrue(factory is SupportViewFactoryTwo);
    
            TryInvalid("a", "v3");
            TryInvalid("c", "v1");
    
            try
            {
                _service = CreateService(new[] {"a"}, new[] {"v1"}, new[] {"abc"});
                Assert.Fail();
            }
            catch (ConfigurationException)
            {
                // expected
            }
        }
    
        private void TryInvalid(string @namespace, string name)
        {
            try
            {
                _service.Create(_container, @namespace, name);
                Assert.Fail();
            }
            catch (ViewProcessingException)
            {
                // expected
            }
        }
        
        [Test]
        public void TestCreate()
        {
            var viewFactory = _service.Create(_container, ViewEnum.UNIVARIATE_STATISTICS.GetNamespace(), ViewEnum.UNIVARIATE_STATISTICS.GetName());
            Assert.IsTrue(viewFactory is UnivariateStatisticsViewFactory);
        }
    
        [Test]
        public void TestInvalidViewName()
        {
            try
            {
                _service.Create(_container, "dummy", "bumblebee");
                Assert.IsFalse(true);
            }
            catch (ViewProcessingException ex)
            {
                Log.Debug(".testInvalidViewName Expected exception caught, msg=" + ex.Message);
            }
        }
    
        private ViewResolutionService CreateService(string[] namespaces, string[] names, string[] classNames)
        {
            var configs = new List<ConfigurationPlugInView>();
            for (var i = 0; i < namespaces.Length; i++)
            {
                var config = new ConfigurationPlugInView();
                config.Namespace = namespaces[i];
                config.Name = names[i];
                config.FactoryClassName = classNames[i];
                configs.Add(config);
            }
    
            var desc = new PluggableObjectCollection();
            desc.AddViews(configs,
                Collections.GetEmptyList<ConfigurationPlugInVirtualDataWindow>(), 
                SupportEngineImportServiceFactory.Make(_container));
            var registry = new PluggableObjectRegistryImpl(new[]{desc});
            return new ViewResolutionServiceImpl(registry, null, null);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
