///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

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
            configuration.AddPlugInSingleRowFunction("makeTest", typeof(SupportBean_ST0_Container), "MakeTest");
        }
    
        public override void Run(EPServiceProvider epService) {
            string epl;
    
            // no parameter while one is expected
            epl = "select Contained.take() from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Contained.take()': Parameters mismatch for enumeration method 'take', the method requires an (non-lambda) expression providing count [select Contained.take() from SupportBean_ST0_Container]");
    
            // primitive array property
            epl = "select ArrayProperty.where(x=>x.BoolPrimitive) from SupportBeanComplexProps";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'ArrayProperty.where()': Error validating enumeration method 'where' parameter 0: Failed to validate declared expression body expression 'x.BoolPrimitive': Failed to resolve property 'x.BoolPrimitive' to a stream or nested property in a stream [select ArrayProperty.where(x=>x.BoolPrimitive) from SupportBeanComplexProps]");
    
            // property not there
            epl = "select Contained.where(x=>x.dummy = 1) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Contained.where()': Error validating enumeration method 'where' parameter 0: Failed to validate declared expression body expression 'x.dummy=1': Failed to resolve property 'x.dummy' to a stream or nested property in a stream [select Contained.where(x=>x.dummy = 1) from SupportBean_ST0_Container]");
            epl = "select * from SupportBean(products.where(p => code = '1'))";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Failed to validate filter expression 'products.where()': Failed to resolve 'products.where' to a property, single-row function, aggregation function, script, stream or class name ");
    
            // test not an enumeration method
            epl = "select Contained.notAMethod(x=>x.BoolPrimitive) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Contained.notAMethod()': Could not find event property, enumeration method or instance method named 'notAMethod' in collection of events of type 'SupportBean_ST0' [select Contained.notAMethod(x=>x.BoolPrimitive) from SupportBean_ST0_Container]");
    
            // invalid lambda expression for non-lambda func
            epl = "select makeTest(x=>1) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'makeTest()': Unexpected lambda-expression encountered as parameter to UDF or static method 'MakeTest' [select makeTest(x=>1) from SupportBean_ST0_Container]");
    
            // invalid lambda expression for non-lambda func
            epl = "select SupportBean_ST0_Container.makeTest(x=>1) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'SupportBean_ST0_Container.makeTest()': Unexpected lambda-expression encountered as parameter to UDF or static method 'makeTest' [select SupportBean_ST0_Container.makeTest(x=>1) from SupportBean_ST0_Container]");
    
            // invalid incompatible params
            epl = "select Contained.take('a') from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Contained.take('a')': Failed to resolve enumeration method, date-time method or mapped property 'Contained.take('a')': Error validating enumeration method 'take', expected a number-type result for expression parameter 0 but received System.String [select Contained.take('a') from SupportBean_ST0_Container]");
    
            // invalid incompatible params
            epl = "select Contained.take(x => x.p00) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Contained.take()': Parameters mismatch for enumeration method 'take', the method requires an (non-lambda) expression providing count, but receives a lambda expression [select Contained.take(x => x.p00) from SupportBean_ST0_Container]");
    
            // invalid too many lambda parameter
            epl = "select Contained.where((x,y,z) => true) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Contained.where()': Parameters mismatch for enumeration method 'where', the method requires a lambda expression providing predicate, but receives a 3-parameter lambda expression [select Contained.where((x,y,z) => true) from SupportBean_ST0_Container]");
    
            // invalid no parameter
            epl = "select Contained.where() from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Contained.where()': Parameters mismatch for enumeration method 'where', the method has multiple footprints accepting a lambda expression providing predicate, or a 2-parameter lambda expression providing (predicate, index), but receives no parameters [select Contained.where() from SupportBean_ST0_Container]");
    
            // invalid no parameter
            epl = "select window(IntPrimitive).takeLast() from SupportBean#length(2)";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'window(IntPrimitive).takeLast()': Parameters mismatch for enumeration method 'takeLast', the method requires an (non-lambda) expression providing count [select window(IntPrimitive).takeLast() from SupportBean#length(2)]");
    
            // invalid wrong parameter
            epl = "select Contained.where(x=>true,y=>true) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Contained.where(,)': Parameters mismatch for enumeration method 'where', the method has multiple footprints accepting a lambda expression providing predicate, or a 2-parameter lambda expression providing (predicate, index), but receives a lambda expression and a lambda expression [select Contained.where(x=>true,y=>true) from SupportBean_ST0_Container]");
    
            // invalid wrong parameter
            epl = "select Contained.where(1) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Contained.where(1)': Parameters mismatch for enumeration method 'where', the method requires a lambda expression providing predicate, but receives an (non-lambda) expression [select Contained.where(1) from SupportBean_ST0_Container]");
    
            // invalid too many parameter
            epl = "select Contained.where(1,2) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Contained.where(1,2)': Parameters mismatch for enumeration method 'where', the method has multiple footprints accepting a lambda expression providing predicate, or a 2-parameter lambda expression providing (predicate, index), but receives an (non-lambda) expression and an (non-lambda) expression [select Contained.where(1,2) from SupportBean_ST0_Container]");
    
            // subselect multiple columns
            epl = "select (select TheString, IntPrimitive from SupportBean#lastevent).where(x=>x.BoolPrimitive) from SupportBean_ST0";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'TheString.where()': Error validating enumeration method 'where' parameter 0: Failed to validate declared expression body expression 'x.BoolPrimitive': Failed to resolve property 'x.BoolPrimitive' to a stream or nested property in a stream [select (select TheString, IntPrimitive from SupportBean#lastevent).where(x=>x.BoolPrimitive) from SupportBean_ST0]");
    
            // subselect individual column
            epl = "select (select TheString from SupportBean#lastevent).where(x=>x.BoolPrimitive) from SupportBean_ST0";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'TheString.where()': Error validating enumeration method 'where' parameter 0: Failed to validate declared expression body expression 'x.BoolPrimitive': Failed to resolve property 'x.BoolPrimitive' (property 'x' is an indexed property and requires an index or enumeration method to access values) [select (select TheString from SupportBean#lastevent).where(x=>x.BoolPrimitive) from SupportBean_ST0]");
    
            // aggregation
            epl = "select avg(IntPrimitive).where(x=>x.BoolPrimitive) from SupportBean_ST0";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Incorrect syntax near '(' ('avg' is a reserved keyword) at line 1 column 10");
    
            // invalid incompatible params
            epl = "select Contained.allOf(x => 1) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, string.Format("Error starting statement: Failed to validate select-clause expression 'Contained.allOf()': Error validating enumeration method 'allOf', expected a bool-type result for expression parameter 0 but received {0} [select Contained.allOf(x => 1) from SupportBean_ST0_Container]", 
                typeof(int).GetCleanName()));
    
            // invalid incompatible params
            epl = "select Contained.allOf(x => 1) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, string.Format("Error starting statement: Failed to validate select-clause expression 'Contained.allOf()': Error validating enumeration method 'allOf', expected a bool-type result for expression parameter 0 but received {0} [select Contained.allOf(x => 1) from SupportBean_ST0_Container]", 
                typeof(int).GetCleanName()));
    
            // invalid incompatible params
            epl = "select Contained.aggregate(0, (result, item) => result || ',') from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, string.Format("Error starting statement: Failed to validate select-clause expression 'Contained.aggregate(0,)': Error validating enumeration method 'aggregate' parameter 1: Failed to validate declared expression body expression 'result||\",\"': Implicit conversion from datatype '{0}' to string is not allowed [select Contained.aggregate(0, (result, item) => result || ',') from SupportBean_ST0_Container]", 
                typeof(int?).GetCleanName()));
    
            // invalid incompatible params
            epl = "select Contained.average(x => x.id) from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Contained.average()': Error validating enumeration method 'average', expected a number-type result for expression parameter 0 but received System.String [select Contained.average(x => x.id) from SupportBean_ST0_Container]");
    
            // not a property
            epl = "select Contained.firstof().dummy from SupportBean_ST0_Container";
            SupportMessageAssertUtil.TryInvalid(epService, epl, string.Format("Error starting statement: Failed to validate select-clause expression 'Contained.firstof().dummy()': Failed to resolve method 'dummy': Could not find enumeration method, date-time method or instance method named 'dummy' in class '{0}' taking no parameters [select Contained.firstof().dummy from SupportBean_ST0_Container]", 
                typeof(SupportBean_ST0).GetCleanName()));
        }
    }
} // end of namespace
