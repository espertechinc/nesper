///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableInvalid
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithInvalidAggMatchSingleFunc(execs);
            WithInvalidAggMatchMultiFunc(execs);
            WithInvalidAnnotations(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidAnnotations(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraInvalidAnnotations());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidAggMatchMultiFunc(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraInvalidAggMatchMultiFunc());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidAggMatchSingleFunc(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraInvalidAggMatchSingleFunc());
            return execs;
        }

        private static void TryInvalidAggMatch(
            RegressionEnvironment env,
            string name,
            string declared,
            bool unbound,
            string provided,
            string messageOrNull)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@Name('create') create table " + name + "(value " + declared + ")", path);

            try {
                var epl = "into table " +
                          name +
                          " select " +
                          provided +
                          " as value from SupportBean" +
                          (unbound ? "#time(1000)" : "");
                env.CompileWCheckedEx(epl, path);
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                if (messageOrNull != null && messageOrNull.Length > 10) {
                    StringAssert.StartsWith(messageOrNull, ex.Message);
                }
                else {
                    StringAssert.Contains("Incompatible aggregation function for table", ex.Message);
                }
            }

            env.UndeployModuleContaining("create");
        }

        public static string MySingleRowFunction(int? value)
        {
            return null;
        }

        internal class InfraInvalidAggMatchSingleFunc : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // sum
                TryInvalidAggMatch(
                    env,
                    "var1",
                    "sum(double)",
                    false,
                    "sum(IntPrimitive)",
                    "Error during compilation: " +
                    "Incompatible aggregation function for table 'var1' column 'value', expecting 'sum(double)' and received 'sum(IntPrimitive)': The required parameter type is System.Double and provided is System.Nullable<System.Int32> [");
                TryInvalidAggMatch(
                    env,
                    "var1",
                    "sum(double)",
                    false,
                    "count(*)",
                    "Error during compilation: " +
                    "Incompatible aggregation function for table 'var1' column 'value', expecting 'sum(double)' and received 'count(*)': The table declares 'sum(double)' and provided is 'count(*)'");
                TryInvalidAggMatch(
                    env,
                    "var1",
                    "sum(double)",
                    false,
                    "sum(DoublePrimitive, TheString='a')",
                    "Error during compilation: " +
                    "Incompatible aggregation function for table 'var1' column 'value', expecting 'sum(double)' and received 'sum(DoublePrimitive,TheString=\"a\")': The aggregation declares no filter expression and provided is a filter expression [");

                TryInvalidAggMatch(
                    env,
                    "var1",
                    "sum(double, boolean)",
                    false,
                    "sum(DoublePrimitive)",
                    "Error during compilation: " +
                    "Incompatible aggregation function for table 'var1' column 'value', expecting 'sum(double,boolean)' and received 'sum(DoublePrimitive)': The aggregation declares a filter expression and provided is no filter expression [");

                // count
                TryInvalidAggMatch(
                    env,
                    "var1",
                    "count(*)",
                    false,
                    "sum(IntPrimitive)",
                    "Error during compilation: " +
                    "Incompatible aggregation function for table 'var1' column 'value', expecting 'count(*)' and received 'sum(IntPrimitive)': The table declares 'count(*)' and provided is 'sum(IntPrimitive)'");
                TryInvalidAggMatch(
                    env,
                    "var1",
                    "count(*)",
                    false,
                    "count(distinct IntPrimitive)",
                    "Error during compilation: " +
                    "Incompatible aggregation function for table 'var1' column 'value', expecting 'count(*)' and received 'count(distinct IntPrimitive)': The aggregation declares no distinct and provided is a distinct [");
                
                TryInvalidAggMatch(env, "var1", "count(*)", false, "count(distinct IntPrimitive, BoolPrimitive)", null);
                TryInvalidAggMatch(env, "var1", "count(distinct int)", false, "count(distinct DoublePrimitive)", null);
                TryInvalidAggMatch(
                    env,
                    "var1",
                    "count(int)",
                    false,
                    "count(*)",
                    "Error during compilation: " +
                    "Incompatible aggregation function for table 'var1' column 'value', expecting 'count(int)' and received 'count(*)': The aggregation declares ignore nulls and provided is no ignore nulls [");

                // avg
                TryInvalidAggMatch(env, "var1", "avg(int)", false, "sum(IntPrimitive)", null);
                TryInvalidAggMatch(env, "var1", "avg(int)", false, "avg(LongPrimitive)", null);
                TryInvalidAggMatch(env, "var1", "avg(int)", false, "avg(IntPrimitive, BoolPrimitive)", null);
                TryInvalidAggMatch(env, "var1", "avg(int)", false, "avg(distinct IntPrimitive)", null);

                // min-max
                TryInvalidAggMatch(
                    env,
                    "var1",
                    "max(int)",
                    false,
                    "min(IntPrimitive)",
                    "Error during compilation: " +
                    "Incompatible aggregation function for table 'var1' column 'value', expecting 'max(int)' and received 'min(IntPrimitive)': The aggregation declares max and provided is min [");
                TryInvalidAggMatch(env, "var1", "min(int)", false, "avg(IntPrimitive)", null);
                TryInvalidAggMatch(env, "var1", "min(int)", false, "min(DoublePrimitive)", null);
                TryInvalidAggMatch(
                    env,
                    "var1",
                    "min(int)",
                    false,
                    "fmin(IntPrimitive, TheString='a')",
                    "Error during compilation: " +
                    "Incompatible aggregation function for table 'var1' column 'value', expecting 'min(int)' and received 'min(IntPrimitive,TheString=\"a\")': The aggregation declares no filter expression and provided is a filter expression [");

                // stddev
                TryInvalidAggMatch(env, "var1", "stddev(int)", false, "avg(IntPrimitive)", null);
                TryInvalidAggMatch(env, "var1", "stddev(int)", false, "stddev(DoublePrimitive)", null);
                TryInvalidAggMatch(env, "var1", "stddev(int)", false, "stddev(IntPrimitive, true)", null);

                // avedev
                TryInvalidAggMatch(env, "var1", "avedev(int)", false, "avg(IntPrimitive)", null);
                TryInvalidAggMatch(env, "var1", "avedev(int)", false, "avedev(DoublePrimitive)", null);
                TryInvalidAggMatch(env, "var1", "avedev(int)", false, "avedev(IntPrimitive, true)", null);

                // median
                TryInvalidAggMatch(env, "var1", "median(int)", false, "avg(IntPrimitive)", null);
                TryInvalidAggMatch(env, "var1", "median(int)", false, "median(DoublePrimitive)", null);
                TryInvalidAggMatch(env, "var1", "median(int)", false, "median(IntPrimitive, true)", null);

                // firstever
                TryInvalidAggMatch(env, "var1", "firstever(int)", false, "lastever(IntPrimitive)", null);
                TryInvalidAggMatch(env, "var1", "firstever(int)", false, "firstever(DoublePrimitive)", null);
                TryInvalidAggMatch(env, "var1", "firstever(int, boolean)", false, "firstever(IntPrimitive)", null);

                // lastever
                TryInvalidAggMatch(env, "var1", "lastever(int)", false, "firstever(IntPrimitive)", null);
                TryInvalidAggMatch(env, "var1", "lastever(int)", false, "lastever(DoublePrimitive)", null);
                TryInvalidAggMatch(env, "var1", "lastever(int, boolean)", false, "lastever(IntPrimitive)", null);

                // countever
                TryInvalidAggMatch(env, "var1", "lastever(int)", true, "countever(IntPrimitive)", null);
                TryInvalidAggMatch(env, "var1", "lastever(int, boolean)", true, "countever(IntPrimitive)", null);
                TryInvalidAggMatch(env, "var1", "lastever(int)", true, "countever(IntPrimitive, true)", null);
                TryInvalidAggMatch(env, "var1", "countever(*)", true, "countever(IntPrimitive)", null);

                // nth
                TryInvalidAggMatch(env, "var1", "nth(int, 10)", false, "avg(20)", null);
                TryInvalidAggMatch(
                    env,
                    "var1",
                    "nth(int, 10)",
                    false,
                    "nth(IntPrimitive, 11)",
                    "Error during compilation: " +
                    "Incompatible aggregation function for table 'var1' column 'value', expecting 'nth(int,10)' and received 'nth(IntPrimitive,11)': The size is 10 and provided is 11 [");
                TryInvalidAggMatch(env, "var1", "nth(int, 10)", false, "nth(DoublePrimitive, 10)", null);

                // rate
                TryInvalidAggMatch(env, "var1", "rate(20)", false, "avg(20)", null);
                TryInvalidAggMatch(
                    env,
                    "var1",
                    "rate(20)",
                    false,
                    "rate(11)",
                    "Error during compilation: " +
                    "Incompatible aggregation function for table 'var1' column 'value', expecting 'rate(20)' and received 'rate(11)': The interval-time is 20000 and provided is 11000 [");

                // leaving
                TryInvalidAggMatch(env, "var1", "leaving()", false, "avg(IntPrimitive)", null);

                // plug-in single-func
                TryInvalidAggMatch(
                    env,
                    "var1",
                    "myaggsingle()",
                    false,
                    "leaving()",
                    "Error during compilation: " +
                    "Incompatible aggregation function for table 'var1' column 'value', expecting 'myaggsingle(*)' and received 'leaving(*)': The table declares 'myaggsingle(*)' and provided is 'leaving(*)'");
            }
        }

        internal class InfraInvalidAggMatchMultiFunc : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Window and related
                //
                // window vs agg method
                TryInvalidAggMatch(
                    env,
                    "var1",
                    "window(*) @type(SupportBean)",
                    true,
                    "avg(IntPrimitive)",
                    "Error during compilation: " +
                    "Incompatible aggregation function for table 'var1' column 'value', expecting 'window(*)' and received 'avg(IntPrimitive)': " +
                    "The table declares 'window(*)' and provided is 'avg(IntPrimitive)'");
                // window vs sorted
                TryInvalidAggMatch(
                    env,
                    "var1",
                    "window(*) @type(SupportBean)",
                    true,
                    "sorted(IntPrimitive)",
                    "Error during compilation: " +
                    "Failed to validate select-clause expression 'sorted(IntPrimitive)': " +
                    "When specifying into-table a sort expression cannot be provided [");
                // wrong type
                TryInvalidAggMatch(
                    env,
                    "var1",
                    "window(*) @type(SupportBean_S0)",
                    true,
                    "window(*)",
                    "Error during compilation: " +
                    "Incompatible aggregation function for table 'var1' column 'value', expecting 'window(*)' and received 'window(*)': " +
                    "The required event type is 'SupportBean_S0' and provided is 'SupportBean' [");

                // sorted
                //
                TryInvalidAggMatch(
                    env,
                    "var1",
                    "sorted(IntPrimitive) @type(SupportBean)",
                    true,
                    "window(*)",
                    "Error during compilation: " +
                    "Incompatible aggregation function for table 'var1' column 'value', expecting 'sorted(IntPrimitive)' and received 'window(*)': " +
                    "The table declares 'sorted(IntPrimitive)' and provided is 'window(*)'");
                TryInvalidAggMatch(
                    env,
                    "var1",
                    "sorted(Id) @type(SupportBean_S0)",
                    true,
                    "sorted(IntPrimitive)",
                    "Error during compilation: " +
                    "Failed to validate select-clause expression 'sorted(IntPrimitive)': " +
                    "When specifying into-table a sort expression cannot be provided [");

                // plug-in
                //
                TryInvalidAggMatch(
                    env,
                    "var1",
                    "se1() @type(SupportBean)",
                    true,
                    "window(*)",
                    "Error during compilation: " +
                    "Incompatible aggregation function for table 'var1' column 'value', expecting 'se1(*)' and received 'window(*)': " +
                    "The table declares 'se1(*)' and provided is 'window(*)'");
            }
        }

        internal class InfraInvalidAnnotations : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // unknown marker
                TryInvalidCompile(
                    env,
                    "create table v1 (abc window(*) @unknown)",
                    "For column 'abc' unrecognized annotation 'unknown' [");

                // no type provided
                TryInvalidCompile(
                    env,
                    "create table v1 (abc window(*) @type)",
                    "For column 'abc' no value provided for annotation 'type', expected a value [");

                // multiple value
                TryInvalidCompile(
                    env,
                    "create table v1 (abc window(*) @type(SupportBean) @type(SupportBean))",
                    "For column 'abc' multiple annotations provided named 'type' [");

                // wrong value
                TryInvalidCompile(
                    env,
                    "create table v1 (abc window(*) @type(1))",
                    "For column 'abc' string value expected for annotation 'type' [");

                // unknown type provided
                TryInvalidCompile(
                    env,
                    "create table v1 (abc window(*) @type(xx))",
                    "For column 'abc' failed to find event type 'xx' [");
            }
        }

        internal class InfraInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create table aggvar_grouped_string (key string primary key, total count(*))", path);
                env.CompileDeploy(
                    "create table aggvar_twogrouped (keyone string primary key, keytwo string primary key, total count(*))",
                    path);
                env.CompileDeploy("create table aggvar_grouped_int (key int primary key, total count(*))", path);
                env.CompileDeploy("create table aggvar_ungrouped as (total count(*))", path);
                env.CompileDeploy("create table aggvar_ungrouped_window as (win window(*) @type(SupportBean))", path);
                env.CompileDeploy(
                    "create context MyContext initiated by SupportBean_S0 terminated by SupportBean_S1",
                    path);
                env.CompileDeploy("context MyContext create table aggvarctx (total count(*))", path);
                env.CompileDeploy(
                    "create context MyOtherContext initiated by SupportBean_S0 terminated by SupportBean_S1",
                    path);
                env.CompileDeploy("create variable int myvariable", path);
                env.CompileDeploy("create window MyNamedWindow#keepall as select * from SupportBean", path);
                env.CompileDeploy("create schema SomeSchema(p0 string)", path);

                // invalid declaration
                //
                //
                // constant
                TryInvalidCompile(
                    env,
                    path,
                    "create constant variable aggvar_ungrouped (total count(*))",
                    "Incorrect syntax near '(' expecting an identifier but found an opening parenthesis '(' at line 1 column 42 [");
                // invalid type
                TryInvalidCompile(
                    env,
                    path,
                    "create table aggvar_notright as (total sum(abc))",
                    "Failed to resolve type 'abc': Could not load class by name 'abc', please check imports [");
                // invalid non-aggregation
                TryInvalidCompile(
                    env,
                    path,
                    "create table aggvar_wrongtoo as (total singlerow(1))",
                    "Expression 'singlerow(1)' is not an aggregation [");
                // can only declare "sorted()" or "window" aggregation function
                // this is to make sure future compatibility when optimizing queries
                TryInvalidCompile(
                    env,
                    path,
                    "create table aggvar_invalid as (mywindow window(IntPrimitive) @type(SupportBean))",
                    "Failed to validate table-column expression 'window(IntPrimitive)': For tables columns, the window aggregation function requires the 'window(*)' declaration [");
                TryInvalidCompile(
                    env,
                    path,
                    "create table aggvar_invalid as (mywindow last(*)@type(SupportBean))",
                    "Failed to validate table-column expression 'last(*)': For tables columns, the last aggregation function requires the 'window(*)' declaration");
                TryInvalidCompile(
                    env,
                    path,
                    "create table aggvar_invalid as (mywindow window(sb.*)@type(SupportBean)",
                    "Incorrect syntax near end-of-input expecting a closing parenthesis ')' but found EOF at line 1 column 71");
                TryInvalidCompile(
                    env,
                    path,
                    "create table aggvar_invalid as (mymax maxBy(IntPrimitive) @type(SupportBean))",
                    "Failed to validate table-column expression 'maxby(IntPrimitive)': For tables columns, the aggregation function requires the 'sorted(*)' declaration [");
                // same column multiple times
                TryInvalidCompile(
                    env,
                    path,
                    "create table aggvar_invalid as (mycount count(*),mycount count(*))",
                    "Column 'mycount' is listed more than once [create table aggvar_invalid as (mycount count(*),mycount count(*))]");
                // already a variable of the same name
                TryInvalidCompile(
                    env,
                    path,
                    "create table myvariable as (mycount count(*))",
                    "A variable by name 'myvariable' has already been declared [");
                TryInvalidCompile(
                    env,
                    path,
                    "create table aggvar_ungrouped as (total count(*))",
                    "A table by name 'aggvar_ungrouped' has already been declared [");
                // invalid primary key use
                TryInvalidCompile(
                    env,
                    path,
                    "create table abc as (total count(*) primary key)",
                    "Column 'total' may not be tagged as primary key, an expression cannot become a primary key column [");
                TryInvalidCompile(
                    env,
                    path,
                    "create table abc as (arr SupportBean primary key)",
                    "Column 'arr' may not be tagged as primary key, received unexpected event type 'SupportBean' [");
                TryInvalidCompile(
                    env,
                    path,
                    "create table abc as (mystr string prim key)",
                    "Invalid keyword 'prim' encountered, expected 'primary key' [");
                TryInvalidCompile(
                    env,
                    path,
                    "create table abc as (mystr string primary keys)",
                    "Invalid keyword 'keys' encountered, expected 'primary key' [");
                TryInvalidCompile(
                    env,
                    path,
                    "create table SomeSchema as (mystr string)",
                    "An event type by name 'SomeSchema' has already been declared");

                // invalid-into
                //
                //
                // table-not-found
                TryInvalidCompile(
                    env,
                    path,
                    "into table xxx select count(*) as total from SupportBean group by IntPrimitive",
                    "Invalid into-table clause: Failed to find table by name 'xxx' [");

                // group-by key type and count of group-by expressions
                TryInvalidCompile(
                    env,
                    path,
                    "into table aggvar_grouped_string select count(*) as total from SupportBean group by IntPrimitive",
                    "Incompatible type returned by a group-by expression for use with table 'aggvar_grouped_string', the group-by expression 'IntPrimitive' returns 'System.Nullable<System.Int32>' but the table expects 'System.String' [");
                TryInvalidCompile(
                    env,
                    path,
                    "into table aggvar_grouped_string select count(*) as total from SupportBean group by TheString, IntPrimitive",
                    "Incompatible number of group-by expressions for use with table 'aggvar_grouped_string', the table expects 1 group-by expressions and provided are 2 group-by expressions [");
                TryInvalidCompile(
                    env,
                    path,
                    "into table aggvar_ungrouped select count(*) as total from SupportBean group by TheString",
                    "Incompatible number of group-by expressions for use with table 'aggvar_ungrouped', the table expects no group-by expressions and provided are 1 group-by expressions [");
                TryInvalidCompile(
                    env,
                    path,
                    "into table aggvar_grouped_string select count(*) as total from SupportBean",
                    "Incompatible number of group-by expressions for use with table 'aggvar_grouped_string', the table expects 1 group-by expressions and provided are no group-by expressions [");
                TryInvalidCompile(
                    env,
                    path,
                    "into table aggvarctx select count(*) as total from SupportBean",
                    "Table by name 'aggvarctx' has been declared for context 'MyContext' and can only be used within the same context [into table aggvarctx select count(*) as total from SupportBean]");
                TryInvalidCompile(
                    env,
                    path,
                    "context MyOtherContext into table aggvarctx select count(*) as total from SupportBean",
                    "Table by name 'aggvarctx' has been declared for context 'MyContext' and can only be used within the same context [context MyOtherContext into table aggvarctx select count(*) as total from SupportBean]");
                TryInvalidCompile(
                    env,
                    path,
                    "into table aggvar_ungrouped select count(*) as total, aggvar_ungrouped from SupportBean",
                    "Invalid use of table 'aggvar_ungrouped', aggregate-into requires write-only, the expression 'aggvar_ungrouped' is not allowed [into table aggvar_ungrouped select count(*) as total, aggvar_ungrouped from SupportBean]");
                // unidirectional join not supported
                TryInvalidCompile(
                    env,
                    path,
                    "into table aggvar_ungrouped select count(*) as total from SupportBean unidirectional, SupportBean_S0#keepall",
                    "Into-table does not allow unidirectional joins [");
                // requires aggregation
                TryInvalidCompile(
                    env,
                    path,
                    "into table aggvar_ungrouped select * from SupportBean",
                    "Into-table requires at least one aggregation function [");

                // invalid consumption
                //

                // invalid access keys type and count
                TryInvalidCompile(
                    env,
                    path,
                    "select aggvar_ungrouped['a'].total from SupportBean",
                    "Failed to validate select-clause expression 'aggvar_ungrouped[\"a\"].total': Incompatible number of key expressions for use with table 'aggvar_ungrouped', the table expects no key expressions and provided are 1 key expressions [select aggvar_ungrouped['a'].total from SupportBean]");
                TryInvalidCompile(
                    env,
                    path,
                    "select aggvar_grouped_string.total from SupportBean",
                    "Failed to validate select-clause expression 'aggvar_grouped_string.total': Failed to resolve property 'aggvar_grouped_string.total' to a stream or nested property in a stream");
                TryInvalidCompile(
                    env,
                    path,
                    "select aggvar_grouped_string[5].total from SupportBean",
                    "Failed to validate select-clause expression 'aggvar_grouped_string[5].total': Incompatible type returned by a key expression for use with table 'aggvar_grouped_string', the key expression '5' returns 'System.Nullable<System.Int32>' but the table expects 'System.String' [select aggvar_grouped_string[5].total from SupportBean]");

                // top-level variable use without "keys" function
                TryInvalidCompile(
                    env,
                    path,
                    "select aggvar_grouped_string.something() from SupportBean",
                    "Invalid use of table 'aggvar_grouped_string', unrecognized use of function 'something', expected 'keys()'");
                TryInvalidCompile(
                    env,
                    path,
                    "select dummy[IntPrimitive] from SupportBean",
                    "Failed to validate select-clause expression 'dummy[IntPrimitive]': Failed to resolve 'dummy' to a property, single-row function, aggregation function, script, stream or class name");

                TryInvalidCompile(
                    env,
                    path,
                    "select aggvarctx.dummy from SupportBean",
                    "Failed to validate select-clause expression 'aggvarctx.dummy': A column 'dummy' could not be found for table 'aggvarctx' [select aggvarctx.dummy from SupportBean]");
                TryInvalidCompile(
                    env,
                    path,
                    "select aggvarctx_ungrouped_window.win.dummy(123) from SupportBean",
                    "Failed to validate select-clause expression 'aggvarctx_ungrouped_window.win.dumm...(41 chars)': Failed to resolve 'aggvarctx_ungrouped_window.win.dummy' to a property, single-row function, aggregation function, script, stream or class name [select aggvarctx_ungrouped_window.win.dummy(123) from SupportBean]");
                TryInvalidCompile(
                    env,
                    path,
                    "context MyOtherContext select aggvarctx.total from SupportBean",
                    "Failed to validate select-clause expression 'aggvarctx.total': Table by name 'aggvarctx' has been declared for context 'MyContext' and can only be used within the same context [context MyOtherContext select aggvarctx.total from SupportBean]");
                TryInvalidCompile(
                    env,
                    path,
                    "context MyOtherContext select aggvarctx.total from SupportBean",
                    "Failed to validate select-clause expression 'aggvarctx.total': Table by name 'aggvarctx' has been declared for context 'MyContext' and can only be used within the same context [context MyOtherContext select aggvarctx.total from SupportBean]");

                TryInvalidCompile(
                    env,
                    path,
                    "select aggvar_grouped_int[0].a.b from SupportBean",
                    "Failed to validate select-clause expression 'aggvar_grouped_int[0].a.b': A column 'a' could not be found for table 'aggvar_grouped_int'");

                // invalid use in non-contextual evaluation
                TryInvalidCompile(
                    env,
                    path,
                    "select * from SupportBean#time(aggvar_ungrouped.total sec)",
                    "Failed to validate data window declaration: Error in view 'time', Invalid parameter expression 0 for Time view: Failed to validate view parameter expression 'aggvar_ungrouped.total seconds': Invalid use of table access expression, expression 'aggvar_ungrouped' is not allowed here");
                // indexed property expression but not an aggregtion-type variable
                env.CompileDeploy("create objectarray schema MyEvent(abc int[])");
                // view use
                TryInvalidCompile(
                    env,
                    path,
                    "select * from aggvar_grouped_string#time(30)",
                    "Views are not supported with tables");
                TryInvalidCompile(
                    env,
                    path,
                    "select (select * from aggvar_ungrouped#keepall) from SupportBean",
                    "Views are not supported with tables [");
                // contained use
                TryInvalidCompile(
                    env,
                    path,
                    "select * from aggvar_grouped_string[books]",
                    "Contained-event expressions are not supported with tables");

                // join invalid
                TryInvalidCompile(
                    env,
                    path,
                    "select aggvar_grouped_int[1].total.countMinSketchFrequency(TheString) from SupportBean",
                    "Failed to validate select-clause expression 'aggvar_grouped_int[1].total.countMi...(62 chars)': Failed to resolve method 'countMinSketchFrequency': Could not find enumeration method, date-time method, instance method or property named 'countMinSketchFrequency' in class 'System.Nullable<System.Int64>' with matching parameter number and expected parameter type(s) 'System.String' ");
                TryInvalidCompile(
                    env,
                    path,
                    "select total.countMinSketchFrequency(TheString) from aggvar_grouped_int, SupportBean unidirectional",
                    "Failed to validate select-clause expression 'total.countMinSketchFrequency(TheString)': Failed to resolve method 'countMinSketchFrequency': Could not find");
                // cannot be marked undirectional
                TryInvalidCompile(
                    env,
                    path,
                    "select * from aggvar_grouped_int unidirectional, SupportBean",
                    "Tables cannot be marked as unidirectional [");
                // cannot be marked with retain
                TryInvalidCompile(
                    env,
                    path,
                    "select * from aggvar_grouped_int retain-union",
                    "Tables cannot be marked with retain [");
                // cannot be used in on-action
                TryInvalidCompile(
                    env,
                    path,
                    "on aggvar_ungrouped select * from aggvar_ungrouped",
                    "Tables cannot be used in an on-action statement triggering stream [");
                // cannot be used in match-recognize
                TryInvalidCompile(
                    env,
                    path,
                    "select * from aggvar_ungrouped " +
                    "match_recognize ( measures a.theString as a pattern (A) define A as true)",
                    "Tables cannot be used with match-recognize [");
                // cannot be used in update-istream
                TryInvalidCompile(
                    env,
                    path,
                    "update istream aggvar_grouped_string set key = 'a'",
                    "Tables cannot be used in an update-istream statement [");
                // cannot be used in create-context
                TryInvalidCompile(
                    env,
                    path,
                    "create context InvalidCtx as start aggvar_ungrouped end after 5 seconds",
                    "Tables cannot be used in a context declaration [");
                // cannot be used in patterns
                TryInvalidCompile(
                    env,
                    path,
                    "select * from pattern[aggvar_ungrouped]",
                    "Tables cannot be used in pattern filter atoms [");
                // schema by the same name
                TryInvalidCompile(
                    env,
                    path,
                    "create schema aggvar_ungrouped as SupportBean",
                    "A table by name 'aggvar_ungrouped' already exists [");

                // cannot use null-type key
                TryInvalidCompile(
                    env,
                    path,
                    "create table MyTable(somefield null primary key, id string)",
                    "Incorrect syntax near 'null' (a reserved keyword)");

                env.UndeployAll();
            }
        }
    }
} // end of namespace