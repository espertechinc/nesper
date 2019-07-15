///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTIntervalOpsCreateSchema : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionCreateSchema(env, rep);
            }

            // test Bean-type Date-type timestamps
            var startA = "2002-05-30T09:00:00.000";
            var epl = " create schema SupportBeanXXX as " +
                      typeof(SupportBean).Name +
                      " starttimestamp LongPrimitive endtimestamp LongBoxed;\n";
            epl += "@Name('s0') select a.Get('month') as val0 from SupportBeanXXX a;\n";
            env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");

            var theEvent = new SupportBean();
            theEvent.LongPrimitive = DateTimeParsingFunctions.ParseDefaultMSec(startA);
            env.EventService.SendEventBean(theEvent, "SupportBeanXXX");
            Assert.AreEqual(4, env.Listener("s0").AssertOneGetNewAndReset().Get("val0"));

            env.UndeployAll();
        }

        private void TryAssertionCreateSchema(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var startA = "2002-05-30T09:00:00.000";
            var endA = "2002-05-30T09:00:01.000";
            var startB = "2002-05-30T09:00:00.500";
            var endB = "2002-05-30T09:00:00.700";

            // test Map type Long-type timestamps
            RunAssertionCreateSchemaWTypes(
                env,
                eventRepresentationEnum,
                "long",
                DateTimeParsingFunctions.ParseDefaultMSec(startA),
                DateTimeParsingFunctions.ParseDefaultMSec(endA),
                DateTimeParsingFunctions.ParseDefaultMSec(startB),
                DateTimeParsingFunctions.ParseDefaultMSec(endB));

            // test Map type DateTimeEx-type timestamps
            if (!eventRepresentationEnum.IsAvroEvent()) {
                RunAssertionCreateSchemaWTypes(
                    env,
                    eventRepresentationEnum,
                    typeof(DateTimeEx).FullName,
                    DateTimeParsingFunctions.ParseDefaultEx(startA),
                    DateTimeParsingFunctions.ParseDefaultEx(endA),
                    DateTimeParsingFunctions.ParseDefaultEx(startB),
                    DateTimeParsingFunctions.ParseDefaultEx(endB));
            }

            // test Map type DateTimeOffset-type timestamps
            if (!eventRepresentationEnum.IsAvroEvent()) {
                RunAssertionCreateSchemaWTypes(
                    env,
                    eventRepresentationEnum,
                    typeof(DateTimeOffset).FullName,
                    DateTimeParsingFunctions.ParseDefaultDateTimeOffset(startA),
                    DateTimeParsingFunctions.ParseDefaultDateTimeOffset(endA),
                    DateTimeParsingFunctions.ParseDefaultDateTimeOffset(startB),
                    DateTimeParsingFunctions.ParseDefaultDateTimeOffset(endB));
            }

            // test Map type DateTime-type timestamps
            if (!eventRepresentationEnum.IsAvroEvent()) {
                RunAssertionCreateSchemaWTypes(
                    env,
                    eventRepresentationEnum,
                    typeof(DateTime).FullName,
                    DateTimeParsingFunctions.ParseDefaultDateTimeOffset(startA).DateTime,
                    DateTimeParsingFunctions.ParseDefaultDateTimeOffset(endA).DateTime,
                    DateTimeParsingFunctions.ParseDefaultDateTimeOffset(startB).DateTime,
                    DateTimeParsingFunctions.ParseDefaultDateTimeOffset(endB).DateTime);
            }
        }

        private void RunAssertionCreateSchemaWTypes(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            string typeOfDatetimeProp,
            object startA,
            object endA,
            object startB,
            object endB)
        {
            var epl = eventRepresentationEnum.GetAnnotationText() +
                      " create schema TypeA as (startts " +
                      typeOfDatetimeProp +
                      ", endts " +
                      typeOfDatetimeProp +
                      ") starttimestamp startts endtimestamp endts;\n";
            epl += eventRepresentationEnum.GetAnnotationText() +
                   " create schema TypeB as (startts " +
                   typeOfDatetimeProp +
                   ", endts " +
                   typeOfDatetimeProp +
                   ") starttimestamp startts endtimestamp endts;\n";
            epl += "@Name('s0') select a.includes(b) as val0 from TypeA#lastevent as a, TypeB#lastevent as b;\n";
            env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");

            MakeSendEvent(env, "TypeA", eventRepresentationEnum, startA, endA);
            MakeSendEvent(env, "TypeB", eventRepresentationEnum, startB, endB);
            Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("val0"));

            env.UndeployAll();
        }

        private void MakeSendEvent(
            RegressionEnvironment env,
            string typeName,
            EventRepresentationChoice eventRepresentationEnum,
            object startTs,
            object endTs)
        {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new[] {startTs, endTs}, typeName);
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                var theEvent = new Dictionary<string, object>();
                theEvent.Put("startts", startTs);
                theEvent.Put("endts", endTs);
                env.SendEventMap(theEvent, typeName);
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var record = new GenericRecord(
                    SupportAvroUtil.GetAvroSchema(
                            env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName))
                        .AsRecordSchema());
                record.Put("startts", startTs);
                record.Put("endts", endTs);
                env.EventService.SendEventAvro(record, typeName);
            }
            else {
                throw new IllegalStateException("Unrecognized enum " + eventRepresentationEnum);
            }
        }
    }
} // end of namespace