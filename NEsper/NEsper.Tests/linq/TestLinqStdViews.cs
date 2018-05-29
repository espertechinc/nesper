///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.linq
{
    [TestFixture]
    public class TestLinqStdViews
    {
        private EPServiceProvider _serviceProvider;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            Configuration configuration = new Configuration(_container);
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportPriceEvent>();
            configuration.AddEventType<SupportTradeEvent>();

            _serviceProvider = EPServiceProviderManager.GetDefaultProvider(_container, configuration);
            _serviceProvider.Initialize();
        }

        [Test]
        public void TestUnique()
        {
            AssertModelEquality(
                _serviceProvider.From<SupportBean>().Unique(x => x.TheString),
                "select * from " + Name.Clean<SupportBean>() + "#unique(x.TheString)");
        }

        private void AssertModelEquality(EsperQuery<SupportBean> stream, string sample)
        {
            sample = "@IterableUnbound " + sample;

            var sampleModel = _serviceProvider.EPAdministrator.CompileEPL(sample);
            var sampleModelEPL = sampleModel.ToEPL();

            var streamEPL = stream.ObjectModel.ToEPL();

            Assert.That(streamEPL, Is.EqualTo(sampleModelEPL));
        }
    }
}
