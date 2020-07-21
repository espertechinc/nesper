///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternStartLoop : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Starting this statement fires an event and the listener starts a new statement (same expression) again,
        ///     causing a loop. This listener limits to 10 - this is a smoke test.
        /// </summary>
        public void Run(RegressionEnvironment env)
        {
            var patternExpr = "@name('s0') select * from pattern [not SupportBean]";
            env.CompileDeploy(patternExpr);
            env.Statement("s0").AddListener(new PatternUpdateListener(env));
            env.UndeployAll();
            env.CompileDeploy(patternExpr);
            env.UndeployAll();
        }

        private class PatternUpdateListener : UpdateListener
        {
            private readonly RegressionEnvironment env;

            public PatternUpdateListener(RegressionEnvironment env)
            {
                this.env = env;
            }

            public int Count { get; private set; }

            public void Update(
                object sender,
                UpdateEventArgs eventArgs)
            {
                log.Warn(".update");

                if (Count < 10) {
                    Count++;
                    var patternExpr = "@name('ST" + Count + "') select * from pattern[not SupportBean]";
                    env.CompileDeploy(patternExpr).AddListener("ST" + Count);
                    env.UndeployModuleContaining("ST" + Count);
                    env.CompileDeploy(patternExpr).AddListener("ST" + Count);
                }
            }
        }
    }
} // end of namespace