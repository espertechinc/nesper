///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewGroupByTypes 
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
    
        [Test]
        public void TestType()
        {
            String viewStmt = "select * from " + typeof(SupportBean).FullName +
                    ".std:groupwin(IntPrimitive).win:length(4).std:groupwin(LongBoxed).stat:uni(DoubleBoxed)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(viewStmt);

            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("IntPrimitive"));
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("LongBoxed"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("stddev"));
            Assert.AreEqual(8, stmt.EventType.PropertyNames.Length);
        }
    }
}
