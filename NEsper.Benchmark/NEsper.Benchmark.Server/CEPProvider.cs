///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compiler.client;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;

using Configuration = com.espertech.esper.common.client.configuration.Configuration;

using NEsper.Benchmark.Common;

namespace NEsper.Benchmark.Server
{
    /// <summary>
    /// A factory and interface to wrap ESP/CEP engine dependency in a single space
    /// </summary>
    public class CEPProvider
    {
        public interface ICEPProvider
        {
            void Init(int sleepListenerMillis);
            void RegisterStatement(string statement, string statementID);
            void SendMarketDataEvent(MarketData marketData);
        }

        public static ICEPProvider GetCEPProvider()
        {
            return new EsperCEPProvider();
        }

        public class EsperCEPProvider : ICEPProvider
        {
            private EPRuntime _runtime;
            private EventSender _marketDataSender;
            private int sleepMillis;

            public void Init(int sleepListenerMillis)
            {
                var container = ContainerExtensions.CreateDefaultContainer();
                var configuration = new Configuration(container);
                configuration.Common.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
                configuration.Common.AddEventType("Market", typeof(MarketData));
                _runtime = EPRuntimeProvider.GetRuntime("benchmark", configuration);
                _marketDataSender = _runtime.EventService.GetEventSender("Market");
                sleepMillis = sleepListenerMillis;
            }
            
            public EPDeployment CompileDeploy(string epl)
            {
                try {
                    var args = new CompilerArguments(_runtime.ConfigurationDeepCopy);
                    args.Path.Add(_runtime.RuntimePath);
			    
                    var compiled = EPCompilerProvider.Compiler.Compile(epl, args);
                    return _runtime.DeploymentService.Deploy(compiled);
                }
                catch (Exception ex) {
                    throw new EPRuntimeException(ex);
                }
            }

            private void HandleEvent(object sender, UpdateEventArgs e)
            {
                if (e.NewEvents != null)
                {
                    if (sleepMillis > 0)
                    {
                        Thread.Sleep(sleepMillis);
                    }
                }
            }

            public void RegisterStatement(string statement, string statementID)
            {
                var stmt = CompileDeploy(statement)
                    .Statements
                    .First(s => s.Name == statementID);
                stmt.Events += HandleEvent;
            }

            public void SendMarketDataEvent(MarketData marketData)
            {
                _marketDataSender.SendEvent(marketData);
            }
        }
    }
} // End of namespace
