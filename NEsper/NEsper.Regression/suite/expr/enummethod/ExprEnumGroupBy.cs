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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumGroupBy
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumKeySelectorOnly());
            execs.Add(new ExprEnumKeyValueSelector());
            return execs;
        }

        public static string ExtractAfterUnderscore(string @string)
        {
            var indexUnderscore = @string.IndexOf("_");
            if (indexUnderscore == -1) {
                Assert.Fail();
            }

            return @string.Substring(indexUnderscore + 1);
        }

        private static EPAssertionUtil.AssertionCollectionValueString GetExtractorScalar()
        {
            return collectionItem => collectionItem.ToString();
        }

        internal class ExprEnumKeySelectorOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // - duplicate key allowed, creates a list of values
                // - null key & value allowed

                var eplFragment = "@Name('s0') select Contained.groupBy(c -> Id) as val from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    new [] { "val" },
                    new[] {typeof(IDictionary<object, object>)});
                EPAssertionUtil.AssertionCollectionValueString extractorEvents = collectionItem => {
                    var p00 = ((SupportBean_ST0) collectionItem).P00;
                    return Convert.ToString(p00);
                };

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E1,2", "E2,5"));
                EPAssertionUtil.AssertMapOfCollection(
                    (IDictionary<object, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val"),
                    new [] { "E1","E2" },
                    new[] {"1,2", "5"},
                    extractorEvents);

                env.SendEventBean(SupportBean_ST0_Container.Make2Value(null));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                env.SendEventBean(SupportBean_ST0_Container.Make2Value());
                Assert.AreEqual(
                    0,
                    ((IDictionary<object, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val")).Count);
                env.UndeployAll();

                // test scalar
                var eplScalar =
                    "@Name('s0') select Strvals.groupBy(c -> extractAfterUnderscore(c)) as val from SupportCollection";
                env.CompileDeploy(eplScalar).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    new [] { "val" },
                    new[] {typeof(IDictionary<object, object>)});

                env.SendEventBean(SupportCollection.MakeString("E1_2,E2_1,E3_2"));
                EPAssertionUtil.AssertMapOfCollection(
                    (IDictionary<object, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val"),
                    new [] { "2","1" },
                    new[] {"E1_2,E3_2", "E2_1"},
                    GetExtractorScalar());

                env.SendEventBean(SupportCollection.MakeString(null));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                env.SendEventBean(SupportCollection.MakeString(""));
                Assert.AreEqual(
                    0,
                    ((IDictionary<object, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val")).Count);

                env.UndeployAll();
            }
        }

        internal class ExprEnumKeyValueSelector : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplFragment =
                    "@Name('s0') select Contained.groupBy(k => Id, v -> P00) as val from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                EPAssertionUtil.AssertionCollectionValueString extractor = collectionItem => {
                    var p00 = collectionItem.AsInt32();
                    return Convert.ToString(p00);
                };

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E1,2", "E2,5"));
                EPAssertionUtil.AssertMapOfCollection(
                    (IDictionary<object, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val"),
                    new [] { "E1","E2" },
                    new[] {"1,2", "5"},
                    extractor);

                env.SendEventBean(SupportBean_ST0_Container.Make2Value(null));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                env.SendEventBean(SupportBean_ST0_Container.Make2Value());
                Assert.AreEqual(
                    0,
                    ((IDictionary<object, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val")).Count);

                env.UndeployModuleContaining("s0");

                // test scalar
                var eplScalar =
                    "@Name('s0') select Strvals.groupBy(k => extractAfterUnderscore(k), v -> v) as val from SupportCollection";
                env.CompileDeploy(eplScalar).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    new [] { "val" },
                    new[] {typeof(IDictionary<object, object>)});

                env.SendEventBean(SupportCollection.MakeString("E1_2,E2_1,E3_2"));
                EPAssertionUtil.AssertMapOfCollection(
                    (IDictionary<object, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val"),
                    new [] { "2","1" },
                    new[] {"E1_2,E3_2", "E2_1"},
                    GetExtractorScalar());

                env.SendEventBean(SupportCollection.MakeString(null));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                env.SendEventBean(SupportCollection.MakeString(""));
                Assert.AreEqual(
                    0,
                    ((IDictionary<object, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val")).Count);

                env.UndeployAll();
            }
        }
    }
} // end of namespace