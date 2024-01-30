///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

using static NEsper.Avro.Extensions.TypeBuilder;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoPopulateCreateStreamAvro
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithCompatExisting(execs);
            With(NewSchema)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithNewSchema(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoNewSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithCompatExisting(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoCompatExisting());
            return execs;
        }

        public static byte[] MakeByteArray()
        {
            return new byte[] { 1, 2, 3 };
        }

        public static IDictionary<string, string> MakeMapStringString()
        {
            return Collections.SingletonMap("k1", "v1");
        }

        internal class EPLInsertIntoCompatExisting : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') insert into AvroExistingType select " +
                          "1 as MyLong," +
                          "{1L, 2L} as MyLongArray," +
                          nameof(EPLInsertIntoPopulateCreateStreamAvro) +
                          ".MakeByteArray() as MyByteArray, " +
                          nameof(EPLInsertIntoPopulateCreateStreamAvro) +
                          ".MakeMapStringString() as MyMap " +
                          "from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStmtTypes(
                    "s0",
                    new[] { "MyLong", "MyLongArray", "MyByteArray", "MyMap" },
                    new[] {
                        typeof(long),
                        typeof(long[]),
                        typeof(byte[]),
                        typeof(IDictionary<string, string>)
                    });

                env.SendEventBean(new SupportBean());
                env.AssertEventNew(
                    "s0",
                    @event => {
                        SupportAvroUtil.AvroToJson(@event);
                        Assert.AreEqual(1L, @event.Get("MyLong"));
                        EPAssertionUtil.AssertEqualsExactOrder(
                            new[] { 1L, 2L },
                            @event.Get("MyLongArray").UnwrapIntoArray<long>());

                        CollectionAssert.AreEqual(
                            new byte[] { 1, 2, 3 },
                            (byte[])@event.Get("MyByteArray"));

                        Assert.AreEqual("{\"k1\"=\"v1\"}", @event.Get("MyMap").RenderAny());
                    });

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoNewSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var insertInto = typeof(EPLInsertIntoPopulateCreateStreamAvro).FullName;
                var epl = "@name('s0') " +
                          EventRepresentationChoice.AVRO.GetAnnotationText() +
                          " select 1 as MyInt," +
                          "{1L, 2L} as myLongArray," +
                          $"{insertInto}.MakeByteArray() as myByteArray, " +
                          $"{insertInto}.MakeMapStringString() as myMap " +
                          "from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean());
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var json = SupportAvroUtil.AvroToJson(@event);
                        Console.Out.WriteLine(json);
                        Assert.AreEqual(1, @event.Get("MyInt"));
                        EPAssertionUtil.AssertEqualsExactOrder(
                            new[] { 1L, 2L },
                            @event.Get("myLongArray").UnwrapIntoArray<long>());
                        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, (byte[])@event.Get("myByteArray"));
                        Assert.AreEqual("{\"k1\"=\"v1\"}", @event.Get("myMap").RenderAny());

                        var designSchema = SchemaBuilder.Record(
                            "name",
                            RequiredInt("MyInt"),
                            Field("myLongArray", Array(LongType())),
                            Field("myByteArray", BytesType()),
                            Field(
                                "myMap",
                                Map(
                                    StringType(
                                        Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE)))));

                        var assembledSchema = ((AvroEventType)@event.EventType).SchemaAvro;
                        var compareMsg = SupportAvroUtil.CompareSchemas(designSchema, assembledSchema);
                        Assert.IsNull(compareMsg, compareMsg);
                    });

                env.UndeployAll();
            }
        }
    }
} // end of namespace