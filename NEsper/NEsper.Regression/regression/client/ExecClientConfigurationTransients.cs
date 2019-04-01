///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientConfigurationTransients : RegressionExecution {
        private const string SERVICE_NAME = "TEST_SERVICE_NAME";
        private static readonly int SECRET_VALUE = 12345;
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
    
            // add service (not serializable, transient configuration)
            var transients = new Dictionary<string, Object>();
            transients.Put(SERVICE_NAME, new MyLocalService(SECRET_VALUE));
            configuration.TransientConfiguration = transients;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionConfigAvailable(epService);
            RunAssertionClassForNameForbiddenClass(epService);
            RunAssertionClassLoader(epService);
        }
    
        private void RunAssertionConfigAvailable(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean");
            var listener = new MyListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(SECRET_VALUE, listener.SecretValue);
    
            stmt.Dispose();
        }
    
        private void RunAssertionClassForNameForbiddenClass(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            string epl = "select System.Environment.Exit(-1) from SupportBean";
    
            epService.EPAdministrator.Configuration.TransientConfiguration.Put(ClassForNameProviderConstants.NAME, new MyClassForNameProvider());
            SupportMessageAssertUtil.TryInvalid(epService, epl,
                    "Error starting statement: Failed to validate select-clause expression 'System.Environment.Exit(-1)': Failed to resolve 'System.Environment.Exit' to");
    
            epService.EPAdministrator.Configuration.TransientConfiguration.Put(ClassForNameProviderConstants.NAME, ClassForNameProviderDefault.INSTANCE);
            epService.EPAdministrator.CreateEPL(epl);
        }
    
        private void RunAssertionClassLoader(EPServiceProvider epService) {
            ConfigurationOperations ops = epService.EPAdministrator.Configuration;
    
            MyClassLoaderProvider.IsInvoked = false;
            MyFastClassClassLoaderProvider.Clazz = null;
            ops.TransientConfiguration.Put(ClassLoaderProviderConstants.NAME, new MyClassLoaderProvider());
            ops.TransientConfiguration.Put(FastClassClassLoaderProviderConstants.NAME, new MyFastClassClassLoaderProvider());
            ops.AddImport(typeof(MyAnnotationSimpleAttribute));
            ops.AddEventType<SupportBean>();
    
            string epl = "@MyAnnotationSimple select System.Environment.Exit(-1) from SupportBean";
            epService.EPAdministrator.CreateEPL(epl);
    
            Assert.IsTrue(MyClassLoaderProvider.IsInvoked);
            //Assert.AreEqual(typeof(System), MyFastClassClassLoaderProvider.Clazz);
        }
    
        public class MyLocalService {
            private readonly int _secretValue;

            public int SecretValue => _secretValue;
            public MyLocalService(int secretValue) {
                this._secretValue = secretValue;
            }
        }

#pragma warning disable 612
        public class MyListener : StatementAwareUpdateListener
#pragma warning restore 612
        {
            private int _secretValue;

            public int SecretValue => _secretValue;

            public void Update(object sender, UpdateEventArgs args)
            {
                Update(
                    args.NewEvents,
                    args.OldEvents,
                    args.Statement,
                    args.ServiceProvider);
            }

            public void Update(
                EventBean[] newEvents,
                EventBean[] oldEvents,
                EPStatement statement,
                EPServiceProvider svcProvider)
            {
                var svc =
                    (MyLocalService) svcProvider.EPAdministrator.Configuration.TransientConfiguration.Get(SERVICE_NAME);
                _secretValue = svc.SecretValue;
            }
        }

        private class MyClassForNameProvider : ClassForNameProvider
        {
            public Type ClassForName(string className)
            {
                if (className.Equals("System.Environment"))
                {
                    throw new UnsupportedOperationException("Access to class '" + className + " is not permitted");
                }

                return ClassForNameProviderDefault.INSTANCE.ClassForName(className);
            }
        }

        public class MyClassLoaderProvider : ClassLoaderProvider
        {
            public static bool IsInvoked { get; set; }
            public static void SetInvoked(bool invoked)
            {
                MyClassLoaderProvider.IsInvoked = invoked;
            }

            public ClassLoader GetClassLoader()
            {
                IsInvoked = true;
                return null;
            }
        }

        public class MyFastClassClassLoaderProvider : FastClassClassLoaderProvider
        {
            public static Type Clazz { get; set; }
            public ClassLoader Classloader(Type clazz) {
                MyFastClassClassLoaderProvider.Clazz = clazz;
                return null;
            }
        }
    }
} // end of namespace
