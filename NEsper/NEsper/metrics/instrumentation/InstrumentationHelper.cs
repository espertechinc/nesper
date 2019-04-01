///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.core.service;
using com.espertech.esper.util;

namespace com.espertech.esper.metrics.instrumentation
{
    public static class InstrumentationHelper
    {
#if DEBUG
        public const bool ENABLED = false;
#else
        public static bool ENABLED { get; set; }
#endif

        public const bool ASSERTIONENABLED = false;

        private const string PROVIDER_PROPERTY = "instrumentation_provider";

        public static Instrumentation DefaultInstrumentation = new InstrumentationDefault();
        public static Instrumentation Instrumentation = DefaultInstrumentation;

        public static InstrumentationAssertionService AssertionService;

        public static Instrumentation Get()
        {
            return Instrumentation;
        }

        public static void StartTest(EPServiceProvider engine, Type testClass, string testName)
        {
            if (!ASSERTIONENABLED)
            {
                return;
            }
            if (AssertionService == null)
            {
                ResolveAssertionService(engine);
            }
            AssertionService.StartTest(engine, testClass, testName);
        }

        public static void EndTest()
        {
            if (!ASSERTIONENABLED)
            {
                return;
            }
            AssertionService.EndTest();
        }

        private static void ResolveAssertionService(EPServiceProvider epServiceProvider)
        {
            string provider = Environment.GetEnvironmentVariable(PROVIDER_PROPERTY);
            if (provider == null)
            {
                throw new EPException("Failed to find '" + PROVIDER_PROPERTY + "' system property");
            }
            if (provider.ToLowerInvariant().Trim().Equals("default"))
            {
                AssertionService = new DefaultInstrumentationAssertionService();
            }
            else
            {
                var spi = (EPServiceProviderSPI) epServiceProvider;
                AssertionService = TypeHelper.Instantiate<InstrumentationAssertionService>(
                    provider, spi.EngineImportService.GetClassForNameProvider());
            }
        }

        private class DefaultInstrumentationAssertionService : InstrumentationAssertionService
        {
            public void StartTest(EPServiceProvider engine, Type testClass, string testName)
            {

            }

            public void EndTest()
            {
            }
        }
    }
} // end of namespace
