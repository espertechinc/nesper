///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.events.vaevent
{
    [TestFixture]
    public class TestPropertyUtility
    {
        private static readonly IDictionary<String, int[]> ExpectedPropertyGroups = new Dictionary<String, int[]>();

        private EventAdapterService _eventSource; 
        private IContainer _container;
        
        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _eventSource = _container.Resolve<EventAdapterService>();

            _types = new EventType[5];
            _types[0] = _eventSource.AddBeanType("D1", typeof(SupportDeltaOne), false, false, false);
            _types[1] = _eventSource.AddBeanType("D2", typeof(SupportDeltaTwo), false, false, false);
            _types[2] = _eventSource.AddBeanType("D3", typeof(SupportDeltaThree), false, false, false);
            _types[3] = _eventSource.AddBeanType("D4", typeof(SupportDeltaFour), false, false, false);
            _types[4] = _eventSource.AddBeanType("D5", typeof(SupportDeltaFive), false, false, false);
        }

        static TestPropertyUtility()
        {
            ExpectedPropertyGroups["P0"] = new[] {1, 2, 3};
            ExpectedPropertyGroups["P1"] = new[] {0};
            ExpectedPropertyGroups["P2"] = new[] {1, 3};
            ExpectedPropertyGroups["P3"] = new[] {1};
            ExpectedPropertyGroups["P4"] = new[] {2};
            ExpectedPropertyGroups["P5"] = new[] {0, 3};
        }

        private EventType[] _types;
        private readonly String[] _fields = "P0,P1,P2,P3,P4,P5".Split(',');

        [Test]
        public void TestAnalyze()
        {
            PropertyGroupDesc[] groups = PropertyUtility.AnalyzeGroups(_fields, _types,
                                                                       new[] {"D1", "D2", "D3", "D4", "D5"});
            Assert.AreEqual(4, groups.Length);

            Assert.AreEqual(0, groups[0].GroupNum);
            EPAssertionUtil.AssertEqualsExactOrder(new[] { "P1", "P5" }, groups[0].Properties);
            Assert.AreEqual(2, groups[0].Types.Count);
            Assert.AreEqual("D1", groups[0].Types.Get(_types[0]));
            Assert.AreEqual("D5", groups[0].Types.Get(_types[4]));

            Assert.AreEqual(1, groups[1].GroupNum);
            EPAssertionUtil.AssertEqualsExactOrder(new[] { "P0", "P2", "P3" }, groups[1].Properties);
            Assert.AreEqual(1, groups[1].Types.Count);
            Assert.AreEqual("D2", groups[1].Types.Get(_types[1]));

            Assert.AreEqual(2, groups[2].GroupNum);
            EPAssertionUtil.AssertEqualsExactOrder(new[] { "P0", "P4" }, groups[2].Properties);
            Assert.AreEqual(1, groups[2].Types.Count);
            Assert.AreEqual("D3", groups[2].Types.Get(_types[2]));

            Assert.AreEqual(3, groups[3].GroupNum);
            EPAssertionUtil.AssertEqualsExactOrder(new[] { "P0", "P2", "P5" }, groups[3].Properties);
            Assert.AreEqual(1, groups[3].Types.Count);
            Assert.AreEqual("D4", groups[3].Types.Get(_types[3]));
        }

        [Test]
        public void TestGetGroups()
        {
            PropertyGroupDesc[] groups = PropertyUtility.AnalyzeGroups(_fields, _types,
                                                                       new[] {"D1", "D2", "D3", "D4", "D5"});
            IDictionary<String, int[]> groupsPerProp = PropertyUtility.GetGroupsPerProperty(groups);

            Assert.AreEqual(groupsPerProp.Count, ExpectedPropertyGroups.Count);
            foreach (var entry in ExpectedPropertyGroups) {
                int[] result = groupsPerProp.Get(entry.Key);
                EPAssertionUtil.AssertEqualsExactOrder(result, entry.Value);
            }
        }
    }
}
