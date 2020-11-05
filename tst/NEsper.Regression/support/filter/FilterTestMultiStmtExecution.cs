///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.regressionlib.support.filter
{
    public class FilterTestMultiStmtExecution : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            _stats = withStats ? new String[] { theCase.Stats } : null;

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
                var epl = "@Name('" + stmtName + "') select * from SupportBean(" + filter + ")";
                env.CompileDeploy(epl).AddListener(stmtName);
                existingStatements[i] = true;
                startedStatements[i] = true;
                initialListeners[i] = env.Listener(stmtName);

                try {
                    AssertSendEvents(existingStatements, startedStatements, initialListeners, env, _theCase.Items);
                }
                catch (AssertionException ex) {
                    var message = "Failed after create stmt " + i + " and before milestone P" + milestone.Get();
                    log.Error(message, ex);
                    Assert.Fail(message, ex);
                }

                env.Milestone(milestone.GetAndIncrement());

                try {
                    AssertSendEvents(existingStatements, startedStatements, initialListeners, env, _theCase.Items);
                }
                catch (AssertionException ex) {
                    Assert.Fail("Failed after create stmt " + i + " and after milestone P" + milestone.Get(), ex);
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
                        "Number of boolean expected-values not matching number of statements for item " + eventNum);
                }

                for (var i = 0; i < startedStatements.Length; i++) {
                    var stmtName = "s" + i;
                    if (!existingStatements[i]) {
                        Assert.IsNull(env.Statement(stmtName), message);
                    }
                    else if (!startedStatements[i]) {
                        Assert.IsNull(env.Statement(stmtName));
                        Assert.IsFalse(initialListeners[i].GetAndClearIsInvoked());
                    }
                    else if (!item.ExpectedPerStmt[i]) {
                        var listener = env.Listener(stmtName);
                        var isInvoked = listener.GetAndClearIsInvoked();
                        Assert.IsFalse(isInvoked, message);
                    }
                    else {
                        var listener = env.Listener(stmtName);
                        Assert.IsTrue(listener.IsInvoked, message);
                        Assert.AreSame(item.Bean, listener.AssertOneGetNewAndReset().Underlying, message);
                    }
                }
            }
        }
    }
} // end of namespace