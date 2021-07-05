///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.regressionlib.support.util;

using NEsper.Scripting.ClearScript;

namespace com.espertech.esper.regressionrun.runner
{
    public class SupportConfigFactory
    {
        private const string TEST_CONFIG_FACTORY_CLASS = "CONFIGFACTORY_CLASS";
        private const string SYSTEM_PROPERTY_LOG_CODE = "esper_logcode";
        private const string SYSTEM_PROPERTY_LOG_PATH = "esper_logcode";

        public static Configuration GetConfiguration()
        {
            var container = SupportContainer.Reset();

            Configuration config;
            string configFactoryClass = Environment.GetEnvironmentVariable(TEST_CONFIG_FACTORY_CLASS);
            if (configFactoryClass != null)
            {
                try
                {
                    var clazz = TypeHelper.ResolveType(configFactoryClass);
                    var instance = TypeHelper.Instantiate(clazz);
                    var m = clazz.GetMethod("GetConfigurationEsperRegression");
                    var result = m.Invoke(instance, new object[] { });
                    config = (Configuration) result;
                    config.Container = container;
                }
                catch (Exception e)
                {
                    throw new EPRuntimeException("Error using configuration factory class '" + configFactoryClass + "'", e);
                }
            }
            else
            {
                config = new Configuration(container);

                config.Compiler.Logging.AuditDirectory = @"E:\Logs\NEsper\NEsper.Regression.Review\generated";
                
#if NETFRAMEWORK
                config.Common.Scripting.AddEngine(typeof(ScriptingEngineJScript));
#endif
                config.Common.Scripting.AddEngine(typeof(ScriptingEngineJavascriptV8));
                
                // Runtime
                config.Runtime.Threading.IsInternalTimerEnabled = false;
                config.Runtime.ExceptionHandling.AddClass(typeof(SupportExceptionHandlerFactoryRethrow));
                config.Runtime.ExceptionHandling.UndeployRethrowPolicy = UndeployRethrowPolicy.RETHROW_FIRST;
                
                // Compiler
                config.Compiler.ByteCode.AttachEPL = true;

                if (!string.IsNullOrWhiteSpace(config.Compiler.Logging.AuditDirectory)) {
                    if (Directory.Exists(config.Compiler.Logging.AuditDirectory)) {
                        foreach (var subDirectory in Directory.GetDirectories(config.Compiler.Logging.AuditDirectory)) {
                            var subDirectoryName = Path.GetFileName(subDirectory);
                            if (subDirectoryName.StartsWith("generation")) {
                                Directory.Delete(subDirectory, true);
                            }
                        }
                    }
                    else {
                        Directory.CreateDirectory(config.Compiler.Logging.AuditDirectory);
                    }
                }
                
                config.Compiler.Logging.AuditDirectory = Environment.GetEnvironmentVariable(SYSTEM_PROPERTY_LOG_PATH);
                config.Compiler.Logging.EnableCode = true;
            }

            return config;
        }
    }
} // end of namespace