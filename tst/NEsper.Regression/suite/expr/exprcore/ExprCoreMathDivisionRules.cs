///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreMathDivisionRules
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithBigInt(execs);
            WithLong(execs);
            WithFloat(execs);
            WithDouble(execs);
            WithInt(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInt(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreMathRulesInt());
            return execs;
        }

        public static IList<RegressionExecution> WithDouble(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreMathRulesDouble());
            return execs;
        }

        public static IList<RegressionExecution> WithFloat(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreMathRulesFloat());
            return execs;
        }

        public static IList<RegressionExecution> WithLong(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreMathRulesLong());
            return execs;
        }

        public static IList<RegressionExecution> WithBigInt(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreMathRulesBigInt());
            return execs;
        }

        public class ExprCoreMathRulesBigInt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select BigInteger.valueOf(4)/BigInteger.valueOf(2) as c0 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStmtType("s0", "c0", typeof(BigInteger?));

                var fields = "c0".SplitCsv();
                env.SendEventBean(new SupportBean());
                env.AssertPropsNew("s0", fields, new object[] { new BigInteger(4) / new BigInteger(2) });

                env.UndeployAll();
            }
        }

        public class ExprCoreMathRulesLong : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select 10L/2L as c0 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStmtType("s0", "c0", typeof(long?));

                var fields = "c0".SplitCsv();
                env.SendEventBean(new SupportBean());
                env.AssertPropsNew("s0", fields, new object[] { 5L });

                env.UndeployAll();
            }
        }

        public class ExprCoreMathRulesFloat : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select 10f/2f as c0 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStmtType("s0", "c0", typeof(float?));

                var fields = "c0".SplitCsv();
                env.SendEventBean(new SupportBean());
                env.AssertPropsNew("s0", fields, new object[] { 5f });

                env.UndeployAll();
            }
        }

        public class ExprCoreMathRulesDouble : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select 10d/0d as c0 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "c0".SplitCsv();
                env.SendEventBean(new SupportBean());
                env.AssertPropsNew("s0", fields, new object[] { null });

                env.UndeployAll();
            }
        }

        public class ExprCoreMathRulesInt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select IntPrimitive/IntBoxed as result from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStmtType("s0", "result", typeof(int?));

                SendEvent(env, 100, 3);
                env.AssertEqualsNew("s0", "result", 33);

                SendEvent(env, 100, null);
                env.AssertEqualsNew("s0", "result", null);

                SendEvent(env, 100, 0);
                env.AssertEqualsNew("s0", "result", null);

                env.UndeployAll();
            }
        }

        private static void SendEvent(
            RegressionEnvironment env,
            int intPrimitive,
            int? intBoxed)
        {
            var bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }
    }
} // end of namespace