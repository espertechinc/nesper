///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternInvalid : RegressionExecution {
        private static readonly string EVENT_NUM = typeof(SupportBean_N).FullName;
        private static readonly string EVENT_COMPLEX = typeof(SupportBeanComplexProps).FullName;
        private static readonly string EVENT_ALLTYPES = typeof(SupportBean).FullName;
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionInvalid(epService);
            RunAssertionStatementException(epService);
            RunAssertionUseResult(epService);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string exceptionText = GetSyntaxExceptionPattern(epService, EVENT_NUM + "(DoublePrimitive='ss'");
            Assert.AreEqual(
                "Incorrect syntax near end-of-input expecting a closing parenthesis ')' " +
                "but found end-of-input at line 1 column 77, " +
                "please check the filter specification within the " +
                "pattern expression [" + Name.Of<SupportBean_N>() + "(DoublePrimitive='ss']",
                exceptionText);
    
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.CreateEPL("select * from pattern[(not a=SupportBean) -> SupportBean(TheString=a.TheString)]");
    
            // test invalid subselect
            epService.EPAdministrator.CreateEPL("create window WaitWindow#keepall as (waitTime int)");
            epService.EPAdministrator.CreateEPL("insert into WaitWindow select IntPrimitive as waitTime from SupportBean");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
    
            try {
                epService.EPAdministrator.CreatePattern("timer:interval((select waitTime from WaitWindow))");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Subselects are not allowed within pattern observer parameters, please consider using a variable instead [timer:interval((select waitTime from WaitWindow))]",
                        ex.Message);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionStatementException(EPServiceProvider epService) {
            EPStatementException exception;
    
            exception = GetStatementExceptionPattern(epService, "timer:at(2,3,4,4,4)");
            SupportMessageAssertUtil.AssertMessage(exception, "Invalid parameter for pattern observer 'timer:at(2,3,4,4,4)': Error computing crontab schedule specification: Invalid combination between days of week and days of month fields for timer:at [");
    
            exception = GetStatementExceptionPattern(epService, "timer:at(*,*,*,*,*,0,-1)");
            SupportMessageAssertUtil.AssertMessage(exception, "Invalid parameter for pattern observer 'timer:at(*,*,*,*,*,0,-1)': Error computing crontab schedule specification: Invalid timezone parameter '-1' for timer:at, expected a string-type value [");
    
            exception = GetStatementExceptionPattern(epService, EVENT_ALLTYPES + " -> timer:within()");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to resolve pattern observer 'timer:within()': Pattern guard function 'within' cannot be used as a pattern observer [");
    
            exception = GetStatementExceptionPattern(epService, EVENT_ALLTYPES + " where timer:interval(100)");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to resolve pattern guard '" + typeof(SupportBean).FullName + " where timer:interval(100)': Pattern observer function 'interval' cannot be used as a pattern guard [");
    
            exception = GetStatementExceptionPattern(epService, EVENT_ALLTYPES + " -> timer:interval()");
            SupportMessageAssertUtil.AssertMessage(exception, "Invalid parameter for pattern observer 'timer:interval()': Timer-interval observer requires a single numeric or time period parameter [");
    
            exception = GetStatementExceptionPattern(epService, EVENT_ALLTYPES + " where timer:within()");
            SupportMessageAssertUtil.AssertMessage(exception, "Invalid parameter for pattern guard '" + typeof(SupportBean).FullName + " where timer:within()': Timer-within guard requires a single numeric or time period parameter [");
    
            // class not found
            exception = GetStatementExceptionPattern(epService, "dummypkg.dummy()");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to resolve event type: Event type or class named 'dummypkg.dummy' was not found [");
    
            // simple property not found
            exception = GetStatementExceptionPattern(epService, EVENT_NUM + "(dummy=1)");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to validate filter expression 'dummy=1': Property named 'dummy' is not valid in any stream [");
    
            // nested property not found
            exception = GetStatementExceptionPattern(epService, EVENT_NUM + "(dummy.nested=1)");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to validate filter expression 'dummy.nested=1': Failed to resolve property 'dummy.nested' to a stream or nested property in a stream [");
    
            // property wrong type
            exception = GetStatementExceptionPattern(epService, EVENT_NUM + "(IntPrimitive='s')");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to validate filter expression 'IntPrimitive=\"s\"': Implicit conversion from datatype '" + Name.Clean<string>() + "' to '" + Name.Clean<int>() + "' is not allowed [");
    
            // property not a primitive type
            exception = GetStatementExceptionPattern(epService, EVENT_COMPLEX + "(nested=1)");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to validate filter expression 'nested=1': Implicit conversion from datatype '" + Name.Clean<int>() + "' to '" + Name.Clean<SupportBeanComplexProps.SupportBeanSpecialGetterNested>() + "' is not allowed [");
    
            // no tag matches prior use
            exception = GetStatementExceptionPattern(epService, EVENT_NUM + "(DoublePrimitive=x.abc)");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to validate filter expression 'DoublePrimitive=x.abc': Failed to resolve property 'x.abc' to a stream or nested property in a stream [");
    
            // range not valid on string
            exception = GetStatementExceptionPattern(epService, EVENT_ALLTYPES + "(TheString in [1:2])");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to validate filter expression 'TheString between 1 and 2': Implicit conversion from datatype '" + Name.Clean<string>() + "' to numeric is not allowed [");
    
            // range does not allow string params
            exception = GetStatementExceptionPattern(epService, EVENT_ALLTYPES + "(DoubleBoxed in ['a':2])");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to validate filter expression 'DoubleBoxed between \"a\" and 2': Implicit conversion from datatype '" + Name.Clean<string>() + "' to numeric is not allowed [");
    
            // invalid observer arg
            exception = GetStatementExceptionPattern(epService, "timer:at(9l)");
            SupportMessageAssertUtil.AssertMessage(exception, "Invalid parameter for pattern observer 'timer:at(9L)': Invalid number of parameters for timer:at [timer:at(9l)]");
    
            // invalid guard arg
            exception = GetStatementExceptionPattern(epService, EVENT_ALLTYPES + " where timer:within('s')");
            SupportMessageAssertUtil.AssertMessage(exception, "Invalid parameter for pattern guard '" + Name.Clean<SupportBean>() + " where timer:within(\"s\")': Timer-within guard requires a single numeric or time period parameter [");
    
            // use-result property is wrong type
            exception = GetStatementExceptionPattern(epService, "x=" + EVENT_ALLTYPES + " -> " + EVENT_ALLTYPES + "(DoublePrimitive=x.BoolBoxed)");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to validate filter expression 'DoublePrimitive=x.BoolBoxed': Implicit conversion from datatype '" + Name.Clean<bool?>() + "' to '" + Name.Clean<double?>() + "' is not allowed [");
    
            // named-parameter for timer:at or timer:interval
            exception = GetStatementExceptionPattern(epService, "timer:interval(interval:10)");
            SupportMessageAssertUtil.AssertMessage(exception, "Invalid parameter for pattern observer 'timer:interval(interval:10)': Timer-interval observer does not allow named parameters [timer:interval(interval:10)]");
            exception = GetStatementExceptionPattern(epService, "timer:at(perhaps:10)");
            SupportMessageAssertUtil.AssertMessage(exception, "Invalid parameter for pattern observer 'timer:at(perhaps:10)': timer:at does not allow named parameters [timer:at(perhaps:10)]");
        }
    
        private void RunAssertionUseResult(EPServiceProvider epService) {
            string @event = typeof(SupportBean_N).FullName;
    
            TryValid(epService, "na=" + @event + " -> nb=" + @event + "(DoublePrimitive = na.DoublePrimitive)");
            TryInvalid(epService, "xx=" + @event + " -> nb=" + @event + "(DoublePrimitive = na.DoublePrimitive)");
            TryInvalid(epService, "na=" + @event + " -> nb=" + @event + "(DoublePrimitive = xx.DoublePrimitive)");
            TryInvalid(epService, "na=" + @event + " -> nb=" + @event + "(DoublePrimitive = na.xx)");
            TryInvalid(epService, "xx=" + @event + " -> nb=" + @event + "(xx = na.DoublePrimitive)");
            TryInvalid(epService, "na=" + @event + " -> nb=" + @event + "(xx = na.xx)");
            TryValid(epService, "na=" + @event + " -> nb=" + @event + "(DoublePrimitive = na.DoublePrimitive, IntBoxed=na.IntBoxed)");
            TryValid(epService, "na=" + @event + "() -> nb=" + @event + "(DoublePrimitive in (na.DoublePrimitive:na.DoubleBoxed))");
            TryValid(epService, "na=" + @event + "() -> nb=" + @event + "(DoublePrimitive in [na.DoublePrimitive:na.DoubleBoxed])");
            TryValid(epService, "na=" + @event + "() -> nb=" + @event + "(DoublePrimitive in [na.IntBoxed:na.IntPrimitive])");
            TryInvalid(epService, "na=" + @event + "() -> nb=" + @event + "(DoublePrimitive in [na.IntBoxed:na.xx])");
            TryInvalid(epService, "na=" + @event + "() -> nb=" + @event + "(DoublePrimitive in [na.IntBoxed:na.BoolBoxed])");
            TryInvalid(epService, "na=" + @event + "() -> nb=" + @event + "(DoublePrimitive in [na.xx:na.IntPrimitive])");
            TryInvalid(epService, "na=" + @event + "() -> nb=" + @event + "(DoublePrimitive in [na.BoolBoxed:na.IntPrimitive])");
        }
    
        private void TryInvalid(EPServiceProvider epService, string eplInvalidPattern) {
            try {
                epService.EPAdministrator.CreatePattern(eplInvalidPattern);
                Assert.Fail();
            } catch (EPException) {
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
            } catch (EPStatementSyntaxException) {
                throw;
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
