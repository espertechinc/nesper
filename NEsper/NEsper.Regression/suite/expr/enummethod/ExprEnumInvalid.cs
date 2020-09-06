///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;


namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
	public class ExprEnumInvalid : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			string epl;

			// no parameter while one is expected
			epl = "select Contained.take() from SupportBean_ST0_Container";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'Contained.take()': Parameters mismatch for enumeration method 'take', the method requires an (non-lambda) expression providing count [select Contained.take() from SupportBean_ST0_Container]");

			// primitive array property
			epl = "select ArrayProperty.where(x=>x.BoolPrimitive) from SupportBeanComplexProps";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'ArrayProperty.where()': Failed to validate enumeration method 'where' parameter 0: Failed to validate declared expression body expression 'x.BoolPrimitive': Failed to resolve property 'x.BoolPrimitive' to a stream or nested property in a stream [select ArrayProperty.where(x=>x.BoolPrimitive) from SupportBeanComplexProps]");

			// property not there
			epl = "select Contained.where(x=>x.dummy = 1) from SupportBean_ST0_Container";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'Contained.where()': Failed to validate enumeration method 'where' parameter 0: Failed to validate declared expression body expression 'x.dummy=1': Failed to resolve property 'x.dummy' to a stream or nested property in a stream [select Contained.where(x=>x.dummy = 1) from SupportBean_ST0_Container]");
			epl = "select * from SupportBean(Products.where(p => code = '1'))";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate filter expression 'Products.where()': Failed to resolve 'Products.where' to a property, single-row function, aggregation function, script, stream or class name ");

			// test not an enumeration method
			epl = "select Contained.notAMethod(x=>x.BoolPrimitive) from SupportBean_ST0_Container";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'Contained.notAMethod()': Could not find event property or method named 'notAMethod' in collection of events of type '");

			// invalid lambda expression for non-lambda func
			epl = "select makeTest(x=>1) from SupportBean_ST0_Container";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'makeTest()': Unrecognized lambda-expression encountered as parameter to UDF or static method 'MakeTest' [select makeTest(x=>1) from SupportBean_ST0_Container]");

			// invalid lambda expression for non-lambda func
			epl = "select SupportBean_ST0_Container.makeTest(x=>1) from SupportBean_ST0_Container";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'SupportBean_ST0_Container.makeTest()': Unrecognized lambda-expression encountered as parameter to UDF or static method 'makeTest' [select SupportBean_ST0_Container.makeTest(x=>1) from SupportBean_ST0_Container]");

			// invalid incompatible params
			epl = "select Contained.take('a') from SupportBean_ST0_Container";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'Contained.take('a')': Failed to resolve enumeration method, date-time method or mapped property 'Contained.take('a')': Failed to validate enumeration method 'take', expected a number-type result for expression parameter 0 but received System.String [select Contained.take('a') from SupportBean_ST0_Container]");

			// invalid incompatible params
			epl = "select Contained.take(x => x.P00) from SupportBean_ST0_Container";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'Contained.take()': Parameters mismatch for enumeration method 'take', the method requires an (non-lambda) expression providing count, but receives a lambda expression [select Contained.take(x => x.P00) from SupportBean_ST0_Container]");

			// invalid too many lambda parameter
			epl = "select Contained.where((x,y,z,a) => true) from SupportBean_ST0_Container";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'Contained.where()': Parameters mismatch for enumeration method 'where', the method requires a lambda expression providing predicate, but receives a 4-parameter lambda expression");

			// invalid no parameter
			epl = "select Contained.where() from SupportBean_ST0_Container";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'Contained.where()': Parameters mismatch for enumeration method 'where', the method has multiple footprints accepting a lambda expression providing predicate, or a 2-parameter lambda expression providing (predicate, index), or a 3-parameter lambda expression providing (predicate, index, size), but receives no parameters");

			// invalid no parameter
			epl = "select window(IntPrimitive).takeLast() from SupportBean#length(2)";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'window(IntPrimitive).takeLast()': Parameters mismatch for enumeration method 'takeLast', the method requires an (non-lambda) expression providing count [select window(IntPrimitive).takeLast() from SupportBean#length(2)]");

			// invalid wrong parameter
			epl = "select Contained.where(x=>true,y=>true) from SupportBean_ST0_Container";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'Contained.where(,)': Parameters mismatch for enumeration method 'where', the method has multiple footprints accepting a lambda expression providing predicate, or a 2-parameter lambda expression providing (predicate, index), or a 3-parameter lambda expression providing (predicate, index, size), but receives a lambda expression and a lambda expression");

			// invalid wrong parameter
			epl = "select Contained.where(1) from SupportBean_ST0_Container";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'Contained.where(1)': Parameters mismatch for enumeration method 'where', the method requires a lambda expression providing predicate, but receives an (non-lambda) expression [select Contained.where(1) from SupportBean_ST0_Container]");

			// invalid too many parameter
			epl = "select Contained.where(1,2) from SupportBean_ST0_Container";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'Contained.where(1,2)': Parameters mismatch for enumeration method 'where', the method has multiple footprints accepting a lambda expression providing predicate, or a 2-parameter lambda expression providing (predicate, index), or a 3-parameter lambda expression providing (predicate, index, size), but receives an (non-lambda) expression and an (non-lambda) expression");

			// subselect multiple columns
			epl = "select (select TheString, IntPrimitive from SupportBean#lastevent).where(x=>x.BoolPrimitive) from SupportBean_ST0";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'TheString.where()': Failed to validate enumeration method 'where' parameter 0: Failed to validate declared expression body expression 'x.BoolPrimitive': Failed to resolve property 'x.BoolPrimitive' to a stream or nested property in a stream [select (select TheString, IntPrimitive from SupportBean#lastevent).where(x=>x.BoolPrimitive) from SupportBean_ST0]");

			// subselect individual column
			epl = "select (select TheString from SupportBean#lastevent).where(x=>x.BoolPrimitive) from SupportBean_ST0";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'TheString.where()': Failed to validate enumeration method 'where' parameter 0: Failed to validate declared expression body expression 'x.BoolPrimitive': Failed to resolve property 'x.BoolPrimitive' to a stream or nested property in a stream [select (select TheString from SupportBean#lastevent).where(x=>x.BoolPrimitive) from SupportBean_ST0]");

			// aggregation
			epl = "select avg(IntPrimitive).where(x=>x.BoolPrimitive) from SupportBean_ST0";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'avg(IntPrimitive).where()': Failed to validate method-chain parameter expression 'IntPrimitive': Property named 'IntPrimitive' is not valid in any stream");

			// invalid incompatible params
			epl = "select Contained.allOf(x => 1) from SupportBean_ST0_Container";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'Contained.allOf()': Failed to validate enumeration method 'allOf', expected a boolean-type result for expression parameter 0 but received int [select Contained.allOf(x => 1) from SupportBean_ST0_Container]");

			// invalid incompatible params
			epl = "select Contained.allOf(x => 1) from SupportBean_ST0_Container";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'Contained.allOf()': Failed to validate enumeration method 'allOf', expected a boolean-type result for expression parameter 0 but received int [select Contained.allOf(x => 1) from SupportBean_ST0_Container]");

			// invalid incompatible params
			epl = "select Contained.aggregate(0, (result, item) => result || ',') from SupportBean_ST0_Container";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'Contained.aggregate(0,)': Failed to validate enumeration method 'aggregate' parameter 1: Failed to validate declared expression body expression 'result||\",\"': Implicit conversion from datatype 'System.Int32' to string is not allowed [select Contained.aggregate(0, (result, item) => result || ',') from SupportBean_ST0_Container]");

			// invalid incompatible params
			epl = "select Contained.average(x => x.Id) from SupportBean_ST0_Container";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'Contained.average()': Failed to validate enumeration method 'average', expected a number-type result for expression parameter 0 but received System.String [select Contained.average(x => x.Id) from SupportBean_ST0_Container]");

			// not a property
			epl = "select Contained.firstof().dummy from SupportBean_ST0_Container";
			SupportMessageAssertUtil.TryInvalidCompile(
				env,
				epl,
				"Failed to validate select-clause expression 'Contained.firstof().dummy': Failed to resolve method 'dummy': Could not find enumeration method, date-time method, instance method or property named 'dummy' in class 'com.espertech.esper.regressionlib.support.bean.SupportBean_ST0' taking no parameters");
		}
	}
} // end of namespace
