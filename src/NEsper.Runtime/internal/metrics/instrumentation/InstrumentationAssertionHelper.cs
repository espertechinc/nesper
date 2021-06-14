///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.metrics.instrumentation
{
    public class InstrumentationAssertionHelper
    {
        private const string PROVIDER_PROPERTY = "instrumentation_provider";

        public const bool ASSERTIONENABLED = false;

        public static InstrumentationAssertionService assertionService;

        public static void StartTest(
            EPRuntime runtime,
            Type testClass,
            string testName)
        {
            if (!ASSERTIONENABLED)
            {
                return;
            }

            if (assertionService == null)
            {
                ResolveAssertionService(runtime);
            }

            assertionService.StartTest(runtime, testClass, testName);
        }

        public static void EndTest()
        {
            if (!ASSERTIONENABLED)
            {
                return;
            }

            assertionService.EndTest();
        }

        private static void ResolveAssertionService(EPRuntime runtime)
        {
            var provider = Environment.GetEnvironmentVariable(PROVIDER_PROPERTY);
            if (provider == null)
            {
                throw new EPRuntimeException("Failed to find '" + PROVIDER_PROPERTY + "' system property");
            }

            if (string.Equals(provider.Trim(), "default", StringComparison.InvariantCultureIgnoreCase))
            {
                assertionService = new DefaultInstrumentationAssertionService();
            }
            else
            {
                var spi = (EPRuntimeSPI) runtime;
                assertionService = TypeHelper.Instantiate<InstrumentationAssertionService>(
                    provider, spi.ServicesContext.ImportServiceRuntime.ClassForNameProvider);
            }
        }

        private class DefaultInstrumentationAssertionService : InstrumentationAssertionService
        {
            public void StartTest(
                EPRuntime runtime,
                Type testClass,
                string testName)
            {
            }

            public void EndTest()
            {
            }
        }
    }
} // end of namespace