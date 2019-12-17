///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.filter
{
    public class FilterTestCaseSingleFieldExecution : RegressionExecution
    {
        private readonly string stats;
        private readonly FilterTestCaseSingleField testCase;
        private readonly string testCaseName;

        public FilterTestCaseSingleFieldExecution(
            Type originator,
            FilterTestCaseSingleField testCase,
            string stats)
        {
            this.testCase = testCase;
            testCaseName = originator.Name + " permutation [" + testCase.FilterExpr + "]";
            this.stats = stats;
        }

        public void Run(RegressionEnvironment env)
        {
            // set up statement
            var stmtName = "stmt";
            var expr = "@Name('" + stmtName + "') select * from SupportBean" + testCase.FilterExpr;
            env.CompileDeployAddListenerMileZero(expr, stmtName);
            var initialListener = env.Listener(stmtName);

            for (var i = 0; i < testCase.Values.Length; i++) {
                SendBean(env, testCase.FieldName, testCase.Values[i]);
                Assert.AreEqual(
                    env.Listener(stmtName).IsInvokedAndReset(),
                    testCase.IsInvoked[i],
                    "Listener invocation unexpected for " +
                    testCase.FilterExpr +
                    " field " +
                    testCase.FieldName +
                    "=" +
                    testCase.Values[i]);
            }

            env.Milestone(1);
            env.UndeployModuleContaining(stmtName);
            env.Milestone(2);

            for (var i = 0; i < testCase.Values.Length; i++) {
                SendBean(env, testCase.FieldName, testCase.Values[i]);
                Assert.IsFalse(initialListener.IsInvoked);
            }
        }

        public string Name()
        {
            return testCaseName;
        }

        private void SendBean(
            RegressionEnvironment env,
            string fieldName,
            object value)
        {
            var theEvent = new SupportBean();
            if (fieldName.Equals("TheString")) {
                theEvent.TheString = (string) value;
            }

            if (fieldName.Equals("BoolPrimitive")) {
                theEvent.BoolPrimitive = (bool) value;
            }

            if (fieldName.Equals("IntBoxed")) {
                theEvent.IntBoxed = (int?) value;
            }

            if (fieldName.Equals("LongBoxed")) {
                theEvent.LongBoxed = value.AsBoxedInt64();
            }

            env.SendEventBean(theEvent);
        }
    }
} // end of namespace