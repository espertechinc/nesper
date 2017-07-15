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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

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
            configuration.EngineDefaults.typeof(EventMetaConfig)PropertyResolutionStyle = PropertyResolutionStyle.CASE_SENSITIVE;
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
            Assert.AreEqual("Unexpected end-of-input []", exceptionText);
    
            exceptionText = GetSyntaxExceptionProperty("-", theEvent);
            Assert.AreEqual("Incorrect syntax near '-' [-]", exceptionText);
    
            exceptionText = GetSyntaxExceptionProperty("a[]", theEvent);
            Assert.AreEqual("Incorrect syntax near ']' expecting any of the following tokens {IntegerLiteral, FloatingPointLiteral} but found a right angle bracket ']' at line 1 column 2 [a[]]", exceptionText);
        }
    
        [Test]
        public void TestInvalidSyntax()
        {
            // keyword in select clause
            string exceptionText = GetSyntaxExceptionView("select inner from MyStream");
            Assert.AreEqual("Incorrect syntax near 'inner' (a reserved keyword) at line 1 column 7, please check the select clause [select inner from MyStream]", exceptionText);
    
            // keyword in from clause
            exceptionText = GetSyntaxExceptionView("select something from Outer");
            Assert.AreEqual("Incorrect syntax near 'Outer' (a reserved keyword) at line 1 column 22, please check the from clause [select something from Outer]", exceptionText);
    
            // keyword used in package
            exceptionText = GetSyntaxExceptionView("select * from com.true.mycompany.MyEvent");
            Assert.AreEqual("Incorrect syntax near 'true' (a reserved keyword) expecting an identifier but found 'true' at line 1 column 18, please check the view specifications within the from clause [select * from com.true.mycompany.MyEvent]", exceptionText);
    
            // keyword as part of identifier
            exceptionText = GetSyntaxExceptionView("select * from MyEvent, MyEvent2 where a.day=b.day");
            Assert.AreEqual("Incorrect syntax near 'day' (a reserved keyword) at line 1 column 40, please check the where clause [select * from MyEvent, MyEvent2 where a.day=b.day]", exceptionText);
    
            exceptionText = GetSyntaxExceptionView("select * * from " + EVENT_NUM);
            Assert.AreEqual("Incorrect syntax near '*' at line 1 column 9 near reserved keyword 'from' [select * * from com.espertech.esper.support.bean.SupportBean_N]", exceptionText);
    
            // keyword in select clause
            exceptionText = GetSyntaxExceptionView("select day from MyEvent, MyEvent2");
            Assert.AreEqual("Incorrect syntax near 'day' (a reserved keyword) at line 1 column 7, please check the select clause [select day from MyEvent, MyEvent2]", exceptionText);
        }
    
        [Test]
        public void TestStatementException() 
        {
            string exceptionText = null;
    
            // property near to spelling
            exceptionText = GetStatementExceptionView("select s0.intPrimitv from " + typeof(SupportBean).FullName + " as s0");
            Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 's0.intPrimitv': Property named 'intPrimitv' is not valid in stream 's0' (did you mean 'IntPrimitive'?) [select s0.intPrimitv from com.espertech.esper.support.bean.SupportBean as s0]", exceptionText);

            exceptionText = GetStatementExceptionView("select INTPRIMITIVE from " + typeof(SupportBean).FullName);
            Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'INTPRIMITIVE': Property named 'INTPRIMITIVE' is not valid in any stream (did you mean 'IntPrimitive'?) [select INTPRIMITIVE from com.espertech.esper.support.bean.SupportBean]", exceptionText);

            exceptionText = GetStatementExceptionView("select theStrring from " + typeof(SupportBean).FullName);
            Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'theStrring': Property named 'theStrring' is not valid in any stream (did you mean 'TheString'?) [select theStrring from com.espertech.esper.support.bean.SupportBean]", exceptionText);
    
            // aggregation in where clause known
            exceptionText = GetStatementExceptionView("select * from " + typeof(SupportBean).FullName + " where sum(IntPrimitive) > 10");
            Assert.AreEqual("Aggregation functions not allowed within filters [select * from com.espertech.esper.support.bean.SupportBean where sum(IntPrimitive) > 10]", exceptionText);
    
            // class not found
            exceptionText = GetStatementExceptionView("select * from dummypkg.dummy().win:length(10)");
            Assert.AreEqual("Failed to resolve event type: Event type or class named 'dummypkg.dummy' was not found [select * from dummypkg.dummy().win:length(10)]", exceptionText);
    
            // invalid view
            exceptionText = GetStatementExceptionView("select * from " + EVENT_NUM + ".dummy:dummy(10)");
            Assert.AreEqual("Error starting statement: View name 'dummy:dummy' is not a known view name [select * from com.espertech.esper.support.bean.SupportBean_N.dummy:dummy(10)]", exceptionText);
    
            // keyword used
            exceptionText = GetSyntaxExceptionView("select order from " + typeof(SupportBean).FullName);
            Assert.AreEqual("Incorrect syntax near 'order' (a reserved keyword) at line 1 column 7, please check the select clause [select order from com.espertech.esper.support.bean.SupportBean]", exceptionText);
    
            // invalid view parameter
            exceptionText = GetStatementExceptionView("select * from " + EVENT_NUM + ".win:length('s')");
            Assert.AreEqual("Error starting statement: Error in view 'win:length', Length view requires a single integer-type parameter [select * from com.espertech.esper.support.bean.SupportBean_N.win:length('s')]", exceptionText);
    
            // where-clause relational op has invalid type
            exceptionText = GetStatementExceptionView("select * from " + EVENT_ALLTYPES + ".win:length(1) where TheString > 5");
            Assert.AreEqual("Error validating expression: Failed to validate filter expression 'TheString>5': Implicit conversion from datatype 'System.String' to numeric is not allowed [select * from com.espertech.esper.support.bean.SupportBean.win:length(1) where TheString > 5]", exceptionText);
    
            // where-clause has aggregation function
            exceptionText = GetStatementExceptionView("select * from " + EVENT_ALLTYPES + ".win:length(1) where sum(IntPrimitive) > 5");
            Assert.AreEqual("Error validating expression: An aggregate function may not appear in a WHERE clause (use the HAVING clause) [select * from com.espertech.esper.support.bean.SupportBean.win:length(1) where sum(IntPrimitive) > 5]", exceptionText);
    
            // invalid numerical expression
            exceptionText = GetStatementExceptionView("select 2 * 's' from " + EVENT_ALLTYPES + ".win:length(1)");
            Assert.AreEqual("Error starting statement: Failed to validate select-clause expression '2*\"s\"': Implicit conversion from datatype 'System.String' to numeric is not allowed [select 2 * 's' from com.espertech.esper.support.bean.SupportBean.win:length(1)]", exceptionText);
    
            // invalid property in select
            exceptionText = GetStatementExceptionView("select a[2].m('a') from " + EVENT_ALLTYPES + ".win:length(1)");
            Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'a[2].m('a')': Failed to resolve enumeration method, date-time method or mapped property 'a[2].m('a')': Failed to resolve 'a[2].m' to a property, single-row function, aggregation function, script, stream or class name [select a[2].m('a') from com.espertech.esper.support.bean.SupportBean.win:length(1)]", exceptionText);
    
            // select clause uses same "as" name twice
            exceptionText = GetStatementExceptionView("select 2 as m, 2 as m from " + EVENT_ALLTYPES + ".win:length(1)");
            Assert.AreEqual("Error starting statement: Column name 'm' appears more then once in select clause [select 2 as m, 2 as m from com.espertech.esper.support.bean.SupportBean.win:length(1)]", exceptionText);
    
            // class in method invocation not found
            exceptionText = GetStatementExceptionView("select unknownClass.method() from " + EVENT_NUM + ".win:length(10)");
            Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'unknownClass.method()': Failed to resolve 'unknownClass.method' to a property, single-row function, aggregation function, script, stream or class name [select unknownClass.method() from com.espertech.esper.support.bean.SupportBean_N.win:length(10)]", exceptionText);
    
            // method not found
            exceptionText = GetStatementExceptionView("select Math.unknownMethod() from " + EVENT_NUM + ".win:length(10)");
            Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'Math.unknownMethod()': Failed to resolve 'Math.unknownMethod' to a property, single-row function, aggregation function, script, stream or class name [select Math.unknownMethod() from com.espertech.esper.support.bean.SupportBean_N.win:length(10)]", exceptionText);
    
            // invalid property in group-by
            exceptionText = GetStatementExceptionView("select IntPrimitive from " + EVENT_ALLTYPES + ".win:length(1) group by xxx");
            Assert.AreEqual("Error starting statement: Failed to validate group-by-clause expression 'xxx': Property named 'xxx' is not valid in any stream [select IntPrimitive from com.espertech.esper.support.bean.SupportBean.win:length(1) group by xxx]", exceptionText);
    
            // group-by not specifying a property
            exceptionText = GetStatementExceptionView("select IntPrimitive from " + EVENT_ALLTYPES + ".win:length(1) group by 5");
            Assert.AreEqual("Error starting statement: Group-by expressions must refer to property names [select IntPrimitive from com.espertech.esper.support.bean.SupportBean.win:length(1) group by 5]", exceptionText);
    
            // group-by specifying aggregates
            exceptionText = GetStatementExceptionView("select IntPrimitive from " + EVENT_ALLTYPES + ".win:length(1) group by sum(IntPrimitive)");
            Assert.AreEqual("Error starting statement: Group-by expressions cannot contain aggregate functions [select IntPrimitive from com.espertech.esper.support.bean.SupportBean.win:length(1) group by sum(IntPrimitive)]", exceptionText);
    
            // invalid property in having clause
            exceptionText = GetStatementExceptionView("select 2 * 's' from " + EVENT_ALLTYPES + ".win:length(1) group by IntPrimitive having xxx > 5");
            Assert.AreEqual("Error starting statement: Failed to validate select-clause expression '2*\"s\"': Implicit conversion from datatype 'System.String' to numeric is not allowed [select 2 * 's' from com.espertech.esper.support.bean.SupportBean.win:length(1) group by IntPrimitive having xxx > 5]", exceptionText);
    
            // invalid having clause - not a symbol in the group-by (non-aggregate)
            exceptionText = GetStatementExceptionView("select sum(IntPrimitive) from " + EVENT_ALLTYPES + ".win:length(1) group by IntBoxed having DoubleBoxed > 5");
            Assert.AreEqual("Error starting statement: Non-aggregated property 'DoubleBoxed' in the HAVING clause must occur in the group-by clause [select sum(IntPrimitive) from com.espertech.esper.support.bean.SupportBean.win:length(1) group by IntBoxed having DoubleBoxed > 5]", exceptionText);
    
            // invalid outer join - not a symbol
            exceptionText = GetStatementExceptionView("select * from " + EVENT_ALLTYPES + ".win:length(1) as aStr " +
                    "left outer join " + EVENT_ALLTYPES + ".win:length(1) on xxxx=yyyy");
            Assert.AreEqual("Error validating expression: Failed to validate on-clause join expression 'xxxx=yyyy': Property named 'xxxx' is not valid in any stream [select * from com.espertech.esper.support.bean.SupportBean.win:length(1) as aStr left outer join com.espertech.esper.support.bean.SupportBean.win:length(1) on xxxx=yyyy]", exceptionText);
    
            // invalid outer join for 3 streams - not a symbol
            exceptionText = GetStatementExceptionView("select * from " + EVENT_ALLTYPES + ".win:length(1) as s0 " +
                    "left outer join " + EVENT_ALLTYPES + ".win:length(1) as s1 on s0.IntPrimitive = s1.IntPrimitive " +
                    "left outer join " + EVENT_ALLTYPES + ".win:length(1) as s2 on s0.IntPrimitive = s2.yyyy");
            Assert.AreEqual("Error validating expression: Failed to validate on-clause join expression 's0.IntPrimitive=s2.yyyy': Failed to resolve property 's2.yyyy' to a stream or nested property in a stream [select * from com.espertech.esper.support.bean.SupportBean.win:length(1) as s0 left outer join com.espertech.esper.support.bean.SupportBean.win:length(1) as s1 on s0.IntPrimitive = s1.IntPrimitive left outer join com.espertech.esper.support.bean.SupportBean.win:length(1) as s2 on s0.IntPrimitive = s2.yyyy]", exceptionText);
    
            // invalid outer join for 3 streams - wrong stream, the properties in on-clause don't refer to streams
            exceptionText = GetStatementExceptionView("select * from " + EVENT_ALLTYPES + ".win:length(1) as s0 " +
                    "left outer join " + EVENT_ALLTYPES + ".win:length(1) as s1 on s0.IntPrimitive = s1.IntPrimitive " +
                    "left outer join " + EVENT_ALLTYPES + ".win:length(1) as s2 on s0.IntPrimitive = s1.IntPrimitive");
            Assert.AreEqual("Error validating expression: Outer join ON-clause must refer to at least one property of the joined stream for stream 2 [select * from com.espertech.esper.support.bean.SupportBean.win:length(1) as s0 left outer join com.espertech.esper.support.bean.SupportBean.win:length(1) as s1 on s0.IntPrimitive = s1.IntPrimitive left outer join com.espertech.esper.support.bean.SupportBean.win:length(1) as s2 on s0.IntPrimitive = s1.IntPrimitive]", exceptionText);
    
            // invalid outer join - referencing next stream
            exceptionText = GetStatementExceptionView("select * from " + EVENT_ALLTYPES + ".win:length(1) as s0 " +
                    "left outer join " + EVENT_ALLTYPES + ".win:length(1) as s1 on s2.IntPrimitive = s1.IntPrimitive " +
                    "left outer join " + EVENT_ALLTYPES + ".win:length(1) as s2 on s1.IntPrimitive = s2.IntPrimitive");
            Assert.AreEqual("Error validating expression: Outer join ON-clause invalid scope for property 'IntPrimitive', expecting the current or a prior stream scope [select * from com.espertech.esper.support.bean.SupportBean.win:length(1) as s0 left outer join com.espertech.esper.support.bean.SupportBean.win:length(1) as s1 on s2.IntPrimitive = s1.IntPrimitive left outer join com.espertech.esper.support.bean.SupportBean.win:length(1) as s2 on s1.IntPrimitive = s2.IntPrimitive]", exceptionText);
    
            // invalid outer join - same properties
            exceptionText = GetStatementExceptionView("select * from " + EVENT_NUM + ".win:length(1) as aStr " +
                    "left outer join " + EVENT_ALLTYPES + ".win:length(1) on TheString=TheString");
            Assert.AreEqual("Error validating expression: Outer join ON-clause cannot refer to properties of the same stream [select * from com.espertech.esper.support.bean.SupportBean_N.win:length(1) as aStr left outer join com.espertech.esper.support.bean.SupportBean.win:length(1) on TheString=TheString]", exceptionText);
    
            // invalid order by
            exceptionText = GetStatementExceptionView("select * from " + EVENT_NUM + ".win:length(1) as aStr order by X");
            Assert.AreEqual("Error starting statement: Failed to validate order-by-clause expression 'X': Property named 'X' is not valid in any stream [select * from com.espertech.esper.support.bean.SupportBean_N.win:length(1) as aStr order by X]", exceptionText);
    
            // insert into with wildcard - not allowed
            exceptionText = GetStatementExceptionView("insert into Google (a, b) select * from " + EVENT_NUM + ".win:length(1) as aStr");
            Assert.AreEqual("Error starting statement: Wildcard not allowed when insert-into specifies column order [insert into Google (a, b) select * from com.espertech.esper.support.bean.SupportBean_N.win:length(1) as aStr]", exceptionText);
    
            // insert into with duplicate column names
            exceptionText = GetStatementExceptionView("insert into Google (a, b, a) select BoolBoxed, BoolPrimitive, IntBoxed from " + EVENT_NUM + ".win:length(1) as aStr");
            Assert.AreEqual("Error starting statement: Property name 'a' appears more then once in insert-into clause [insert into Google (a, b, a) select BoolBoxed, BoolPrimitive, IntBoxed from com.espertech.esper.support.bean.SupportBean_N.win:length(1) as aStr]", exceptionText);
    
            // insert into mismatches selected columns
            exceptionText = GetStatementExceptionView("insert into Google (a, b, c) select BoolBoxed, BoolPrimitive from " + EVENT_NUM + ".win:length(1) as aStr");
            Assert.AreEqual("Error starting statement: Number of supplied values in the select or values clause does not match insert-into clause [insert into Google (a, b, c) select BoolBoxed, BoolPrimitive from com.espertech.esper.support.bean.SupportBean_N.win:length(1) as aStr]", exceptionText);
    
            // mismatched type on coalesce columns
            exceptionText = GetStatementExceptionView("select coalesce(BoolBoxed, TheString) from " + typeof(SupportBean).FullName + ".win:length(1) as aStr");
            Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'coalesce(BoolBoxed,TheString)': Implicit conversion not allowed: Cannot coerce to bool type System.String [select coalesce(BoolBoxed, TheString) from com.espertech.esper.support.bean.SupportBean.win:length(1) as aStr]", exceptionText);
    
            // mismatched case compare type
            exceptionText = GetStatementExceptionView("select case BoolPrimitive when 1 then true end from " + typeof(SupportBean).FullName + ".win:length(1) as aStr");
            Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'case BoolPrimitive when 1 then true end': Implicit conversion not allowed: Cannot coerce to bool type " + Name.Of<int>() + " [select case BoolPrimitive when 1 then true end from com.espertech.esper.support.bean.SupportBean.win:length(1) as aStr]", exceptionText);
    
            // mismatched case result type
            exceptionText = GetStatementExceptionView("select case when 1=2 then 1 when 1=3 then true end from " + typeof(SupportBean).FullName + ".win:length(1) as aStr");
            Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'case when 1=2 then 1 when 1=3 then ...(43 chars)': Implicit conversion not allowed: Cannot coerce types " + Name.Of<int>() + " and " + Name.Of<bool>() + " [select case when 1=2 then 1 when 1=3 then true end from com.espertech.esper.support.bean.SupportBean.win:length(1) as aStr]", exceptionText);
    
            // case expression not returning bool
            exceptionText = GetStatementExceptionView("select case when 3 then 1 end from " + typeof(SupportBean).FullName + ".win:length(1) as aStr");
            Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'case when 3 then 1 end': Case node 'when' expressions must return a boolean value [select case when 3 then 1 end from com.espertech.esper.support.bean.SupportBean.win:length(1) as aStr]", exceptionText);
    
            // function not known
            exceptionText = GetStatementExceptionView("select gogglex(1) from " + EVENT_NUM + ".win:length(1)");
            Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'gogglex(1)': Unknown single-row function, aggregation function or mapped or indexed property named 'gogglex' could not be resolved [select gogglex(1) from com.espertech.esper.support.bean.SupportBean_N.win:length(1)]", exceptionText);
    
            // insert into column name incorrect
            _epService.EPAdministrator.CreateEPL("insert into Xyz select 1 as dodi from System.String");
            exceptionText = GetStatementExceptionView("select pox from pattern[Xyz(yodo=4)]");
            Assert.AreEqual("Failed to validate filter expression 'yodo=4': Property named 'yodo' is not valid in any stream (did you mean 'dodi'?) [select pox from pattern[Xyz(yodo=4)]]", exceptionText);
        }
    
        [Test]
        public void TestInvalidViewX()
        {
            string eventClass = typeof(SupportBean).FullName;
    
            TryInvalid("select * from " + eventClass + "(dummy='a').win:length(3)");
            TryValid("select * from " + eventClass + "(TheString='a').win:length(3)");
            TryInvalid("select * from " + eventClass + ".dummy:length(3)");
    
            TryInvalid("select djdjdj from " + eventClass + ".win:length(3)");
            TryValid("select BoolBoxed as xx, IntPrimitive from " + eventClass + ".win:length(3)");
            TryInvalid("select BoolBoxed as xx, IntPrimitive as xx from " + eventClass + ".win:length(3)");
            TryValid("select BoolBoxed as xx, IntPrimitive as yy from " + eventClass + "().win:length(3)");

            TryValid("select BoolBoxed as xx, IntPrimitive as yy from " + eventClass + "().win:length(3)" +
                    " where BoolBoxed = true");
            TryInvalid("select BoolBoxed as xx, IntPrimitive as yy from " + eventClass + "().win:length(3)" +
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
    
        private string GetSyntaxExceptionView(string expression)
        {
            string exceptionText = null;
            try
            {
                _epService.EPAdministrator.CreateEPL(expression);
                Assert.Fail();
            }
            catch (EPStatementSyntaxException ex)
            {
                exceptionText = ex.Message;
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".getSyntaxExceptionView expression=" + expression, ex);
                }
                // Expected exception
            }
    
            return exceptionText;
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
                    Log.Debug(".getSyntaxExceptionProperty expression=" + expression, ex);
                }
                // Expected exception
            }
    
            return exceptionText;
        }
    
        private string GetStatementExceptionView(string expression) 
        {
            return GetStatementExceptionView(expression, false);
        }
    
        private string GetStatementExceptionView(string expression, bool isLogException) 
        {
            string exceptionText = null;
            try
            {
                _epService.EPAdministrator.CreateEPL(expression, "MyStatement");
                Assert.Fail();
            }
            catch (EPStatementSyntaxException es)
            {
                throw;
            }
            catch (EPStatementException ex)
            {
                // Expected exception
                exceptionText = ex.Message;
                if (isLogException)
                {
                    Log.Debug(".getStatementExceptionView expression=" + expression, ex);
                }
            }
    
            Assert.IsNull(_epService.EPAdministrator.GetStatement("MyStatement"));
    
            return exceptionText;
        }
    
        private void TryValid(string viewStmt)
        {
            _epService.EPAdministrator.CreateEPL(viewStmt);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
