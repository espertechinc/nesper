///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTablePlugInAggregation
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithAggMethodCSVLast3Strings(execs);
            WithAccessRefCountedMap(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithAccessRefCountedMap(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraPlugInAccessRefCountedMap());
            return execs;
        }

        public static IList<RegressionExecution> WithAggMethodCSVLast3Strings(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraPlugInAggMethodCSVLast3Strings());
            return execs;
        }

        private static void SendWordAssert(
            RegressionEnvironment env,
            string word,
            string expected)
        {
            env.SendEventBean(new SupportBean(word, 0));
            env.SendEventBean(new SupportBean_S0(0));
            Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
        }

        private static void SendWordAssert(
            RegressionEnvironment env,
            string word,
            string wordCSV,
            int?[] counts)
        {
            env.SendEventBean(new SupportBean(word, 0));

            var words = wordCSV.SplitCsv();
            for (var i = 0; i < words.Length; i++) {
                env.SendEventBean(new SupportBean_S0(0, words[i]));
                var listener = env.Listener("s0");
                var theEvent = listener.AssertOneGetNewAndReset();
                var count = theEvent.Get("c0").AsBoxedInt32();
                Assert.AreEqual(counts[i], count, "failed for word '" + words[i] + "'");
            }
        }

        // CSV-building over a limited set of values.
        //
        // Use aggregation method single-value when the aggregation has a natural current value
        // that can be obtained without asking it a question.
        internal class InfraPlugInAggMethodCSVLast3Strings : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create table varaggPIN (csv csvWords(string))", path);
                env.CompileDeploy("@Name('s0') select varaggPIN.csv as c0 from SupportBean_S0", path).AddListener("s0");
                env.CompileDeploy("into table varaggPIN select csvWords(TheString) as csv from SupportBean#length(3)", path);

                SendWordAssert(env, "the", "the");
                SendWordAssert(env, "fox", "the,fox");
                SendWordAssert(env, "jumps", "the,fox,jumps");
                SendWordAssert(env, "over", "fox,jumps,over");

                env.UndeployAll();
            }
        }

        internal class InfraPlugInAccessRefCountedMap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create table varaggRCM (wordCount referenceCountedMap(string))", path);
                env.CompileDeploy("into table varaggRCM select referenceCountedMap(TheString) as wordCount from SupportBean#length(3)", path);
                env.CompileDeploy("@Name('s0') select varaggRCM.wordCount.referenceCountLookup(P00) as c0 from SupportBean_S0", path)
                    .AddListener("s0");

                var words = "the,house,is,green";
                SendWordAssert(env, "the", words, new int?[] {1, null, null, null});
                SendWordAssert(env, "house", words, new int?[] {1, 1, null, null});
                SendWordAssert(env, "the", words, new int?[] {2, 1, null, null});
                SendWordAssert(env, "green", words, new int?[] {1, 1, null, 1});
                SendWordAssert(env, "is", words, new int?[] {1, null, 1, 1});

                env.UndeployAll();
            }
        }
    }
} // end of namespace