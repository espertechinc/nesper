///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.@event.avro;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NEsper.Avro.Extensions;

using NUnit.Framework;

using static NEsper.Avro.Extensions.TypeBuilder;

namespace com.espertech.esper.regressionrun.suite.@event
{
    [TestFixture]
    public class TestSuiteEventAvroWConfig : AbstractTestContainer
    {
        [Test, RunInApplicationDomain]
        public void TestEventAvroHook()
        {
            using RegressionSession session = RegressionRunner.Session(Container);

            foreach (Type clazz in new Type[] {
                typeof(SupportBean),
                typeof(SupportBean_S0),
                typeof(SupportEventWithDateTime),
                typeof(SupportEventWithDateTimeOffset)
            })
            {
                session.Configuration.Common.AddEventType(clazz);
            }

            session.Configuration.Common.EventMeta.AvroSettings.IsEnableAvro = true;
            session.Configuration.Common.EventMeta.AvroSettings.TypeRepresentationMapperClass =
                typeof(EventAvroHook.MyTypeRepresentationMapper).FullName;
            session.Configuration.Common.EventMeta.AvroSettings.ObjectValueTypeWidenerFactoryClass =
                typeof(EventAvroHook.MyObjectValueTypeWidenerFactory).FullName;

            EventAvroHook.MySupportBeanWidener.supportBeanSchema = SchemaBuilder.Record(
                "SupportBeanSchema",
                RequiredString("TheString"),
                RequiredInt("IntPrimitive"));
            Schema schemaMyEventPopulate = SchemaBuilder.Record("MyEventSchema",
                Field("sb", EventAvroHook.MySupportBeanWidener.supportBeanSchema));
            session.Configuration.Common.AddEventTypeAvro("MyEventPopulate", new ConfigurationCommonEventTypeAvro(schemaMyEventPopulate));

            Schema schemaMyEventSchema = SchemaBuilder.Record("MyEventSchema", RequiredString("isodate"));
            session.Configuration.Common.AddEventTypeAvro("MyEvent", new ConfigurationCommonEventTypeAvro(schemaMyEventSchema));

            EventAvroHook.MySupportBeanWidener.supportBeanSchema = SchemaBuilder.Record("SupportBeanSchema",
                RequiredString("TheString"),
                RequiredInt("IntPrimitive"));
            Schema schemaMyEventWSchema = SchemaBuilder.Record("MyEventSchema",
                Field("sb", Union(
                    NullType(),
                    EventAvroHook.MySupportBeanWidener.supportBeanSchema)));
            session.Configuration.Common.AddEventTypeAvro("MyEventWSchema", new ConfigurationCommonEventTypeAvro(schemaMyEventWSchema));

            RegressionRunner.Run(session, EventAvroHook.Executions());
        }
    }
} // end of namespace