///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Configuration;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.util;

using Configuration = com.espertech.esper.client.Configuration;

using NEsper.Benchmark.Common;

namespace NEsper.Benchmark.Server
{
    /// <summary>
    /// A factory and interface to wrap ESP/CEP engine dependency in a single space
    /// </summary>
    /// <author>Alexandre Vasseur http://avasseur.blogspot.com</author>
    public class CEPProvider
    {
        public interface ICEPProvider
        {
            void Init(int sleepListenerMillis);
            void RegisterStatement(String statement, String statementID);
            void SendEvent(Object eventObject);
        }

        public static ICEPProvider GetCEPProvider()
        {
            String className = ConfigurationManager.AppSettings.Get("esper.benchmark.provider");
            if (className == null)
            {
                className = typeof(EsperCEPProvider).FullName;
            }

            Type klass = TypeHelper.ResolveType(className);
            return (ICEPProvider)Activator.CreateInstance(klass);
        }

        public class EsperCEPProvider : ICEPProvider
        {
            private EPAdministrator epAdministrator;
            private EPRuntime epRuntime;
            private int sleepMillis;

            public void Init(int sleepListenerMillis)
            {
                var container = ContainerExtensions.CreateDefaultContainer();
                Configuration configuration = new Configuration(container);
                configuration.EngineDefaults.EventMeta.ClassPropertyResolutionStyle =
                    PropertyResolutionStyle.CASE_INSENSITIVE;
                configuration.AddEventType("Market", typeof(MarketData));
                EPServiceProvider epService = EPServiceProviderManager.GetProvider("benchmark", configuration);
                epAdministrator = epService.EPAdministrator;
                epRuntime = epService.EPRuntime;
                sleepMillis = sleepListenerMillis;
            }

            private void HandleEvent(Object sender, UpdateEventArgs e)
            {
                if (e.NewEvents != null)
                {
                    if (sleepMillis > 0)
                    {
                        Thread.Sleep(sleepMillis);
                    }
                }
            }

            public void RegisterStatement(String statement, String statementID)
            {
                EPStatement stmt = epAdministrator.CreateEPL(statement, statementID);
                stmt.Events += HandleEvent;
            }

            public void SendEvent(Object eventObject)
            {
                epRuntime.SendEvent(eventObject);
            }
        }
    }
} // End of namespace
