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

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.enummethod
{
    public class ExecEnumInvalid : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            configuration.AddEventType("SupportBean_ST0_Container", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportBeanComplexProps", typeof(SupportBeanComplexProps));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
            configuration.AddImport(typeof(SupportBean_ST0_Container));
            configuration.AddPlugInSingleRowFunction("makeTest", typeof(SupportBean_ST0_Container).Name, "makeTest");
        }
    
        public override void Run(EPServiceProvider epService) {
            string epl;
    
            // no parameter while one is expected
            epl = "select Contained.Take() from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.Take()': Parameters mismatch for enumeration method 'take', the method requires an (non-lambda) expression providing count [select Contained.Take() from SupportBean_ST0_Container]");
    
            // primitive array property
            epl = "select ArrayProperty.Where(x=>x.boolPrimitive) from SupportBeanComplexProps";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'arrayProperty.Where()': Error validating enumeration method 'where' parameter 0: Failed to validate declared expression body expression 'x.boolPrimitive': Failed to resolve property 'x.boolPrimitive' to a stream or nested property in a stream [select ArrayProperty.Where(x=>x.boolPrimitive) from SupportBeanComplexProps]");
    
            // property not there
            epl = "select Contained.Where(x=>x.dummy = 1) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.Where()': Error validating enumeration method 'where' parameter 0: Failed to validate declared expression body expression 'x.dummy=1': Failed to resolve property 'x.dummy' to a stream or nested property in a stream [select Contained.Where(x=>x.dummy = 1) from SupportBean_ST0_Container]");
            epl = "select * from SupportBean(products.Where(p => code = '1'))";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Failed to validate filter expression 'products.Where()': Failed to resolve 'products.where' to a property, single-row function, aggregation function, script, stream or class name ");
    
            // test not an enumeration method
            epl = "select Contained.NotAMethod(x=>x.boolPrimitive) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.NotAMethod()': Could not find event property, enumeration method or instance method named 'notAMethod' in collection of events of type 'SupportBean_ST0' [select Contained.NotAMethod(x=>x.boolPrimitive) from SupportBean_ST0_Container]");
    
            // invalid lambda expression for non-lambda func
            epl = "select MakeTest(x=>1) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'MakeTest()': Unexpected lambda-expression encountered as parameter to UDF or static method 'makeTest' [select MakeTest(x=>1) from SupportBean_ST0_Container]");
    
            // invalid lambda expression for non-lambda func
            epl = "select SupportBean_ST0_Container.MakeTest(x=>1) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'SupportBean_ST0_Container.MakeTest()': Unexpected lambda-expression encountered as parameter to UDF or static method 'makeTest' [select SupportBean_ST0_Container.MakeTest(x=>1) from SupportBean_ST0_Container]");
    
            // invalid incompatible params
            epl = "select Contained.Take('a') from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.Take('a')': Failed to resolve enumeration method, date-time method or mapped property 'contained.Take('a')': Error validating enumeration method 'take', expected a number-type result for expression parameter 0 but received java.lang.string [select Contained.Take('a') from SupportBean_ST0_Container]");
    
            // invalid incompatible params
            epl = "select Contained.Take(x => x.p00) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.Take()': Parameters mismatch for enumeration method 'take', the method requires an (non-lambda) expression providing count, but receives a lambda expression [select Contained.Take(x => x.p00) from SupportBean_ST0_Container]");
    
            // invalid too many lambda parameter
            epl = "select Contained.Where((x,y,z) => true) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.Where()': Parameters mismatch for enumeration method 'where', the method requires a lambda expression providing predicate, but receives a 3-parameter lambda expression [select Contained.Where((x,y,z) => true) from SupportBean_ST0_Container]");
    
            // invalid no parameter
            epl = "select Contained.Where() from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.Where()': Parameters mismatch for enumeration method 'where', the method has multiple footprints accepting a lambda expression providing predicate, or a 2-parameter lambda expression providing (predicate, index), but receives no parameters [select Contained.Where() from SupportBean_ST0_Container]");
    
            // invalid no parameter
            epl = "select window(intPrimitive).TakeLast() from SupportBean#length(2)";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'window(intPrimitive).TakeLast()': Parameters mismatch for enumeration method 'takeLast', the method requires an (non-lambda) expression providing count [select window(intPrimitive).TakeLast() from SupportBean#length(2)]");
    
            // invalid wrong parameter
            epl = "select Contained.Where(x=>true,y=>true) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.Where(,)': Parameters mismatch for enumeration method 'where', the method has multiple footprints accepting a lambda expression providing predicate, or a 2-parameter lambda expression providing (predicate, index), but receives a lambda expression and a lambda expression [select Contained.Where(x=>true,y=>true) from SupportBean_ST0_Container]");
    
            // invalid wrong parameter
            epl = "select Contained.Where(1) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.Where(1)': Parameters mismatch for enumeration method 'where', the method requires a lambda expression providing predicate, but receives an (non-lambda) expression [select Contained.Where(1) from SupportBean_ST0_Container]");
    
            // invalid too many parameter
            epl = "select Contained.Where(1,2) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.Where(1,2)': Parameters mismatch for enumeration method 'where', the method has multiple footprints accepting a lambda expression providing predicate, or a 2-parameter lambda expression providing (predicate, index), but receives an (non-lambda) expression and an (non-lambda) expression [select Contained.Where(1,2) from SupportBean_ST0_Container]");
    
            // subselect multiple columns
            epl = "select (select theString, intPrimitive from SupportBean#lastevent).Where(x=>x.boolPrimitive) from SupportBean_ST0";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'theString.Where()': Error validating enumeration method 'where' parameter 0: Failed to validate declared expression body expression 'x.boolPrimitive': Failed to resolve property 'x.boolPrimitive' to a stream or nested property in a stream [select (select theString, intPrimitive from SupportBean#lastevent).Where(x=>x.boolPrimitive) from SupportBean_ST0]");
    
            // subselect individual column
            epl = "select (select theString from SupportBean#lastevent).Where(x=>x.boolPrimitive) from SupportBean_ST0";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'theString.Where()': Error validating enumeration method 'where' parameter 0: Failed to validate declared expression body expression 'x.boolPrimitive': Failed to resolve property 'x.boolPrimitive' to a stream or nested property in a stream [select (select theString from SupportBean#lastevent).Where(x=>x.boolPrimitive) from SupportBean_ST0]");
    
            // aggregation
            epl = "select avg(intPrimitive).Where(x=>x.boolPrimitive) from SupportBean_ST0";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Incorrect syntax near '(' ('avg' is a reserved keyword) at line 1 column 10");
    
            // invalid incompatible params
            epl = "select Contained.AllOf(x => 1) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.AllOf()': Error validating enumeration method 'allOf', expected a bool-type result for expression parameter 0 but received java.lang.int? [select Contained.AllOf(x => 1) from SupportBean_ST0_Container]");
    
            // invalid incompatible params
            epl = "select Contained.AllOf(x => 1) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.AllOf()': Error validating enumeration method 'allOf', expected a bool-type result for expression parameter 0 but received java.lang.int? [select Contained.AllOf(x => 1) from SupportBean_ST0_Container]");
    
            // invalid incompatible params
            epl = "select Contained.Aggregate(0, (result, item) => result || ',') from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.Aggregate(0,)': Error validating enumeration method 'aggregate' parameter 1: Failed to validate declared expression body expression 'result||\",\"': Implicit conversion from datatype 'int?' to string is not allowed [select Contained.Aggregate(0, (result, item) => result || ',') from SupportBean_ST0_Container]");
    
            // invalid incompatible params
            epl = "select Contained.Average(x => x.id) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.Average()': Error validating enumeration method 'average', expected a number-type result for expression parameter 0 but received java.lang.string [select Contained.Average(x => x.id) from SupportBean_ST0_Container]");
    
            // not a property
            epl = "select Contained.Firstof().dummy from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.Firstof().Dummy()': Failed to resolve method 'dummy': Could not find enumeration method, date-time method or instance method named 'dummy' in class '" + typeof(SupportBean_ST0).Name + "' taking no parameters [select Contained.Firstof().dummy from SupportBean_ST0_Container]");
        }
    }
} // end of namespace
