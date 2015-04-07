///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.util;

namespace com.espertech.esper.metrics.instrumentation
{
    public class InstrumentationHelper
    {
        private const String PROVIDER_PROPERTY = "instrumentation_provider";

        public const bool ENABLED = false;
        public const bool ASSERTIONENABLED = false;

        public static Instrumentation DEFAULT_INSTRUMENTATION = new InstrumentationDefault();
        public static Instrumentation Instrumentation = DEFAULT_INSTRUMENTATION;

        public static InstrumentationAssertionService AssertionService;

        public static Instrumentation Get()
        {
            return Instrumentation;
        }

        public static void StartTest(EPServiceProvider engine, Type testClass, String testName)
        {
            if (!ASSERTIONENABLED)
            {
                return;
            }
            if (AssertionService == null)
            {
                ResolveAssertionService();
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

        private static void ResolveAssertionService()
        {
            var provider = Environment.GetEnvironmentVariable(PROVIDER_PROPERTY);
            if (provider == null)
            {
                throw new ApplicationException("Failed to find '" + PROVIDER_PROPERTY + "' system property");
            }
            if (provider.ToLower().Trim() == "default")
            {
                AssertionService = new DefaultInstrumentationAssertionService();
            }
            else
            {
                AssertionService = TypeHelper.Instantiate<InstrumentationAssertionService>(provider);
            }
        }

        private class DefaultInstrumentationAssertionService : InstrumentationAssertionService
        {
            public void StartTest(EPServiceProvider engine, Type testClass, String testName)
            {
            }

            public void EndTest()
            {
            }
        }
    }
}