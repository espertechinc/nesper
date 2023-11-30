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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.avro
{
    public class EventAvroHook
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSimpleWriteablePropertyCoerce(execs);
            WithSchemaFromClass(execs);
            WithPopulate(execs);
            WithNamedWindowPropertyAssignment(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowPropertyAssignment(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventAvroHookNamedWindowPropertyAssignment());
            return execs;
        }

        public static IList<RegressionExecution> WithPopulate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventAvroHookPopulate());
            return execs;
        }

        public static IList<RegressionExecution> WithSchemaFromClass(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventAvroHookSchemaFromClass());
            return execs;
        }

        public static IList<RegressionExecution> WithSimpleWriteablePropertyCoerce(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventAvroHookSimpleWriteablePropertyCoerce());
            return execs;
        }

        /// <summary>
        /// Writeable-property tests: when a simple writable property needs to be converted
        /// </summary>
        internal class EventAvroHookSimpleWriteablePropertyCoerce : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // invalid without explicit conversion
                env.TryInvalidCompile(
                    "insert into MyEvent(isodate) select DateTime from SupportEventWithDateTime",
                    "Invalid assignment of column 'isodate' of type '" +
                    typeof(DateTime?).CleanName() +
                    "' to event property 'isodate' typed as '" +
                    typeof(string).CleanName() +
                    "', column and parameter types mismatch");

                // with hook
                env.CompileDeploy(
                        "@name('s0') insert into MyEvent(isodate) select DateTimeOffset from SupportEventWithDateTimeOffset")
                    .AddListener("s0");

                var now = DateTimeHelper.GetCurrentTimeUniversal();
                env.SendEventBean(new SupportEventWithDateTimeOffset(now));
                env.AssertEqualsNew("s0", "isodate", DateTimeFormat.ISO_DATE_TIME.Format(now));

                env.UndeployAll();
            }
        }

        /// <summary>
        /// Schema-from-Class
        /// </summary>
        internal class EventAvroHookSchemaFromClass : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') @public " +
                          EventRepresentationChoice.AVRO.GetAnnotationText() +
                          "insert into MyEventOut select " +
                          typeof(EventAvroHook).FullName +
                          ".MakeDateTimeOffset() as isodate from SupportBean as e1";
                env.CompileDeploy(epl).AddListener("s0");

                var schema = env.RuntimeAvroSchemaByDeployment("s0", "MyEventOut");
                Assert.AreEqual(
                    "{\"type\":\"record\",\"name\":\"MyEventOut\",\"fields\":[{\"name\":\"isodate\",\"type\":\"string\"}]}",
                    schema.ToString());

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        SupportAvroUtil.AvroToJson(@event);
                        Assert.That(@event.Get("isodate").ToString().Length, Is.GreaterThan(10));
                    });

                env.UndeployAll();
            }
        }

        /// <summary>
        /// Mapping of Class to GenericRecord
        /// </summary>
        internal class EventAvroHookPopulate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') insert into MyEventPopulate(sb) select " +
                          typeof(EventAvroHook).FullName +
                          ".MakeSupportBean() from SupportBean_S0 as e1";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(10));
                env.AssertEventNew(
                    "s0",
                    @event => Assert.AreEqual(
                        "{\"sb\":{\"TheString\":\"E1\",\"IntPrimitive\":10}}",
                        SupportAvroUtil.AvroToJson(@event)));

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }
        }

        internal class EventAvroHookNamedWindowPropertyAssignment : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('NamedWindow') @public create window MyWindow#keepall as MyEventWSchema",
                    path);
                env.CompileDeploy("insert into MyWindow select * from MyEventWSchema", path);
                env.CompileDeploy("on SupportBean thebean update MyWindow set sb = thebean", path);

                var schema = env
                    .RuntimeAvroSchemaPreconfigured("MyEventWSchema")
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

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }
        }

        public static DateTimeOffset MakeDateTimeOffset()
        {
            return DateTimeEx.NowUtc().DateTime;
        }
        
        public static SupportBean MakeSupportBean()
        {
            return new SupportBean("E1", 10);
        }

        public class MyObjectValueTypeWidenerFactory : ObjectValueTypeWidenerFactory
        {
            public ObjectValueTypeWidenerFactoryContext Context { get; private set; }

            public TypeWidenerSPI Make(ObjectValueTypeWidenerFactoryContext context)
            {
                Context = context;

                var contextClazz = context.Clazz.GetBoxedType();
                if (contextClazz == typeof(DateTimeOffset?)) {
                    return MyDateTimeOffsetTypeWidener.INSTANCE;
                }

                if (contextClazz == typeof(SupportBean)) {
                    return new MySupportBeanWidener();
                }

                return null;
            }
        }


        public class MyDateTimeOffsetTypeWidener : TypeWidenerSPI
        {
            public static readonly MyDateTimeOffsetTypeWidener INSTANCE = new MyDateTimeOffsetTypeWidener();

            private MyDateTimeOffsetTypeWidener()
            {
            }

            public Type WidenResultType {
                get => typeof(string);
            }

            public object Widen(object input)
            {
                var dateTimeOffset = (DateTimeOffset)input;
                return DateTimeFormat.ISO_DATE_TIME.Format(dateTimeOffset);
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

            public Type WidenResultType => typeof(GenericRecord);

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
                var sb = (SupportBean)input;
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
                if (context.Clazz.GetBoxedType() == typeof(DateTimeOffset?)) {
                    return TypeBuilder.StringType();
                }

                return null;
            }
        }
    }
} // end of namespace