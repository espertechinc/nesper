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
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherInvalid
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLOtherInvalidFuncParams());
            execs.Add(new EPLOtherInvalidSyntax());
            execs.Add(new EPLOtherLongTypeConstant());
            execs.Add(new EPLOtherDifferentJoins());
            return execs;
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

        internal class EPLOtherInvalidFuncParams : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(
                    env,
                    "select count(theString, theString, theString) from SupportBean",
                    "Failed to validate select-clause expression 'count(theString,theString,theString)': The 'count' function expects at least 1 and up to 2 parameters");

                TryInvalidCompile(
                    env,
                    "select leaving(TheString) from SupportBean",
                    "Failed to validate select-clause expression 'leaving(TheString)': The 'leaving' function expects no parameters");
            }
        }

        internal class EPLOtherInvalidSyntax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var exceptionText = GetSyntaxExceptionEPL(env, "select * from *");
                Assert.AreEqual(
                    "Incorrect syntax near '*' at line 1 column 14, please check the from clause [select * from *]",
                    exceptionText);

                exceptionText = GetSyntaxExceptionEPL(
                    env,
                    "select * from SupportBean a where a.IntPrimitive between r.start and r.end");
                Assert.AreEqual(
                    "Incorrect syntax near 'start' (a reserved keyword) at line 1 column 59, please check the where clause [select * from SupportBean a where a.IntPrimitive between r.start and r.end]",
                    exceptionText);

                TryInvalidCompile(
                    env,
                    "select * from SupportBean(1=2=3)",
                    "Failed to validate filter expression '1=2': Invalid use of equals, expecting left-hand side and right-hand side but received 3 expressions");
            }
        }

        internal class EPLOtherLongTypeConstant : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select 2512570244 as value from SupportBean";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(new SupportBean());
                Assert.AreEqual(2512570244L, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherDifferentJoins : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(env, "select *", "The from-clause is required but has not been specified");

                var streamDef = "select * from " +
                                "SupportBean#length(3) as sa," +
                                "SupportBean#length(3) as sb" +
                                " where ";

                var streamDefTwo = "select * from " +
                                   "SupportBean#length(3)," +
                                   "SupportMarketDataBean#length(3)" +
                                   " where ";

                TryInvalid(env, streamDef + "sa.IntPrimitive = sb.TheString");
                TryValid(env, streamDef + "sa.IntPrimitive = sb.intBoxed");
                TryValid(env, streamDef + "sa.IntPrimitive = sb.IntPrimitive");
                TryValid(env, streamDef + "sa.IntPrimitive = sb.longBoxed");

                TryInvalid(env, streamDef + "sa.IntPrimitive = sb.IntPrimitive and sb.intBoxed = sa.boolPrimitive");
                TryValid(env, streamDef + "sa.IntPrimitive = sb.IntPrimitive and sb.boolBoxed = sa.boolPrimitive");

                TryInvalid(
                    env,
                    streamDef +
                    "sa.IntPrimitive = sb.IntPrimitive and sb.intBoxed = sa.IntPrimitive and sa.TheString=sX.TheString");
                TryValid(
                    env,
                    streamDef +
                    "sa.IntPrimitive = sb.IntPrimitive and sb.intBoxed = sa.IntPrimitive and sa.TheString=sb.TheString");

                TryInvalid(env, streamDef + "sa.IntPrimitive = sb.IntPrimitive or sa.TheString=sX.TheString");
                TryValid(env, streamDef + "sa.IntPrimitive = sb.IntPrimitive or sb.intBoxed = sa.IntPrimitive");

                // try constants
                TryValid(env, streamDef + "sa.IntPrimitive=5");
                TryValid(env, streamDef + "sa.TheString='4'");
                TryValid(env, streamDef + "sa.TheString=\"4\"");
                TryValid(env, streamDef + "sa.boolPrimitive=false");
                TryValid(env, streamDef + "sa.longPrimitive=-5L");
                TryValid(env, streamDef + "sa.doubleBoxed=5.6d");
                TryValid(env, streamDef + "sa.floatPrimitive=-5.6f");

                TryInvalid(env, streamDef + "sa.IntPrimitive='5'");
                TryInvalid(env, streamDef + "sa.TheString=5");
                TryInvalid(env, streamDef + "sa.boolBoxed=f");
                TryInvalid(env, streamDef + "sa.IntPrimitive=x");
                TryValid(env, streamDef + "sa.IntPrimitive=5.5");

                // try addition and subtraction
                TryValid(env, streamDef + "sa.IntPrimitive=sa.intBoxed + 5");
                TryValid(env, streamDef + "sa.IntPrimitive=2*sa.intBoxed - sa.IntPrimitive/10 + 1");
                TryValid(env, streamDef + "sa.IntPrimitive=2*(sa.intBoxed - sa.IntPrimitive)/(10 + 1)");
                TryInvalid(env, streamDef + "sa.IntPrimitive=2*(sa.intBoxed");

                // try comparison
                TryValid(env, streamDef + "sa.IntPrimitive > sa.intBoxed and sb.doublePrimitive < sb.doubleBoxed");
                TryValid(env, streamDef + "sa.IntPrimitive >= sa.intBoxed and sa.doublePrimitive <= sa.doubleBoxed");
                TryValid(env, streamDef + "sa.IntPrimitive > (sa.intBoxed + sb.doublePrimitive)");
                TryInvalid(env, streamDef + "sa.IntPrimitive >= sa.TheString");
                TryInvalid(env, streamDef + "sa.boolBoxed >= sa.boolPrimitive");

                // Try some nested
                TryValid(env, streamDef + "(sa.IntPrimitive=3) or (sa.intBoxed=3 and sa.IntPrimitive=1)");
                TryValid(env, streamDef + "((sa.IntPrimitive>3) or (sa.intBoxed<3)) and sa.boolBoxed=false");
                TryValid(
                    env,
                    streamDef +
                    "(sa.IntPrimitive<=3 and sa.IntPrimitive>=1) or (sa.boolBoxed=false and sa.boolPrimitive=true)");
                TryInvalid(env, streamDef + "sa.IntPrimitive=3 or (sa.intBoxed=2");
                TryInvalid(env, streamDef + "sa.IntPrimitive=3 or sa.intBoxed=2)");
                TryInvalid(env, streamDef + "sa.IntPrimitive=3 or ((sa.intBoxed=2)");

                // Try some without stream name
                TryInvalid(env, streamDef + "IntPrimitive=3");
                TryValid(env, streamDefTwo + "IntPrimitive=3");

                // Try invalid outer join criteria
                var outerJoinDef = "select * from " +
                                   "SupportBean#length(3) as sa " +
                                   "left outer join " +
                                   "SupportBean#length(3) as sb ";
                TryValid(env, outerJoinDef + "on sa.IntPrimitive = sb.intBoxed");
                TryInvalid(env, outerJoinDef + "on sa.IntPrimitive = sb.XX");
                TryInvalid(env, outerJoinDef + "on sa.XX = sb.XX");
                TryInvalid(env, outerJoinDef + "on sa.XX = sb.intBoxed");
                TryInvalid(env, outerJoinDef + "on sa.boolBoxed = sb.intBoxed");
                TryValid(env, outerJoinDef + "on sa.boolPrimitive = sb.boolBoxed");
                TryInvalid(env, outerJoinDef + "on sa.boolPrimitive = sb.TheString");
                TryInvalid(env, outerJoinDef + "on sa.IntPrimitive <= sb.intBoxed");
                TryInvalid(env, outerJoinDef + "on sa.IntPrimitive = sa.intBoxed");
                TryInvalid(env, outerJoinDef + "on sb.IntPrimitive = sb.intBoxed");
                TryValid(env, outerJoinDef + "on sb.IntPrimitive = sa.intBoxed");

                env.UndeployAll();
            }
        }
    }
} // end of namespace