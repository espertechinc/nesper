///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

namespace com.espertech.esper.supportregression.execution
{
    public class RegressionRunner
    {
        public static void Run(RegressionExecution execution)
        {
            Log.Info("Test {0} - starting", execution.GetType().FullName);

            try {
                SupportContainer.Reset();

                var configuration = SupportConfigFactory.GetConfiguration();
                try {
                    execution.Configure(configuration);
                }
                catch (Exception ex) {
                    throw new EPRuntimeException("Configuration-time exception thrown: " + ex.Message, ex);
                }

                var epService = EPServiceProviderManager.GetDefaultProvider(
                    SupportContainer.Instance, configuration);
                epService.Initialize();

                if (!execution.ExcludeWhenInstrumented()) {
                    if (InstrumentationHelper.ENABLED) {
                        InstrumentationHelper.StartTest(epService, execution.GetType(), execution.GetType().Name);
                    }
                }

#if CATCH_RETHROW
            try {
#endif
                execution.Run(epService);
#if CATCH_RETHROW
            }
            catch (Exception ex) {
                throw new EPRuntimeException("Exception thrown: " + ex.Message, ex);
            }
#endif

                if (!execution.ExcludeWhenInstrumented()) {
                    if (InstrumentationHelper.ENABLED) {
                        InstrumentationHelper.EndTest();
                    }
                }
            }
            finally {
                Log.Info("Test {0} - finished", execution.GetType().FullName);
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
