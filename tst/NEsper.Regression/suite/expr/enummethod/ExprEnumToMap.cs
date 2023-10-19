///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.support.SupportEventPropUtil; // assertTypesAllSame

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumToMap
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithEvent(execs);
            WithScalar(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumToMapInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithScalar(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumToMapScalar());
            return execs;
        }

        public static IList<RegressionExecution> WithEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumToMapEvent());
            return execs;
        }

        private class ExprEnumToMapEvent : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
                builder.WithExpression(fields[0], "contained.toMap(c => id, d=> p00)");
                builder.WithExpression(
                    fields[1],
                    "contained.toMap((c, index) => id || '_' || Integer.toString(index), (d, index) => p00 + 10*index)");
                builder.WithExpression(
                    fields[2],
                    "contained.toMap((c, index, size) => id || '_' || Integer.toString(index) || '_' || Integer.toString(size), (d, index, size) => p00 + 10*index + 100*size)");

                builder.WithStatementConsumer(
                    stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(IDictionary<string, int>)));

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E3,12", "E2,5"))
                    .Verify("c0", val => CompareMap(val, "E1,E3,E2", 1, 12, 5))
                    .Verify("c1", val => CompareMap(val, "E1_0,E3_1,E2_2", 1, 22, 25))
                    .Verify("c2", val => CompareMap(val, "E1_0_3,E3_1_3,E2_2_3", 301, 322, 325));

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E3,4", "E2,7", "E1,2"))
                    .Verify("c0", val => CompareMap(val, "E1,E3,E2", 2, 4, 7))
                    .Verify("c1", val => CompareMap(val, "E1_0,E3_1,E2_2,E1_3", 1, 14, 27, 32))
                    .Verify("c2", val => CompareMap(val, "E1_0_4,E3_1_4,E2_2_4,E1_3_4", 401, 414, 427, 432));

                builder.WithAssertion(
                        new SupportBean_ST0_Container(Collections.SingletonList(new SupportBean_ST0(null, null))))
                    .Verify("c0", val => CompareMap(val, "E1,E2,E3", null, null, null))
                    .Verify("c1", val => CompareMap(val, "E1,E2,E3", null, null, null))
                    .Verify("c2", val => CompareMap(val, "E1,E2,E3", null, null, null));

                builder.Run(env);
            }
        }

        private class ExprEnumToMapScalar : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "strvals.toMap(k => k, v => extractNum(v))");
                builder.WithExpression(
                    fields[1],
                    "strvals.toMap((k, i) => k || '_' || Integer.toString(i), (v, idx) => extractNum(v) + 10*idx)");
                builder.WithExpression(
                    fields[2],
                    "strvals.toMap((k, i, s) => k || '_' || Integer.toString(i) || '_' || Integer.toString(s), (v, idx, sz) => extractNum(v) + 10*idx + 100*sz)");

                builder.WithStatementConsumer(
                    stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(IDictionary<string, int>)));

                builder.WithAssertion(SupportCollection.MakeString("E2,E1,E3"))
                    .Verify("c0", val => CompareMap(val, "E1,E2,E3", 1, 2, 3))
                    .Verify("c1", val => CompareMap(val, "E1_1,E2_0,E3_2", 11, 2, 23))
                    .Verify("c2", val => CompareMap(val, "E1_1_3,E2_0_3,E3_2_3", 311, 302, 323));

                builder.WithAssertion(SupportCollection.MakeString("E1"))
                    .Verify("c0", val => CompareMap(val, "E1", 1))
                    .Verify("c1", val => CompareMap(val, "E1_0", 1))
                    .Verify("c2", val => CompareMap(val, "E1_0_1", 101));

                builder.WithAssertion(SupportCollection.MakeString(null))
                    .Verify("c0", Assert.IsNull)
                    .Verify("c1", Assert.IsNull)
                    .Verify("c2", Assert.IsNull);

                builder.WithAssertion(SupportCollection.MakeString(""))
                    .Verify("c0", val => CompareMap(val, ""))
                    .Verify("c1", val => CompareMap(val, ""))
                    .Verify("c2", val => CompareMap(val, ""));

                builder.Run(env);
            }
        }

        private class ExprEnumToMapInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "select strvals.toMap(k => k, (v, i) => extractNum(v)) from SupportCollection";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'strvals.toMap(,)': Parameters mismatch for enumeration method 'toMap', the method requires a lambda expression providing key-selector and a lambda expression providing value-selector, but receives a lambda expression and a 2-parameter lambda expression");
            }
        }

        private static void CompareMap(
            object received,
            string keyCSV,
            params object[] values)
        {
            var keys = string.IsNullOrEmpty(keyCSV) ? Array.Empty<string>() : keyCSV.SplitCsv();
            EPAssertionUtil.AssertPropsMap((IDictionary<string, object>)received, keys, values);
        }
    };
} // end of namespace