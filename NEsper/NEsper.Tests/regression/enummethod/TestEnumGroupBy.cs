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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.bean.lambda;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.enummethod
{
    using Map = IDictionary<string, object>;
    using GroupMap = IDictionary<object, ICollection<object>>;

    [TestFixture]
    public class TestEnumGroupBy
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("Bean", typeof(SupportBean_ST0_Container));
            config.AddEventType("SupportCollection", typeof(SupportCollection));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestKeySelectorOnly()
        {
            // - duplicate key allowed, creates a list of values
            // - null key & value allowed
            
            String eplFragment = "select contained.GroupBy(c => id) as val from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, "val".Split(','), new Type[]{typeof(GroupMap)});
            EPAssertionUtil.AssertionCollectionValueString extractorEvents = new EPAssertionUtil.ProxyAssertionCollectionValueString(
                collectionItem => 
                {
                    int p00 = ((SupportBean_ST0) collectionItem).P00;
                    return Convert.ToString(p00);
                });
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E1,2", "E2,5"));
            EPAssertionUtil.AssertMapOfCollection(
                (GroupMap) _listener.AssertOneGetNewAndReset().Get("val"),
                new string[] { "E1", "E2" },
                new string[] { "1,2", "5" },
                extractorEvents);
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("val"));
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            Assert.AreEqual(0, ((GroupMap)_listener.AssertOneGetNewAndReset().Get("val")).Count);
            stmtFragment.Dispose();
    
            // test scalar
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractAfterUnderscore", this.GetType().FullName, "ExtractAfterUnderscore");
            String eplScalar = "select Strvals.GroupBy(c => extractAfterUnderscore(c)) as val from SupportCollection";
            EPStatement stmtScalar = _epService.EPAdministrator.CreateEPL(eplScalar);
            stmtScalar.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtScalar.EventType, "val".Split(','), new Type[] { typeof(GroupMap) });
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1_2,E2_1,E3_2"));
            EPAssertionUtil.AssertMapOfCollection((GroupMap)_listener.AssertOneGetNewAndReset().Get("val"), "2,1".Split(','),
                    new String[]{"E1_2,E3_2", "E2_1"}, GetExtractorScalar());
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("val"));
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            Assert.AreEqual(0, ((GroupMap)_listener.AssertOneGetNewAndReset().Get("val")).Count);
        }
    
        [Test]
        public void TestKeyValueSelector()
        {
            String eplFragment = "select contained.GroupBy(k => id, v => p00) as val from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            EPAssertionUtil.AssertionCollectionValueString extractor = new EPAssertionUtil.ProxyAssertionCollectionValueString(
                collectionItem => Convert.ToString((int) collectionItem));
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E1,2", "E2,5"));
            EPAssertionUtil.AssertMapOfCollection(
                (GroupMap)_listener.AssertOneGetNewAndReset().Get("val"), 
                new String[]{"E1", "E2"},
                new String[]{"1,2", "5"},
                extractor);
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("val"));
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            Assert.AreEqual(0, ((GroupMap)_listener.AssertOneGetNewAndReset().Get("val")).Count);
    
            // test scalar
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractAfterUnderscore", this.GetType().FullName, "ExtractAfterUnderscore");
            String eplScalar = "select Strvals.GroupBy(k => extractAfterUnderscore(k), v => v) as val from SupportCollection";
            EPStatement stmtScalar = _epService.EPAdministrator.CreateEPL(eplScalar);
            stmtScalar.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtScalar.EventType, "val".Split(','), new Type[] { typeof(GroupMap) });
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1_2,E2_1,E3_2"));
            EPAssertionUtil.AssertMapOfCollection((GroupMap)_listener.AssertOneGetNewAndReset().Get("val"), "2,1".Split(','),
                    new String[]{"E1_2,E3_2", "E2_1"}, GetExtractorScalar());
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("val"));
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            Assert.AreEqual(0, ((GroupMap)_listener.AssertOneGetNewAndReset().Get("val")).Count);
        }

        public static String ExtractAfterUnderscore(String theString)
        {
            int indexUnderscore = theString.IndexOf("_");
            if (indexUnderscore == -1)
            {
                Assert.Fail();
            }
            return theString.Substring(indexUnderscore + 1);
        }

        private static EPAssertionUtil.AssertionCollectionValueString GetExtractorScalar()
        {
            return new EPAssertionUtil.ProxyAssertionCollectionValueString(
                collectionItem => collectionItem.ToString()
            );
        }
    }
}
