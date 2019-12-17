///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;
using com.espertech.esper.runtime.@internal.kernel.service;
using com.espertech.esper.runtime.@internal.kernel.statement;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportAdminUtil
    {
        public static void AssertStatelessStmt(
            RegressionEnvironment env,
            string stmtname,
            bool flag)
        {
            var stmt = (EPStatementSPI) GetRequireStatement(stmtname, env.Runtime);
            Assert.AreEqual(flag, stmt.StatementContext.IsStatelessSelect);
        }

        public static SupportListener GetRequireStatementListener(
            string statementName,
            EPRuntime epService)
        {
            var statement = GetRequireStatement(statementName, epService);
            if (statement == null) {
                Assert.Fail("Statement by name '" + statementName + "' not found");
            }

            var first = statement.UpdateListeners
                .FirstOrDefault(l => l is SupportListener);
            if (first == null) {
                Assert.Fail(
                    "Statement by name '" +
                    statementName +
                    "' does not have expected listener " +
                    typeof(SupportListener).FullName);
            }

            return (SupportListener) first;
        }

        public static EPStatement GetRequireStatement(
            string statementName,
            EPRuntime epService)
        {
            var found = GetStatement(statementName, epService);
            if (found == null) {
                throw new ArgumentException("Failed to find statements '" + statementName + "'");
            }

            return found;
        }

        public static EPStatement GetStatement(
            string statementName,
            EPRuntime epService)
        {
            var spi = (EPDeploymentServiceSPI) epService.DeploymentService;
            EPStatement found = null;
            foreach (var entry in spi.DeploymentMap) {
                var statements = entry.Value.Statements;
                foreach (var stmt in statements) {
                    if (statementName == stmt.Name) {
                        if (found != null) {
                            throw new ArgumentException(
                                "Found multiple statements of name '" +
                                statementName +
                                "', statement name is unique within a deployment only");
                        }

                        found = stmt;
                    }
                }
            }

            return found;
        }
    }
} // end of namespace