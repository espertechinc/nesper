///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestInvalidView 
    {
        private readonly string EVENT_NUM = typeof(SupportBean_N).FullName;
        private readonly string EVENT_ALLTYPES = typeof(SupportBean).FullName;
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_SENSITIVE;
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }
    
        [Test]
        public void TestInvalidPropertyExpression()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("@IterableUnbound select * from SupportBean");
            _epService.EPRuntime.SendEvent(new SupportBean());
            EventBean theEvent = stmt.First();

            string exceptionText = GetSyntaxExceptionProperty("", theEvent);
            Assert.AreEqual("Property named '' is not a valid property name for this type", exceptionText);

            exceptionText = GetSyntaxExceptionProperty("-", theEvent);
            Assert.AreEqual("Property named '-' is not a valid property name for this type", exceptionText);

            exceptionText = GetSyntaxExceptionProperty("a[]", theEvent);
            Assert.AreEqual("Property named 'a[]' is not a valid property name for this type", exceptionText);
        }
    
        [Test]
        public void TestInvalidSyntax()
        {
            // keyword in select clause
            EPStatementException exception = GetSyntaxExceptionView("select inner from MyStream");
            SupportMessageAssertUtil.AssertMessage(exception, "Incorrect syntax near 'inner' (a reserved keyword) at line 1 column 7, please check the select clause [select inner from MyStream]");
    
            // keyword in from clause
            exception = GetSyntaxExceptionView("select something from Outer");
            SupportMessageAssertUtil.AssertMessage(exception, "Incorrect syntax near 'Outer' (a reserved keyword) at line 1 column 22, please check the from clause [select something from Outer]");
    
            // keyword used in package
            exception = GetSyntaxExceptionView("select * from com.true.mycompany.MyEvent");
            SupportMessageAssertUtil.AssertMessage(exception, "Incorrect syntax near 'true' (a reserved keyword) expecting an identifier but found 'true' at line 1 column 18, please check the view specifications within the from clause [select * from com.true.mycompany.MyEvent]");
    
            // keyword as part of identifier
            exception = GetSyntaxExceptionView("select * from MyEvent, MyEvent2 where a.day=b.day");
            SupportMessageAssertUtil.AssertMessage(exception, "Incorrect syntax near 'day' (a reserved keyword) at line 1 column 40, please check the where clause [select * from MyEvent, MyEvent2 where a.day=b.day]");
    
            exception = GetSyntaxExceptionView("select * * from " + EVENT_NUM);
            SupportMessageAssertUtil.AssertMessage(exception, "Incorrect syntax near '*' at line 1 column 9 near reserved keyword 'from' [select * * from " + Name.Of<SupportBean_N>() + "]");
    
            // keyword in select clause
            exception = GetSyntaxExceptionView("select day from MyEvent, MyEvent2");
            SupportMessageAssertUtil.AssertMessage(exception, "Incorrect syntax near 'day' (a reserved keyword) at line 1 column 7, please check the select clause [select day from MyEvent, MyEvent2]");
        }
    
        [Test]
        public void TestStatementException() 
        {
            EPStatementException exception = null;
    
            // property near to spelling
            exception = GetStatementExceptionView("select s0.intPrimitv from " + typeof(SupportBean).FullName + " as s0");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 's0.intPrimitv': Property named 'intPrimitv' is not valid in stream 's0' (did you mean 'IntPrimitive'?) [select s0.intPrimitv from " + Name.Of<SupportBean>() + " as s0]");

            exception = GetStatementExceptionView("select INTPRIMITIVE from " + typeof(SupportBean).FullName);
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'INTPRIMITIVE': Property named 'INTPRIMITIVE' is not valid in any stream (did you mean 'IntPrimitive'?) [select INTPRIMITIVE from " + Name.Of<SupportBean>() + "]");

            exception = GetStatementExceptionView("select theStrring from " + typeof(SupportBean).FullName);
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'theStrring': Property named 'theStrring' is not valid in any stream (did you mean 'TheString'?) [select theStrring from " + Name.Of<SupportBean>() + "]");
    
            // aggregation in where clause known
            exception = GetStatementExceptionView("select * from " + typeof(SupportBean).FullName + " where sum(IntPrimitive) > 10");
            SupportMessageAssertUtil.AssertMessage(exception, "Aggregation functions not allowed within filters [select * from " + Name.Of<SupportBean>() + " where sum(IntPrimitive) > 10]");
    
            // class not found
            exception = GetStatementExceptionView("select * from dummypkg.dummy()#length(10)");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to resolve event type: Event type or class named 'dummypkg.dummy' was not found [select * from dummypkg.dummy()#length(10)]");
    
            // invalid view
            exception = GetStatementExceptionView("select * from " + EVENT_NUM + ".dummy:dummy(10)");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: View name 'dummy:dummy' is not a known view name [select * from " + Name.Of<SupportBean_N>() + ".dummy:dummy(10)]");
    
            // keyword used
            exception = GetSyntaxExceptionView("select order from " + typeof(SupportBean).FullName);
            SupportMessageAssertUtil.AssertMessage(exception, "Incorrect syntax near 'order' (a reserved keyword) at line 1 column 7, please check the select clause [select order from " + Name.Of<SupportBean>() + "]");
    
            // invalid view parameter
            exception = GetStatementExceptionView("select * from " + EVENT_NUM + "#length('s')");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Error in view 'length', Length view requires a single integer-type parameter [select * from " + Name.Of<SupportBean_N>() + "#length('s')]");
    
            // where-clause relational op has invalid type
            exception = GetStatementExceptionView("select * from " + EVENT_ALLTYPES + "#length(1) where TheString > 5");
            SupportMessageAssertUtil.AssertMessage(exception, "Error validating expression: Failed to validate filter expression 'TheString>5': Implicit conversion from datatype 'System.String' to numeric is not allowed [select * from " + Name.Of<SupportBean>() + "#length(1) where TheString > 5]");
    
            // where-clause has aggregation function
            exception = GetStatementExceptionView("select * from " + EVENT_ALLTYPES + "#length(1) where sum(IntPrimitive) > 5");
            SupportMessageAssertUtil.AssertMessage(exception, "Error validating expression: An aggregate function may not appear in a WHERE clause (use the HAVING clause) [select * from " + Name.Of<SupportBean>() + "#length(1) where sum(IntPrimitive) > 5]");
    
            // invalid numerical expression
            exception = GetStatementExceptionView("select 2 * 's' from " + EVENT_ALLTYPES + "#length(1)");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression '2*\"s\"': Implicit conversion from datatype 'System.String' to numeric is not allowed [select 2 * 's' from " + Name.Of<SupportBean>() + "#length(1)]");
    
            // invalid property in select
            exception = GetStatementExceptionView("select a[2].m('a') from " + EVENT_ALLTYPES + "#length(1)");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'a[2].m('a')': Failed to resolve enumeration method, date-time method or mapped property 'a[2].m('a')': Failed to resolve 'a[2].m' to a property, single-row function, aggregation function, script, stream or class name [select a[2].m('a') from " + Name.Of<SupportBean>() + "#length(1)]");
    
            // select clause uses same "as" name twice
            exception = GetStatementExceptionView("select 2 as m, 2 as m from " + EVENT_ALLTYPES + "#length(1)");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Column name 'm' appears more then once in select clause [select 2 as m, 2 as m from " + Name.Of<SupportBean>() + "#length(1)]");
    
            // class in method invocation not found
            exception = GetStatementExceptionView("select unknownClass.method() from " + EVENT_NUM + "#length(10)");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'unknownClass.method()': Failed to resolve 'unknownClass.method' to a property, single-row function, aggregation function, script, stream or class name [select unknownClass.method() from " + Name.Of<SupportBean_N>() + "#length(10)]");
    
            // method not found
            exception = GetStatementExceptionView("select Math.unknownMethod() from " + EVENT_NUM + "#length(10)");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'Math.unknownMethod()': Failed to resolve 'Math.unknownMethod' to a property, single-row function, aggregation function, script, stream or class name [select Math.unknownMethod() from " + Name.Of<SupportBean_N>() + "#length(10)]");
    
            // invalid property in group-by
            exception = GetStatementExceptionView("select IntPrimitive from " + EVENT_ALLTYPES + "#length(1) group by xxx");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Failed to validate group-by-clause expression 'xxx': Property named 'xxx' is not valid in any stream [select IntPrimitive from " + Name.Of<SupportBean>() + "#length(1) group by xxx]");
    
            // group-by not specifying a property
            exception = GetStatementExceptionView("select IntPrimitive from " + EVENT_ALLTYPES + "#length(1) group by 5");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Group-by expressions must refer to property names [select IntPrimitive from " + Name.Of<SupportBean>() + "#length(1) group by 5]");
    
            // group-by specifying aggregates
            exception = GetStatementExceptionView("select IntPrimitive from " + EVENT_ALLTYPES + "#length(1) group by sum(IntPrimitive)");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Group-by expressions cannot contain aggregate functions [select IntPrimitive from " + Name.Of<SupportBean>() + "#length(1) group by sum(IntPrimitive)]");
    
            // invalid property in having clause
            exception = GetStatementExceptionView("select 2 * 's' from " + EVENT_ALLTYPES + "#length(1) group by IntPrimitive having xxx > 5");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression '2*\"s\"': Implicit conversion from datatype 'System.String' to numeric is not allowed [select 2 * 's' from " + Name.Of<SupportBean>() + "#length(1) group by IntPrimitive having xxx > 5]");
    
            // invalid having clause - not a symbol in the group-by (non-aggregate)
            exception = GetStatementExceptionView("select sum(IntPrimitive) from " + EVENT_ALLTYPES + "#length(1) group by IntBoxed having DoubleBoxed > 5");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Non-aggregated property 'DoubleBoxed' in the HAVING clause must occur in the group-by clause [select sum(IntPrimitive) from " + Name.Of<SupportBean>() + "#length(1) group by IntBoxed having DoubleBoxed > 5]");
    
            // invalid outer join - not a symbol
            exception = GetStatementExceptionView("select * from " + EVENT_ALLTYPES + "#length(1) as aStr " +
                    "left outer join " + EVENT_ALLTYPES + "#length(1) on xxxx=yyyy");
            SupportMessageAssertUtil.AssertMessage(exception, "Error validating expression: Failed to validate on-clause join expression 'xxxx=yyyy': Property named 'xxxx' is not valid in any stream [select * from " + Name.Of<SupportBean>() + "#length(1) as aStr left outer join " + Name.Of<SupportBean>() + "#length(1) on xxxx=yyyy]");
    
            // invalid outer join for 3 streams - not a symbol
            exception = GetStatementExceptionView("select * from " + EVENT_ALLTYPES + "#length(1) as s0 " +
                    "left outer join " + EVENT_ALLTYPES + "#length(1) as s1 on s0.IntPrimitive = s1.IntPrimitive " +
                    "left outer join " + EVENT_ALLTYPES + "#length(1) as s2 on s0.IntPrimitive = s2.yyyy");
            SupportMessageAssertUtil.AssertMessage(exception, "Error validating expression: Failed to validate on-clause join expression 's0.IntPrimitive=s2.yyyy': Failed to resolve property 's2.yyyy' to a stream or nested property in a stream [select * from " + Name.Of<SupportBean>() + "#length(1) as s0 left outer join " + Name.Of<SupportBean>() + "#length(1) as s1 on s0.IntPrimitive = s1.IntPrimitive left outer join " + Name.Of<SupportBean>() + "#length(1) as s2 on s0.IntPrimitive = s2.yyyy]");
    
            // invalid outer join for 3 streams - wrong stream, the properties in on-clause don't refer to streams
            exception = GetStatementExceptionView("select * from " + EVENT_ALLTYPES + "#length(1) as s0 " +
                    "left outer join " + EVENT_ALLTYPES + "#length(1) as s1 on s0.IntPrimitive = s1.IntPrimitive " +
                    "left outer join " + EVENT_ALLTYPES + "#length(1) as s2 on s0.IntPrimitive = s1.IntPrimitive");
            SupportMessageAssertUtil.AssertMessage(exception, "Error validating expression: Outer join ON-clause must refer to at least one property of the joined stream for stream 2 [select * from " + Name.Of<SupportBean>() + "#length(1) as s0 left outer join " + Name.Of<SupportBean>() + "#length(1) as s1 on s0.IntPrimitive = s1.IntPrimitive left outer join " + Name.Of<SupportBean>() + "#length(1) as s2 on s0.IntPrimitive = s1.IntPrimitive]");
    
            // invalid outer join - referencing next stream
            exception = GetStatementExceptionView("select * from " + EVENT_ALLTYPES + "#length(1) as s0 " +
                    "left outer join " + EVENT_ALLTYPES + "#length(1) as s1 on s2.IntPrimitive = s1.IntPrimitive " +
                    "left outer join " + EVENT_ALLTYPES + "#length(1) as s2 on s1.IntPrimitive = s2.IntPrimitive");
            SupportMessageAssertUtil.AssertMessage(exception, "Error validating expression: Outer join ON-clause invalid scope for property 'IntPrimitive', expecting the current or a prior stream scope [select * from " + Name.Of<SupportBean>() + "#length(1) as s0 left outer join " + Name.Of<SupportBean>() + "#length(1) as s1 on s2.IntPrimitive = s1.IntPrimitive left outer join " + Name.Of<SupportBean>() + "#length(1) as s2 on s1.IntPrimitive = s2.IntPrimitive]");
    
            // invalid outer join - same properties
            exception = GetStatementExceptionView("select * from " + EVENT_NUM + "#length(1) as aStr " +
                    "left outer join " + EVENT_ALLTYPES + "#length(1) on TheString=TheString");
            SupportMessageAssertUtil.AssertMessage(exception, "Error validating expression: Outer join ON-clause cannot refer to properties of the same stream [select * from " + Name.Of<SupportBean_N>() + "#length(1) as aStr left outer join " + Name.Of<SupportBean>() + "#length(1) on TheString=TheString]");
    
            // invalid order by
            exception = GetStatementExceptionView("select * from " + EVENT_NUM + "#length(1) as aStr order by X");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Failed to validate order-by-clause expression 'X': Property named 'X' is not valid in any stream [select * from " + Name.Of<SupportBean_N>() + "#length(1) as aStr order by X]");
    
            // insert into with wildcard - not allowed
            exception = GetStatementExceptionView("insert into Google (a, b) select * from " + EVENT_NUM + "#length(1) as aStr");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Wildcard not allowed when insert-into specifies column order [insert into Google (a, b) select * from " + Name.Of<SupportBean_N>() + "#length(1) as aStr]");
    
            // insert into with duplicate column names
            exception = GetStatementExceptionView("insert into Google (a, b, a) select BoolBoxed, BoolPrimitive, IntBoxed from " + EVENT_NUM + "#length(1) as aStr");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Property name 'a' appears more then once in insert-into clause [insert into Google (a, b, a) select BoolBoxed, BoolPrimitive, IntBoxed from " + Name.Of<SupportBean_N>() + "#length(1) as aStr]");
    
            // insert into mismatches selected columns
            exception = GetStatementExceptionView("insert into Google (a, b, c) select BoolBoxed, BoolPrimitive from " + EVENT_NUM + "#length(1) as aStr");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Number of supplied values in the select or values clause does not match insert-into clause [insert into Google (a, b, c) select BoolBoxed, BoolPrimitive from " + Name.Of<SupportBean_N>() + "#length(1) as aStr]");
    
            // mismatched type on coalesce columns
            exception = GetStatementExceptionView("select coalesce(BoolBoxed, TheString) from " + typeof(SupportBean).FullName + "#length(1) as aStr");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'coalesce(BoolBoxed,TheString)': Implicit conversion not allowed: Cannot coerce to bool type System.String [select coalesce(BoolBoxed, TheString) from " + Name.Of<SupportBean>() + "#length(1) as aStr]");
    
            // mismatched case compare type
            exception = GetStatementExceptionView("select case BoolPrimitive when 1 then true end from " + typeof(SupportBean).FullName + "#length(1) as aStr");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'case BoolPrimitive when 1 then true end': Implicit conversion not allowed: Cannot coerce to bool type " + Name.Of<int>() + " [select case BoolPrimitive when 1 then true end from " + Name.Of<SupportBean>() + "#length(1) as aStr]");
    
            // mismatched case result type
            exception = GetStatementExceptionView("select case when 1=2 then 1 when 1=3 then true end from " + typeof(SupportBean).FullName + "#length(1) as aStr");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'case when 1=2 then 1 when 1=3 then ...(43 chars)': Implicit conversion not allowed: Cannot coerce types " + Name.Of<int>() + " and " + Name.Of<bool>() + " [select case when 1=2 then 1 when 1=3 then true end from " + Name.Of<SupportBean>() + "#length(1) as aStr]");
    
            // case expression not returning bool
            exception = GetStatementExceptionView("select case when 3 then 1 end from " + typeof(SupportBean).FullName + "#length(1) as aStr");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'case when 3 then 1 end': Case node 'when' expressions must return a boolean value [select case when 3 then 1 end from " + Name.Of<SupportBean>() + "#length(1) as aStr]");
    
            // function not known
            exception = GetStatementExceptionView("select gogglex(1) from " + EVENT_NUM + "#length(1)");
            SupportMessageAssertUtil.AssertMessage(exception, "Error starting statement: Failed to validate select-clause expression 'gogglex(1)': Unknown single-row function, aggregation function or mapped or indexed property named 'gogglex' could not be resolved [select gogglex(1) from " + Name.Of<SupportBean_N>() + "#length(1)]");
    
            // insert into column name incorrect
            _epService.EPAdministrator.CreateEPL("insert into Xyz select 1 as dodi from System.String");
            exception = GetStatementExceptionView("select pox from pattern[Xyz(yodo=4)]");
            SupportMessageAssertUtil.AssertMessage(exception, "Failed to validate filter expression 'yodo=4': Property named 'yodo' is not valid in any stream (did you mean 'dodi'?) [select pox from pattern[Xyz(yodo=4)]]");
        }
    
        [Test]
        public void TestInvalidViewX()
        {
            string eventClass = typeof(SupportBean).FullName;
    
            TryInvalid("select * from " + eventClass + "(dummy='a')#length(3)");
            TryValid("select * from " + eventClass + "(TheString='a')#length(3)");
            TryInvalid("select * from " + eventClass + ".dummy:length(3)");
    
            TryInvalid("select djdjdj from " + eventClass + "#length(3)");
            TryValid("select BoolBoxed as xx, IntPrimitive from " + eventClass + "#length(3)");
            TryInvalid("select BoolBoxed as xx, IntPrimitive as xx from " + eventClass + "#length(3)");
            TryValid("select BoolBoxed as xx, IntPrimitive as yy from " + eventClass + "()#length(3)");

            TryValid("select BoolBoxed as xx, IntPrimitive as yy from " + eventClass + "()#length(3)" +
                    " where BoolBoxed = true");
            TryInvalid("select BoolBoxed as xx, IntPrimitive as yy from " + eventClass + "()#length(3)" +
                    " where xx = true");
        }
    
        private void TryInvalid(string viewStmt)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(viewStmt);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                // Expected exception
            }
        }
    
        private EPStatementException GetSyntaxExceptionView(string expression)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(expression);
                Assert.Fail();
            }
            catch (EPStatementSyntaxException ex)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".getSyntaxExceptionView expression=" + expression, ex);
                }
                // Expected exception
                return ex;
            }

            throw new IllegalStateException();
        }
    
        private string GetSyntaxExceptionProperty(string expression, EventBean theEvent)
        {
            string exceptionText = null;
            try
            {
                theEvent.Get(expression);
                Assert.Fail();
            }
            catch (PropertyAccessException ex)
            {
                exceptionText = ex.Message;
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".GetSyntaxExceptionProperty expression=" + expression, ex);
                }
                // Expected exception
            }
    
            return exceptionText;
        }
    
        private EPStatementException GetStatementExceptionView(string expression) 
        {
            return GetStatementExceptionView(expression, false);
        }
    
        private EPStatementException GetStatementExceptionView(string expression, bool isLogException) 
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(expression, "MyStatement");
                Assert.Fail();
            }
            catch (EPStatementSyntaxException)
            {
                throw;
            }
            catch (EPStatementException ex)
            {
                // Expected exception
                if (isLogException)
                {
                    Log.Debug(".GetStatementExceptionView expression=" + expression, ex);
                }

                return ex;
            }

            throw new IllegalStateException();
        }
    
        private void TryValid(string viewStmt)
        {
            _epService.EPAdministrator.CreateEPL(viewStmt);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
