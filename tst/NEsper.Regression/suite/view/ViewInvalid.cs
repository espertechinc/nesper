///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using static com.espertech.esper.regressionlib.framework.RegressionFlag; // INVALIDITY
using NUnit.Framework;

using SupportBean_N = com.espertech.esper.regressionlib.support.bean.SupportBean_N; // assertTrue

// fail

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewInvalid : RegressionExecution
    {
        private const string EVENT_NUM = nameof(SupportBean_N);
        private const string EVENT_ALLTYPES = nameof(SupportBean);

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(INVALIDITY);
        }

        public void Run(RegressionEnvironment env)
        {
            RunAssertionInvalidPropertyExpression(env);
            RunAssertionInvalidSyntax(env);
            RunAssertionStatementException(env);
            RunAssertionInvalidView(env);
        }

        private void RunAssertionInvalidPropertyExpression(RegressionEnvironment env)
        {
            var epl = "@name('s0') @IterableUnbound select * from SupportBean";
            env.CompileDeploy(epl);
            env.SendEventBean(new SupportBean());
            env.AssertIterator(
                "s0",
                enumerator => {
                    var theEvent = enumerator.Advance();

                    var exceptionText = GetSyntaxExceptionProperty("", theEvent);
                    Assert.IsTrue(
                        exceptionText.StartsWith("Property named '' is not a valid property name for this type"));

                    exceptionText = GetSyntaxExceptionProperty("-", theEvent);
                    Assert.IsTrue(
                        exceptionText.StartsWith("Property named '-' is not a valid property name for this type"));

                    exceptionText = GetSyntaxExceptionProperty("a[]", theEvent);
                    Assert.IsTrue(
                        exceptionText.StartsWith("Property named 'a[]' is not a valid property name for this type"));
                });

            env.UndeployAll();
        }

        private void RunAssertionInvalidSyntax(RegressionEnvironment env)
        {
            // keyword in select clause
            var exception = GetSyntaxExceptionView(env, "select inner from MyStream");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Incorrect syntax near 'inner' (a reserved keyword) at line 1 column 7, please check the select clause");

            // keyword in from clause
            exception = GetSyntaxExceptionView(env, "select something from Outer");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Incorrect syntax near 'Outer' (a reserved keyword) at line 1 column 22, please check the from clause");

            // keyword used in package
            exception = GetSyntaxExceptionView(env, "select * from com.true.mycompany.MyEvent");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Incorrect syntax near 'true' (a reserved keyword) expecting an identifier but found 'true' at line 1 column 18, please check the view specifications within the from clause");

            // keyword as part of identifier
            exception = GetSyntaxExceptionView(env, "select * from MyEvent, MyEvent2 where a.day=b.day");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Incorrect syntax near 'day' (a reserved keyword) at line 1 column 40, please check the where clause");

            exception = GetSyntaxExceptionView(env, "select * * from " + EVENT_NUM);
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Incorrect syntax near '*' at line 1 column 9 near reserved keyword 'from'");

            // keyword in select clause
            exception = GetSyntaxExceptionView(env, "select day from MyEvent, MyEvent2");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Incorrect syntax near 'day' (a reserved keyword) at line 1 column 7, please check the select clause");
        }

        private void RunAssertionStatementException(RegressionEnvironment env)
        {
            EPCompileExceptionItem exception;

            // property near to spelling
            exception = GetStatementExceptionView(env, "select s0.intPrimitv from SupportBean as s0");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate select-clause expression 's0.intPrimitv': Property named 'intPrimitv' is not valid in stream 's0' (did you mean 'intPrimitive'?)");

            exception = GetStatementExceptionView(env, "select INTPRIMITIVE from SupportBean");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate select-clause expression 'INTPRIMITIVE': Property named 'INTPRIMITIVE' is not valid in any stream (did you mean 'intPrimitive'?)");

            exception = GetStatementExceptionView(env, "select theStrring from SupportBean");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate select-clause expression 'theStrring': Property named 'theStrring' is not valid in any stream (did you mean 'theString'?)");

            // aggregation in where clause known
            exception = GetStatementExceptionView(env, "select * from SupportBean where sum(intPrimitive) > 10");
            SupportMessageAssertUtil.AssertMessage(exception, "Aggregation functions not allowed within filters");

            // class not found
            exception = GetStatementExceptionView(env, "select * from dummypkg.dummy()#length(10)");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to resolve event type, named window or table by name 'dummypkg.dummy'");

            // invalid view
            exception = GetStatementExceptionView(env, "select * from " + EVENT_NUM + ".dummy:dummy(10)");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate data window declaration: View name 'dummy:dummy' is not a known view name");

            // keyword used
            exception = GetSyntaxExceptionView(env, "select order from SupportBean");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Incorrect syntax near 'order' (a reserved keyword) at line 1 column 7, please check the select clause");

            // invalid view parameter
            exception = GetStatementExceptionView(env, "select * from " + EVENT_NUM + "#length('s')");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate data window declaration: Error in view 'length', Length view requires a single integer-type parameter");

            // where-clause relational op has invalid type
            exception = GetStatementExceptionView(
                env,
                "select * from " + EVENT_ALLTYPES + "#length(1) where theString > 5");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate expression: Failed to validate filter expression 'theString>5': Implicit conversion from datatype 'String' to numeric is not allowed");

            // where-clause has aggregation function
            exception = GetStatementExceptionView(
                env,
                "select * from " + EVENT_ALLTYPES + "#length(1) where sum(intPrimitive) > 5");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate expression: An aggregate function may not appear in a WHERE clause (use the HAVING clause)");

            // invalid numerical expression
            exception = GetStatementExceptionView(env, "select 2 * 's' from " + EVENT_ALLTYPES + "#length(1)");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate select-clause expression '2*\"s\"': Implicit conversion from datatype 'String' to numeric is not allowed");

            // invalid property in select
            exception = GetStatementExceptionView(env, "select a[2].m('a') from " + EVENT_ALLTYPES + "#length(1)");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate select-clause expression 'a[2].m('a')': Failed to resolve enumeration method, date-time method or mapped property 'a[2].m('a')': Failed to resolve 'a[2].m' to a property, single-row function, aggregation function, script, stream or class name");

            // select clause uses same "as" name twice
            exception = GetStatementExceptionView(env, "select 2 as m, 2 as m from " + EVENT_ALLTYPES + "#length(1)");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Column name 'm' appears more then once in select clause");

            // class in method invocation not found
            exception = GetStatementExceptionView(
                env,
                "select unknownClass.method() from " + EVENT_NUM + "#length(10)");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate select-clause expression 'unknownClass.method()': Failed to resolve 'unknownClass.method' to a property, single-row function, aggregation function, script, stream or class name");

            // method not found
            exception = GetStatementExceptionView(env, "select Math.unknownMethod() from " + EVENT_NUM + "#length(10)");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate select-clause expression 'Math.unknownMethod()': Failed to resolve 'Math.unknownMethod' to a property, single-row function, aggregation function, script, stream or class name");

            // invalid property in group-by
            exception = GetStatementExceptionView(
                env,
                "select intPrimitive from " + EVENT_ALLTYPES + "#length(1) group by xxx");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate group-by-clause expression 'xxx': Property named 'xxx' is not valid in any stream");

            // group-by not specifying a property
            exception = GetStatementExceptionView(
                env,
                "select intPrimitive from " + EVENT_ALLTYPES + "#length(1) group by 5");
            SupportMessageAssertUtil.AssertMessage(exception, "Group-by expressions must refer to property names");

            // group-by specifying aggregates
            exception = GetStatementExceptionView(
                env,
                "select intPrimitive from " + EVENT_ALLTYPES + "#length(1) group by sum(intPrimitive)");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Group-by expressions cannot contain aggregate functions");

            // invalid property in having clause
            exception = GetStatementExceptionView(
                env,
                "select 2 * 's' from " + EVENT_ALLTYPES + "#length(1) group by intPrimitive having xxx > 5");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate select-clause expression '2*\"s\"': Implicit conversion from datatype 'String' to numeric is not allowed");

            // invalid having clause - not a symbol in the group-by (non-aggregate)
            exception = GetStatementExceptionView(
                env,
                "select sum(intPrimitive) from " +
                EVENT_ALLTYPES +
                "#length(1) group by intBoxed having doubleBoxed > 5");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Non-aggregated property 'doubleBoxed' in the HAVING clause must occur in the group-by clause");

            // invalid outer join - not a symbol
            exception = GetStatementExceptionView(
                env,
                "select * from " +
                EVENT_ALLTYPES +
                "#length(1) as aStr " +
                "left outer join " +
                EVENT_ALLTYPES +
                "#length(1) on xxxx=yyyy");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate outer-join expression: Failed to validate on-clause join expression 'xxxx=yyyy': Property named 'xxxx' is not valid in any stream");

            // invalid outer join for 3 streams - not a symbol
            exception = GetStatementExceptionView(
                env,
                "select * from " +
                EVENT_ALLTYPES +
                "#length(1) as s0 " +
                "left outer join " +
                EVENT_ALLTYPES +
                "#length(1) as s1 on s0.intPrimitive = s1.intPrimitive " +
                "left outer join " +
                EVENT_ALLTYPES +
                "#length(1) as s2 on s0.intPrimitive = s2.yyyy");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate outer-join expression: Failed to validate on-clause join expression 's0.intPrimitive=s2.yyyy': Failed to resolve property 's2.yyyy' to a stream or nested property in a stream");

            // invalid outer join for 3 streams - wrong stream, the properties in on-clause don't refer to streams
            exception = GetStatementExceptionView(
                env,
                "select * from " +
                EVENT_ALLTYPES +
                "#length(1) as s0 " +
                "left outer join " +
                EVENT_ALLTYPES +
                "#length(1) as s1 on s0.intPrimitive = s1.intPrimitive " +
                "left outer join " +
                EVENT_ALLTYPES +
                "#length(1) as s2 on s0.intPrimitive = s1.intPrimitive");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate outer-join expression: Outer join ON-clause must refer to at least one property of the joined stream for stream 2");

            // invalid outer join - referencing next stream
            exception = GetStatementExceptionView(
                env,
                "select * from " +
                EVENT_ALLTYPES +
                "#length(1) as s0 " +
                "left outer join " +
                EVENT_ALLTYPES +
                "#length(1) as s1 on s2.intPrimitive = s1.intPrimitive " +
                "left outer join " +
                EVENT_ALLTYPES +
                "#length(1) as s2 on s1.intPrimitive = s2.intPrimitive");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate outer-join expression: Outer join ON-clause invalid scope for property 'intPrimitive', expecting the current or a prior stream scope");

            // invalid outer join - same properties
            exception = GetStatementExceptionView(
                env,
                "select * from " +
                EVENT_NUM +
                "#length(1) as aStr " +
                "left outer join " +
                EVENT_ALLTYPES +
                "#length(1) on theString=theString");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate outer-join expression: Outer join ON-clause cannot refer to properties of the same stream");

            // invalid order by
            exception = GetStatementExceptionView(env, "select * from " + EVENT_NUM + "#length(1) as aStr order by X");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate order-by-clause expression 'X': Property named 'X' is not valid in any stream");

            // insert into with wildcard - not allowed
            exception = GetStatementExceptionView(
                env,
                "insert into Google (a, b) select * from " + EVENT_NUM + "#length(1) as aStr");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Wildcard not allowed when insert-into specifies column order");

            // insert into with duplicate column names
            exception = GetStatementExceptionView(
                env,
                "insert into Google (a, b, a) select boolBoxed, boolPrimitive, intBoxed from " +
                EVENT_NUM +
                "#length(1) as aStr");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Property name 'a' appears more then once in insert-into clause");

            // insert into mismatches selected columns
            exception = GetStatementExceptionView(
                env,
                "insert into Google (a, b, c) select boolBoxed, boolPrimitive from " +
                EVENT_NUM +
                "#length(1) as aStr");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Number of supplied values in the select or values clause does not match insert-into clause");

            // mismatched type on coalesce columns
            exception = GetStatementExceptionView(
                env,
                "select coalesce(boolBoxed, theString) from SupportBean#length(1) as aStr");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate select-clause expression 'coalesce(boolBoxed,theString)': Implicit conversion not allowed: Cannot coerce to Boolean type String");

            // mismatched case compare type
            exception = GetStatementExceptionView(
                env,
                "select case boolPrimitive when 1 then true end from SupportBean#length(1) as aStr");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate select-clause expression 'case boolPrimitive when 1 then true end': Implicit conversion not allowed: Cannot coerce to Boolean type Integer");

            // mismatched case result type
            exception = GetStatementExceptionView(
                env,
                "select case when 1=2 then 1 when 1=3 then true end from SupportBean#length(1) as aStr");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate select-clause expression 'case when 1=2 then 1 when 1=3 then ...(43 chars)': Implicit conversion not allowed: Cannot coerce types Integer and Boolean");

            // case expression not returning bool
            exception = GetStatementExceptionView(
                env,
                "select case when 3 then 1 end from SupportBean#length(1) as aStr");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate select-clause expression 'case when 3 then 1 end': Case node 'when' expressions must return a boolean value");

            // function not known
            exception = GetStatementExceptionView(env, "select gogglex(1) from " + EVENT_NUM + "#length(1)");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate select-clause expression 'gogglex(1)': Unknown single-row function, aggregation function or mapped or indexed property named 'gogglex' could not be resolved");

            // insert into column name incorrect
            exception = GetStatementExceptionView(
                env,
                "insert into Xyz select 1 as dodi from SupportBean;\n" +
                "select pox from pattern[Xyz(yodo=4)]");
            SupportMessageAssertUtil.AssertMessage(
                exception,
                "Failed to validate filter expression 'yodo=4': Property named 'yodo' is not valid in any stream (did you mean 'dodi'?)");
            env.UndeployAll();
        }

        private void RunAssertionInvalidView(RegressionEnvironment env)
        {
            TryInvalid(env, "select * from SupportBean(dummy='a')#length(3)");
            TryValid(env, "select * from SupportBean(theString='a')#length(3)");
            TryInvalid(env, "select * from SupportBean.dummy:length(3)");

            TryInvalid(env, "select djdjdj from SupportBean#length(3)");
            TryValid(env, "select boolBoxed as xx, intPrimitive from SupportBean#length(3)");
            TryInvalid(env, "select boolBoxed as xx, intPrimitive as xx from SupportBean#length(3)");
            TryValid(env, "select boolBoxed as xx, intPrimitive as yy from SupportBean()#length(3)");

            TryValid(
                env,
                "select boolBoxed as xx, intPrimitive as yy from SupportBean()#length(3) where boolBoxed = true");
            TryInvalid(env, "select boolBoxed as xx, intPrimitive as yy from SupportBean()#length(3) where xx = true");
        }

        private void TryInvalid(
            RegressionEnvironment env,
            string epl)
        {
            try {
                env.CompileWCheckedEx(epl);
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                // Expected exception
            }
        }

        private EPCompileExceptionSyntaxItem GetSyntaxExceptionView(
            RegressionEnvironment env,
            string expression)
        {
            try {
                env.CompileWCheckedEx(expression);
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                if (log.IsDebugEnabled) {
                    log.Debug(".getSyntaxExceptionView expression=" + expression, ex);
                }

                // Expected exception
                return (EPCompileExceptionSyntaxItem)ex.Items[0];
            }

            throw new IllegalStateException();
        }

        private string GetSyntaxExceptionProperty(
            string expression,
            EventBean theEvent)
        {
            string exceptionText = null;
            try {
                theEvent.Get(expression);
                Assert.Fail();
            }
            catch (PropertyAccessException ex) {
                exceptionText = ex.Message;
                if (log.IsDebugEnabled) {
                    log.Debug(".getSyntaxExceptionProperty expression=" + expression, ex);
                }
                // Expected exception
            }

            return exceptionText;
        }

        private EPCompileExceptionItem GetStatementExceptionView(
            RegressionEnvironment env,
            string expression)
        {
            return GetStatementExceptionView(env, expression, false);
        }

        private EPCompileExceptionItem GetStatementExceptionView(
            RegressionEnvironment env,
            string expression,
            bool isLogException)
        {
            try {
                env.CompileWCheckedEx(expression);
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                var first = ex.Items[0];
                if (isLogException) {
                    log.Debug(".getStatementExceptionView expression=" + first, first);
                }

                if (first is EPCompileExceptionSyntaxItem) {
                    Assert.Fail();
                }

                return first;
            }

            throw new IllegalStateException();
        }

        private void TryValid(
            RegressionEnvironment env,
            string epl)
        {
            env.Compile(epl);
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(ViewInvalid));
    }
} // end of namespace