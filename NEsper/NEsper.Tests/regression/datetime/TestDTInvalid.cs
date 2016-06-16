///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.datetime
{
    [TestFixture]
    public class TestDTInvalid
    {
        private EPServiceProvider _epService;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            config.AddEventType("SupportBean_ST0_Container", typeof(SupportBean_ST0_Container));
            config.AddEventType("SupportDateTime", typeof(SupportDateTime));
            config.AddImport(typeof(SupportBean_ST0_Container));
            config.AddPlugInSingleRowFunction("makeTest", typeof(SupportBean_ST0_Container).FullName, "makeTest");
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestInvalid()
        {
            String epl;

            // invalid incompatible params
            epl = "select contained.set('hour', 1) from SupportBean_ST0_Container";
            TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'contained.set(\"hour\",1)': Date-time enumeration method 'set' requires either a DateTime or long value as input or events of an event type that declares a timestamp property but received collection of events of type 'com.espertech.esper.support.bean.SupportBean_ST0' [select contained.set('hour', 1) from SupportBean_ST0_Container]");

            // invalid incompatible params
            epl = "select window(*).set('hour', 1) from SupportBean.win:keepall()";
            TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'window(*).set(\"hour\",1)': Date-time enumeration method 'set' requires either a DateTime or long value as input or events of an event type that declares a timestamp property but received collection of events of type 'SupportBean' [select window(*).set('hour', 1) from SupportBean.win:keepall()]");

            // invalid incompatible params
            epl = "select Utildate.set('invalid') from SupportDateTime";
            TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'Utildate.set(\"invalid\")': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value [select Utildate.set('invalid') from SupportDateTime]");

            // invalid lambda parameter
            epl = "select Utildate.set(x => true) from SupportDateTime";
            TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'Utildate.set()': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value [select Utildate.set(x => true) from SupportDateTime]");

            // invalid no parameter
            epl = "select Utildate.set() from SupportDateTime";
            TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'Utildate.set()': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value [select Utildate.set() from SupportDateTime]");

            // invalid wrong parameter
            epl = "select Utildate.set(1) from SupportDateTime";
            TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'Utildate.set(1)': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value [select Utildate.set(1) from SupportDateTime]");

            // invalid wrong parameter
            epl = "select Utildate.between('a', 'b') from SupportDateTime";
            TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'Utildate.between(\"a\",\"b\")': Error validating date-time method 'between', expected a long-typed or DateTime-typed result for expression parameter 0 but received System.String [select Utildate.between('a', 'b') from SupportDateTime]");

            // invalid wrong parameter
            epl = "select Utildate.between(Utildate, Utildate, 1, true) from SupportDateTime";
            TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'Utildate.between(Utildate,Utildate,...(42 chars)': Error validating date-time method 'between', expected a boolean-type result for expression parameter 2 but received " + Name.Of<int>() + " [select Utildate.between(Utildate, Utildate, 1, true) from SupportDateTime]");
        }

        private void TryInvalid(String epl, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }
    }
}
