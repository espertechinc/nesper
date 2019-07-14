///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.epl.spatial;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLSpatial
    {
        private RegressionSession session;

        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configure(session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Destroy();
            session = null;
        }

        [Test]
        public void TestEPLSpatialMXCIFQuadTreeEventIndex()
        {
            RegressionRunner.Run(session, EPLSpatialMXCIFQuadTreeEventIndex.Executions());
        }

        [Test]
        public void TestEPLSpatialMXCIFQuadTreeFilterIndex()
        {
            RegressionRunner.Run(session, EPLSpatialMXCIFQuadTreeFilterIndex.Executions());
        }

        [Test]
        public void TestEPLSpatialMXCIFQuadTreeInvalid()
        {
            RegressionRunner.Run(session, EPLSpatialMXCIFQuadTreeInvalid.Executions());
        }

        [Test]
        public void TestEPLSpatialPointRegionQuadTreeEventIndex()
        {
            RegressionRunner.Run(session, EPLSpatialPointRegionQuadTreeEventIndex.Executions());
        }

        [Test]
        public void TestEPLSpatialPointRegionQuadTreeFilterIndex()
        {
            RegressionRunner.Run(session, EPLSpatialPointRegionQuadTreeFilterIndex.Executions());
        }

        [Test]
        public void TestEPLSpatialPointRegionQuadTreeInvalid()
        {
            RegressionRunner.Run(session, EPLSpatialPointRegionQuadTreeInvalid.Executions());
        }

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[]{typeof(SupportBean), typeof(SupportSpatialAABB), typeof(SupportSpatialEventRectangle),
                typeof(SupportSpatialDualAABB), typeof(SupportEventRectangleWithOffset), typeof(SupportSpatialPoint),
                typeof(SupportSpatialDualPoint)})
            {
                configuration.Common.AddEventType(clazz);
            }
            configuration.Common.Logging.IsEnableQueryPlan = true;
        }
    }
} // end of namespace