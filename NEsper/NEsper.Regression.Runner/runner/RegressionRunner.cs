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

using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NEsper.Avro.Core;

namespace com.espertech.esper.regressionrun.Runner
{
    public class RegressionRunner
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // References required to ensure type loading
        private static readonly Type eventTypeAvroHandlerImpl = typeof(EventTypeAvroHandlerImpl);

        static RegressionRunner()
        {
            RegressionCore.Initialize();
        }

        public static void RunConfigurable(RegressionExecutionWithConfigure configurable)
        {
            var session = Session();
            configurable.Configure(session.Configuration);
            Run(session, configurable);
            session.Destroy();
        }

        public static RegressionSession Session()
        {
            return new RegressionSession(SupportConfigFactory.GetConfiguration());
        }

        public static void Run<T>(
            RegressionSession session,
            ICollection<T> executions)
            where T : RegressionExecution
        {
            foreach (var execution in RegressionFilter.FilterBySystemProperty(executions))
            {
                Run(session, execution);
            }
        }

        public static void Run(
            RegressionSession session,
            RegressionExecution execution)
        {
            if (session.Runtime == null)
            {
                var exists = EPRuntimeProvider.HasRuntime(EPRuntimeProvider.DEFAULT_RUNTIME_URI);
                var runtime = EPRuntimeProvider.GetDefaultRuntime(session.Configuration);
                if (exists)
                {
                    runtime.Initialize();
                }

                session.Runtime = runtime;
            }

            log.Info("Running test " + execution.Name());
            execution.Run(new RegressionEnvironmentEsper(session.Configuration, session.Runtime));
        }
    }
} // end of namespace