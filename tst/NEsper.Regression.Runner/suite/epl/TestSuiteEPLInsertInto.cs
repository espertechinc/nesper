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
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

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
            session.Dispose();
            session = null;
        }

        [Test, RunInApplicationDomain]
        public void TestEPLInsertIntoIRStreamFunc()
        {
            RegressionRunner.Run(session, new EPLInsertIntoIRStreamFunc());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLInsertIntoPopulateCreateStream()
        {
            RegressionRunner.Run(session, new EPLInsertIntoPopulateCreateStream());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLInsertIntoPopulateSingleColByMethodCall()
        {
            RegressionRunner.Run(session, new EPLInsertIntoPopulateSingleColByMethodCall());
        }

        /// <summary>
        /// Auto-test(s): EPLInsertIntoEventTypedColumnFromProp
        /// <code>
        /// RegressionRunner.Run(_session, EPLInsertIntoEventTypedColumnFromProp.Executions());
        /// </code>
        /// </summary>

        public class TestEPLInsertIntoEventTypedColumnFromProp : AbstractTestBase
        {
            public TestEPLInsertIntoEventTypedColumnFromProp() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithPONOTypedColumnOnMerge() => RegressionRunner.Run(_session, EPLInsertIntoEventTypedColumnFromProp.WithPONOTypedColumnOnMerge());

            [Test, RunInApplicationDomain]
            public void WithEventTypedColumnOnMerge() => RegressionRunner.Run(_session, EPLInsertIntoEventTypedColumnFromProp.WithEventTypedColumnOnMerge());
        }
        
        /// <summary>
        /// Auto-test(s): EPLInsertIntoWrapper
        /// <code>
        /// RegressionRunner.Run(_session, EPLInsertIntoWrapper.Executions());
        /// </code>
        /// </summary>

        public class TestEPLInsertIntoWrapper : AbstractTestBase
        {
            public TestEPLInsertIntoWrapper() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithOnSplitForkJoin() => RegressionRunner.Run(_session, EPLInsertIntoWrapper.WithOnSplitForkJoin());

            [Test, RunInApplicationDomain]
            public void With3StreamWrapper() => RegressionRunner.Run(_session, EPLInsertIntoWrapper.With3StreamWrapper());

            [Test, RunInApplicationDomain]
            public void WithWrapperBean() => RegressionRunner.Run(_session, EPLInsertIntoWrapper.WithWrapperBean());
        }
        
        /// <summary>
        /// Auto-test(s): EPLInsertIntoFromPattern
        /// <code>
        /// RegressionRunner.Run(_session, EPLInsertIntoFromPattern.Executions());
        /// </code>
        /// </summary>

        public class TestEPLInsertIntoFromPattern : AbstractTestBase
        {
            public TestEPLInsertIntoFromPattern() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithFromPatternNamedWindow() => RegressionRunner.Run(_session, EPLInsertIntoFromPattern.WithFromPatternNamedWindow());

            [Test, RunInApplicationDomain]
            public void WithNoProps() => RegressionRunner.Run(_session, EPLInsertIntoFromPattern.WithNoProps());

            [Test, RunInApplicationDomain]
            public void WithProps() => RegressionRunner.Run(_session, EPLInsertIntoFromPattern.WithProps());

            [Test, RunInApplicationDomain]
            public void WithPropsWildcard() => RegressionRunner.Run(_session, EPLInsertIntoFromPattern.WithPropsWildcard());
        }
        
        /// <summary>
        /// Auto-test(s): EPLInsertIntoPopulateEventTypeColumn
        /// <code>
        /// RegressionRunner.Run(_session, EPLInsertIntoPopulateEventTypeColumn.Executions());
        /// </code>
        /// </summary>

        public class TestEPLInsertIntoPopulateEventTypeColumn : AbstractTestBase
        {
            public TestEPLInsertIntoPopulateEventTypeColumn() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithEnumerationSubquery() => RegressionRunner.Run(_session, EPLInsertIntoPopulateEventTypeColumn.WithEnumerationSubquery());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLInsertIntoPopulateEventTypeColumn.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithTypableAndCaseNew() => RegressionRunner.Run(_session, EPLInsertIntoPopulateEventTypeColumn.WithTypableAndCaseNew());

            [Test, RunInApplicationDomain]
            public void WithTypableNewOperatorDocSample() => RegressionRunner.Run(
                _session,
                EPLInsertIntoPopulateEventTypeColumn.WithTypableNewOperatorDocSample());

            [Test, RunInApplicationDomain]
            public void WithTypableSubquery() => RegressionRunner.Run(_session, EPLInsertIntoPopulateEventTypeColumn.WithTypableSubquery());
        }

        /// <summary>
        /// Auto-test(s): EPLInsertIntoPopulateUnderlying
        /// <code>
        /// RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.Executions());
        /// </code>
        /// </summary>

        public class TestEPLInsertIntoPopulateUnderlying : AbstractTestBase
        {
            public TestEPLInsertIntoPopulateUnderlying() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithWindowAggregationAtEventBean() =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithWindowAggregationAtEventBean());

            [Test, RunInApplicationDomain]
            public void WithArrayMapInsert() => RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithArrayMapInsert());

            [Test, RunInApplicationDomain]
            public void WithArrayPONOInsert() => RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithArrayPONOInsert());

            [Test, RunInApplicationDomain]
            public void WithBeanFactoryMethod() => RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithBeanFactoryMethod());

            [Test, RunInApplicationDomain]
            public void WithPopulateUnderlyingSimple() => RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithPopulateUnderlyingSimple());

            [Test, RunInApplicationDomain]
            public void WithPopulateBeanObjects() => RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithPopulateBeanObjects());

            [Test, RunInApplicationDomain]
            public void WithBeanWildcard() => RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithBeanWildcard());

            [Test, RunInApplicationDomain]
            public void WithPopulateBeanSimple() => RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithPopulateBeanSimple());

            [Test, RunInApplicationDomain]
            public void WithBeanJoin() => RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithBeanJoin());

            [Test, RunInApplicationDomain]
            public void WithCtorWithPattern() => RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithCtorWithPattern());

            [Test, RunInApplicationDomain]
            public void WithCtor() => RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithCtor());
        }

        /// <summary>
        /// Auto-test(s): EPLInsertIntoPopulateUndStreamSelect
        /// <code>
        /// RegressionRunner.Run(_session, EPLInsertIntoPopulateUndStreamSelect.Executions());
        /// </code>
        /// </summary>

        public class TestEPLInsertIntoPopulateUndStreamSelect : AbstractTestBase
        {
            public TestEPLInsertIntoPopulateUndStreamSelect() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLInsertIntoPopulateUndStreamSelect.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithStreamInsertWWidenOA() => RegressionRunner.Run(_session, EPLInsertIntoPopulateUndStreamSelect.WithStreamInsertWWidenOA());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowRep() => RegressionRunner.Run(_session, EPLInsertIntoPopulateUndStreamSelect.WithNamedWindowRep());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowInheritsMap() => RegressionRunner.Run(_session, EPLInsertIntoPopulateUndStreamSelect.WithNamedWindowInheritsMap());
        }

        /// <summary>
        /// Auto-test(s): EPLInsertIntoTransposeStream
        /// <code>
        /// RegressionRunner.Run(_session, EPLInsertIntoTransposeStream.Executions());
        /// </code>
        /// </summary>

        public class TestEPLInsertIntoTransposeStream : AbstractTestBase
        {
            public TestEPLInsertIntoTransposeStream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalidTranspose() => RegressionRunner.Run(_session, EPLInsertIntoTransposeStream.WithInvalidTranspose());

            [Test, RunInApplicationDomain]
            public void WithTransposePONOPropertyStream() => RegressionRunner.Run(_session, EPLInsertIntoTransposeStream.WithTransposePONOPropertyStream());

            [Test, RunInApplicationDomain]
            public void WithTransposeEventJoinPONO() => RegressionRunner.Run(_session, EPLInsertIntoTransposeStream.WithTransposeEventJoinPONO());

            [Test, RunInApplicationDomain]
            public void WithTransposeEventJoinMap() => RegressionRunner.Run(_session, EPLInsertIntoTransposeStream.WithTransposeEventJoinMap());

            [Test, RunInApplicationDomain]
            public void WithTransposeSingleColumnInsert() => RegressionRunner.Run(_session, EPLInsertIntoTransposeStream.WithTransposeSingleColumnInsert());

            [Test, RunInApplicationDomain]
            public void WithTransposeFunctionToStream() => RegressionRunner.Run(_session, EPLInsertIntoTransposeStream.WithTransposeFunctionToStream());

            [Test, RunInApplicationDomain]
            public void WithTransposeFunctionToStreamWithProps() => RegressionRunner.Run(
                _session,
                EPLInsertIntoTransposeStream.WithTransposeFunctionToStreamWithProps());

            [Test, RunInApplicationDomain]
            public void WithTransposeMapAndObjectArrayAndOthers() => RegressionRunner.Run(
                _session,
                EPLInsertIntoTransposeStream.WithTransposeMapAndObjectArrayAndOthers());

            [Test, RunInApplicationDomain]
            public void WithTransposeCreateSchemaPONO() => RegressionRunner.Run(_session, EPLInsertIntoTransposeStream.WithTransposeCreateSchemaPONO());
        }

        /// <summary>
        /// Auto-test(s): EPLInsertIntoPopulateCreateStreamAvro
        /// <code>
        /// RegressionRunner.Run(_session, EPLInsertIntoPopulateCreateStreamAvro.Executions());
        /// </code>
        /// </summary>

        public class TestEPLInsertIntoPopulateCreateStreamAvro : AbstractTestBase
        {
            public TestEPLInsertIntoPopulateCreateStreamAvro() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithNewSchema() => RegressionRunner.Run(_session, EPLInsertIntoPopulateCreateStreamAvro.WithNewSchema());

            [Test, RunInApplicationDomain]
            public void WithCompatExisting() => RegressionRunner.Run(_session, EPLInsertIntoPopulateCreateStreamAvro.WithCompatExisting());
        }

        /// <summary>
        /// Auto-test(s): EPLInsertInto
        /// <code>
        /// RegressionRunner.Run(_session, EPLInsertInto.Executions());
        /// </code>
        /// </summary>

        public class TestEPLInsertInto : AbstractTestBase
        {
            public TestEPLInsertInto() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithEventRepresentationsSimple() => RegressionRunner.Run(_session, EPLInsertInto.WithEventRepresentationsSimple());

            [Test, RunInApplicationDomain]
            public void WithTypeMismatchInvalid() => RegressionRunner.Run(_session, EPLInsertInto.WithTypeMismatchInvalid());

            [Test, RunInApplicationDomain]
            public void WithUnnamedJoin() => RegressionRunner.Run(_session, EPLInsertInto.WithUnnamedJoin());

            [Test, RunInApplicationDomain]
            public void WithUnnamedWildcard() => RegressionRunner.Run(_session, EPLInsertInto.WithUnnamedWildcard());

            [Test, RunInApplicationDomain]
            public void WithUnnamedSimple() => RegressionRunner.Run(_session, EPLInsertInto.WithUnnamedSimple());

            [Test, RunInApplicationDomain]
            public void WithNamedColsJoinWildcard() => RegressionRunner.Run(_session, EPLInsertInto.WithNamedColsJoinWildcard());

            [Test, RunInApplicationDomain]
            public void WithNamedColsJoin() => RegressionRunner.Run(_session, EPLInsertInto.WithNamedColsJoin());

            [Test, RunInApplicationDomain]
            public void WithNamedColsWildcard() => RegressionRunner.Run(_session, EPLInsertInto.WithNamedColsWildcard());

            [Test, RunInApplicationDomain]
            public void WithNamedColsStateless() => RegressionRunner.Run(_session, EPLInsertInto.WithNamedColsStateless());

            [Test, RunInApplicationDomain]
            public void WithNamedColsSimple() => RegressionRunner.Run(_session, EPLInsertInto.WithNamedColsSimple());

            [Test, RunInApplicationDomain]
            public void WithNamedColsEPLToOMStmt() => RegressionRunner.Run(_session, EPLInsertInto.WithNamedColsEPLToOMStmt());

            [Test, RunInApplicationDomain]
            public void WithNamedColsOMToStmt() => RegressionRunner.Run(_session, EPLInsertInto.WithNamedColsOMToStmt());

            [Test, RunInApplicationDomain]
            public void WithRStreamOMToStmt() => RegressionRunner.Run(_session, EPLInsertInto.WithRStreamOMToStmt());

            [Test, RunInApplicationDomain]
            public void WithProvidePartitialCols() => RegressionRunner.Run(_session, EPLInsertInto.WithProvidePartitialCols());

            [Test, RunInApplicationDomain]
            public void WithSingleBeanToMulti() => RegressionRunner.Run(_session, EPLInsertInto.WithSingleBeanToMulti());

            [Test, RunInApplicationDomain]
            public void WithMultiBeanToMulti() => RegressionRunner.Run(_session, EPLInsertInto.WithMultiBeanToMulti());

            [Test, RunInApplicationDomain]
            public void WithChain() => RegressionRunner.Run(_session, EPLInsertInto.WithChain());

            [Test, RunInApplicationDomain]
            public void WithNullType() => RegressionRunner.Run(_session, EPLInsertInto.WithNullType());

            [Test, RunInApplicationDomain]
            public void WithInsertIntoPlusPattern() => RegressionRunner.Run(_session, EPLInsertInto.WithInsertIntoPlusPattern());

            [Test, RunInApplicationDomain]
            public void WithInsertFromPattern() => RegressionRunner.Run(_session, EPLInsertInto.WithInsertFromPattern());

            [Test, RunInApplicationDomain]
            public void WithStaggeredWithWildcard() => RegressionRunner.Run(_session, EPLInsertInto.WithStaggeredWithWildcard());

            [Test, RunInApplicationDomain]
            public void WithWithOutputLimitAndSort() => RegressionRunner.Run(_session, EPLInsertInto.WithWithOutputLimitAndSort());

            [Test, RunInApplicationDomain]
            public void WithJoinWildcard() => RegressionRunner.Run(_session, EPLInsertInto.WithJoinWildcard());

            [Test, RunInApplicationDomain]
            public void WithAssertionWildcardRecast() => RegressionRunner.Run(_session, EPLInsertInto.WithAssertionWildcardRecast());
        }
        
        /// <summary>
        /// Auto-test(s): EPLInsertIntoEmptyPropType
        /// <code>
        /// RegressionRunner.Run(_session, EPLInsertIntoEmptyPropType.Executions());
        /// </code>
        /// </summary>

        public class TestEPLInsertIntoEmptyPropType : AbstractTestBase
        {
            public TestEPLInsertIntoEmptyPropType() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithCreateSchemaInsertInto() => RegressionRunner.Run(_session, EPLInsertIntoEmptyPropType.WithCreateSchemaInsertInto());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowModelAfter() => RegressionRunner.Run(_session, EPLInsertIntoEmptyPropType.WithNamedWindowModelAfter());
        }

        /// <summary>
        /// Auto-test(s): EPLInsertIntoTransposePattern
        /// <code>
        /// RegressionRunner.Run(_session, EPLInsertIntoTransposePattern.Executions());
        /// </code>
        /// </summary>

        public class TestEPLInsertIntoTransposePattern : AbstractTestBase
        {
            public TestEPLInsertIntoTransposePattern() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithransposeMapEventPattern() => RegressionRunner.Run(_session, EPLInsertIntoTransposePattern.WithransposeMapEventPattern());

            [Test, RunInApplicationDomain]
            public void WithransposePONOEventPattern() => RegressionRunner.Run(_session, EPLInsertIntoTransposePattern.WithransposePONOEventPattern());

            [Test, RunInApplicationDomain]
            public void WithhisAsColumn() => RegressionRunner.Run(_session, EPLInsertIntoTransposePattern.WithhisAsColumn());
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
            }) {
                configuration.Common.AddEventType(clazz);
            }

            Schema avroExistingTypeSchema = SchemaBuilder.Record(
                "name",
                TypeBuilder.RequiredLong("MyLong"),
                TypeBuilder.Field(
                    "MyLongArray",
                    TypeBuilder.Array(TypeBuilder.LongType())),
                TypeBuilder.Field("MyByteArray", TypeBuilder.BytesType()),
                TypeBuilder.Field(
                    "MyMap",
                    TypeBuilder.Map(
                        TypeBuilder.StringType(
                            TypeBuilder.Property(
                                AvroConstant.PROP_STRING_KEY,
                                AvroConstant.PROP_STRING_VALUE)))));
            configuration.Common.AddEventTypeAvro("AvroExistingType", new ConfigurationCommonEventTypeAvro(avroExistingTypeSchema));

            IDictionary<string, object> mapTypeInfo = new Dictionary<string, object>();
            mapTypeInfo.Put("one", typeof(string));
            mapTypeInfo.Put("two", typeof(string));
            configuration.Common.AddEventType("MapOne", mapTypeInfo);
            configuration.Common.AddEventType("MapTwo", mapTypeInfo);

            string[] props = {"one", "two"};
            object[] types = {typeof(string), typeof(string)};
            configuration.Common.AddEventType("OAOne", props, types);
            configuration.Common.AddEventType("OATwo", props, types);

            Schema avroOneAndTwoSchema = SchemaBuilder.Record(
                "name",
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

            string[] propsMyOAType = new string[] {"intVal", "stringVal", "doubleVal", "nullVal"};
            object[] typesMyOAType = new object[] {typeof(int), typeof(string), typeof(double?), null};
            configuration.Common.AddEventType("MyOAType", propsMyOAType, typesMyOAType);

            Schema schema = SchemaBuilder.Record(
                "MyAvroType",
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

            IDictionary<string, object> type = MakeMap(
                new object[][] {
                    new object[] {"Id", typeof(string)}
                });
            configuration.Common.AddEventType("AEventMap", type);
            configuration.Common.AddEventType("BEventMap", type);

            IDictionary<string, object> metadata = MakeMap(
                new object[][] {new object[] {"Id", typeof(string)}});
            configuration.Common.AddEventType("AEventTE", metadata);
            configuration.Common.AddEventType("BEventTE", metadata);

            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));
            configuration.Common.AddImportNamespace(typeof(EPLInsertIntoPopulateUnderlying));

            IDictionary<string, object> complexMapMetadata = MakeMap(
                new object[][] {
                    new object[] {
                        "Nested", MakeMap(
                            new object[][] {
                                new object[] {"NestedValue", typeof(string)}
                            })
                    }
                });
            configuration.Common.AddEventType("ComplexMap", complexMapMetadata);

            configuration.Compiler.ByteCode.AllowSubscriber = true;
            configuration.Compiler.AddPlugInSingleRowFunction("generateMap", typeof(EPLInsertIntoTransposeStream), "LocalGenerateMap");
            configuration.Compiler.AddPlugInSingleRowFunction("generateOA", typeof(EPLInsertIntoTransposeStream), "LocalGenerateOA");
            configuration.Compiler.AddPlugInSingleRowFunction("generateAvro", typeof(EPLInsertIntoTransposeStream), "LocalGenerateAvro");
            configuration.Compiler.AddPlugInSingleRowFunction("generateJson", typeof(EPLInsertIntoTransposeStream), "LocalGenerateJson");
            configuration.Compiler.AddPlugInSingleRowFunction("custom", typeof(SupportStaticMethodLib), "MakeSupportBean");
            configuration.Compiler.AddPlugInSingleRowFunction("customOne", typeof(SupportStaticMethodLib), "MakeSupportBean");
            configuration.Compiler.AddPlugInSingleRowFunction("customTwo", typeof(SupportStaticMethodLib), "MakeSupportBeanNumeric");
        }

        private static IDictionary<string, object> MakeMap(object[][] entries)
        {
            var result = new Dictionary<string, object>();
            foreach (object[] entry in entries) {
                result.Put((string) entry[0], entry[1]);
            }

            return result;
        }
    }
} // end of namespace