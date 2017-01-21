///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.util;

namespace com.espertech.esper.support.client
{
    public class SupportConfigFactory
    {
        private const string TEST_CONFIG_FACTORY_CLASS = "CONFIGFACTORY_CLASS";

        public static Configuration GetConfiguration()
        {
            Configuration config;
            String configFactoryClass = Environment.GetEnvironmentVariable(TEST_CONFIG_FACTORY_CLASS);
            if (configFactoryClass != null)
            {
                try
                {
                    var clazz = TypeHelper.ResolveType(configFactoryClass);
                    var instance = Activator.CreateInstance(clazz);
                    var m = clazz.GetMethod("GetConfigurationEsperRegression");
                    var result = m.Invoke(instance, new object[] {});
                    config = (Configuration) result;
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Error using configuration factory class '" + configFactoryClass + "'", e);
                }
            }
            else
            {
                config = new Configuration();
                config.EngineDefaults.EventMetaConfig.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
                config.EngineDefaults.EventMetaConfig.DefaultAccessorStyle = AccessorStyleEnum.NATIVE;
                config.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled = false;
                config.EngineDefaults.ExceptionHandlingConfig.AddClass<SupportExceptionHandlerFactoryRethrow>();
                config.EngineDefaults.ExceptionHandlingConfig.UndeployRethrowPolicy = ConfigurationEngineDefaults.UndeployRethrowPolicy.RETHROW_FIRST;
            }
            return config;
        }
    }
}
