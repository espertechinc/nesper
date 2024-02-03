///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTInvalid : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            string epl;

            // invalid incompatible params
            epl = "select Contained.set('hour', 1) from SupportBean_ST0_Container";
            env.TryInvalidCompile(
                epl,
                "Failed to validate select-clause expression 'Contained.set(\"hour\",1)': Date-time enumeration method 'set' requires either a DateTimeEx, DateTimeOffset, DateTime, or long value as input or events of an event type that declares a timestamp property but received collection of events of type '" +
                typeof(SupportBean_ST0).CleanName() +
                "'");

            // invalid incompatible params
            epl = "select window(*).set('hour', 1) from SupportBean#keepall";
            env.TryInvalidCompile(
                epl,
                "Failed to validate select-clause expression 'window(*).set(\"hour\",1)': Date-time enumeration method 'set' requires either a DateTimeEx, DateTimeOffset, DateTime, or long value as input or events of an event type that declares a timestamp property but received collection of events of type 'SupportBean'");
            
            // invalid incompatible params
            epl = "select DateTimeOffset.set('invalid') from SupportDateTime";
            env.TryInvalidCompile(
                epl,
                "Failed to validate select-clause expression 'DateTimeOffset.set('invalid')': Failed to resolve enumeration method, date-time method or mapped property 'DateTimeOffset.set('invalid')': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value");

            // invalid lambda parameter
            epl = "select DateTimeOffset.set(x => true) from SupportDateTime";
            env.TryInvalidCompile(
                epl,
                "Failed to validate select-clause expression 'DateTimeOffset.set()': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value");

            // invalid no parameter
            epl = "select DateTimeOffset.set() from SupportDateTime";
            env.TryInvalidCompile(
                epl,
                "Failed to validate select-clause expression 'DateTimeOffset.set()': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value");

            // invalid wrong parameter
            epl = "select DateTimeOffset.set(1) from SupportDateTime";
            env.TryInvalidCompile(
                epl,
                "Failed to validate select-clause expression 'DateTimeOffset.set(1)': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value");

            // invalid wrong parameter
            epl = "select DateTimeOffset.between('a', 'b') from SupportDateTime";
            env.TryInvalidCompile(
                epl,
                "Failed to validate select-clause expression 'DateTimeOffset.between(\"a\",\"b\")': Failed to validate date-time method 'between', expected a long-typed, Date-typed or Calendar-typed result for expression parameter 0 but received System.String");

            // invalid wrong parameter
            epl = "select DateTimeOffset.between(DateTime, DateTime, 1, true) from SupportDateTime";
            env.TryInvalidCompile(
                epl,
                "Failed to validate select-clause expression 'DateTimeOffset.between(DateTime,Dat...(48 chars)': Failed to validate date-time method 'between', expected a boolean-type result for expression parameter 2 but received System.Int32");

#if NOT_APPLICABLE // use case exercises code that examines DateFormatters on legacy date types - not applicable
            // mispatch parameter to input
            var formatProvider = GetType().FullName;
            epl = $"select DateTimeOffset.format({formatProvider}.GetInvalidFormat()) from SupportDateTime";
            env.TryInvalidCompile(
                epl,
                $"Failed to validate select-clause expression 'DateTimeOffset.format(ParseCaseSensitive(...(114 chars)': Date-time enumeration method 'format' invalid format, expected string-format or DateFormat but received {formatProvider}");

            var simpleDateFormat = typeof(SimpleDateFormat).CleanName(); 
            epl = $"select DateTimeEx.format({simpleDateFormat}.GetInstance()) from SupportDateTime";
            env.TryInvalidCompile(
                epl,
                "Failed to validate select-clause expression 'DateTimeEx.format({simpleDateFormat}.g...(48 chars)': Date-time enumeration method 'format' invalid format, expected string-format or DateTimeFormatter but received java.text.DateFormat");
#endif
            
            // invalid date format null
            epl = "select DateTimeOffset.format(null) from SupportDateTime";
            env.TryInvalidCompile(
                epl,
                "Failed to validate select-clause expression 'DateTimeOffset.format(null)': Failed to validate date-time method 'format', expected a non-null result for expression parameter 0 but received a null-typed expression");
        }
    }
} // end of namespace