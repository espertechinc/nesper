///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

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
using SupportBeanWithThis = com.espertech.esper.regressionlib.support.bean.SupportBeanWithThis;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLInsertInto : AbstractTestBase
    {
        public static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[]
                     {
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
                         typeof(SupportEventContainsSupportBean),
                         typeof(SupportBeanWithThis)
                     })
            {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.EventMeta.AvroSettings.IsEnableAvro = true;
            
            var avroExistingTypeSchema = SchemaBuilder.Record("name", TypeBuilder.RequiredLong("MyLong"),
                TypeBuilder.Field("MyLongArray", TypeBuilder.Array(TypeBuilder.LongType())),
                TypeBuilder.Field("MyByteArray", TypeBuilder.BytesType()),
                TypeBuilder.Field("MyMap",
                    TypeBuilder.Map(TypeBuilder.StringType(TypeBuilder.Property(AvroConstant.PROP_STRING_KEY,
                        AvroConstant.PROP_STRING_VALUE)))));
            
            configuration.Common.AddEventTypeAvro("AvroExistingType",
                new ConfigurationCommonEventTypeAvro(avroExistingTypeSchema));
            
            var mapTypeInfo = new Dictionary<string, object>();
            mapTypeInfo.Put("one", typeof(string));
            mapTypeInfo.Put("two", typeof(string));
            configuration.Common.AddEventType("MapOne", mapTypeInfo);
            configuration.Common.AddEventType("MapTwo", mapTypeInfo);

            string[] props = { "one", "two" };
            object[] types = { typeof(string), typeof(string) };
            configuration.Common.AddEventType("OAOne", props, types);
            configuration.Common.AddEventType("OATwo", props, types);
            
            var avroOneAndTwoSchema = SchemaBuilder.Record("name", TypeBuilder.RequiredString("one"),
                TypeBuilder.RequiredString("two"));
            configuration.Common.AddEventTypeAvro("AvroOne", new ConfigurationCommonEventTypeAvro(avroOneAndTwoSchema));
            configuration.Common.AddEventTypeAvro("AvroTwo", new ConfigurationCommonEventTypeAvro(avroOneAndTwoSchema));
            
            var legacySupportBeanString = new ConfigurationCommonEventTypeBean();
            legacySupportBeanString.FactoryMethod = "GetInstance";
            configuration.Common.AddEventType("SupportBeanString", typeof(SupportBeanString), legacySupportBeanString);
            
            var legacySupportSensorEvent = new ConfigurationCommonEventTypeBean();
            legacySupportSensorEvent.FactoryMethod = typeof(SupportSensorEventFactory).FullName + ".GetInstance";
            configuration.Common.AddEventType("SupportSensorEvent", typeof(SupportSensorEvent),
                legacySupportSensorEvent);
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
            var propsMyOAType = new string[]
            {
                "intVal",
                "stringVal",
                "doubleVal",
                "nullVal"
            };
            var typesMyOAType = new object[]
            {
                typeof(int),
                typeof(string),
                typeof(double?),
                null
            };

            configuration.Common.AddEventType("MyOAType", propsMyOAType, typesMyOAType);
            
            var schema = SchemaBuilder.Record("MyAvroType", TypeBuilder.RequiredInt("intVal"),
                TypeBuilder.RequiredString("stringVal"), TypeBuilder.RequiredDouble("doubleVal"),
                TypeBuilder.Field("nullVal", TypeBuilder.NullType()));
            configuration.Common.AddEventTypeAvro("MyAvroType", new ConfigurationCommonEventTypeAvro(schema));
            
            var xml = new ConfigurationCommonEventTypeXMLDOM();
            xml.RootElementName = "abc";
            configuration.Common.AddEventType("xmltype", xml);
            
            var mapDef = new Dictionary<string, object>();
            mapDef.Put("IntPrimitive", typeof(int));
            mapDef.Put("LongBoxed", typeof(long?));
            mapDef.Put("TheString", typeof(string));
            mapDef.Put("BoolPrimitive", typeof(bool?));
            configuration.Common.AddEventType("MySupportMap", mapDef);

            var type = MakeMap(new object[][] { new object[] { "Id", typeof(string) } });
            configuration.Common.AddEventType("AEventMap", type);
            configuration.Common.AddEventType("BEventMap", type);
            
            var metadata = MakeMap(new object[][] { new object[] { "Id", typeof(string) } });
            configuration.Common.AddEventType("AEventTE", metadata);
            configuration.Common.AddEventType("BEventTE", metadata);
            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));
            configuration.Common.AddImportNamespace(typeof(EPLInsertIntoPopulateUnderlying));
            
            var complexMapMetadata = MakeMap(new object[][]
            {
                new object[] { "Nested", MakeMap(new object[][] { new object[] { "NestedValue", typeof(string) } }) }
            });
            configuration.Common.AddEventType("ComplexMap", complexMapMetadata);
            configuration.Compiler.ByteCode.IsAllowSubscriber = true;
            configuration.Compiler.AddPlugInSingleRowFunction("generateMap", typeof(EPLInsertIntoTransposeStream),
                "LocalGenerateMap");
            configuration.Compiler.AddPlugInSingleRowFunction("generateOA", typeof(EPLInsertIntoTransposeStream),
                "LocalGenerateOA");
            configuration.Compiler.AddPlugInSingleRowFunction("generateAvro", typeof(EPLInsertIntoTransposeStream),
                "LocalGenerateAvro");
            configuration.Compiler.AddPlugInSingleRowFunction("generateJson", typeof(EPLInsertIntoTransposeStream),
                "LocalGenerateJson");
            configuration.Compiler.AddPlugInSingleRowFunction("custom", typeof(SupportStaticMethodLib),
                "MakeSupportBean");
            configuration.Compiler.AddPlugInSingleRowFunction("customOne", typeof(SupportStaticMethodLib),
                "MakeSupportBean");
            configuration.Compiler.AddPlugInSingleRowFunction("customTwo", typeof(SupportStaticMethodLib),
                "MakeSupportBeanNumeric");
        }

        private static IDictionary<string, object> MakeMap(object[][] entries)
        {
            var result = new Dictionary<string, object>();
            foreach (var entry in entries)
            {
                result.Put((string)entry[0], entry[1]);
            }

            return result;
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
            public void WithAssertionWildcardRecast() =>
                RegressionRunner.Run(_session, EPLInsertInto.WithAssertionWildcardRecast());

            [Test, RunInApplicationDomain]
            public void WithJoinWildcard() => RegressionRunner.Run(_session, EPLInsertInto.WithJoinWildcard());

            [Test, RunInApplicationDomain]
            public void WithWithOutputLimitAndSort() =>
                RegressionRunner.Run(_session, EPLInsertInto.WithWithOutputLimitAndSort());

            [Test, RunInApplicationDomain]
            public void WithStaggeredWithWildcard() =>
                RegressionRunner.Run(_session, EPLInsertInto.WithStaggeredWithWildcard());

            [Test, RunInApplicationDomain]
            public void WithInsertFromPattern() =>
                RegressionRunner.Run(_session, EPLInsertInto.WithInsertFromPattern());

            [Test, RunInApplicationDomain]
            public void WithInsertIntoPlusPattern() =>
                RegressionRunner.Run(_session, EPLInsertInto.WithInsertIntoPlusPattern());

            [Test, RunInApplicationDomain]
            public void WithNullType() => RegressionRunner.Run(_session, EPLInsertInto.WithNullType());

            [Test, RunInApplicationDomain]
            public void WithChain() => RegressionRunner.Run(_session, EPLInsertInto.WithChain());

            [Test, RunInApplicationDomain]
            public void WithMultiBeanToMulti() => RegressionRunner.Run(_session, EPLInsertInto.WithMultiBeanToMulti());

            [Test, RunInApplicationDomain]
            public void WithProvidePartitialCols() =>
                RegressionRunner.Run(_session, EPLInsertInto.WithProvidePartitialCols());

            [Test, RunInApplicationDomain]
            public void WithRStreamOMToStmt() => RegressionRunner.Run(_session, EPLInsertInto.WithRStreamOMToStmt());

            [Test, RunInApplicationDomain]
            public void WithNamedColsOMToStmt() =>
                RegressionRunner.Run(_session, EPLInsertInto.WithNamedColsOMToStmt());

            [Test, RunInApplicationDomain]
            public void WithNamedColsEPLToOMStmt() =>
                RegressionRunner.Run(_session, EPLInsertInto.WithNamedColsEPLToOMStmt());

            [Test, RunInApplicationDomain]
            public void WithNamedColsSimple() => RegressionRunner.Run(_session, EPLInsertInto.WithNamedColsSimple());

            [Test, RunInApplicationDomain]
            public void WithNamedColsStateless() =>
                RegressionRunner.Run(_session, EPLInsertInto.WithNamedColsStateless());

            [Test, RunInApplicationDomain]
            public void WithNamedColsWildcard() =>
                RegressionRunner.Run(_session, EPLInsertInto.WithNamedColsWildcard());

            [Test, RunInApplicationDomain]
            public void WithNamedColsJoin() => RegressionRunner.Run(_session, EPLInsertInto.WithNamedColsJoin());

            [Test, RunInApplicationDomain]
            public void WithNamedColsJoinWildcard() =>
                RegressionRunner.Run(_session, EPLInsertInto.WithNamedColsJoinWildcard());

            [Test, RunInApplicationDomain]
            public void WithUnnamedSimple() => RegressionRunner.Run(_session, EPLInsertInto.WithUnnamedSimple());

            [Test, RunInApplicationDomain]
            public void WithUnnamedWildcard() => RegressionRunner.Run(_session, EPLInsertInto.WithUnnamedWildcard());

            [Test, RunInApplicationDomain]
            public void WithUnnamedJoin() => RegressionRunner.Run(_session, EPLInsertInto.WithUnnamedJoin());

            [Test, RunInApplicationDomain]
            public void WithTypeMismatchInvalid() =>
                RegressionRunner.Run(_session, EPLInsertInto.WithTypeMismatchInvalid());

            [Test, RunInApplicationDomain]
            public void WithEventRepresentationsSimple() =>
                RegressionRunner.Run(_session, EPLInsertInto.WithEventRepresentationsSimple());

            [Test, RunInApplicationDomain]
            public void WithLenientPropCount() => RegressionRunner.Run(_session, EPLInsertInto.WithLenientPropCount());
        }

        /// <summary>
        /// Auto-test(s): EPLInsertIntoEmptyPropType
        /// <code>
        /// RegressionRunner.Run(_session, EPLInsertIntoEmptyPropType.Executions());
        /// </code>
        /// </summary>
        public class TestEPLInsertIntoEmptyPropType : AbstractTestBase
        {
            public TestEPLInsertIntoEmptyPropType() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithNamedWindowModelAfter() =>
                RegressionRunner.Run(_session, EPLInsertIntoEmptyPropType.WithNamedWindowModelAfter());

            [Test, RunInApplicationDomain]
            public void WithCreateSchemaInsertInto() =>
                RegressionRunner.Run(_session, EPLInsertIntoEmptyPropType.WithCreateSchemaInsertInto());
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
            public void WithCompatExisting() =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateCreateStreamAvro.WithCompatExisting());

            [Test, RunInApplicationDomain]
            public void WithNewSchema() =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateCreateStreamAvro.WithNewSchema());
        }

        /// <summary>
        /// Auto-test(s): EPLInsertIntoPopulateEventTypeColumnBean
        /// <code>
        /// RegressionRunner.Run(_session, EPLInsertIntoPopulateEventTypeColumnBean.Executions());
        /// </code>
        /// </summary>
        public class TestEPLInsertIntoPopulateEventTypeColumnBean : AbstractTestBase
        {
            public TestEPLInsertIntoPopulateEventTypeColumnBean() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithFromSubquerySingle() => RegressionRunner.Run(_session,
                EPLInsertIntoPopulateEventTypeColumnBean.WithFromSubquerySingle());

            [Test, RunInApplicationDomain]
            public void WithFromSubqueryMulti() => RegressionRunner.Run(_session,
                EPLInsertIntoPopulateEventTypeColumnBean.WithFromSubqueryMulti());

            [Test, RunInApplicationDomain]
            public void WithSingleToMulti() =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateEventTypeColumnBean.WithSingleToMulti());

            [Test, RunInApplicationDomain]
            public void WithMultiToSingle() =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateEventTypeColumnBean.WithMultiToSingle());

            [Test, RunInApplicationDomain]
            public void WithInvalid() =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateEventTypeColumnBean.WithInvalid());
        }

        /// <summary>
        /// Auto-test(s): EPLInsertIntoPopulateEventTypeColumnNonBean
        /// <code>
        /// RegressionRunner.Run(_session, EPLInsertIntoPopulateEventTypeColumnNonBean.Executions());
        /// </code>
        /// </summary>
        public class TestEPLInsertIntoPopulateEventTypeColumnNonBean : AbstractTestBase
        {
            public TestEPLInsertIntoPopulateEventTypeColumnNonBean() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithFromSubquerySingle() => RegressionRunner.Run(_session,
                EPLInsertIntoPopulateEventTypeColumnNonBean.WithFromSubquerySingle());

            [Test, RunInApplicationDomain]
            public void WithFromSubqueryMulti() => RegressionRunner.Run(_session,
                EPLInsertIntoPopulateEventTypeColumnNonBean.WithFromSubqueryMulti());

            [Test, RunInApplicationDomain]
            public void WithFromSubqueryMultiFilter() => RegressionRunner.Run(_session,
                EPLInsertIntoPopulateEventTypeColumnNonBean.WithFromSubqueryMultiFilter());

            [Test, RunInApplicationDomain]
            public void WithNewOperatorDocSample() => RegressionRunner.Run(_session,
                EPLInsertIntoPopulateEventTypeColumnNonBean.WithNewOperatorDocSample());

            [Test, RunInApplicationDomain]
            public void WithCaseNew() =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateEventTypeColumnNonBean.WithCaseNew());

            [Test, RunInApplicationDomain]
            public void WithSingleColNamedWindow() => RegressionRunner.Run(_session,
                EPLInsertIntoPopulateEventTypeColumnNonBean.WithSingleColNamedWindow());

            [Test, RunInApplicationDomain]
            public void WithSingleToMulti() => RegressionRunner.Run(_session,
                EPLInsertIntoPopulateEventTypeColumnNonBean.WithSingleToMulti());

            [Test, RunInApplicationDomain]
            public void WithBeanInvalid() =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateEventTypeColumnNonBean.WithBeanInvalid());
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
            public void WithCtor() => RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithCtor());

            [Test, RunInApplicationDomain]
            public void WithCtorWithPattern() =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithCtorWithPattern());

            [Test, RunInApplicationDomain]
            public void WithBeanJoin() =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithBeanJoin());

            [Test, RunInApplicationDomain]
            public void WithPopulateBeanSimple() =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithPopulateBeanSimple());

            [Test, RunInApplicationDomain]
            public void WithBeanWildcard() =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithBeanWildcard());

            [Test, RunInApplicationDomain]
            public void WithPopulateBeanObjects() =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithPopulateBeanObjects());

            [Test, RunInApplicationDomain]
            public void WithPopulateUnderlyingSimple() => RegressionRunner.Run(_session,
                EPLInsertIntoPopulateUnderlying.WithPopulateUnderlyingSimple());

            [Test, RunInApplicationDomain]
            public void WithCharSequenceCompat([Values] EventRepresentationChoice rep) =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithCharSequenceCompat(rep));

            [Test, RunInApplicationDomain]
            public void WithBeanFactoryMethod() =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithBeanFactoryMethod());

            [Test, RunInApplicationDomain]
            public void WithArrayPONOInsert() =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithArrayPONOInsert());

            [Test, RunInApplicationDomain]
            public void WithArrayMapInsert() =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithArrayMapInsert());

            [Test, RunInApplicationDomain]
            public void WithWindowAggregationAtEventBean() => RegressionRunner.Run(_session,
                EPLInsertIntoPopulateUnderlying.WithWindowAggregationAtEventBean());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLInsertIntoPopulateUnderlying.WithInvalid());
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
            public void WithNamedWindowInheritsMap() => RegressionRunner.Run(_session,
                EPLInsertIntoPopulateUndStreamSelect.WithNamedWindowInheritsMap());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowRep([Values] EventRepresentationChoice rep) =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateUndStreamSelect.WithNamedWindowRep(rep));

            [Test, RunInApplicationDomain]
            public void WithStreamInsertWWidenOA([Values] EventRepresentationChoice rep) => RegressionRunner
                .Run(_session, EPLInsertIntoPopulateUndStreamSelect.WithStreamInsertWWidenOA(rep));

            [Test, RunInApplicationDomain]
            public void WithInvalid([Values] EventRepresentationChoice rep) =>
                RegressionRunner.Run(_session, EPLInsertIntoPopulateUndStreamSelect.WithInvalid(rep));
        }

        /// <summary>
        /// Auto-test(s): EPLInsertIntoTransposePattern
        /// <code>
        /// RegressionRunner.Run(_session, EPLInsertIntoTransposePattern.Executions());
        /// </code>
        /// </summary>
        public class TestEPLInsertIntoTransposePattern : AbstractTestBase
        {
            public TestEPLInsertIntoTransposePattern() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithThisAsColumn() =>
                RegressionRunner.Run(_session, EPLInsertIntoTransposePattern.WithThisAsColumn());

            [Test, RunInApplicationDomain]
            public void WithTransposePONOEventPattern() => RegressionRunner.Run(_session,
                EPLInsertIntoTransposePattern.WithTransposePONOEventPattern());

            [Test, RunInApplicationDomain]
            public void WithTransposeMapEventPattern() => RegressionRunner.Run(_session,
                EPLInsertIntoTransposePattern.WithTransposeMapEventPattern());
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
            public void WithTransposeCreateSchemaPONO() => RegressionRunner.Run(_session,
                EPLInsertIntoTransposeStream.WithTransposeCreateSchemaPONO());

            [Test, RunInApplicationDomain]
            public void WithTransposeMapAndObjectArrayAndOthers() => RegressionRunner.Run(_session,
                EPLInsertIntoTransposeStream.WithTransposeMapAndObjectArrayAndOthers());

            [Test, RunInApplicationDomain]
            public void WithTransposeFunctionToStreamWithProps() => RegressionRunner.Run(_session,
                EPLInsertIntoTransposeStream.WithTransposeFunctionToStreamWithProps());

            [Test, RunInApplicationDomain]
            public void WithTransposeFunctionToStream() => RegressionRunner.Run(_session,
                EPLInsertIntoTransposeStream.WithTransposeFunctionToStream());

            [Test, RunInApplicationDomain]
            public void WithTransposeSingleColumnInsert() => RegressionRunner.Run(_session,
                EPLInsertIntoTransposeStream.WithTransposeSingleColumnInsert());

            [Test, RunInApplicationDomain]
            public void WithTransposeSingleColumnInsertInvalid() => RegressionRunner.Run(_session,
                EPLInsertIntoTransposeStream.WithTransposeSingleColumnInsertInvalid());

            [Test, RunInApplicationDomain]
            public void WithTransposeEventJoinMap() =>
                RegressionRunner.Run(_session, EPLInsertIntoTransposeStream.WithTransposeEventJoinMap());

            [Test, RunInApplicationDomain]
            public void WithTransposeEventJoinPONO() =>
                RegressionRunner.Run(_session, EPLInsertIntoTransposeStream.WithTransposeEventJoinPONO());

            [Test, RunInApplicationDomain]
            public void WithTransposePONOPropertyStream() => RegressionRunner.Run(_session,
                EPLInsertIntoTransposeStream.WithTransposePONOPropertyStream());

            [Test, RunInApplicationDomain]
            public void WithInvalidTranspose() =>
                RegressionRunner.Run(_session, EPLInsertIntoTransposeStream.WithInvalidTranspose());
        }

        /// <summary>
        /// Auto-test(s): EPLInsertIntoFromPattern
        /// <code>
        /// RegressionRunner.Run(_session, EPLInsertIntoFromPattern.Executions());
        /// </code>
        /// </summary>
        public class TestEPLInsertIntoFromPattern : AbstractTestBase
        {
            public TestEPLInsertIntoFromPattern() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithPropsWildcard() =>
                RegressionRunner.Run(_session, EPLInsertIntoFromPattern.WithPropsWildcard());

            [Test, RunInApplicationDomain]
            public void WithProps() => RegressionRunner.Run(_session, EPLInsertIntoFromPattern.WithProps());

            [Test, RunInApplicationDomain]
            public void WithNoProps() => RegressionRunner.Run(_session, EPLInsertIntoFromPattern.WithNoProps());

            [Test, RunInApplicationDomain]
            public void WithFromPatternNamedWindow() =>
                RegressionRunner.Run(_session, EPLInsertIntoFromPattern.WithFromPatternNamedWindow());
        }

        /// <summary>
        /// Auto-test(s): EPLInsertIntoWrapper
        /// <code>
        /// RegressionRunner.Run(_session, EPLInsertIntoWrapper.Executions());
        /// </code>
        /// </summary>
        public class TestEPLInsertIntoWrapper : AbstractTestBase
        {
            public TestEPLInsertIntoWrapper() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWrapperBean() => RegressionRunner.Run(_session, EPLInsertIntoWrapper.WithWrapperBean());

            [Test, RunInApplicationDomain]
            public void With3StreamWrapper() =>
                RegressionRunner.Run(_session, EPLInsertIntoWrapper.With3StreamWrapper());

            [Test, RunInApplicationDomain]
            public void WithOnSplitForkJoin() =>
                RegressionRunner.Run(_session, EPLInsertIntoWrapper.WithOnSplitForkJoin());
        }

        /// <summary>
        /// Auto-test(s): EPLInsertIntoEventTypedColumnFromProp
        /// <code>
        /// RegressionRunner.Run(_session, EPLInsertIntoEventTypedColumnFromProp.Executions());
        /// </code>
        /// </summary>
        public class TestEPLInsertIntoEventTypedColumnFromProp : AbstractTestBase
        {
            public TestEPLInsertIntoEventTypedColumnFromProp() : base(Configure)
            {
            }
        }
    }
} // end of namespace
