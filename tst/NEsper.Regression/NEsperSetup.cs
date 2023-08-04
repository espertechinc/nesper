///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat.logging;

using NEsper.Avro.Extensions;

using NLog;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib
{
    [SetUpFixture]
    public class NEsperSetup
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
#if NETCOREAPP3_0_OR_GREATER
#else
            //var clearScript = typeof(ScriptingEngineJScript);
#endif

            // Ensure that AVRO support is loaded before we change directories
            SchemaBuilder.Record("dummy");

            var dir = TestContext.CurrentContext.TestDirectory;
            if (dir != null) {
                Environment.CurrentDirectory = dir;
                Directory.SetCurrentDirectory(dir);
            }

            var logConfig = LoggerNLog.BasicConfig();

            logConfig.AddRule(
                LogLevel.Info,
                LogLevel.Fatal,
                LoggerNLog.Console,
                "com.espertech.esper.regressionrun.runner.RegressionRunner");

            logConfig.AddRule(
                LogLevel.Warn,
                LogLevel.Fatal,
                LoggerNLog.Console,
                "com.espertech.esper.regressionlib.support.multithread");
            
            LoggerNLog.ResetConfig(logConfig);
            LoggerNLog.Register();
        }

        public void UnusedMethodToBindAvro()
        {
            SchemaBuilder.Record("dummy");
        }
    }
}