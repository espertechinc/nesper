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

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.epl.insertinto;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionrun.Runner;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBean_N = com.espertech.esper.regressionlib.support.bean.SupportBean_N;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;
using SupportBeanSimple = com.espertech.esper.regressionlib.support.bean.SupportBeanSimple;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLInsertInto
    {
        private RegressionSession session;

        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configure(session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Destroy();
            session = null;
        }

        [Test]
        public void TestEPLInsertInto()
        {
            RegressionRunner.Run(session, EPLInsertInto.Executions());
        }

        [Test]
        public void TestEPLInsertIntoEmptyPropType()
        {
            RegressionRunner.Run(session, EPLInsertIntoEmptyPropType.Executions());
        }

        [Test]
        public void TestEPLInsertIntoIRStreamFunc()
        {
            RegressionRunner.Run(session, new EPLInsertIntoIRStreamFunc());
        }

        [Test]
        public void TestEPLInsertIntoPopulateCreateStream()
        {
            RegressionRunner.Run(session, new EPLInsertIntoPopulateCreateStream());
        }

        [Test]
        public void TestEPLInsertIntoPopulateCreateStreamAvro()
        {
            RegressionRunner.Run(session, EPLInsertIntoPopulateCreateStreamAvro.Executions());
        }

        [Test]
        public void TestEPLInsertIntoPopulateEventTypeColumn()
        {
            RegressionRunner.Run(session, EPLInsertIntoPopulateEventTypeColumn.Executions());
        }

        [Test]
        public void TestEPLInsertIntoPopulateSingleColByMethodCall()
        {
            RegressionRunner.Run(session, new EPLInsertIntoPopulateSingleColByMethodCall());
        }

        [Test]
        public void TestEPLInsertIntoPopulateUnderlying()
        {
            RegressionRunner.Run(session, EPLInsertIntoPopulateUnderlying.Executions());
        }

        [Test]
        public void TestEPLInsertIntoPopulateUndStreamSelect()
        {
            RegressionRunner.Run(session, EPLInsertIntoPopulateUndStreamSelect.Executions());
        }

        [Test]
        public void TestEPLInsertIntoTransposePattern()
        {
            RegressionRunner.Run(session, EPLInsertIntoTransposePattern.Executions());
        }

        [Test]
        public void TestEPLInsertIntoTransposeStream()
        {
            RegressionRunner.Run(session, EPLInsertIntoTransposeStream.Executions());
        }

        [Test]
        public void TestEPLInsertIntoFromPattern()
        {
            RegressionRunner.Run(session, EPLInsertIntoFromPattern.Executions());
        }

        [Test]
        public void TestEPLInsertIntoWrapper()
        {
            RegressionRunner.Run(session, EPLInsertIntoWrapper.Executions());
        }

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new[] {
                typeof(SupportBean),
                typeof(SupportObjectArrayOneDim),
                typeof(SupportBeanSimple),
                typeof(SupportBean_A),
                typeof(SupportRFIDEvent),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportMarketDataBean),
                typeof(SupportTemperatureBean),
                typeof(SupportBeanComplexProps),
                typeof(SupportBeanInterfaceProps),
                typeof(SupportBeanErrorTestingOne),
                typeof(SupportBeanErrorTestingTwo),
                typeof(SupportBeanReadOnly),
                typeof(SupportBeanArrayCollMap),
                typeof(SupportBean_N),
                typeof(SupportBeanObject),
                typeof(SupportBeanCtorOne),
                typeof(SupportBeanCtorTwo),
                typeof(SupportBean_ST0),
                typeof(SupportBean_ST1),
                typeof(SupportEventWithCtorSameType),
                typeof(SupportBeanCtorThree),
                typeof(SupportBeanCtorOne),
                typeof(SupportBean_ST0),
                typeof(SupportBean_ST1),
                typeof(SupportEventWithMapFieldSetter),
                typeof(SupportBeanNumeric),
                typeof(SupportBeanArrayEvent),
                typeof(SupportBean_A),
                typeof(SupportBean_B),
                typeof(SupportEventContainsSupportBean)
            })
            {
                configuration.Common.AddEventType(clazz);
            }

            Schema avroExistingTypeSchema = SchemaBuilder.Record("name",
                    TypeBuilder.RequiredLong("myLong"),
                    TypeBuilder.Field("myLongArray",
                            TypeBuilder.Array(TypeBuilder.LongType())),
                    TypeBuilder.Field("myByteArray", TypeBuilder.BytesType()),
                    TypeBuilder.Field("myMap", TypeBuilder.Map(
                            TypeBuilder.StringType(TypeBuilder.Property(
                                    AvroConstant.PROP_STRING_KEY, 
                                    AvroConstant.PROP_STRING_VALUE)))));
            configuration.Common.AddEventTypeAvro("AvroExistingType", new ConfigurationCommonEventTypeAvro(avroExistingTypeSchema));

            IDictionary<string, object> mapTypeInfo = new Dictionary<string, object>();
            mapTypeInfo.Put("one", typeof(string));
            mapTypeInfo.Put("two", typeof(string));
            configuration.Common.AddEventType("MapOne", mapTypeInfo);
            configuration.Common.AddEventType("MapTwo", mapTypeInfo);

            string[] props = { "one", "two" };
            object[] types = { typeof(string), typeof(string) };
            configuration.Common.AddEventType("OAOne", props, types);
            configuration.Common.AddEventType("OATwo", props, types);

            Schema avroOneAndTwoSchema = SchemaBuilder.Record("name",
                TypeBuilder.RequiredString("one"),
                TypeBuilder.RequiredString("two"));
            configuration.Common.AddEventTypeAvro("AvroOne", new ConfigurationCommonEventTypeAvro(avroOneAndTwoSchema));
            configuration.Common.AddEventTypeAvro("AvroTwo", new ConfigurationCommonEventTypeAvro(avroOneAndTwoSchema));

            ConfigurationCommonEventTypeBean legacySupportBeanString = new ConfigurationCommonEventTypeBean();
            legacySupportBeanString.FactoryMethod = "GetInstance";
            configuration.Common.AddEventType("SupportBeanString", typeof(SupportBeanString), legacySupportBeanString);

            ConfigurationCommonEventTypeBean legacySupportSensorEvent = new ConfigurationCommonEventTypeBean();
            legacySupportSensorEvent.FactoryMethod = typeof(SupportSensorEventFactory).FullName + ".GetInstance";
            configuration.Common.AddEventType("SupportSensorEvent", typeof(SupportSensorEvent), legacySupportSensorEvent);
            configuration.Common.AddImportType(typeof(SupportEnum));

            IDictionary<string, object> mymapDef = new Dictionary<string, object>();
            mymapDef.Put("Anint", typeof(int));
            mymapDef.Put("IntBoxed", typeof(int?));
            mymapDef.Put("FloatBoxed", typeof(float?));
            mymapDef.Put("IntArr", typeof(int[]));
            mymapDef.Put("MapProp", typeof(IDictionary<string, object>));
            mymapDef.Put("IsaImpl", typeof(ISupportAImpl));
            mymapDef.Put("IsbImpl", typeof(ISupportBImpl));
            mymapDef.Put("IsgImpl", typeof(ISupportAImplSuperGImpl));
            mymapDef.Put("IsabImpl", typeof(ISupportBaseABImpl));
            mymapDef.Put("Nested", typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested));
            configuration.Common.AddEventType("MyMap", mymapDef);

            IDictionary<string, object> defMap = new Dictionary<string, object>();
            defMap.Put("intVal", typeof(int));
            defMap.Put("stringVal", typeof(string));
            defMap.Put("doubleVal", typeof(double?));
            defMap.Put("nullVal", null);
            configuration.Common.AddEventType("MyMapType", defMap);

            string[] propsMyOAType = new string[] { "intVal", "stringVal", "doubleVal", "nullVal" };
            object[] typesMyOAType = new object[] { typeof(int), typeof(string), typeof(double?), null };
            configuration.Common.AddEventType("MyOAType", propsMyOAType, typesMyOAType);

            Schema schema = SchemaBuilder.Record("MyAvroType",
                    TypeBuilder.RequiredInt("intVal"),
                    TypeBuilder.RequiredString("stringVal"),
                    TypeBuilder.RequiredDouble("doubleVal"),
                    TypeBuilder.Field("nullVal", TypeBuilder.NullType()));
            configuration.Common.AddEventTypeAvro("MyAvroType", new ConfigurationCommonEventTypeAvro(schema));

            ConfigurationCommonEventTypeXMLDOM xml = new ConfigurationCommonEventTypeXMLDOM();
            xml.RootElementName = "abc";
            configuration.Common.AddEventType("xmltype", xml);

            IDictionary<string, object> mapDef = new Dictionary<string, object>();
            mapDef.Put("IntPrimitive", typeof(int));
            mapDef.Put("LongBoxed", typeof(long?));
            mapDef.Put("TheString", typeof(string));
            mapDef.Put("BoolPrimitive", typeof(bool?));
            configuration.Common.AddEventType("MySupportMap", mapDef);

            IDictionary<string, object> type = MakeMap(new object[][] {
                new object[]{"id", typeof(string)}
            });
            configuration.Common.AddEventType("AEventMap", type);
            configuration.Common.AddEventType("BEventMap", type);

            IDictionary<string, object> metadata = MakeMap(
                new object[][] { new object[] { "id", typeof(string) } });
            configuration.Common.AddEventType("AEventTE", metadata);
            configuration.Common.AddEventType("BEventTE", metadata);

            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));
            configuration.Common.AddImportNamespace(typeof(EPLInsertIntoPopulateUnderlying).Namespace);

            IDictionary<string, object> complexMapMetadata = MakeMap(new object[][]{
                new object[] {"nested", MakeMap(new object[][] {
                    new object[]{"NestedValue", typeof(string)}
                })}
            });
            configuration.Common.AddEventType("ComplexMap", complexMapMetadata);

            configuration.Compiler.ByteCode.AllowSubscriber = true;
            configuration.Compiler.AddPlugInSingleRowFunction("generateMap", typeof(EPLInsertIntoTransposeStream), "LocalGenerateMap");
            configuration.Compiler.AddPlugInSingleRowFunction("generateOA", typeof(EPLInsertIntoTransposeStream), "LocalGenerateOA");
            configuration.Compiler.AddPlugInSingleRowFunction("generateAvro", typeof(EPLInsertIntoTransposeStream), "LocalGenerateAvro");
            configuration.Compiler.AddPlugInSingleRowFunction("custom", typeof(SupportStaticMethodLib), "MakeSupportBean");
            configuration.Compiler.AddPlugInSingleRowFunction("customOne", typeof(SupportStaticMethodLib), "MakeSupportBean");
            configuration.Compiler.AddPlugInSingleRowFunction("customTwo", typeof(SupportStaticMethodLib), "MakeSupportBeanNumeric");
        }

        private static IDictionary<string, object> MakeMap(object[][] entries)
        {
            var result = new Dictionary<string, object>();
            foreach (object[] entry in entries)
            {
                result.Put((string) entry[0], entry[1]);
            }
            return result;
        }
    }
} // end of namespace