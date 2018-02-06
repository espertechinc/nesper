///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    [TestFixture]
    public class TestDatabaseTimeout
    {
        private EPServiceProvider _epService;

        [SetUp]
        public void SetUp()
        {
            var configDB = new ConfigurationDBRef();
            configDB.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative);
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;

            Configuration configuration = new Configuration();
            configuration.AddDatabaseReference("MyDB", configDB);
            configuration.EngineDefaults.Threading.IsInternalTimerEnabled = false;
            configuration.EngineDefaults.ExceptionHandling.AddClass<MyExceptionHandlerFactory>();

            _epService = EPServiceProviderManager.GetProvider("TestDatabaseTimeout", configuration);
            _epService.Initialize();
        }

        [Test]
        public void TestInsideTimeout()
        {
#if MSSQL
            string stmtText = "@SQLTimeout(10) " +
                              "select myint from " +
                              " sql:MyDB ['exec [dbo].[spDelayTest] @timeout = \"00:00:05\", @testValue = ${s1.IntPrimitive}'] as s0," +
                              typeof(SupportBean).FullName + " as s1";
#elif MYSQL
            string stmtText = "@SQLTimeout(10) " +
                              "select myint from " +
                              " sql:MyDB ['call spDelayTest(5, ${s1.IntPrimitive})'] as s0," +
                              typeof(SupportBean).FullName + " as s1";
#endif

            var statement = _epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("", 1));
        }

        [Test]
        public void TestOutsideTimeout()
        {
#if MSSQL
            string stmtText = "@SQLTimeout(1) " +
                  "select myint from " +
                  " sql:MyDB ['exec [dbo].[spDelayTest] @timeout = \"00:00:05\", @testValue = ${s1.IntPrimitive}'] as s0," +
                  typeof(SupportBean).FullName + " as s1";
#elif MYSQL
            string stmtText = "@SQLTimeout(1) " +
                  "select myint from " +
                  " sql:MyDB ['call spDelayTest(5, ${s1.IntPrimitive})'] as s0," +
                  typeof(SupportBean).FullName + " as s1";
#endif

            var statement = _epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;

            MyExceptionHandlerFactory.ContextList.Clear();
            Assert.That(MyExceptionHandlerFactory.ContextList.Count, Is.EqualTo(0));

            _epService.EPRuntime.SendEvent(new SupportBean("", 1));

            Assert.That(
                MyExceptionHandlerFactory.ContextList.Count, 
                Is.EqualTo(1));
            Assert.That(
                MyExceptionHandlerFactory.ContextList[0].Exception,
                Is.Not.Null);
#if MSSQL
            Assert.That(
                MyExceptionHandlerFactory.ContextList[0].Exception.Message,
                Is.EqualTo("Error executing statement 'exec [dbo].[spDelayTest] @timeout = \"00:00:05\", @testValue = @arg0'"));
#elif MYSQL
            Assert.That(
                MyExceptionHandlerFactory.ContextList[0].Exception.Message,
                Is.EqualTo("Error executing statement 'call spDelayTest(5, ?arg0)'"));
#endif
            Assert.That(
                MyExceptionHandlerFactory.ContextList[0].Exception.InnerException,
                Is.Not.Null);
            Assert.That(
                MyExceptionHandlerFactory.ContextList[0].Exception.InnerException.Message,
                Is.EqualTo("Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding."));
        }

        internal class MyExceptionHandlerFactory 
            : ExceptionHandlerFactory
        {
            /// <summary>
            /// Gets or sets the context list.
            /// </summary>
            /// <value>The context list.</value>
            public static IList<ExceptionHandlerContext> ContextList { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="MyExceptionHandlerFactory"/> class.
            /// </summary>
            public MyExceptionHandlerFactory()
            {
                ContextList = new List<ExceptionHandlerContext>();
            }

            /// <summary>
            /// Handles the exception.
            /// </summary>
            /// <param name="context">The context.</param>
            public void HandleException(ExceptionHandlerContext context)
            {
                ContextList.Add(context);
            }
            
            /// <summary>
            /// Returns an exception handler instances, or null if the factory decided not to contribute an exception handler.
            /// </summary>
            /// <param name="context">contains the engine URI</param>
            /// <returns>exception handler</returns>
            public ExceptionHandler GetHandler(ExceptionHandlerFactoryContext context)
            {
                return HandleException;
            }
        }
    }
}
