///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

using SupportBean_N = com.espertech.esper.regressionlib.support.bean.SupportBean_N;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternInvalid
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithInvalidExpr(execs);
            WithStatementException(execs);
            WithUseResult(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithUseResult(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new PatternUseResult());
            return execs;
        }

        public static IList<RegressionExecution> WithStatementException(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new PatternStatementException());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidExpr(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new PatternInvalidExpr());
            return execs;
        }

        private static void TryInvalid(
            RegressionEnvironment env,
            string eplInvalidPattern)
        {
            try {
                env.CompileWCheckedEx("select * from pattern[" + eplInvalidPattern + "]");
                Assert.Fail();
            }
            catch (EPCompileException) {
                // Expected exception
            }
        }

        private static string GetSyntaxExceptionPattern(
            RegressionEnvironment env,
            string expression)
        {
            string exceptionText = null;
            try {
                env.CompileWCheckedEx("select * from pattern[" + expression + "]");
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                exceptionText = ex.Message;
                log.Debug(".getSyntaxExceptionPattern pattern=" + expression, ex);
                // Expected exception
            }

            return exceptionText;
        }

        private static EPCompileException GetStatementExceptionPattern(
            RegressionEnvironment env,
            string expression)
        {
            return GetStatementExceptionPattern(env, expression, false);
        }

        private static EPCompileException GetStatementExceptionPattern(
            RegressionEnvironment env,
            string expression,
            bool isLogException)
        {
            try {
                env.CompileWCheckedEx("select * from pattern[" + expression + "]");
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                // Expected exception
                if (isLogException) {
                    log.Debug(expression, ex);
                }

                return ex;
            }

            throw new IllegalStateException();
        }

        private static void TryValid(
            RegressionEnvironment env,
            string expression)
        {
            env.CompileDeploy("select * from pattern[" + expression + "]").UndeployAll();
        }

        internal class PatternInvalidExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var exceptionText = GetSyntaxExceptionPattern(env, "SupportBean_N(DoublePrimitive='ss'");
                AssertMessage(
                    exceptionText,
                    "Incorrect syntax near ']' expecting a closing parenthesis ')' but found a right angle bracket ']' at line 1 column 56, please check the filter specification within the pattern expression within the from clause");

                env.CompileDeploy("select * from pattern[(not a=SupportBean) -> SupportBean(TheString=a.TheString)]");

                // test invalid subselect
                var epl = "create window WaitWindow#keepall as (waitTime int);\n" +
                          "insert into WaitWindow select IntPrimitive as waitTime from SupportBean;\n";
                env.CompileDeploy(epl);
                env.SendEventBean(new SupportBean("E1", 100));

                TryInvalidCompile(
                    env,
                    "select * from pattern[timer:interval((select waitTime from WaitWindow))]",
                    "Subselects are not allowed within pattern observer parameters, please consider using a variable instead");

                env.UndeployAll();
            }
        }

        internal class PatternStatementException : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                EPCompileException exception;

                exception = GetStatementExceptionPattern(env, "timer:at(2,3,4,4,4)");
                AssertMessage(
                    exception,
                    "Invalid parameter for pattern observer 'timer:at(2,3,4,4,4)': Error computing crontab schedule specification: Invalid combination between days of week and days of month fields for timer:at [");

                exception = GetStatementExceptionPattern(env, "timer:at(*,*,*,*,*,0,-1)");
                AssertMessage(
                    exception,
                    "Invalid parameter for pattern observer 'timer:at(*,*,*,*,*,0,-1)': Error computing crontab schedule specification: Invalid timezone parameter '-1' for timer:at, expected a string-type value [");

                exception = GetStatementExceptionPattern(env, "SupportBean -> timer:within()");
                AssertMessage(
                    exception,
                    "Failed to resolve pattern observer 'timer:within()': Pattern guard function 'within' cannot be used as a pattern observer [");

                exception = GetStatementExceptionPattern(env, "SupportBean where timer:interval(100)");
                AssertMessage(
                    exception,
                    "Failed to resolve pattern guard '" +
                    nameof(SupportBean) +
                    " where timer:interval(100)': Pattern observer function 'interval' cannot be used as a pattern guard [");

                exception = GetStatementExceptionPattern(env, "SupportBean -> timer:interval()");
                AssertMessage(
                    exception,
                    "Invalid parameter for pattern observer 'timer:interval()': Timer-interval observer requires a single numeric or time period parameter [");

                exception = GetStatementExceptionPattern(env, "SupportBean where timer:within()");
                AssertMessage(
                    exception,
                    "Invalid parameter for pattern guard '" +
                    nameof(SupportBean) +
                    " where timer:within()': Timer-within guard requires a single numeric or time period parameter [");

                // class not found
                exception = GetStatementExceptionPattern(env, "dummypkg.dummy()");
                AssertMessage(
                    exception,
                    "Failed to resolve event type, named window or table by name 'dummypkg.dummy' [");

                // simple property not found
                exception = GetStatementExceptionPattern(env, "SupportBean_N(dummy=1)");
                AssertMessage(
                    exception,
                    "Failed to validate filter expression 'dummy=1': Property named 'dummy' is not valid in any stream [");

                // nested property not found
                exception = GetStatementExceptionPattern(env, "SupportBean_N(dummy.Nested=1)");
                AssertMessage(
                    exception,
                    "Failed to validate filter expression 'dummy.Nested=1': Failed to resolve property 'dummy.Nested' to a stream or nested property in a stream [");

                // property wrong type
                exception = GetStatementExceptionPattern(env, "SupportBean_N(IntPrimitive='s')");
                AssertMessage(
                    exception,
                    $"Failed to validate filter expression 'IntPrimitive=\"s\"': Implicit conversion from datatype '{typeof(string).CleanName()}' to '{typeof(int?).CleanName()}' is not allowed [");

                // property not a primitive type
                exception = GetStatementExceptionPattern(env, "SupportBeanComplexProps(Nested=1)");
                AssertMessage(
                    exception,
                    $"Failed to validate filter expression 'Nested=1': Implicit conversion from datatype '{typeof(int?).CleanName()}' to '{typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested).CleanName()}' is not allowed [");

                // no tag matches prior use
                exception = GetStatementExceptionPattern(env, "SupportBean_N(DoublePrimitive=x.abc)");
                AssertMessage(
                    exception,
                    "Failed to validate filter expression 'DoublePrimitive=x.abc': Failed to resolve property 'x.abc' to a stream or nested property in a stream [");

                // range not valid on string
                exception = GetStatementExceptionPattern(env, "SupportBean(TheString in [1:2])");
                AssertMessage(
                    exception,
                    $"Failed to validate filter expression 'TheString between 1 and 2': Implicit conversion from datatype '{typeof(string).CleanName()}' to numeric is not allowed [");

                // range does not allow string params
                exception = GetStatementExceptionPattern(env, "SupportBean(DoubleBoxed in ['a':2])");
                AssertMessage(
                    exception,
                    $"Failed to validate filter expression 'DoubleBoxed between \"a\" and 2': Implicit conversion from datatype '{typeof(string).CleanName()}' to numeric is not allowed [");

                // invalid observer arg
                exception = GetStatementExceptionPattern(env, "timer:at(9l)");
                AssertMessage(
                    exception,
                    "Invalid parameter for pattern observer 'timer:at(9L)': Invalid number of parameters for timer:at");

                // invalid guard arg
                exception = GetStatementExceptionPattern(env, "SupportBean where timer:within('s')");
                AssertMessage(
                    exception,
                    $"Invalid parameter for pattern guard '{nameof(SupportBean)} where timer:within(\"s\")': Timer-within guard requires a single numeric or time period parameter [");

                // use-result property is wrong type
                exception = GetStatementExceptionPattern(
                    env,
                    "x=SupportBean -> SupportBean(DoublePrimitive=x.BoolBoxed)");
                AssertMessage(
                    exception,
                    $"Failed to validate filter expression 'DoublePrimitive=x.BoolBoxed': Implicit conversion from datatype '{typeof(bool?).CleanName()}' to '{typeof(double?).CleanName()}' is not allowed [");

                // named-parameter for timer:at or timer:interval
                exception = GetStatementExceptionPattern(env, "timer:interval(interval:10)");
                AssertMessage(
                    exception,
                    "Invalid parameter for pattern observer 'timer:interval(interval:10)': Timer-interval observer does not allow named parameters ");
                exception = GetStatementExceptionPattern(env, "timer:at(perhaps:10)");
                AssertMessage(
                    exception,
                    "Invalid parameter for pattern observer 'timer:at(perhaps:10)': timer:at does not allow named parameters");
            }
        }

        internal class PatternUseResult : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var @event = nameof(SupportBean_N);

                TryValid(env, $"na={@event} -> nb={@event}(DoublePrimitive = na.DoublePrimitive)");
                TryInvalid(env, $"xx={@event} -> nb={@event}(DoublePrimitive = na.DoublePrimitive)");
                TryInvalid(env, $"na={@event} -> nb={@event}(DoublePrimitive = xx.DoublePrimitive)");
                TryInvalid(env, $"na={@event} -> nb={@event}(DoublePrimitive = na.xx)");
                TryInvalid(env, $"xx={@event} -> nb={@event}(xx = na.DoublePrimitive)");
                TryInvalid(env, $"na={@event} -> nb={@event}(xx = na.xx)");
                TryValid(
                    env,
                    $"na={@event} -> nb={@event}(DoublePrimitive = na.DoublePrimitive, IntBoxed=na.IntBoxed)");
                TryValid(
                    env,
                    $"na={@event}() -> nb={@event}(DoublePrimitive in (na.DoublePrimitive:na.DoubleBoxed))");
                TryValid(
                    env,
                    $"na={@event}() -> nb={@event}(DoublePrimitive in [na.DoublePrimitive:na.DoubleBoxed])");
                TryValid(
                    env,
                    $"na={@event}() -> nb={@event}(DoublePrimitive in [na.IntBoxed:na.IntPrimitive])");
                TryInvalid(env, "na=" + @event + "() -> nb=" + @event + "(DoublePrimitive in [na.IntBoxed:na.xx])");
                TryInvalid(
                    env,
                    $"na={@event}() -> nb={@event}(DoublePrimitive in [na.IntBoxed:na.BoolBoxed])");
                TryInvalid(env, "na=" + @event + "() -> nb=" + @event + "(DoublePrimitive in [na.xx:na.IntPrimitive])");
                TryInvalid(
                    env,
                    $"na={@event}() -> nb={@event}(DoublePrimitive in [na.BoolBoxed:na.IntPrimitive])");
            }
        }
    }
} // end of namespace