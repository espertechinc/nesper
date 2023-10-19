///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework; // assertEquals

// fail

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherInvalid
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithInvalidFuncParams(execs);
            WithInvalidSyntax(execs);
            WithLongTypeConstant(execs);
            With(DifferentJoins)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithDifferentJoins(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherDifferentJoins());
            return execs;
        }

        public static IList<RegressionExecution> WithLongTypeConstant(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherLongTypeConstant());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidSyntax(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherInvalidSyntax());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidFuncParams(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherInvalidFuncParams());
            return execs;
        }

        private class EPLOtherInvalidFuncParams : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select count(theString, theString, theString) from SupportBean",
                    "Failed to validate select-clause expression 'count(theString,theString,theString)': The 'count' function expects at least 1 and up to 2 parameters");

                env.TryInvalidCompile(
                    "select leaving(theString) from SupportBean",
                    "Failed to validate select-clause expression 'leaving(theString)': The 'leaving' function expects no parameters");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private class EPLOtherInvalidSyntax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var exceptionText = GetSyntaxExceptionEPL(env, "select * from *");
                Assert.AreEqual(
                    "Incorrect syntax near '*' at line 1 column 14, please check the from clause [select * from *]",
                    exceptionText);

                exceptionText = GetSyntaxExceptionEPL(
                    env,
                    "select * from SupportBean a where a.intPrimitive between r.start and r.end");
                Assert.AreEqual(
                    "Incorrect syntax near 'start' (a reserved keyword) at line 1 column 59, please check the where clause [select * from SupportBean a where a.intPrimitive between r.start and r.end]",
                    exceptionText);

                env.TryInvalidCompile(
                    "select * from SupportBean(1=2=3)",
                    "Failed to validate filter expression '1=2': Invalid use of equals, expecting left-hand side and right-hand side but received 3 expressions");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private class EPLOtherLongTypeConstant : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select 2512570244 as value from SupportBean";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(new SupportBean());
                env.AssertEqualsNew("s0", "value", 2512570244L);

                env.UndeployAll();
            }
        }

        private class EPLOtherDifferentJoins : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var streamDef = "select * from " +
                                "SupportBean#length(3) as sa," +
                                "SupportBean#length(3) as sb" +
                                " where ";

                var streamDefTwo = "select * from " +
                                   "SupportBean#length(3)," +
                                   "SupportMarketDataBean#length(3)" +
                                   " where ";

                TryInvalid(env, streamDef + "sa.intPrimitive = sb.theString");
                TryValid(env, streamDef + "sa.intPrimitive = sb.intBoxed");
                TryValid(env, streamDef + "sa.intPrimitive = sb.intPrimitive");
                TryValid(env, streamDef + "sa.intPrimitive = sb.longBoxed");

                TryInvalid(env, streamDef + "sa.intPrimitive = sb.intPrimitive and sb.intBoxed = sa.boolPrimitive");
                TryValid(env, streamDef + "sa.intPrimitive = sb.intPrimitive and sb.boolBoxed = sa.boolPrimitive");

                TryInvalid(
                    env,
                    streamDef +
                    "sa.intPrimitive = sb.intPrimitive and sb.intBoxed = sa.intPrimitive and sa.theString=sX.theString");
                TryValid(
                    env,
                    streamDef +
                    "sa.intPrimitive = sb.intPrimitive and sb.intBoxed = sa.intPrimitive and sa.theString=sb.theString");

                TryInvalid(env, streamDef + "sa.intPrimitive = sb.intPrimitive or sa.theString=sX.theString");
                TryValid(env, streamDef + "sa.intPrimitive = sb.intPrimitive or sb.intBoxed = sa.intPrimitive");

                // try constants
                TryValid(env, streamDef + "sa.intPrimitive=5");
                TryValid(env, streamDef + "sa.theString='4'");
                TryValid(env, streamDef + "sa.theString=\"4\"");
                TryValid(env, streamDef + "sa.boolPrimitive=false");
                TryValid(env, streamDef + "sa.longPrimitive=-5L");
                TryValid(env, streamDef + "sa.doubleBoxed=5.6d");
                TryValid(env, streamDef + "sa.floatPrimitive=-5.6f");

                TryInvalid(env, streamDef + "sa.intPrimitive='5'");
                TryInvalid(env, streamDef + "sa.theString=5");
                TryInvalid(env, streamDef + "sa.boolBoxed=f");
                TryInvalid(env, streamDef + "sa.intPrimitive=x");
                TryValid(env, streamDef + "sa.intPrimitive=5.5");

                // try addition and subtraction
                TryValid(env, streamDef + "sa.intPrimitive=sa.intBoxed + 5");
                TryValid(env, streamDef + "sa.intPrimitive=2*sa.intBoxed - sa.intPrimitive/10 + 1");
                TryValid(env, streamDef + "sa.intPrimitive=2*(sa.intBoxed - sa.intPrimitive)/(10 + 1)");
                TryInvalid(env, streamDef + "sa.intPrimitive=2*(sa.intBoxed");

                // try comparison
                TryValid(env, streamDef + "sa.intPrimitive > sa.intBoxed and sb.doublePrimitive < sb.doubleBoxed");
                TryValid(env, streamDef + "sa.intPrimitive >= sa.intBoxed and sa.doublePrimitive <= sa.doubleBoxed");
                TryValid(env, streamDef + "sa.intPrimitive > (sa.intBoxed + sb.doublePrimitive)");
                TryInvalid(env, streamDef + "sa.intPrimitive >= sa.theString");
                TryInvalid(env, streamDef + "sa.boolBoxed >= sa.boolPrimitive");

                // Try some nested
                TryValid(env, streamDef + "(sa.intPrimitive=3) or (sa.intBoxed=3 and sa.intPrimitive=1)");
                TryValid(env, streamDef + "((sa.intPrimitive>3) or (sa.intBoxed<3)) and sa.boolBoxed=false");
                TryValid(
                    env,
                    streamDef +
                    "(sa.intPrimitive<=3 and sa.intPrimitive>=1) or (sa.boolBoxed=false and sa.boolPrimitive=true)");
                TryInvalid(env, streamDef + "sa.intPrimitive=3 or (sa.intBoxed=2");
                TryInvalid(env, streamDef + "sa.intPrimitive=3 or sa.intBoxed=2)");
                TryInvalid(env, streamDef + "sa.intPrimitive=3 or ((sa.intBoxed=2)");

                // Try some without stream name
                TryInvalid(env, streamDef + "intPrimitive=3");
                TryValid(env, streamDefTwo + "intPrimitive=3");

                // Try invalid outer join criteria
                var outerJoinDef = "select * from " +
                                   "SupportBean#length(3) as sa " +
                                   "left outer join " +
                                   "SupportBean#length(3) as sb ";
                TryValid(env, outerJoinDef + "on sa.intPrimitive = sb.intBoxed");
                TryInvalid(env, outerJoinDef + "on sa.intPrimitive = sb.XX");
                TryInvalid(env, outerJoinDef + "on sa.XX = sb.XX");
                TryInvalid(env, outerJoinDef + "on sa.XX = sb.intBoxed");
                TryInvalid(env, outerJoinDef + "on sa.boolBoxed = sb.intBoxed");
                TryValid(env, outerJoinDef + "on sa.boolPrimitive = sb.boolBoxed");
                TryInvalid(env, outerJoinDef + "on sa.boolPrimitive = sb.theString");
                TryInvalid(env, outerJoinDef + "on sa.intPrimitive <= sb.intBoxed");
                TryInvalid(env, outerJoinDef + "on sa.intPrimitive = sa.intBoxed");
                TryInvalid(env, outerJoinDef + "on sb.intPrimitive = sb.intBoxed");
                TryValid(env, outerJoinDef + "on sb.intPrimitive = sa.intBoxed");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private static void TryInvalid(
            RegressionEnvironment env,
            string eplInvalidEPL)
        {
            try {
                env.CompileWCheckedEx(eplInvalidEPL);
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                // Expected exception
            }
        }

        private static void TryValid(
            RegressionEnvironment env,
            string invalidEPL)
        {
            env.CompileDeploy(invalidEPL);
        }

        private static string GetSyntaxExceptionEPL(
            RegressionEnvironment env,
            string expression)
        {
            string exceptionText = null;
            try {
                env.CompileWCheckedEx(expression);
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                exceptionText = ex.Message;
                log.Debug(".getSyntaxExceptionEPL epl=" + expression, ex);
                // Expected exception
            }

            return exceptionText;
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(EPLOtherInvalid));
    }
} // end of namespace