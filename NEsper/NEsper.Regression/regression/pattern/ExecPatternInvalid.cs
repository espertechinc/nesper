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
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

// using static org.junit.Assert.assertEquals;
// using static org.junit.Assert.fail;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternInvalid : RegressionExecution {
        private static readonly string EVENT_NUM = typeof(SupportBean_N).Name;
        private static readonly string EVENT_COMPLEX = typeof(SupportBeanComplexProps).Name;
        private static readonly string EVENT_ALLTYPES = typeof(SupportBean).FullName;
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionInvalid(epService);
            RunAssertionStatementException(epService);
            RunAssertionUseResult(epService);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string exceptionText = GetSyntaxExceptionPattern(epService, EVENT_NUM + "(doublePrimitive='ss'");
            Assert.AreEqual("Incorrect syntax near end-of-input expecting a closing parenthesis ')' but found end-of-input at line 1 column 77, please check the filter specification within the pattern expression [" + typeof(SupportBean_N).Name + "(doublePrimitive='ss']", exceptionText);
    
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.CreateEPL("select * from pattern[(not a=SupportBean) -> SupportBean(theString=a.theString)]");
    
            // test invalid subselect
            epService.EPAdministrator.CreateEPL("create window WaitWindow#keepall as (waitTime int)");
            epService.EPAdministrator.CreateEPL("insert into WaitWindow select intPrimitive as waitTime from SupportBean");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
    
            try {
                epService.EPAdministrator.CreatePattern("timer:Interval((select waitTime from WaitWindow))");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Subselects are not allowed within pattern observer parameters, please consider using a variable instead [timer:Interval((select waitTime from WaitWindow))]",
                        ex.Message);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionStatementException(EPServiceProvider epService) {
            EPStatementException exception;
    
            exception = GetStatementExceptionPattern(epService, "timer:At(2,3,4,4,4)");
            SupportMessageAssertUtil.AssertMessage(exception, "Invalid parameter for pattern observer 'timer:At(2,3,4,4,4)': Error computing crontab schedule specification: Invalid combination between days of week and days of month fields for timer:at [");
    
            exception = GetStatementExceptionPattern(epService, "timer:At(*,*,*,*,*,0,-1)");
            SupportMessageAssertUtil.AssertMessage(exception, "Invalid parameter for pattern observer 'timer:At(*,*,*,*,*,0,-1)': Error computing crontab schedule specification: Invalid timezone parameter '-1' for timer:at, expected a string-type value [");
    
            exception = GetStatementExceptionPattern(epService, EVENT_ALLTYPES + " -> timer:Within()");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to resolve pattern observer 'timer:Within()': Pattern guard function 'within' cannot be used as a pattern observer [");
    
            exception = GetStatementExceptionPattern(epService, EVENT_ALLTYPES + " where timer:Interval(100)");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to resolve pattern guard '" + typeof(SupportBean).FullName + " where timer:Interval(100)': Pattern observer function 'interval' cannot be used as a pattern guard [");
    
            exception = GetStatementExceptionPattern(epService, EVENT_ALLTYPES + " -> timer:Interval()");
            SupportMessageAssertUtil.AssertMessage(exception, "Invalid parameter for pattern observer 'timer:Interval()': Timer-interval observer requires a single numeric or time period parameter [");
    
            exception = GetStatementExceptionPattern(epService, EVENT_ALLTYPES + " where timer:Within()");
            SupportMessageAssertUtil.AssertMessage(exception, "Invalid parameter for pattern guard '" + typeof(SupportBean).FullName + " where timer:Within()': Timer-within guard requires a single numeric or time period parameter [");
    
            // class not found
            exception = GetStatementExceptionPattern(epService, "dummypkg.Dummy()");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to resolve event type: Event type or class named 'dummypkg.dummy' was not found [");
    
            // simple property not found
            exception = GetStatementExceptionPattern(epService, EVENT_NUM + "(dummy=1)");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to validate filter expression 'dummy=1': Property named 'dummy' is not valid in any stream [");
    
            // nested property not found
            exception = GetStatementExceptionPattern(epService, EVENT_NUM + "(dummy.nested=1)");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to validate filter expression 'dummy.nested=1': Failed to resolve property 'dummy.nested' to a stream or nested property in a stream [");
    
            // property wrong type
            exception = GetStatementExceptionPattern(epService, EVENT_NUM + "(intPrimitive='s')");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to validate filter expression 'intPrimitive=\"s\"': Implicit conversion from datatype 'string' to 'int?' is not allowed [");
    
            // property not a primitive type
            exception = GetStatementExceptionPattern(epService, EVENT_COMPLEX + "(nested=1)");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to validate filter expression 'nested=1': Implicit conversion from datatype 'int?' to 'SupportBeanSpecialGetterNested' is not allowed [");
    
            // no tag matches prior use
            exception = GetStatementExceptionPattern(epService, EVENT_NUM + "(doublePrimitive=x.abc)");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to validate filter expression 'doublePrimitive=x.abc': Failed to resolve property 'x.abc' to a stream or nested property in a stream [");
    
            // range not valid on string
            exception = GetStatementExceptionPattern(epService, EVENT_ALLTYPES + "(theString in [1:2])");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to validate filter expression 'theString between 1 and 2': Implicit conversion from datatype 'string' to numeric is not allowed [");
    
            // range does not allow string params
            exception = GetStatementExceptionPattern(epService, EVENT_ALLTYPES + "(doubleBoxed in ['a':2])");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to validate filter expression 'doubleBoxed between \"a\" and 2': Implicit conversion from datatype 'string' to numeric is not allowed [");
    
            // invalid observer arg
            exception = GetStatementExceptionPattern(epService, "timer:At(9l)");
            SupportMessageAssertUtil.AssertMessage(exception, "Invalid parameter for pattern observer 'timer:At(9)': Invalid number of parameters for timer:at [timer:At(9l)]");
    
            // invalid guard arg
            exception = GetStatementExceptionPattern(epService, EVENT_ALLTYPES + " where timer:Within('s')");
            SupportMessageAssertUtil.AssertMessage(exception, "Invalid parameter for pattern guard '" + typeof(SupportBean).FullName + " where timer:Within(\"s\")': Timer-within guard requires a single numeric or time period parameter [");
    
            // use-result property is wrong type
            exception = GetStatementExceptionPattern(epService, "x=" + EVENT_ALLTYPES + " -> " + EVENT_ALLTYPES + "(doublePrimitive=x.boolBoxed)");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to validate filter expression 'doublePrimitive=x.boolBoxed': Implicit conversion from datatype 'bool?' to 'double?' is not allowed [");
    
            // named-parameter for timer:at or timer:interval
            exception = GetStatementExceptionPattern(epService, "timer:Interval(interval:10)");
            SupportMessageAssertUtil.AssertMessage(exception, "Invalid parameter for pattern observer 'timer:Interval(interval:10)': Timer-interval observer does not allow named parameters [timer:Interval(interval:10)]");
            exception = GetStatementExceptionPattern(epService, "timer:At(perhaps:10)");
            SupportMessageAssertUtil.AssertMessage(exception, "Invalid parameter for pattern observer 'timer:At(perhaps:10)': timer:at does not allow named parameters [timer:At(perhaps:10)]");
        }
    
        private void RunAssertionUseResult(EPServiceProvider epService) {
            string @event = typeof(SupportBean_N).Name;
    
            TryValid(epService, "na=" + @event + " -> nb=" + @event + "(doublePrimitive = na.doublePrimitive)");
            TryInvalid(epService, "xx=" + @event + " -> nb=" + @event + "(doublePrimitive = na.doublePrimitive)");
            TryInvalid(epService, "na=" + @event + " -> nb=" + @event + "(doublePrimitive = xx.doublePrimitive)");
            TryInvalid(epService, "na=" + @event + " -> nb=" + @event + "(doublePrimitive = na.xx)");
            TryInvalid(epService, "xx=" + @event + " -> nb=" + @event + "(xx = na.doublePrimitive)");
            TryInvalid(epService, "na=" + @event + " -> nb=" + @event + "(xx = na.xx)");
            TryValid(epService, "na=" + @event + " -> nb=" + @event + "(doublePrimitive = na.doublePrimitive, intBoxed=na.intBoxed)");
            TryValid(epService, "na=" + @event + "() -> nb=" + @event + "(doublePrimitive in (na.doublePrimitive:na.doubleBoxed))");
            TryValid(epService, "na=" + @event + "() -> nb=" + @event + "(doublePrimitive in [na.doublePrimitive:na.doubleBoxed])");
            TryValid(epService, "na=" + @event + "() -> nb=" + @event + "(doublePrimitive in [na.intBoxed:na.intPrimitive])");
            TryInvalid(epService, "na=" + @event + "() -> nb=" + @event + "(doublePrimitive in [na.intBoxed:na.xx])");
            TryInvalid(epService, "na=" + @event + "() -> nb=" + @event + "(doublePrimitive in [na.intBoxed:na.boolBoxed])");
            TryInvalid(epService, "na=" + @event + "() -> nb=" + @event + "(doublePrimitive in [na.xx:na.intPrimitive])");
            TryInvalid(epService, "na=" + @event + "() -> nb=" + @event + "(doublePrimitive in [na.boolBoxed:na.intPrimitive])");
        }
    
        private void TryInvalid(EPServiceProvider epService, string eplInvalidPattern) {
            try {
                epService.EPAdministrator.CreatePattern(eplInvalidPattern);
                Assert.Fail();
            } catch (EPException ex) {
                // Expected exception
            }
        }
    
        private string GetSyntaxExceptionPattern(EPServiceProvider epService, string expression) {
            string exceptionText = null;
            try {
                epService.EPAdministrator.CreatePattern(expression);
                Assert.Fail();
            } catch (EPStatementSyntaxException ex) {
                exceptionText = ex.Message;
                Log.Debug(".getSyntaxExceptionPattern pattern=" + expression, ex);
                // Expected exception
            }
    
            return exceptionText;
        }
    
        private EPStatementException GetStatementExceptionPattern(EPServiceProvider epService, string expression) {
            return GetStatementExceptionPattern(epService, expression, false);
        }
    
        private EPStatementException GetStatementExceptionPattern(EPServiceProvider epService, string expression, bool isLogException) {
            try {
                epService.EPAdministrator.CreatePattern(expression);
                Assert.Fail();
            } catch (EPStatementSyntaxException es) {
                throw es;
            } catch (EPStatementException ex) {
                // Expected exception
                if (isLogException) {
                    Log.Debug(".getSyntaxExceptionPattern pattern=" + expression, ex);
                }
                return ex;
            }
            throw new IllegalStateException();
        }
    
        private void TryValid(EPServiceProvider epService, string eplInvalidPattern) {
            epService.EPAdministrator.CreatePattern(eplInvalidPattern);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
