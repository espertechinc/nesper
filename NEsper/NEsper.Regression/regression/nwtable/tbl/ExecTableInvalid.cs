///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableInvalid : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionInvalidAggMatchSingleFunc(epService);
            RunAssertionInvalidAggMatchMultiFunc(epService);
            RunAssertionInvalidAnnotations(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionInvalidAggMatchSingleFunc(EPServiceProvider epService) {
            // sum
            TryInvalidAggMatch(epService, "var1", "sum(double)", false, "sum(IntPrimitive)",
                    "Error starting statement: Incompatible aggregation function for table 'var1' column 'value', expecting 'sum(double)' and received 'sum(IntPrimitive)': The required parameter type is " + Name.Clean<double>(false) + " and provided is " + Name.Clean<int>(false) + " [");
            TryInvalidAggMatch(epService, "var1", "sum(double)", false, "count(*)",
                    "Error starting statement: Incompatible aggregation function for table 'var1' column 'value', expecting 'sum(double)' and received 'count(*)': Not a 'sum' aggregation [");
            TryInvalidAggMatch(epService, "var1", "sum(double)", false, "sum(DoublePrimitive, TheString='a')",
                    "Error starting statement: Incompatible aggregation function for table 'var1' column 'value', expecting 'sum(double)' and received 'sum(DoublePrimitive,TheString=\"a\")': The aggregation declares no filter expression and provided is a filter expression [");
            TryInvalidAggMatch(epService, "var1", "sum(double, bool)", false, "sum(DoublePrimitive)",
                    "Error starting statement: Incompatible aggregation function for table 'var1' column 'value', expecting 'sum(double,bool)' and received 'sum(DoublePrimitive)': The aggregation declares a filter expression and provided is no filter expression [");
    
            // count
            TryInvalidAggMatch(epService, "var1", "count(*)", false, "sum(IntPrimitive)",
                    "Error starting statement: Incompatible aggregation function for table 'var1' column 'value', expecting 'count(*)' and received 'sum(IntPrimitive)': Not a 'count' aggregation [");
            TryInvalidAggMatch(epService, "var1", "count(*)", false, "count(distinct IntPrimitive)",
                    "Error starting statement: Incompatible aggregation function for table 'var1' column 'value', expecting 'count(*)' and received 'count(distinct IntPrimitive)': The aggregation declares no distinct and provided is a distinct [");
            TryInvalidAggMatch(epService, "var1", "count(*)", false, "count(distinct IntPrimitive, BoolPrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "count(distinct int)", false, "count(distinct DoublePrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "count(int)", false, "count(*)",
                    "Error starting statement: Incompatible aggregation function for table 'var1' column 'value', expecting 'count(int)' and received 'count(*)': The aggregation declares ignore nulls and provided is no ignore nulls [");
    
            // avg
            TryInvalidAggMatch(epService, "var1", "avg(int)", false, "sum(IntPrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "avg(int)", false, "avg(LongPrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "avg(int)", false, "avg(IntPrimitive, BoolPrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "avg(int)", false, "avg(distinct IntPrimitive)", null);
    
            // min-max
            TryInvalidAggMatch(epService, "var1", "max(int)", false, "min(IntPrimitive)",
                    "Error starting statement: Incompatible aggregation function for table 'var1' column 'value', expecting 'max(int)' and received 'min(IntPrimitive)': The aggregation declares max and provided is min [");
            TryInvalidAggMatch(epService, "var1", "min(int)", false, "avg(IntPrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "min(int)", false, "min(DoublePrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "min(int)", false, "fmin(IntPrimitive, TheString='a')",
                    "Error starting statement: Incompatible aggregation function for table 'var1' column 'value', expecting 'min(int)' and received 'min(IntPrimitive,TheString=\"a\")': The aggregation declares no filter expression and provided is a filter expression [");
    
            // stddev
            TryInvalidAggMatch(epService, "var1", "stddev(int)", false, "avg(IntPrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "stddev(int)", false, "stddev(DoublePrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "stddev(int)", false, "stddev(IntPrimitive, true)", null);
    
            // avedev
            TryInvalidAggMatch(epService, "var1", "avedev(int)", false, "avg(IntPrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "avedev(int)", false, "avedev(DoublePrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "avedev(int)", false, "avedev(IntPrimitive, true)", null);
    
            // median
            TryInvalidAggMatch(epService, "var1", "median(int)", false, "avg(IntPrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "median(int)", false, "median(DoublePrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "median(int)", false, "median(IntPrimitive, true)", null);
    
            // firstever
            TryInvalidAggMatch(epService, "var1", "firstever(int)", false, "lastever(IntPrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "firstever(int)", false, "firstever(DoublePrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "firstever(int, bool)", false, "firstever(IntPrimitive)", null);
    
            // lastever
            TryInvalidAggMatch(epService, "var1", "lastever(int)", false, "firstever(IntPrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "lastever(int)", false, "lastever(DoublePrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "lastever(int, bool)", false, "lastever(IntPrimitive)", null);
    
            // countever
            TryInvalidAggMatch(epService, "var1", "lastever(int)", true, "countever(IntPrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "lastever(int, bool)", true, "countever(IntPrimitive)", null);
            TryInvalidAggMatch(epService, "var1", "lastever(int)", true, "countever(IntPrimitive, true)", null);
            TryInvalidAggMatch(epService, "var1", "countever(*)", true, "countever(IntPrimitive)", null);
    
            // nth
            TryInvalidAggMatch(epService, "var1", "nth(int, 10)", false, "avg(20)", null);
            TryInvalidAggMatch(epService, "var1", "nth(int, 10)", false, "nth(IntPrimitive, 11)",
                    "Error starting statement: Incompatible aggregation function for table 'var1' column 'value', expecting 'nth(int,10)' and received 'nth(IntPrimitive,11)': The size is 10 and provided is 11 [");
            TryInvalidAggMatch(epService, "var1", "nth(int, 10)", false, "nth(DoublePrimitive, 10)", null);
    
            // rate
            TryInvalidAggMatch(epService, "var1", "rate(20)", false, "avg(20)", null);
            TryInvalidAggMatch(epService, "var1", "rate(20)", false, "rate(11)",
                    "Error starting statement: Incompatible aggregation function for table 'var1' column 'value', expecting 'rate(20)' and received 'rate(11)': The size is 20000 and provided is 11000 [");
    
            // leaving
            TryInvalidAggMatch(epService, "var1", "leaving()", false, "avg(IntPrimitive)", null);
    
            // plug-in single-func
            epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory("myaggsingle", typeof(MyAggregationFunctionFactory));
            TryInvalidAggMatch(epService, "var1", "Myaggsingle()", false, "leaving()",
                    "Error starting statement: Incompatible aggregation function for table 'var1' column 'value', expecting 'Myaggsingle(*)' and received 'leaving(*)': Not a 'Myaggsingle' aggregation [");
        }
    
        private void RunAssertionInvalidAggMatchMultiFunc(EPServiceProvider epService) {
            // Window and related
            //
    
            // window vs agg method
            TryInvalidAggMatch(epService, "var1", "window(*) @Type(SupportBean)", true, "avg(IntPrimitive)",
                    "Error starting statement: Incompatible aggregation function for table 'var1' column 'value', expecting 'window(*)' and received 'avg(IntPrimitive)': Not a 'window' aggregation [");
            // window vs sorted
            TryInvalidAggMatch(epService, "var1", "window(*) @Type(SupportBean)", true, "sorted(IntPrimitive)",
                    "Error starting statement: Failed to validate select-clause expression 'sorted(IntPrimitive)': When specifying into-table a sort expression cannot be provided [");
            // wrong type
            TryInvalidAggMatch(epService, "var1", "window(*) @Type(SupportBean_S0)", true, "window(*)",
                    "Error starting statement: Incompatible aggregation function for table 'var1' column 'value', expecting 'window(*)' and received 'window(*)': The required event type is 'SupportBean_S0' and provided is 'SupportBean' [");
    
            // sorted
            //
            TryInvalidAggMatch(epService, "var1", "sorted(IntPrimitive) @Type(SupportBean)", true, "window(*)",
                    "Error starting statement: Incompatible aggregation function for table 'var1' column 'value', expecting 'sorted(IntPrimitive)' and received 'window(*)': Not a 'sorted' aggregation [");
            TryInvalidAggMatch(epService, "var1", "sorted(id) @Type(SupportBean_S0)", true, "sorted(IntPrimitive)",
                    "Error starting statement: Failed to validate select-clause expression 'sorted(IntPrimitive)': When specifying into-table a sort expression cannot be provided [");
    
            // plug-in
            //
            var config = new ConfigurationPlugInAggregationMultiFunction(SupportAggMFFuncExtensions.GetFunctionNames(), typeof(SupportAggMFFactory));
            epService.EPAdministrator.Configuration.AddPlugInAggregationMultiFunction(config);
            TryInvalidAggMatch(epService, "var1", "se1() @Type(SupportBean)", true, "window(*)",
                    "Error starting statement: Incompatible aggregation function for table 'var1' column 'value', expecting 'se1(*)' and received 'window(*)': Not a 'se1' aggregation [");
        }
    
        private void TryInvalidAggMatch(EPServiceProvider epService, string name, string declared, bool unbound, string provided, string messageOrNull) {
            EPStatement stmtDeclare = epService.EPAdministrator.CreateEPL("create table " + name + "(value " + declared + ")");
    
            try {
                string epl = "into table " + name + " select " + provided + " as value from SupportBean" +
                        (unbound ? "#time(1000)" : "");
                epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            } catch (EPStatementException ex) {
                if (messageOrNull != null && messageOrNull.Length > 10) {
                    if (!ex.Message.StartsWith(messageOrNull)) {
                        Assert.Fail("\nExpected:" + messageOrNull + "\nReceived:" + ex.Message);
                    }
                } else {
                    Assert.IsTrue(ex.Message.Contains("Incompatible aggregation function for table"));
                }
            }
    
            stmtDeclare.Dispose();
            epService.EPAdministrator.Configuration.RemoveEventType("table_" + name + "__internal", false);
            epService.EPAdministrator.Configuration.RemoveEventType("table_" + name + "__public", false);
        }
    
        private void RunAssertionInvalidAnnotations(EPServiceProvider epService) {
            // unknown marker
            SupportMessageAssertUtil.TryInvalid(epService, "create table v1 (abc window(*) @unknown)",
                    "Error starting statement: For column 'abc' unrecognized annotation 'unknown' [");
    
            // no type provided
            SupportMessageAssertUtil.TryInvalid(epService, "create table v1 (abc window(*) @type)",
                    "Error starting statement: For column 'abc' no value provided for annotation 'type', expected a value [");
    
            // multiple value
            SupportMessageAssertUtil.TryInvalid(epService, "create table v1 (abc window(*) @Type(SupportBean) @Type(SupportBean))",
                    "Error starting statement: For column 'abc' multiple annotations provided named 'Type' [");
    
            // wrong value
            SupportMessageAssertUtil.TryInvalid(epService, "create table v1 (abc window(*) @Type(1))",
                    "Error starting statement: For column 'abc' string value expected for annotation 'Type' [");
    
            // unknown type provided
            SupportMessageAssertUtil.TryInvalid(epService, "create table v1 (abc window(*) @Type(xx))",
                    "Error starting statement: For column 'abc' failed to find event type 'xx' [");
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("singlerow", GetType(), "MySingleRowFunction");
            epService.EPAdministrator.CreateEPL("create table aggvar_grouped_string (key string primary key, total count(*))");
            epService.EPAdministrator.CreateEPL("create table aggvar_twogrouped (keyone string primary key, keytwo string primary key, total count(*))");
            epService.EPAdministrator.CreateEPL("create table aggvar_grouped_int (key int primary key, total count(*))");
            epService.EPAdministrator.CreateEPL("create table aggvar_ungrouped as (total count(*))");
            epService.EPAdministrator.CreateEPL("create table aggvar_ungrouped_window as (win window(*) @Type(SupportBean))");
            epService.EPAdministrator.CreateEPL("create context MyContext initiated by SupportBean_S0 terminated by SupportBean_S1");
            epService.EPAdministrator.CreateEPL("context MyContext create table aggvarctx (total count(*))");
            epService.EPAdministrator.CreateEPL("create context MyOtherContext initiated by SupportBean_S0 terminated by SupportBean_S1");
            epService.EPAdministrator.CreateEPL("create variable int myvariable");
            epService.EPAdministrator.CreateEPL("create window MyNamedWindow#keepall as select * from SupportBean");
            epService.EPAdministrator.CreateEPL("create schema SomeSchema(p0 string)");
    
            // invalid declaration
            //
            //
            // constant
            SupportMessageAssertUtil.TryInvalid(epService, "create constant variable aggvar_ungrouped (total count(*))",
                    "Incorrect syntax near '(' expecting an identifier but found an opening parenthesis '(' at line 1 column 42 [");
            // invalid type
            SupportMessageAssertUtil.TryInvalid(epService, "create table aggvar_notright as (total sum(abc))",
                    "Error starting statement: Failed to resolve type 'abc': Could not load class by name 'abc', please check imports [");
            // invalid non-aggregation
            SupportMessageAssertUtil.TryInvalid(epService, "create table aggvar_wrongtoo as (total Singlerow(1))",
                    "Error starting statement: Expression 'Singlerow(1)' is not an aggregation [");
            // can only declare "sorted()" or "window" aggregation function
            // this is to make sure future compatibility when optimizing queries
            SupportMessageAssertUtil.TryInvalid(epService, "create table aggvar_invalid as (mywindow window(IntPrimitive) @Type(SupportBean))",
                    "Error starting statement: Failed to validate table-column expression 'window(IntPrimitive)': For tables columns, the window aggregation function requires the 'window(*)' declaration [");
            SupportMessageAssertUtil.TryInvalid(epService, "create table aggvar_invalid as (mywindow last(*)@Type(SupportBean))", "skip");
            SupportMessageAssertUtil.TryInvalid(epService, "create table aggvar_invalid as (mywindow window(sb.*)@Type(SupportBean)", "skip");
            SupportMessageAssertUtil.TryInvalid(epService, "create table aggvar_invalid as (mymax maxby(IntPrimitive) @Type(SupportBean))",
                    "Error starting statement: Failed to validate table-column expression 'maxby(IntPrimitive)': For tables columns, the aggregation function requires the 'sorted(*)' declaration [");
            // same column multiple times
            SupportMessageAssertUtil.TryInvalid(epService, "create table aggvar_invalid as (mycount count(*),mycount count(*))",
                    "Error starting statement: Column 'mycount' is listed more than once [create table aggvar_invalid as (mycount count(*),mycount count(*))]");
            // already a variable of the same name
            SupportMessageAssertUtil.TryInvalid(epService, "create table myvariable as (mycount count(*))",
                    "Error starting statement: Variable by name 'myvariable' has already been created [");
            SupportMessageAssertUtil.TryInvalid(epService, "create table aggvar_ungrouped as (total count(*))",
                    "Error starting statement: Table by name 'aggvar_ungrouped' has already been created [");
            // invalid primary key use
            SupportMessageAssertUtil.TryInvalid(epService, "create table abc as (total count(*) primary key)",
                    "Error starting statement: Column 'total' may not be tagged as primary key, an expression cannot become a primary key column [");
            SupportMessageAssertUtil.TryInvalid(epService, "create table abc as (arr int[] primary key)",
                    "Error starting statement: Column 'arr' may not be tagged as primary key, an array-typed column cannot become a primary key column [");
            SupportMessageAssertUtil.TryInvalid(epService, "create table abc as (arr SupportBean primary key)",
                    "Error starting statement: Column 'arr' may not be tagged as primary key, received unexpected event type 'SupportBean' [");
            SupportMessageAssertUtil.TryInvalid(epService, "create table abc as (mystr string prim key)",
                    "Invalid keyword 'prim' encountered, expected 'primary key' [");
            SupportMessageAssertUtil.TryInvalid(epService, "create table abc as (mystr string primary keys)",
                    "Invalid keyword 'keys' encountered, expected 'primary key' [");
            SupportMessageAssertUtil.TryInvalid(epService, "create table SomeSchema as (mystr string)",
                    "Error starting statement: An event type or schema by name 'SomeSchema' already exists [");
    
            // invalid-into
            //
            //
            // table-not-found
            SupportMessageAssertUtil.TryInvalid(epService, "into table xxx select count(*) as total from SupportBean group by IntPrimitive",
                    "Error starting statement: Invalid into-table clause: Failed to find table by name 'xxx' [");
            // group-by key type and count of group-by expressions
            SupportMessageAssertUtil.TryInvalid(epService, "into table aggvar_grouped_string select count(*) as total from SupportBean group by IntPrimitive",
                    "Error starting statement: Incompatible type returned by a group-by expression for use with table 'aggvar_grouped_string', the group-by expression 'IntPrimitive' returns '" + Name.Clean<int>() + "' but the table expects 'System.String' [");
            SupportMessageAssertUtil.TryInvalid(epService, "into table aggvar_grouped_string select count(*) as total from SupportBean group by TheString, IntPrimitive",
                    "Error starting statement: Incompatible number of group-by expressions for use with table 'aggvar_grouped_string', the table expects 1 group-by expressions and provided are 2 group-by expressions [");
            SupportMessageAssertUtil.TryInvalid(epService, "into table aggvar_ungrouped select count(*) as total from SupportBean group by TheString",
                    "Error starting statement: Incompatible number of group-by expressions for use with table 'aggvar_ungrouped', the table expects no group-by expressions and provided are 1 group-by expressions [");
            SupportMessageAssertUtil.TryInvalid(epService, "into table aggvar_grouped_string select count(*) as total from SupportBean",
                    "Error starting statement: Incompatible number of group-by expressions for use with table 'aggvar_grouped_string', the table expects 1 group-by expressions and provided are no group-by expressions [");
            SupportMessageAssertUtil.TryInvalid(epService, "into table aggvarctx select count(*) as total from SupportBean",
                    "Error starting statement: Table by name 'aggvarctx' has been declared for context 'MyContext' and can only be used within the same context [into table aggvarctx select count(*) as total from SupportBean]");
            SupportMessageAssertUtil.TryInvalid(epService, "context MyOtherContext into table aggvarctx select count(*) as total from SupportBean",
                    "Error starting statement: Table by name 'aggvarctx' has been declared for context 'MyContext' and can only be used within the same context [context MyOtherContext into table aggvarctx select count(*) as total from SupportBean]");
            SupportMessageAssertUtil.TryInvalid(epService, "into table aggvar_ungrouped select count(*) as total, aggvar_ungrouped from SupportBean",
                    "Error starting statement: Invalid use of table 'aggvar_ungrouped', aggregate-into requires write-only, the expression 'aggvar_ungrouped' is not allowed [into table aggvar_ungrouped select count(*) as total, aggvar_ungrouped from SupportBean]");
            // unidirectional join not supported
            SupportMessageAssertUtil.TryInvalid(epService, "into table aggvar_ungrouped select count(*) as total from SupportBean unidirectional, SupportBean_S0#keepall",
                    "Error starting statement: Into-table does not allow unidirectional joins [");
            // requires aggregation
            SupportMessageAssertUtil.TryInvalid(epService, "into table aggvar_ungrouped select * from SupportBean",
                    "Error starting statement: Into-table requires at least one aggregation function [");
    
            // invalid consumption
            //
    
            // invalid access keys type and count
            SupportMessageAssertUtil.TryInvalid(epService, "select aggvar_ungrouped['a'].total from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'aggvar_ungrouped[\"a\"].total': Incompatible number of key expressions for use with table 'aggvar_ungrouped', the table expects no key expressions and provided are 1 key expressions [select aggvar_ungrouped['a'].total from SupportBean]");
            SupportMessageAssertUtil.TryInvalid(epService, "select aggvar_grouped_string.total from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'aggvar_grouped_string.total': Failed to resolve property 'aggvar_grouped_string.total' to a stream or nested property in a stream [");
            SupportMessageAssertUtil.TryInvalid(epService, "select aggvar_grouped_string[5].total from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'aggvar_grouped_string[5].total': Incompatible type returned by a key expression for use with table 'aggvar_grouped_string', the key expression '5' returns '" + Name.Clean<int>() + "' but the table expects 'System.String' [select aggvar_grouped_string[5].total from SupportBean]");
            // top-level variable use without "keys" function
            SupportMessageAssertUtil.TryInvalid(epService, "select aggvar_grouped_string.something() from SupportBean",
                    "Invalid use of variable 'aggvar_grouped_string', unrecognized use of function 'something', expected 'keys()' [");
            SupportMessageAssertUtil.TryInvalid(epService, "select dummy['a'] from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'dummy[\"a\"]': A table 'dummy' could not be found [select dummy['a'] from SupportBean]");
            SupportMessageAssertUtil.TryInvalid(epService, "select aggvarctx.dummy from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'aggvarctx.dummy': A column 'dummy' could not be found for table 'aggvarctx' [select aggvarctx.dummy from SupportBean]");
            SupportMessageAssertUtil.TryInvalid(epService, "select aggvarctx_ungrouped_window.win.dummy(123) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'aggvarctx_ungrouped_window.win.dumm...(41 chars)': Failed to resolve 'aggvarctx_ungrouped_window.win.dummy' to a property, single-row function, aggregation function, script, stream or class name [select aggvarctx_ungrouped_window.win.dummy(123) from SupportBean]");
            SupportMessageAssertUtil.TryInvalid(epService, "context MyOtherContext select aggvarctx.total from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'aggvarctx.total': Table by name 'aggvarctx' has been declared for context 'MyContext' and can only be used within the same context [context MyOtherContext select aggvarctx.total from SupportBean]");
            SupportMessageAssertUtil.TryInvalid(epService, "context MyOtherContext select aggvarctx.total from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'aggvarctx.total': Table by name 'aggvarctx' has been declared for context 'MyContext' and can only be used within the same context [context MyOtherContext select aggvarctx.total from SupportBean]");
            SupportMessageAssertUtil.TryInvalid(epService, "select aggvar_grouped_int[0].a.b from SupportBean",
                    "Invalid table expression 'aggvar_grouped_int[0].a.b [select aggvar_grouped_int[0].a.b from SupportBean]");
            // invalid use in non-contextual evaluation
            SupportMessageAssertUtil.TryInvalid(epService, "select * from SupportBean#time(aggvar_ungrouped.total sec)",
                    "Error starting statement: Error in view 'time', Invalid parameter expression 0 for Time view: Failed to validate view parameter expression 'aggvar_ungrouped.total seconds': Invalid use of table access expression, expression 'aggvar_ungrouped' is not allowed here [select * from SupportBean#time(aggvar_ungrouped.total sec)]");
            // indexed property expression but not an aggregtion-type variable
            epService.EPAdministrator.CreateEPL("create objectarray schema MyEvent(abc int[])");
            SupportMessageAssertUtil.TryInvalid(epService, "select abc[5*5] from MyEvent",
                    "Error starting statement: Failed to validate select-clause expression 'abc[5*5]': A table 'abc' could not be found [select abc[5*5] from MyEvent]");
            // view use
            SupportMessageAssertUtil.TryInvalid(epService, "select * from aggvar_grouped_string#time(30)",
                    "Views are not supported with tables");
            SupportMessageAssertUtil.TryInvalid(epService, "select (select * from aggvar_ungrouped#keepall) from SupportBean",
                    "Views are not supported with tables [");
            // contained use
            SupportMessageAssertUtil.TryInvalid(epService, "select * from aggvar_grouped_string[books]",
                    "Contained-event expressions are not supported with tables");
            // join invalid
            SupportMessageAssertUtil.TryInvalid(epService, "select aggvar_grouped_int[1].total.countMinSketchFrequency(TheString) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'aggvar_grouped_int[1].total.countMi...(62 chars)': Invalid combination of aggregation state and aggregation accessor [");
            SupportMessageAssertUtil.TryInvalid(epService, "select total.countMinSketchFrequency(TheString) from aggvar_grouped_int, SupportBean unidirectional",
                    "Error starting statement: Failed to validate select-clause expression 'total.countMinSketchFrequency(TheString)': Failed to validate method-chain expression 'total.countMinSketchFrequency(TheString)': Invalid combination of aggregation state and aggregation accessor [");
            // cannot be marked undirectional
            SupportMessageAssertUtil.TryInvalid(epService, "select * from aggvar_grouped_int unidirectional, SupportBean",
                    "Error starting statement: Tables cannot be marked as unidirectional [");
            // cannot be marked with retain
            SupportMessageAssertUtil.TryInvalid(epService, "select * from aggvar_grouped_int retain-union",
                    "Error starting statement: Tables cannot be marked with retain [");
            // cannot be used in on-action
            SupportMessageAssertUtil.TryInvalid(epService, "on aggvar_ungrouped select * from aggvar_ungrouped",
                    "Error starting statement: Tables cannot be used in an on-action statement triggering stream [");
            // cannot be used in match-recognize
            SupportMessageAssertUtil.TryInvalid(epService, "select * from aggvar_ungrouped " +
                            "match_recognize ( measures a.TheString as a pattern (A) define A as true)",
                    "Error starting statement: Tables cannot be used with match-recognize [");
            // cannot be used in update-istream
            SupportMessageAssertUtil.TryInvalid(epService, "update istream aggvar_grouped_string set key = 'a'",
                    "Error starting statement: Tables cannot be used in an update-istream statement [");
            // cannot be used in create-context
            SupportMessageAssertUtil.TryInvalid(epService, "create context InvalidCtx as start aggvar_ungrouped end after 5 seconds",
                    "Error starting statement: Tables cannot be used in a context declaration [");
            // cannot be used in patterns
            SupportMessageAssertUtil.TryInvalid(epService, "select * from pattern[aggvar_ungrouped]",
                    "Tables cannot be used in pattern filter atoms [");
            // schema by the same name
            SupportMessageAssertUtil.TryInvalid(epService, "create schema aggvar_ungrouped as " + typeof(SupportBean).FullName,
                    "Error starting statement: A table by name 'aggvar_ungrouped' already exists [");
            try {
                epService.EPAdministrator.Configuration.AddEventType("aggvar_ungrouped", "p0".Split(','), new object[]{typeof(int)});
                Assert.Fail();
            } catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "A table by name 'aggvar_ungrouped' already exists");
            }
        }
    
        public static string MySingleRowFunction(int? value) {
            return null;
        }
    
        public class MyAggregationFunctionFactory : AggregationFunctionFactory {
            public string FunctionName
            {
                set { }
            }

            public void Validate(AggregationValidationContext validationContext) {
    
            }
    
            public AggregationMethod NewAggregator() {
                return null;
            }

            public Type ValueType
            {
                get { return null; }
            }
        }
    }
} // end of namespace
