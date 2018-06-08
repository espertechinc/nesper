///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Common.Logging.Configuration;
using Common.Logging.Log4Net;

using NEsper.Avro.Extensions;

#if NETSTANDARD2_0
#else
using NEsper.Scripting.ClearScript;
#endif

using NUnit.Framework;

namespace com.espertech.esper
{
    using Directory = System.IO.Directory;

    [SetUpFixture]
    public class NEsperSetup
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
#if NETSTANDARD2_0
#else
            var clearScript = typeof(ScriptingEngineJScript);
#endif

            // Ensure that AVRO support is loaded before we change directories
            SchemaBuilder.Record("dummy");

            var dir = TestContext.CurrentContext.TestDirectory;
            if (dir != null)
            {
                Environment.CurrentDirectory = dir;
                Directory.SetCurrentDirectory(dir);

                var logConfigurationProperties = new NameValueCollection();
                logConfigurationProperties["configType"] = "FILE";
                logConfigurationProperties["configFile"] = "log4net.config";

                var logConfiguration = new LogConfiguration();
                logConfiguration.FactoryAdapter = new FactoryAdapterConfiguration();
                logConfiguration.FactoryAdapter.Type = typeof(Log4NetLoggerFactoryAdapter).AssemblyQualifiedName;
                logConfiguration.FactoryAdapter.Arguments = logConfigurationProperties;

                Common.Logging.LogManager.Configure(logConfiguration);

                var logInstance = Common.Logging.LogManager.GetLogger(GetType());
                var logAdapter = Common.Logging.LogManager.Adapter;
            }
        }

        public void UnusedMethodToBindAvro()
        {
            SchemaBuilder.Record("dummy");
        }
    }
}
