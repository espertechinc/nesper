///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client.hook.type;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.regressionlib.suite.@event.avro
{
    public class EventAvroHook
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EventAvroHookSimpleWriteablePropertyCoerce());
            execs.Add(new EventAvroHookSchemaFromClass());
            execs.Add(new EventAvroHookPopulate());
            execs.Add(new EventAvroHookNamedWindowPropertyAssignment());
            return execs;
        }

        public static DateTime MakeDateTime()
        {
            return new DateTime();
        }

        public static SupportBean MakeSupportBean()
        {
            return new SupportBean("E1", 10);
        }

        /// <summary>
        ///     Writeable-property tests: when a simple writable property needs to be converted
        /// </summary>
        internal class EventAvroHookSimpleWriteablePropertyCoerce : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // invalid without explicit conversion
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "insert into MyEvent(isodate) select dto from SupportEventWithDateTime",
                    "Invalid assignment of column 'isodate' of type '" +
                    TypeHelper.GetCleanName<DateTimeOffset>() +
                    "' to event property 'isodate' typed as '" +
                    TypeHelper.GetCleanName<char[]>() +
                    "', column and parameter types mismatch");

                // with hook
                env.CompileDeploy(
                        "@Name('s0') insert into MyEvent(isodate) select ldt from SupportEventWithDateTimeOffset")
                    .AddListener("s0");

                var now = new DateTime();
                env.SendEventBean(new SupportEventWithDateTimeOffset(now));
                Assert.AreEqual(
                    DateTimeFormat.ISO_DATE_TIME.Format(now),
                    env.Listener("s0").AssertOneGetNewAndReset().Get("isodate"));

                env.UndeployAll();
            }
        }

        /// <summary>
        ///     Schema-from-Class
        /// </summary>
        internal class EventAvroHookSchemaFromClass : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') " +
                          EventRepresentationChoice.AVRO.GetAnnotationText() +
                          "insert into MyEventOut select " +
                          typeof(EventAvroHook).Name +
                          ".makeDateTimeOffset() as isodate from SupportBean as e1";
                env.CompileDeploy(epl).AddListener("s0");

                var schema = SupportAvroUtil.GetAvroSchema(env.Statement("s0").EventType);
                Assert.AreEqual(
                    "{\"type\":\"record\",\"name\":\"MyEventOut\",\"fields\":[{\"name\":\"isodate\",\"type\":\"string\"}]}",
                    schema.ToString());

                env.SendEventBean(new SupportBean("E1", 10));
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                SupportAvroUtil.AvroToJson(@event);
                Assert.IsTrue(@event.Get("isodate").ToString().Length > 10);

                env.UndeployAll();
            }
        }

        /// <summary>
        ///     Mapping of Class to GenericRecord
        ///     }
        /// </summary>
        internal class EventAvroHookPopulate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') insert into MyEventPopulate(sb) select " +
                          typeof(EventAvroHook).Name +
                          ".makeSupportBean() from SupportBean_S0 as e1";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(10));
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(
                    "{\"sb\":{\"TheString\":\"E1\",\"IntPrimitive\":10}}",
                    SupportAvroUtil.AvroToJson(@event));

                env.UndeployAll();
            }
        }

        internal class EventAvroHookNamedWindowPropertyAssignment : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@Name('NamedWindow') create window MyWindow#keepall as MyEventWSchema", path);
                env.CompileDeploy("insert into MyWindow select * from MyEventWSchema", path);
                env.CompileDeploy("on SupportBean thebean update MyWindow set sb = thebean", path);

                var schema = AvroSchemaUtil
                    .ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured("MyEventWSchema"))
                    .AsRecordSchema();
                var @event = new GenericRecord(schema);
                env.SendEventAvro(@event, "MyEventWSchema");
                env.SendEventBean(new SupportBean("E1", 10));

                var eventBean = env.GetEnumerator("NamedWindow").Advance();
                Assert.AreEqual(
                    "{\"sb\":{\"SupportBeanSchema\":{\"TheString\":\"E1\",\"IntPrimitive\":10}}}",
                    SupportAvroUtil.AvroToJson(eventBean));

                env.UndeployAll();
            }
        }

        public class MyObjectValueTypeWidenerFactory : ObjectValueTypeWidenerFactory
        {
            public ObjectValueTypeWidenerFactoryContext Context { get; private set; }

            public TypeWidenerSPI Make(ObjectValueTypeWidenerFactoryContext context)
            {
                Context = context;
                if (context.Clazz == typeof(DateTime)) {
                    return MyLDTTypeWidener.INSTANCE;
                }

                if (context.Clazz == typeof(SupportBean)) {
                    return new MySupportBeanWidener();
                }

                return null;
            }
        }

        public class MyLDTTypeWidener : TypeWidenerSPI
        {
            public static readonly MyLDTTypeWidener INSTANCE = new MyLDTTypeWidener();

            private MyLDTTypeWidener()
            {
            }

            public object Widen(object input)
            {
                var ldt = (DateTime) input;
                return DateTimeFormat.ISO_DATE_TIME.Format(ldt);
            }

            public CodegenExpression WidenCodegen(
                CodegenExpression expression,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return ExprDotMethod(
                    EnumValue(typeof(DateTimeFormat), "ISO_DATE_TIME"),
                    "Format",
                    Cast(typeof(DateTimeOffset), expression));
            }
        }

        public class MySupportBeanWidener : TypeWidenerSPI
        {
            public static RecordSchema supportBeanSchema;

            public object Widen(object input)
            {
                return WidenInput(input);
            }

            public CodegenExpression WidenCodegen(
                CodegenExpression expression,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return StaticMethod(typeof(MySupportBeanWidener), "WidenInput", expression);
            }

            public static object WidenInput(object input)
            {
                var sb = (SupportBean) input;
                var record = new GenericRecord(supportBeanSchema);
                record.Put("TheString", sb.TheString);
                record.Put("IntPrimitive", sb.IntPrimitive);
                return record;
            }
        }

        public class MyTypeRepresentationMapper : TypeRepresentationMapper
        {
            public object Map(TypeRepresentationMapperContext context)
            {
                if (context.Clazz == typeof(DateTime)) {
                    return TypeBuilder.StringType();
                }

                return null;
            }
        }
    }
} // end of namespace