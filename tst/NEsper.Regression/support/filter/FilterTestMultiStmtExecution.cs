///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.support.filter
{
    public class FilterTestMultiStmtExecution : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly FilterTestMultiStmtCase _theCase;
        private readonly string _testCaseName;
        private string[] _stats;

        public FilterTestMultiStmtExecution(
            Type originator,
            FilterTestMultiStmtCase theCase,
            bool withStats)
        {
            _theCase = theCase;
            _testCaseName = originator.Name + " permutation " + theCase.Filters.RenderAny();
            _stats = withStats ? new string[] { theCase.Stats } : null;
        }

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.OBSERVEROPS);
        }

        public void Run(RegressionEnvironment env)
        {
            var milestone = new AtomicLong();
            var existingStatements = new bool[_theCase.Filters.Length];
            var startedStatements = new bool[_theCase.Filters.Length];
            var initialListeners = new SupportListener[_theCase.Filters.Length];

            // create statements
            for (var i = 0; i < _theCase.Filters.Length; i++) {
                var filter = _theCase.Filters[i];
                var stmtName = "s" + i;
                var epl = "@name('" + stmtName + "') select * from SupportBean(" + filter + ")";
                env.CompileDeploy(epl).AddListener(stmtName);
                existingStatements[i] = true;
                startedStatements[i] = true;
                initialListeners[i] = env.Listener(stmtName);

                try {
                    AssertSendEvents(existingStatements, startedStatements, initialListeners, env, _theCase.Items);
                }
                catch (AssertionException ex) {
                    var message = "Failed after create stmt " + i + " and before milestone P" + milestone.Get();
                    Log.Error(message, ex);
                    Assert.Fail(message);
                }

                env.Milestone(milestone.GetAndIncrement());

                try {
                    AssertSendEvents(existingStatements, startedStatements, initialListeners, env, _theCase.Items);
                }
                catch (AssertionException) {
                    Assert.Fail("Failed after create stmt " + i + " and after milestone P" + milestone.Get());
                }
            }

            // stop statements
            for (var i = 0; i < _theCase.Filters.Length; i++) {
                var stmtName = "s" + i;
                env.UndeployModuleContaining(stmtName);
                startedStatements[i] = false;

                try {
                    AssertSendEvents(existingStatements, startedStatements, initialListeners, env, _theCase.Items);
                }
                catch (AssertionException ex) {
                    throw new AssertionException(
                        "Failed after stop stmt " + i + " and before milestone P" + milestone.Get(),
                        ex);
                }

                env.Milestone(milestone.Get());

                try {
                    AssertSendEvents(existingStatements, startedStatements, initialListeners, env, _theCase.Items);
                }
                catch (AssertionException ex) {
                    throw new EPException(
                        "Failed after stop stmt " + i + " and after milestone P" + milestone.Get(),
                        ex);
                }
                catch (Exception ex) {
                    throw new EPException(
                        "Failed after stop stmt " + i + " and after milestone P" + milestone.Get(),
                        ex);
                }

                milestone.GetAndIncrement();
            }

            // destroy statements
            env.UndeployAll();
        }

        public string Name()
        {
            return _testCaseName;
        }

        public string[] MilestoneStats()
        {
            return _stats;
        }

        private static void AssertSendEvents(
            bool[] existingStatements,
            bool[] startedStatements,
            SupportListener[] initialListeners,
            RegressionEnvironment env,
            IList<FilterTestMultiStmtAssertItem> items)
        {
            var eventNum = -1;
            foreach (var item in items) {
                eventNum++;
                env.SendEventBean(item.Bean);
                var message = "Failed at event " + eventNum;

                if (item.ExpectedPerStmt.Length != startedStatements.Length) {
                    Assert.Fail(
"Number of boolean expected-values not matching number of statements for Item "+ eventNum);
                }

                for (var i = 0; i < startedStatements.Length; i++) {
                    var stmtName = "s" + i;
                    if (!existingStatements[i]) {
                        ClassicAssert.IsNull(env.Statement(stmtName), message);
                    }
                    else if (!startedStatements[i]) {
                        ClassicAssert.IsNull(env.Statement(stmtName));
                        ClassicAssert.IsFalse(initialListeners[i].GetAndClearIsInvoked());
                    }
                    else if (!item.ExpectedPerStmt[i]) {
                        var listener = env.Listener(stmtName);
                        var isInvoked = listener.GetAndClearIsInvoked();
                        ClassicAssert.IsFalse(isInvoked, message);
                    }
                    else {
                        var listener = env.Listener(stmtName);
                        ClassicAssert.IsTrue(listener.IsInvoked, message);
                        ClassicAssert.AreSame(item.Bean, listener.AssertOneGetNewAndReset().Underlying, message);
                    }
                }
            }
        }
    }
} // end of namespace