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
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
	public class TestTransientConfiguration  {
	    private const string SERVICE_NAME = "TEST_SERVICE_NAME";
	    private const int SECRET_VALUE = 12345;

        [Test]
	    public void TestConfigAvailable() {
	        Configuration configuration = SupportConfigFactory.GetConfiguration();
	        configuration.AddEventType(typeof(SupportBean));

	        // add service (not serializable, transient configuration)
	        Dictionary<string, object> transients = new Dictionary<string, object>();
	        transients.Put(SERVICE_NAME, new MyLocalService(SECRET_VALUE));
	        configuration.TransientConfiguration = transients;

	        EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(epService, this.GetType(), this.GetType().FullName);
	        }

	        EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean");
	        MyListener listener = new MyListener();
            stmt.Events += (sender, e) => listener.Update(e.NewEvents, e.OldEvents, e.Statement, e.ServiceProvider);

	        epService.EPRuntime.SendEvent(new SupportBean());
	        Assert.AreEqual(SECRET_VALUE, listener.SecretValue);

	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	    }

        [Test]
	    public void TestClassForNameForbiddenClass() {

	        EPServiceProvider epService = GetDefaultEngine();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(epService, this.GetType(), this.GetType().FullName);
	        }

	        epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));

	        string epl = "select System.Environment.Exit(-1) from SupportBean";

	        epService.EPAdministrator.Configuration.TransientConfiguration.Put(ClassForNameProviderConstants.NAME, new MyClassForNameProvider());
	        SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'System.Environment.Exit(-1)': Failed to resolve 'System.Environment.Exit' to");

	        epService.EPAdministrator.Configuration.TransientConfiguration.Put(ClassForNameProviderConstants.NAME, ClassForNameProviderDefault.INSTANCE);
	        epService.EPAdministrator.CreateEPL(epl);

	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	    }

        [Test]
	    public void TestClassLoader() {
	        EPServiceProvider epService = GetDefaultEngine();
	        ConfigurationOperations ops = epService.EPAdministrator.Configuration;
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(epService, this.GetType(), this.GetType().FullName);
	        }

	        MyClassLoaderProvider.SetInvoked(false);
	        MyFastClassClassLoaderProvider.Clazz = null;
	        ops.TransientConfiguration.Put(ClassLoaderProviderConstants.NAME, new MyClassLoaderProvider());
	        ops.TransientConfiguration.Put(FastClassClassLoaderProviderConstants.NAME, new MyFastClassClassLoaderProvider());
	        ops.AddImport(typeof(MyAnnotationSimpleAttribute));
	        ops.AddEventType(typeof(SupportBean));

            string epl = "@MyAnnotationSimple select System.Environment.Exit(-1) from SupportBean";
	        epService.EPAdministrator.CreateEPL(epl);

	        Assert.IsTrue(MyClassLoaderProvider.IsInvoked);
	        //Assert.AreEqual(typeof(System.Console), MyFastClassClassLoaderProvider.Clazz);

	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	    }

	    private EPServiceProvider GetDefaultEngine() {
	        Configuration configuration = SupportConfigFactory.GetConfiguration();
	        EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        epService.Initialize();
	        return epService;
	    }

	    public class MyLocalService
        {
	        public MyLocalService(int secretValue) {
	            this.SecretValue = secretValue;
	        }

	        public int SecretValue { get; private set; }
	    }

	    public class MyListener : StatementAwareUpdateListener {
	        private int _secretValue;

	        public void Update(EventBean[] newEvents, EventBean[] oldEvents, EPStatement statement, EPServiceProvider epServiceProvider) {
	            MyLocalService svc = (MyLocalService) epServiceProvider.EPAdministrator.Configuration.TransientConfiguration.Get(SERVICE_NAME);
	            _secretValue = svc.SecretValue;
	        }

	        public int SecretValue
	        {
	            get { return _secretValue; }
	        }
	    }

	    private class MyClassForNameProvider : ClassForNameProvider {
	        public Type ClassForName(string className) {
	            if (className.Equals("System.Environment")) {
	                throw new UnsupportedOperationException("Access to class '" + className + " is not permitted");
	            }
	            return ClassForNameProviderDefault.INSTANCE.ClassForName(className);
	        }
	    }

	    public class MyClassLoaderProvider : ClassLoaderProvider {
	        public ClassLoader GetClassLoader() {
	            IsInvoked = true;
	            return null;
                //return Thread.CurrentThread().ContextClassLoader;
	        }

	        public static bool IsInvoked { get; private set; }

	        public static void SetInvoked(bool invoked) {
	            MyClassLoaderProvider.IsInvoked = invoked;
	        }
	    }

	    public class MyFastClassClassLoaderProvider : FastClassClassLoaderProvider {
	        private static Type clazz;

	        public ClassLoader Classloader(Type clazz) {
	            MyFastClassClassLoaderProvider.clazz = clazz;
	            return null;
	            //return Thread.CurrentThread().ContextClassLoader;
	        }

	        public static Type Clazz
	        {
	            get { return clazz; }
	            set { MyFastClassClassLoaderProvider.clazz = value; }
	        }
	    }
	}
} // end of namespace
