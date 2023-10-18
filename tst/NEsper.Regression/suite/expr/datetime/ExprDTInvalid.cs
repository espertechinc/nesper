///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
	public class ExprDTInvalid : RegressionExecution {

	    public void Run(RegressionEnvironment env) {
	        string epl;

	        // invalid incompatible params
	        epl = "select contained.set('hour', 1) from SupportBean_ST0_Container";
	        env.TryInvalidCompile(epl, "Failed to validate select-clause expression 'contained.set(\"hour\",1)': Date-time enumeration method 'set' requires either a Calendar, Date, long, LocalDateTime or ZonedDateTime value as input or events of an event type that declares a timestamp property but received collection of events of type '" + typeof(SupportBean_ST0).FullName + "'");

	        // invalid incompatible params
	        epl = "select window(*).set('hour', 1) from SupportBean#keepall";
	        env.TryInvalidCompile(epl, "Failed to validate select-clause expression 'window(*).set(\"hour\",1)': Date-time enumeration method 'set' requires either a Calendar, Date, long, LocalDateTime or ZonedDateTime value as input or events of an event type that declares a timestamp property but received collection of events of type 'SupportBean'");

	        // invalid incompatible params
	        epl = "select utildate.set('invalid') from SupportDateTime";
	        env.TryInvalidCompile(epl, "Failed to validate select-clause expression 'utildate.set('invalid')': Failed to resolve enumeration method, date-time method or mapped property 'utildate.set('invalid')': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value");

	        // invalid lambda parameter
	        epl = "select utildate.set(x => true) from SupportDateTime";
	        env.TryInvalidCompile(epl, "Failed to validate select-clause expression 'utildate.set()': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value");

	        // invalid no parameter
	        epl = "select utildate.set() from SupportDateTime";
	        env.TryInvalidCompile(epl, "Failed to validate select-clause expression 'utildate.set()': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value");

	        // invalid wrong parameter
	        epl = "select utildate.set(1) from SupportDateTime";
	        env.TryInvalidCompile(epl, "Failed to validate select-clause expression 'utildate.set(1)': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value");

	        // invalid wrong parameter
	        epl = "select utildate.between('a', 'b') from SupportDateTime";
	        env.TryInvalidCompile(epl, "Failed to validate select-clause expression 'utildate.between(\"a\",\"b\")': Failed to validate date-time method 'between', expected a long-typed, Date-typed or Calendar-typed result for expression parameter 0 but received String");

	        // invalid wrong parameter
	        epl = "select utildate.between(utildate, utildate, 1, true) from SupportDateTime";
	        env.TryInvalidCompile(epl, "Failed to validate select-clause expression 'utildate.between(utildate,utildate,...(42 chars)': Failed to validate date-time method 'between', expected a boolean-type result for expression parameter 2 but received int");

	        // mispatch parameter to input
	        epl = "select utildate.format(java.time.format.DateTimeFormatter.ISO_ORDINAL_DATE) from SupportDateTime";
	        env.TryInvalidCompile(epl, "Failed to validate select-clause expression 'utildate.format(ParseCaseSensitive(...(114 chars)': Date-time enumeration method 'format' invalid format, expected string-format or DateFormat but received java.time.format.DateTimeFormatter");
	        epl = "select zoneddate.format(SimpleDateFormat.getInstance()) from SupportDateTime";
	        env.TryInvalidCompile(epl, "Failed to validate select-clause expression 'zoneddate.format(SimpleDateFormat.g...(48 chars)': Date-time enumeration method 'format' invalid format, expected string-format or DateTimeFormatter but received java.text.DateFormat");

	        // invalid date format null
	        epl = "select utildate.format(null) from SupportDateTime";
	        env.TryInvalidCompile(epl, "Failed to validate select-clause expression 'utildate.format(null)': Failed to validate date-time method 'format', expected a non-null result for expression parameter 0 but received a null-typed expression");
	    }
	}
} // end of namespace
