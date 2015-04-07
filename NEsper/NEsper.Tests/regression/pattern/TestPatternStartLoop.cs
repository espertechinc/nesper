///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using com.espertech.esper.compat.logging;

using NUnit.Framework;


namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestPatternStartLoop 
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        /// <summary>
        /// Starting this statement fires an event and the listener starts a new statement (same expression) again, 
        /// causing a loop. This listener limits to 10 - this is a smoke test.
        /// </summary>
        [Test]
        public void TestStartFireLoop()
        {
            String patternExpr = "not " + typeof(SupportBean).FullName;
            EPStatement patternStmt = _epService.EPAdministrator.CreatePattern(patternExpr);
            patternStmt.Events += (new PatternUpdateListener(_epService)).Update;
            patternStmt.Stop();
            patternStmt.Start();
        }

        public class PatternUpdateListener
        {
            private readonly EPServiceProvider _epService;

            public PatternUpdateListener(EPServiceProvider epService)
            {
                Count = 0;
                _epService = epService;
            }

            public void Update(Object sender, UpdateEventArgs e)
            {
                Log.Warn(".Update");
    
                if (Count < 10)
                {
                    Count++;
                    String patternExpr = "not " + typeof(SupportBean).FullName;
                    EPStatement patternStmt = _epService.EPAdministrator.CreatePattern(patternExpr);
                    patternStmt.Events += Update;
                    patternStmt.Stop();
                    patternStmt.Start();
                }
            }

            public int Count { get; private set; }
        };
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
