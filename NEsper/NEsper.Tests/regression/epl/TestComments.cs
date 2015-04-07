///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestComments 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _updateListener;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _updateListener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _updateListener = null;
        }
    
        [Test]
        public void TestComment()
        {
            String lineSeparator = Environment.NewLine;
            String statement = "select TheString, /* this is my string */\n" +
                    "IntPrimitive, // same line comment\n" +
                    "/* comment taking one line */\n" +
                    "// another comment taking a line\n" +
                    "IntPrimitive as /* rename */ MyPrimitive\n" +
                    "from " + typeof(SupportBean).FullName + lineSeparator +
                    " where /* inside a where */ IntPrimitive /* */ = /* */ 100";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(statement);
            stmt.Events += _updateListener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("e1", 100));
    
            EventBean theEvent = _updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual("e1", theEvent.Get("TheString"));
            Assert.AreEqual(100, theEvent.Get("IntPrimitive"));
            Assert.AreEqual(100, theEvent.Get("MyPrimitive"));
            _updateListener.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportBean("e1", -1));
            Assert.IsFalse(_updateListener.GetAndClearIsInvoked());
        }
    }
}
