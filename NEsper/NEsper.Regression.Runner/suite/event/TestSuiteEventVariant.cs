///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.@event.variant;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.@event
{
    [TestFixture]
    public class TestSuiteEventVariant
    {
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

        private RegressionSession session;

        private static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {
                typeof(SupportBean),
                typeof(SupportBeanVariantStream),
                typeof(SupportBeanVariantOne),
                typeof(SupportBeanVariantTwo),
                typeof(SupportBean_A),
                typeof(SupportBean_B),
                typeof(SupportBean_S0),
                typeof(SupportMarketDataBean)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            IDictionary<string, object> types = new Dictionary<string, object>();
            types.Put("someprop", typeof(string));
            configuration.Common.AddEventType("MyEvent", types);
            configuration.Common.AddEventType("MySecondEvent", types);

            var myVariantTwoTypedSB = new ConfigurationCommonVariantStream();
            myVariantTwoTypedSB.AddEventTypeName("SupportBean");
            myVariantTwoTypedSB.AddEventTypeName("SupportBeanVariantStream");
            configuration.Common.AddVariantStream("MyVariantTwoTypedSB", myVariantTwoTypedSB);

            var myVariantAnyTyped = new ConfigurationCommonVariantStream();
            myVariantAnyTyped.TypeVariance = TypeVariance.ANY;
            configuration.Common.AddVariantStream("MyVariantAnyTyped", myVariantAnyTyped);
            Assert.IsTrue(configuration.Common.IsVariantStreamExists("MyVariantAnyTyped"));

            var myVariantTwoTyped = new ConfigurationCommonVariantStream();
            myVariantTwoTyped.AddEventTypeName("MyEvent");
            myVariantTwoTyped.AddEventTypeName("MySecondEvent");
            configuration.Common.AddVariantStream("MyVariantTwoTyped", myVariantTwoTyped);

            var myVariantTwoTypedSBVariant = new ConfigurationCommonVariantStream();
            myVariantTwoTypedSBVariant.AddEventTypeName("SupportBeanVariantStream");
            myVariantTwoTypedSBVariant.AddEventTypeName("SupportBean");
            configuration.Common.AddVariantStream("MyVariantTwoTypedSBVariant", myVariantTwoTypedSBVariant);

            var myVariantStreamTwo = new ConfigurationCommonVariantStream();
            myVariantStreamTwo.AddEventTypeName("SupportBeanVariantOne");
            myVariantStreamTwo.AddEventTypeName("SupportBeanVariantTwo");
            configuration.Common.AddVariantStream("MyVariantStreamTwo", myVariantStreamTwo);

            var myVariantStreamFour = new ConfigurationCommonVariantStream();
            myVariantStreamFour.AddEventTypeName("SupportBeanVariantStream");
            myVariantStreamFour.AddEventTypeName("SupportBean");
            configuration.Common.AddVariantStream("MyVariantStreamFour", myVariantStreamFour);

            var myVariantStreamFive = new ConfigurationCommonVariantStream();
            myVariantStreamFive.AddEventTypeName("SupportBean");
            myVariantStreamFive.AddEventTypeName("SupportBeanVariantStream");
            configuration.Common.AddVariantStream("MyVariantStreamFive", myVariantStreamFive);

            var varStreamABPredefined = new ConfigurationCommonVariantStream();
            varStreamABPredefined.AddEventTypeName("SupportBean_A");
            varStreamABPredefined.AddEventTypeName("SupportBean_B");
            configuration.Common.AddVariantStream("VarStreamABPredefined", varStreamABPredefined);

            var varStreamAny = new ConfigurationCommonVariantStream();
            varStreamAny.TypeVariance = TypeVariance.ANY;
            configuration.Common.AddVariantStream("VarStreamAny", varStreamAny);

            // test insert into staggered with map
            var configVariantStream = new ConfigurationCommonVariantStream();
            configVariantStream.TypeVariance = TypeVariance.ANY;
            configuration.Common.AddEventType("SupportBean", typeof(SupportBean));
            configuration.Common.AddEventType("SupportMarketDataBean", typeof(SupportMarketDataBean));
            configuration.Common.AddVariantStream("VarStreamMD", configVariantStream);

            configuration.Common.AddImportType(typeof(EventVariantStream));
        }

        [Test]
        public void TestEventVariantStream()
        {
            RegressionRunner.Run(session, EventVariantStream.Executions());
        }
    }
} // end of namespace