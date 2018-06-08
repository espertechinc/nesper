///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewInvalid : RegressionExecution {
        private static readonly string EVENT_NUM = typeof(SupportBean_N).FullName;
        private static readonly string EVENT_ALLTYPES = typeof(SupportBean).FullName;

        public override void Configure(Configuration configuration)
        {
            base.Configure(configuration);
            configuration.EngineDefaults.EventMeta.ClassPropertyResolutionStyle =
                PropertyResolutionStyle.CASE_SENSITIVE;
        }

        public override void Run(EPServiceProvider epService) {
            RunAssertionInvalidPropertyExpression(epService);
            RunAssertionInvalidSyntax(epService);
            RunAssertionStatementException(epService);
            RunAssertionInvalidView(epService);
        }
    
        private void RunAssertionInvalidPropertyExpression(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@IterableUnbound select * from SupportBean");
            epService.EPRuntime.SendEvent(new SupportBean());
            EventBean theEvent = stmt.First();
    
            string exceptionText = GetSyntaxExceptionProperty("", theEvent);
            Assert.AreEqual("Property named '' is not a valid property name for this type", exceptionText);
    
            exceptionText = GetSyntaxExceptionProperty("-", theEvent);
            Assert.AreEqual("Property named '-' is not a valid property name for this type", exceptionText);
    
            exceptionText = GetSyntaxExceptionProperty("a[]", theEvent);
            Assert.AreEqual("Property named 'a[]' is not a valid property name for this type", exceptionText);
    
            stmt.Dispose();
        }
    
        private void RunAssertionInvalidSyntax(EPServiceProvider epService) {
            // keyword in select clause
            EPStatementSyntaxException exception = GetSyntaxExceptionView(epService, "select inner from MyStream");
            AssertMessage(exception, "Incorrect syntax near 'inner' (a reserved keyword) at line 1 column 7, please check the select clause [");
    
            // keyword in from clause
            exception = GetSyntaxExceptionView(epService, "select something from Outer");
            AssertMessage(exception, "Incorrect syntax near 'Outer' (a reserved keyword) at line 1 column 22, please check the from clause [");
    
            // keyword used in package
            exception = GetSyntaxExceptionView(epService, "select * from com.true.mycompany.MyEvent");
            AssertMessage(exception, "Incorrect syntax near 'true' (a reserved keyword) expecting an identifier but found 'true' at line 1 column 18, please check the view specifications within the from clause [");
    
            // keyword as part of identifier
            exception = GetSyntaxExceptionView(epService, "select * from MyEvent, MyEvent2 where a.day=b.day");
            AssertMessage(exception, "Incorrect syntax near 'day' (a reserved keyword) at line 1 column 40, please check the where clause [");
    
            exception = GetSyntaxExceptionView(epService, "select * * from " + EVENT_NUM);
            AssertMessage(exception, "Incorrect syntax near '*' at line 1 column 9 near reserved keyword 'from' [");
    
            // keyword in select clause
            exception = GetSyntaxExceptionView(epService, "select day from MyEvent, MyEvent2");
            AssertMessage(exception, "Incorrect syntax near 'day' (a reserved keyword) at line 1 column 7, please check the select clause [");
        }
    
        private void RunAssertionStatementException(EPServiceProvider epService) {
            EPStatementException exception;
    
            // property near to spelling
            exception = GetStatementExceptionView(epService, "select s0.intPrimitv from " + typeof(SupportBean).FullName + " as s0");
            AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 's0.intPrimitv': Property named 'intPrimitv' is not valid in stream 's0' (did you mean 'IntPrimitive'?) [");
    
            exception = GetStatementExceptionView(epService, "select INTPRIMITIVE from " + typeof(SupportBean).FullName);
            AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'INTPRIMITIVE': Property named 'INTPRIMITIVE' is not valid in any stream (did you mean 'IntPrimitive'?) [");
    
            exception = GetStatementExceptionView(epService, "select theStrring from " + typeof(SupportBean).FullName);
            AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'theStrring': Property named 'theStrring' is not valid in any stream (did you mean 'TheString'?) [");
    
            // aggregation in where clause known
            exception = GetStatementExceptionView(epService, "select * from " + typeof(SupportBean).FullName + " where sum(IntPrimitive) > 10");
            AssertMessage(exception, "Aggregation functions not allowed within filters [");
    
            // class not found
            exception = GetStatementExceptionView(epService, "select * from dummypkg.dummy()#length(10)");
            AssertMessage(exception, "Failed to resolve event type: Event type or class named 'dummypkg.dummy' was not found [select * from dummypkg.dummy()#length(10)]");
    
            // invalid view
            exception = GetStatementExceptionView(epService, "select * from " + EVENT_NUM + ".dummy:dummy(10)");
            AssertMessage(exception, "Error starting statement: View name 'dummy:dummy' is not a known view name [");
    
            // keyword used
            exception = GetSyntaxExceptionView(epService, "select order from " + typeof(SupportBean).FullName);
            AssertMessage(exception, "Incorrect syntax near 'order' (a reserved keyword) at line 1 column 7, please check the select clause [");
    
            // invalid view parameter
            exception = GetStatementExceptionView(epService, "select * from " + EVENT_NUM + "#length('s')");
            AssertMessage(exception, "Error starting statement: Error in view 'length', Length view requires a single integer-type parameter [");
    
            // where-clause relational op has invalid type
            exception = GetStatementExceptionView(epService, "select * from " + EVENT_ALLTYPES + "#length(1) where TheString > 5");
            AssertMessage(exception, "Error validating expression: Failed to validate filter expression 'TheString>5': Implicit conversion from datatype 'System.String' to numeric is not allowed [");
    
            // where-clause has aggregation function
            exception = GetStatementExceptionView(epService, "select * from " + EVENT_ALLTYPES + "#length(1) where sum(IntPrimitive) > 5");
            AssertMessage(exception, "Error validating expression: An aggregate function may not appear in a WHERE clause (use the HAVING clause) [");
    
            // invalid numerical expression
            exception = GetStatementExceptionView(epService, "select 2 * 's' from " + EVENT_ALLTYPES + "#length(1)");
            AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression '2*\"s\"': Implicit conversion from datatype 'System.String' to numeric is not allowed [");
    
            // invalid property in select
            exception = GetStatementExceptionView(epService, "select a[2].M('a') from " + EVENT_ALLTYPES + "#length(1)");
            AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'a[2].M('a')': Failed to resolve enumeration method, date-time method or mapped property 'a[2].M('a')': Failed to resolve 'a[2].M' to a property, single-row function, aggregation function, script, stream or class name [");
    
            // select clause uses same "as" name twice
            exception = GetStatementExceptionView(epService, "select 2 as m, 2 as m from " + EVENT_ALLTYPES + "#length(1)");
            AssertMessage(exception, "Error starting statement: Column name 'm' appears more then once in select clause [");
    
            // class in method invocation not found
            exception = GetStatementExceptionView(epService, "select UnknownClass.Method() from " + EVENT_NUM + "#length(10)");
            AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'UnknownClass.Method()': Failed to resolve 'UnknownClass.Method' to a property, single-row function, aggregation function, script, stream or class name [");
    
            // method not found
            exception = GetStatementExceptionView(epService, "select Math.UnknownMethod() from " + EVENT_NUM + "#length(10)");
            AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'Math.UnknownMethod()': Failed to resolve 'Math.UnknownMethod' to a property, single-row function, aggregation function, script, stream or class name [");
    
            // invalid property in group-by
            exception = GetStatementExceptionView(epService, "select IntPrimitive from " + EVENT_ALLTYPES + "#length(1) group by xxx");
            AssertMessage(exception, "Error starting statement: Failed to validate group-by-clause expression 'xxx': Property named 'xxx' is not valid in any stream [");
    
            // group-by not specifying a property
            exception = GetStatementExceptionView(epService, "select IntPrimitive from " + EVENT_ALLTYPES + "#length(1) group by 5");
            AssertMessage(exception, "Error starting statement: Group-by expressions must refer to property names [");
    
            // group-by specifying aggregates
            exception = GetStatementExceptionView(epService, "select IntPrimitive from " + EVENT_ALLTYPES + "#length(1) group by sum(IntPrimitive)");
            AssertMessage(exception, "Error starting statement: Group-by expressions cannot contain aggregate functions [");
    
            // invalid property in having clause
            exception = GetStatementExceptionView(epService, "select 2 * 's' from " + EVENT_ALLTYPES + "#length(1) group by IntPrimitive having xxx > 5");
            AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression '2*\"s\"': Implicit conversion from datatype 'System.String' to numeric is not allowed [");
    
            // invalid having clause - not a symbol in the group-by (non-aggregate)
            exception = GetStatementExceptionView(epService, "select sum(IntPrimitive) from " + EVENT_ALLTYPES + "#length(1) group by IntBoxed having DoubleBoxed > 5");
            AssertMessage(exception, "Error starting statement: Non-aggregated property 'DoubleBoxed' in the HAVING clause must occur in the group-by clause [");
    
            // invalid outer join - not a symbol
            exception = GetStatementExceptionView(epService, "select * from " + EVENT_ALLTYPES + "#length(1) as aStr " +
                    "left outer join " + EVENT_ALLTYPES + "#length(1) on xxxx=yyyy");
            AssertMessage(exception, "Error validating expression: Failed to validate on-clause join expression 'xxxx=yyyy': Property named 'xxxx' is not valid in any stream [");
    
            // invalid outer join for 3 streams - not a symbol
            exception = GetStatementExceptionView(epService, "select * from " + EVENT_ALLTYPES + "#length(1) as s0 " +
                    "left outer join " + EVENT_ALLTYPES + "#length(1) as s1 on s0.IntPrimitive = s1.IntPrimitive " +
                    "left outer join " + EVENT_ALLTYPES + "#length(1) as s2 on s0.IntPrimitive = s2.yyyy");
            AssertMessage(exception, "Error validating expression: Failed to validate on-clause join expression 's0.IntPrimitive=s2.yyyy': Failed to resolve property 's2.yyyy' to a stream or nested property in a stream [");
    
            // invalid outer join for 3 streams - wrong stream, the properties in on-clause don't refer to streams
            exception = GetStatementExceptionView(epService, "select * from " + EVENT_ALLTYPES + "#length(1) as s0 " +
                    "left outer join " + EVENT_ALLTYPES + "#length(1) as s1 on s0.IntPrimitive = s1.IntPrimitive " +
                    "left outer join " + EVENT_ALLTYPES + "#length(1) as s2 on s0.IntPrimitive = s1.IntPrimitive");
            AssertMessage(exception, "Error validating expression: Outer join ON-clause must refer to at least one property of the joined stream for stream 2 [");
    
            // invalid outer join - referencing next stream
            exception = GetStatementExceptionView(epService, "select * from " + EVENT_ALLTYPES + "#length(1) as s0 " +
                    "left outer join " + EVENT_ALLTYPES + "#length(1) as s1 on s2.IntPrimitive = s1.IntPrimitive " +
                    "left outer join " + EVENT_ALLTYPES + "#length(1) as s2 on s1.IntPrimitive = s2.IntPrimitive");
            AssertMessage(exception, "Error validating expression: Outer join ON-clause invalid scope for property 'IntPrimitive', expecting the current or a prior stream scope [");
    
            // invalid outer join - same properties
            exception = GetStatementExceptionView(epService, "select * from " + EVENT_NUM + "#length(1) as aStr " +
                    "left outer join " + EVENT_ALLTYPES + "#length(1) on TheString=TheString");
            AssertMessage(exception, "Error validating expression: Outer join ON-clause cannot refer to properties of the same stream [");
    
            // invalid order by
            exception = GetStatementExceptionView(epService, "select * from " + EVENT_NUM + "#length(1) as aStr order by X");
            AssertMessage(exception, "Error starting statement: Failed to validate order-by-clause expression 'X': Property named 'X' is not valid in any stream [");
    
            // insert into with wildcard - not allowed
            exception = GetStatementExceptionView(epService, "insert into Google (a, b) select * from " + EVENT_NUM + "#length(1) as aStr");
            AssertMessage(exception, "Error starting statement: Wildcard not allowed when insert-into specifies column order [");
    
            // insert into with duplicate column names
            exception = GetStatementExceptionView(epService, "insert into Google (a, b, a) select BoolBoxed, BoolPrimitive, IntBoxed from " + EVENT_NUM + "#length(1) as aStr");
            AssertMessage(exception, "Error starting statement: Property name 'a' appears more then once in insert-into clause [");
    
            // insert into mismatches selected columns
            exception = GetStatementExceptionView(epService, "insert into Google (a, b, c) select BoolBoxed, BoolPrimitive from " + EVENT_NUM + "#length(1) as aStr");
            AssertMessage(exception, "Error starting statement: Number of supplied values in the select or values clause does not match insert-into clause [");
    
            // mismatched type on coalesce columns
            exception = GetStatementExceptionView(epService, "select coalesce(BoolBoxed, TheString) from " + typeof(SupportBean).FullName + "#length(1) as aStr");
            AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'coalesce(BoolBoxed,TheString)': Implicit conversion not allowed: Cannot coerce to bool type System.String [");
    
            // mismatched case compare type
            exception = GetStatementExceptionView(epService, "select case BoolPrimitive when 1 then true end from " + typeof(SupportBean).FullName + "#length(1) as aStr");
            AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'case BoolPrimitive when 1 then true end': Implicit conversion not allowed: Cannot coerce to bool type " + Name.Clean<int>() + " [");
    
            // mismatched case result type
            exception = GetStatementExceptionView(epService, "select case when 1=2 then 1 when 1=3 then true end from " + typeof(SupportBean).FullName + "#length(1) as aStr");
            AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'case when 1=2 then 1 when 1=3 then ...(43 chars)': Implicit conversion not allowed: Cannot coerce types " + Name.Clean<int>() + " and " + Name.Clean<bool>() + " [");
    
            // case expression not returning bool
            exception = GetStatementExceptionView(epService, "select case when 3 then 1 end from " + typeof(SupportBean).FullName + "#length(1) as aStr");
            AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'case when 3 then 1 end': Case node 'when' expressions must return a boolean value [");
    
            // function not known
            exception = GetStatementExceptionView(epService, "select gogglex(1) from " + EVENT_NUM + "#length(1)");
            AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'gogglex(1)': Unknown single-row function, aggregation function or mapped or indexed property named 'gogglex' could not be resolved [");
    
            // insert into column name incorrect
            epService.EPAdministrator.CreateEPL("insert into xyz select 1 as dodi from System.String");
            exception = GetStatementExceptionView(epService, "select pox from pattern[xyz(yodo=4)]");
            AssertMessage(exception, "Failed to validate filter expression 'yodo=4': Property named 'yodo' is not valid in any stream (did you mean 'dodi'?) [select pox from pattern[xyz(yodo=4)]]");
        }
    
        private void RunAssertionInvalidView(EPServiceProvider epService) {
            string eventClass = typeof(SupportBean).FullName;
    
            TryInvalid(epService, "select * from " + eventClass + "(dummy='a')#length(3)");
            TryValid(epService, "select * from " + eventClass + "(TheString='a')#length(3)");
            TryInvalid(epService, "select * from " + eventClass + ".dummy:Length(3)");
    
            TryInvalid(epService, "select djdjdj from " + eventClass + "#length(3)");
            TryValid(epService, "select BoolBoxed as xx, IntPrimitive from " + eventClass + "#length(3)");
            TryInvalid(epService, "select BoolBoxed as xx, IntPrimitive as xx from " + eventClass + "#length(3)");
            TryValid(epService, "select BoolBoxed as xx, IntPrimitive as yy from " + eventClass + "()#length(3)");
    
            TryValid(epService, "select BoolBoxed as xx, IntPrimitive as yy from " + eventClass + "()#length(3)" +
                    " where BoolBoxed = true");
            TryInvalid(epService, "select BoolBoxed as xx, IntPrimitive as yy from " + eventClass + "()#length(3)" +
                    " where xx = true");
        }
    
        private void TryInvalid(EPServiceProvider epService, string viewStmt) {
            try {
                epService.EPAdministrator.CreateEPL(viewStmt);
                Assert.Fail();
            } catch (EPException) {
                // Expected exception
            }
        }
    
        private EPStatementSyntaxException GetSyntaxExceptionView(EPServiceProvider epService, string expression) {
            try {
                epService.EPAdministrator.CreateEPL(expression);
                Assert.Fail();
            } catch (EPStatementSyntaxException ex) {
                if (Log.IsDebugEnabled) {
                    Log.Debug(".getSyntaxExceptionView expression=" + expression, ex);
                }
                // Expected exception
                return ex;
            }
            throw new IllegalStateException();
        }
    
        private string GetSyntaxExceptionProperty(string expression, EventBean theEvent) {
            string exceptionText = null;
            try {
                theEvent.Get(expression);
                Assert.Fail();
            } catch (PropertyAccessException ex) {
                exceptionText = ex.Message;
                if (Log.IsDebugEnabled) {
                    Log.Debug(".getSyntaxExceptionProperty expression=" + expression, ex);
                }
                // Expected exception
            }
    
            return exceptionText;
        }
    
        private EPStatementException GetStatementExceptionView(EPServiceProvider epService, string expression) {
            return GetStatementExceptionView(epService, expression, false);
        }
    
        private EPStatementException GetStatementExceptionView(EPServiceProvider epService, string expression, bool isLogException) {
            try {
                epService.EPAdministrator.CreateEPL(expression, "MyStatement");
                Assert.Fail();
            } catch (EPStatementSyntaxException es) {
                throw es;
            } catch (EPStatementException ex) {
                // Expected exception
                if (isLogException) {
                    Log.Debug(".getStatementExceptionView expression=" + expression, ex);
                }
                return ex;
            }
            throw new IllegalStateException();
        }
    
        private void TryValid(EPServiceProvider epService, string viewStmt) {
            epService.EPAdministrator.CreateEPL(viewStmt);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
