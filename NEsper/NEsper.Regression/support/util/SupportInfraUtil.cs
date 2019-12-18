///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.@internal.kernel.service;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportInfraUtil
    {
        public static int GetIndexCountNoContext(
            RegressionEnvironment env,
            bool namedWindow,
            string infraStatementName,
            string infraName)
        {
            if (namedWindow) {
                var instanceX = GetInstanceNoContextNW(env, infraStatementName, infraName);
                return instanceX.IndexDescriptors.Length;
            }

            var instance = GetInstanceNoContextTable(env, infraStatementName, infraName);
            return instance.IndexRepository.IndexDescriptors.Length;
        }

        public static long GetDataWindowCountNoContext(
            RegressionEnvironment env,
            string statementNameNamedWindow,
            string windowName)
        {
            var instance = GetInstanceNoContextNW(env, statementNameNamedWindow, windowName);
            return instance.CountDataWindow;
        }

        public static NamedWindowInstance GetInstanceNoContextNW(
            RegressionEnvironment env,
            string statementNameNamedWindow,
            string windowName)
        {
            var namedWindow = GetNamedWindow(env, statementNameNamedWindow, windowName);
            return namedWindow.GetNamedWindowInstance(null);
        }

        public static TableInstance GetInstanceNoContextTable(
            RegressionEnvironment env,
            string statementNameTable,
            string tableName)
        {
            var table = GetTable(env, statementNameTable, tableName);
            return table.GetTableInstance(-1);
        }

        public static NamedWindow GetNamedWindow(
            RegressionEnvironment env,
            string statementNameNamedWindow,
            string windowName)
        {
            var spi = (EPRuntimeSPI) env.Runtime;
            var namedWindowManagementService = spi.ServicesContext.NamedWindowManagementService;
            var deploymentId = env.DeploymentId(statementNameNamedWindow);
            var namedWindow = namedWindowManagementService.GetNamedWindow(deploymentId, windowName);
            if (namedWindow == null) {
                Assert.Fail(
                    "Failed to find statement-name '" +
                    statementNameNamedWindow +
                    "' named window '" +
                    windowName +
                    "'");
            }

            return namedWindow;
        }

        public static Table GetTable(
            RegressionEnvironment env,
            string statementNameTable,
            string tableName)
        {
            var spi = (EPRuntimeSPI) env.Runtime;
            var tables = spi.ServicesContext.TableManagementService;
            var deploymentId = env.DeploymentId(statementNameTable);
            var table = tables.GetTable(deploymentId, tableName);
            if (table == null) {
                Assert.Fail("Failed to find statement-name '" + statementNameTable + "' table '" + tableName + "'");
            }

            return table;
        }
    }
} // end of namespace