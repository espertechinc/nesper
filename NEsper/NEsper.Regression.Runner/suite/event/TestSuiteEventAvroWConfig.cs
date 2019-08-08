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
using com.espertech.esper.regressionrun.Runner;

using NEsper.Avro.Extensions;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.@event
{
    [TestFixture]
    public class TestSuiteEventAvroWConfig
    {
        [Test]
        public void TestEventAvroHook()
        {
            RegressionSession session = RegressionRunner.Session();

            foreach (Type clazz in new Type[] {
                typeof(SupportBean),
                typeof(SupportBean_S0)
            })
            {
                session.Configuration.Common.AddEventType(clazz);
            }

            session.Configuration.Common.EventMeta.AvroSettings.TypeRepresentationMapperClass =
                typeof(EventAvroHook.MyTypeRepresentationMapper).FullName;
            session.Configuration.Common.EventMeta.AvroSettings.ObjectValueTypeWidenerFactoryClass =
                typeof(EventAvroHook.MyObjectValueTypeWidenerFactory).FullName;

            EventAvroHook.MySupportBeanWidener.supportBeanSchema = SchemaBuilder.Record(
                "SupportBeanSchema",
                TypeBuilder.RequiredString("TheString"),
                TypeBuilder.RequiredInt("IntPrimitive"));
            Schema schemaMyEventPopulate = SchemaBuilder.Record("MyEventSchema",
                TypeBuilder.Field("sb", EventAvroHook.MySupportBeanWidener.supportBeanSchema));
            session.Configuration.Common.AddEventTypeAvro("MyEventPopulate", new ConfigurationCommonEventTypeAvro(schemaMyEventPopulate));

            Schema schemaMyEventSchema = SchemaBuilder.Record("MyEventSchema", TypeBuilder.RequiredString("isodate"));
            session.Configuration.Common.AddEventTypeAvro("MyEvent", new ConfigurationCommonEventTypeAvro(schemaMyEventSchema));

            EventAvroHook.MySupportBeanWidener.supportBeanSchema = SchemaBuilder.Record("SupportBeanSchema",
                TypeBuilder.RequiredString("TheString"),
                TypeBuilder.RequiredInt("IntPrimitive"));
            Schema schemaMyEventWSchema = SchemaBuilder.Record("MyEventSchema",
                TypeBuilder.Field("sb", TypeBuilder.Union(
                    TypeBuilder.NullType(),
                    EventAvroHook.MySupportBeanWidener.supportBeanSchema)));
            session.Configuration.Common.AddEventTypeAvro("MyEventWSchema", new ConfigurationCommonEventTypeAvro(schemaMyEventWSchema));

            RegressionRunner.Run(session, EventAvroHook.Executions());

            session.Destroy();
        }
    }
} // end of namespace