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
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestInvalidPattern 
    {
        private EPServiceProvider _epService;
        private readonly String EVENT_NUM = typeof(SupportBean_N).FullName;
        private readonly String EVENT_COMPLEX = typeof(SupportBeanComplexProps).FullName;
        private readonly String EVENT_ALLTYPES = typeof(SupportBean).FullName;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
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
            String exceptionText = GetSyntaxExceptionPattern(EVENT_NUM + "(DoublePrimitive='ss'");
            Assert.AreEqual("Incorrect syntax near end-of-input expecting a closing parenthesis ')' but found end-of-input at line 1 column 67, please check the filter specification within the pattern expression [com.espertech.esper.support.bean.SupportBean_N(DoublePrimitive='ss']", exceptionText);
    
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.CreateEPL("select * from pattern[(not a=SupportBean) -> SupportBean(TheString=a.TheString)]");
    
            // test invalid subselect
            _epService.EPAdministrator.CreateEPL("create window WaitWindow.win:keepall() as (waitTime int)");
            _epService.EPAdministrator.CreateEPL("insert into WaitWindow select IntPrimitive as waitTime from SupportBean");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
    
            try {
                _epService.EPAdministrator.CreatePattern("timer:interval((select waitTime from WaitWindow))");
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual("Subselects are not allowed within pattern observer parameters, please consider using a variable instead [timer:interval((select waitTime from WaitWindow))]",
                        ex.Message);
            }
        }
    
        [Test]
        public void TestStatementException()
        {
            String exceptionText;
    
            exceptionText = GetStatementExceptionPattern("timer:at(2,3,4,4,4)");
            Assert.AreEqual("Invalid parameter for pattern observer 'timer:at(2,3,4,4,4)': Error computing crontab schedule specification: Invalid combination between days of week and days of month fields for timer:at [timer:at(2,3,4,4,4)]", exceptionText);
    
            exceptionText = GetStatementExceptionPattern("timer:at(*,*,*,*,*,0,-1)");
            Assert.AreEqual("Invalid parameter for pattern observer 'timer:at(*,*,*,*,*,0,-1)': Error computing crontab schedule specification: Invalid timezone parameter '-1' for timer:at, expected a string-type value [timer:at(*,*,*,*,*,0,-1)]", exceptionText);
    
            exceptionText = GetStatementExceptionPattern(EVENT_ALLTYPES + " -> timer:within()");
            Assert.AreEqual("Failed to resolve pattern observer 'timer:within()': Pattern guard function 'within' cannot be used as a pattern observer [com.espertech.esper.support.bean.SupportBean -> timer:within()]", exceptionText);
    
            exceptionText = GetStatementExceptionPattern(EVENT_ALLTYPES + " where timer:interval(100)");
            Assert.AreEqual("Failed to resolve pattern guard 'com.espertech.esper.support.bean.SupportBean where timer:interval(100)': Pattern observer function 'interval' cannot be used as a pattern guard [com.espertech.esper.support.bean.SupportBean where timer:interval(100)]", exceptionText);
    
            exceptionText = GetStatementExceptionPattern(EVENT_ALLTYPES + " -> timer:interval()");
            Assert.AreEqual("Invalid parameter for pattern observer 'timer:interval()': Timer-interval observer requires a single numeric or time period parameter [com.espertech.esper.support.bean.SupportBean -> timer:interval()]", exceptionText);
    
            exceptionText = GetStatementExceptionPattern(EVENT_ALLTYPES + " where timer:within()");
            Assert.AreEqual("Invalid parameter for pattern guard 'com.espertech.esper.support.bean.SupportBean where timer:within()': Timer-within guard requires a single numeric or time period parameter [com.espertech.esper.support.bean.SupportBean where timer:within()]", exceptionText);
    
            // class not found
            exceptionText = GetStatementExceptionPattern("dummypkg.Dummy()");
            Assert.AreEqual("Failed to resolve event type: Event type or class named 'dummypkg.Dummy' was not found [dummypkg.Dummy()]", exceptionText);
    
            // simple property not found
            exceptionText = GetStatementExceptionPattern(EVENT_NUM + "(dummy=1)");
            Assert.AreEqual("Failed to validate filter expression 'dummy=1': Property named 'dummy' is not valid in any stream [com.espertech.esper.support.bean.SupportBean_N(dummy=1)]", exceptionText);
    
            // nested property not found
            exceptionText = GetStatementExceptionPattern(EVENT_NUM + "(dummy.nested=1)");
            Assert.AreEqual("Failed to validate filter expression 'dummy.nested=1': Failed to resolve property 'dummy.nested' to a stream or nested property in a stream [com.espertech.esper.support.bean.SupportBean_N(dummy.nested=1)]", exceptionText);
    
            // property wrong type
            exceptionText = GetStatementExceptionPattern(EVENT_NUM + "(IntPrimitive='s')");
            Assert.AreEqual("Failed to validate filter expression 'IntPrimitive=\"s\"': Implicit conversion from datatype 'System.String' to '" + Name.Of<int?>() + "' is not allowed [com.espertech.esper.support.bean.SupportBean_N(IntPrimitive='s')]", exceptionText);
    
            // property not a primitive type
            exceptionText = GetStatementExceptionPattern(EVENT_COMPLEX + "(nested=1)");
            Assert.AreEqual("Failed to validate filter expression 'nested=1': Implicit conversion from datatype '" + Name.Of<int?>() + "' to '" + Name.Of<SupportBeanComplexProps.SupportBeanSpecialGetterNested>() + "' is not allowed [com.espertech.esper.support.bean.SupportBeanComplexProps(nested=1)]", exceptionText);
    
            // no tag matches prior use
            exceptionText = GetStatementExceptionPattern(EVENT_NUM + "(DoublePrimitive=x.abc)");
            Assert.AreEqual("Failed to validate filter expression 'DoublePrimitive=x.abc': Failed to resolve property 'x.abc' to a stream or nested property in a stream [com.espertech.esper.support.bean.SupportBean_N(DoublePrimitive=x.abc)]", exceptionText);
    
            // range not valid on string
            exceptionText = GetStatementExceptionPattern(EVENT_ALLTYPES + "(TheString in [1:2])");
            Assert.AreEqual("Failed to validate filter expression 'TheString between 1 and 2': Implicit conversion from datatype 'System.String' to numeric is not allowed [com.espertech.esper.support.bean.SupportBean(TheString in [1:2])]", exceptionText);
    
            // range does not allow string params
            exceptionText = GetStatementExceptionPattern(EVENT_ALLTYPES + "(DoubleBoxed in ['a':2])");
            Assert.AreEqual("Failed to validate filter expression 'DoubleBoxed between \"a\" and 2': Implicit conversion from datatype 'System.String' to numeric is not allowed [com.espertech.esper.support.bean.SupportBean(DoubleBoxed in ['a':2])]", exceptionText);
    
            // invalid observer arg
            exceptionText = GetStatementExceptionPattern("timer:at(9l)");
            Assert.AreEqual("Invalid parameter for pattern observer 'timer:at(9L)': Invalid number of parameters for timer:at [timer:at(9l)]", exceptionText);
    
            // invalid guard arg
            exceptionText = GetStatementExceptionPattern(EVENT_ALLTYPES + " where timer:within('s')");
            Assert.AreEqual("Invalid parameter for pattern guard 'com.espertech.esper.support.bean.SupportBean where timer:within(\"s\")': Timer-within guard requires a single numeric or time period parameter [com.espertech.esper.support.bean.SupportBean where timer:within('s')]", exceptionText);
    
            // use-result property is wrong type
            exceptionText = GetStatementExceptionPattern("x=" + EVENT_ALLTYPES + " -> " + EVENT_ALLTYPES + "(DoublePrimitive=x.BoolBoxed)");
            Assert.AreEqual("Failed to validate filter expression 'DoublePrimitive=x.BoolBoxed': Implicit conversion from datatype '" + Name.Of<bool?>() + "' to '" + Name.Of<double?>() + "' is not allowed [x=com.espertech.esper.support.bean.SupportBean -> com.espertech.esper.support.bean.SupportBean(DoublePrimitive=x.BoolBoxed)]", exceptionText);

            // named-parameter for timer:at or timer:interval
            exceptionText = GetStatementExceptionPattern("timer:interval(interval:10)");
            Assert.AreEqual("Invalid parameter for pattern observer 'timer:interval(interval:10)': Timer-interval observer does not allow named parameters [timer:interval(interval:10)]", exceptionText);
            exceptionText = GetStatementExceptionPattern("timer:at(perhaps:10)");
            Assert.AreEqual("Invalid parameter for pattern observer 'timer:at(perhaps:10)': timer:at does not allow named parameters [timer:at(perhaps:10)]", exceptionText);
        }
    
        [Test]
        public void TestUseResult()
        {
            String EVENT = typeof(SupportBean_N).FullName;
    
            TryValid("na=" + EVENT + " -> nb=" + EVENT + "(DoublePrimitive = na.DoublePrimitive)");
            TryInvalid("xx=" + EVENT + " -> nb=" + EVENT + "(DoublePrimitive = na.DoublePrimitive)");
            TryInvalid("na=" + EVENT + " -> nb=" + EVENT + "(DoublePrimitive = xx.DoublePrimitive)");
            TryInvalid("na=" + EVENT + " -> nb=" + EVENT + "(DoublePrimitive = na.xx)");
            TryInvalid("xx=" + EVENT + " -> nb=" + EVENT + "(xx = na.DoublePrimitive)");
            TryInvalid("na=" + EVENT + " -> nb=" + EVENT + "(xx = na.xx)");
            TryValid("na=" + EVENT + " -> nb=" + EVENT + "(DoublePrimitive = na.DoublePrimitive, IntBoxed=na.IntBoxed)");
            TryValid("na=" + EVENT + "() -> nb=" + EVENT + "(DoublePrimitive in (na.DoublePrimitive:na.DoubleBoxed))");
            TryValid("na=" + EVENT + "() -> nb=" + EVENT + "(DoublePrimitive in [na.DoublePrimitive:na.DoubleBoxed])");
            TryValid("na=" + EVENT + "() -> nb=" + EVENT + "(DoublePrimitive in [na.IntBoxed:na.IntPrimitive])");
            TryInvalid("na=" + EVENT + "() -> nb=" + EVENT + "(DoublePrimitive in [na.IntBoxed:na.xx])");
            TryInvalid("na=" + EVENT + "() -> nb=" + EVENT + "(DoublePrimitive in [na.IntBoxed:na.BoolBoxed])");
            TryInvalid("na=" + EVENT + "() -> nb=" + EVENT + "(DoublePrimitive in [na.xx:na.IntPrimitive])");
            TryInvalid("na=" + EVENT + "() -> nb=" + EVENT + "(DoublePrimitive in [na.BoolBoxed:na.IntPrimitive])");
        }
    
        private void TryInvalid(String eplInvalidPattern)
        {
            try
            {
                _epService.EPAdministrator.CreatePattern(eplInvalidPattern);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                // Expected exception
            }
        }
    
        private String GetSyntaxExceptionPattern(String expression)
        {
            String exceptionText = null;
            try
            {
                _epService.EPAdministrator.CreatePattern(expression);
                Assert.Fail();
            }
            catch (EPStatementSyntaxException ex)
            {
                exceptionText = ex.Message;
                log.Debug(".getSyntaxExceptionPattern pattern=" + expression, ex);
                // Expected exception
            }
    
            return exceptionText;
        }
    
        private String GetStatementExceptionPattern(String expression)
        {
            return GetStatementExceptionPattern(expression, false);
        }
    
        private String GetStatementExceptionPattern(String expression, bool isLogException)
        {
            String exceptionText = null;
            try
            {
                _epService.EPAdministrator.CreatePattern(expression);
                Assert.Fail();
            }
            catch (EPStatementSyntaxException es)
            {
                throw es;
            }
            catch (EPStatementException ex)
            {
                // Expected exception
                exceptionText = ex.Message;
                if (isLogException)
                {
                    log.Debug(".getSyntaxExceptionPattern pattern=" + expression, ex);
                }
            }
    
            return exceptionText;
        }
    
        private void TryValid(String eplInvalidPattern)
        {
            _epService.EPAdministrator.CreatePattern(eplInvalidPattern);
        }
    
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
