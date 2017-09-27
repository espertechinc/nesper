///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util;

using NUnit.Framework;

using NEsper.Avro.Extensions;
using NEsper.Avro.Core;
using NEsper.Avro.Util.Support;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestJoinEventRepresentation
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp() {
	        Configuration config = SupportConfigFactory.GetConfiguration();

	        IDictionary<string, object> typeInfo = new Dictionary<string, object>();
	        typeInfo.Put("id", typeof(string));
	        typeInfo.Put("p00", typeof(int));
	        config.AddEventType("MapS0", typeInfo);
	        config.AddEventType("MapS1", typeInfo);

	        config.EngineDefaults.Logging.IsEnableQueryPlan = true;
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	        _listener = null;
	    }

        [Test]
	    public void TestJoinEventRepresentations() {
	        string eplOne = "select S0.id as S0_id, S1.id as S1_id, S0.p00 as S0_p00, S1.p00 as S1_p00 from S0#keepall as S0, S1#keepall as S1 where S0.id = S1.id";
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>())
            {
	            RunAssertion(eplOne, rep, "S0_id,S1_id,S0_p00,S1_p00");
	        }

	        string eplTwo = "select * from S0#keepall as S0, S1#keepall as S1 where S0.id = S1.id";
	        foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
	            RunAssertion(eplTwo, rep, "S0.id,S1.id,S0.p00,S1.p00");
	        }
	    }

	    private void RunAssertion(string epl, EventRepresentationChoice rep, string columnNames) {
	        if (rep.IsMapEvent()) {
	            IDictionary<string, object> typeInfo = new Dictionary<string, object>();
	            typeInfo.Put("id", typeof(string));
	            typeInfo.Put("p00", typeof(int));
	            _epService.EPAdministrator.Configuration.AddEventType("S0", typeInfo);
	            _epService.EPAdministrator.Configuration.AddEventType("S1", typeInfo);
            }
            else if (rep.IsObjectArrayEvent())
            {
	            string[] names = "id,p00".SplitCsv();
	            object[] types = new object[] {typeof(string), typeof(int)};
	            _epService.EPAdministrator.Configuration.AddEventType("S0", names, types);
	            _epService.EPAdministrator.Configuration.AddEventType("S1", names, types);
            }
            else if (rep.IsAvroEvent())
            {
                var schema = SchemaBuilder.Record("name",
                    TypeBuilder.Field("id", TypeBuilder.String(
                        TypeBuilder.Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE))),
                    TypeBuilder.RequiredInt("p00"));

	            _epService.EPAdministrator.Configuration.AddEventTypeAvro("S0", new ConfigurationEventTypeAvro().SetAvroSchema(schema));
                _epService.EPAdministrator.Configuration.AddEventTypeAvro("S1", new ConfigurationEventTypeAvro().SetAvroSchema(schema));
	        }

	        _listener.Reset();
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(rep.GetAnnotationText() + epl);
	        stmt.AddListener(_listener);

	        SendRepEvent(rep, "S0", "a", 1);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendRepEvent(rep, "S1", "a", 2);
	        EventBean output = _listener.AssertOneGetNewAndReset();
	        EPAssertionUtil.AssertProps(output, columnNames.SplitCsv(), new object[] {"a", "a", 1, 2});
	        Assert.IsTrue(rep.MatchesClass(output.Underlying.GetType()));

	        SendRepEvent(rep, "S1", "b", 3);
	        SendRepEvent(rep, "S0", "c", 4);
	        Assert.IsFalse(_listener.IsInvoked);

	        stmt.Dispose();
	        _epService.EPAdministrator.Configuration.RemoveEventType("S0", true);
	        _epService.EPAdministrator.Configuration.RemoveEventType("S1", true);
	    }

        [Test]
	    public void TestJoinMapEventNotUnique() {
	        // Test for Esper-122
	        string joinStatement = "select S0.id, S1.id, S0.p00, S1.p00 from MapS0#keepall as S0, MapS1#keepall as S1" +
	                               " where S0.id = S1.id";

	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(joinStatement);
	        stmt.AddListener(_listener);

	        for (int i = 0; i < 100; i++) {
	            if (i % 2 == 1) {
	                SendMapEvent("MapS0", "a", 1);
	            } else {
	                SendMapEvent("MapS1", "a", 1);
	            }
	        }
	    }

        [Test]
	    public void TestJoinWrapperEventNotUnique() {
	        // Test for Esper-122
	        _epService.EPAdministrator.CreateEPL("insert into S0 select 's0' as streamone, * from " + typeof(SupportBean).FullName);
	        _epService.EPAdministrator.CreateEPL("insert into S1 select 's1' as streamtwo, * from " + typeof(SupportBean).FullName);
	        string joinStatement = "select * from S0#keepall as a, S1#keepall as b where a.intBoxed = b.intBoxed";

	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(joinStatement);
	        stmt.AddListener(_listener);

	        for (int i = 0; i < 100; i++) {
	            _epService.EPRuntime.SendEvent(new SupportBean());
	        }
	    }

	    private void SendMapEvent(string name, string id, int p00) {
	        IDictionary<string, object> theEvent = new Dictionary<string, object>();
	        theEvent.Put("id", id);
	        theEvent.Put("p00", p00);
	        _epService.EPRuntime.SendEvent(theEvent, name);
	    }

	    private void SendRepEvent(EventRepresentationChoice rep, string name, string id, int p00) {
	        if (rep.IsMapEvent()) {
	            IDictionary<string, object> theEvent = new Dictionary<string, object>();
	            theEvent.Put("id", id);
	            theEvent.Put("p00", p00);
	            _epService.EPRuntime.SendEvent(theEvent, name);
	        } else if (rep.IsObjectArrayEvent()) {
	            _epService.EPRuntime.SendEvent(new object[] {id, p00}, name);
	        } else if (rep.IsAvroEvent()) {
	            GenericRecord theEvent = new GenericRecord(SupportAvroUtil.GetAvroSchema(_epService, name).AsRecordSchema());
	            theEvent.Put("id", id);
	            theEvent.Put("p00", p00);
	            _epService.EPRuntime.SendEventAvro(theEvent, name);
	        } else {
	            Assert.Fail();
	        }
	    }
	}
} // end of namespace
