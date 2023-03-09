///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.regressionlib.suite.infra.namedwindow;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.bookexample;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.util.CollectionUtil;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.infra
{
    // see INFRA suite for additional Named Window tests
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class TestSuiteInfraNamedWindow : AbstractTestBase
    {
        public static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {
                typeof(SupportBean),
                typeof(OrderBean),
                typeof(OrderWithItems),
                typeof(SupportBeanAtoFBase),
                typeof(SupportBean_A),
                typeof(SupportMarketDataBean),
                typeof(SupportSimpleBeanTwo),
                typeof(SupportSimpleBeanOne),
                typeof(SupportVariableSetEvent),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportBeanRange),
                typeof(SupportBean_B),
                typeof(SupportOverrideOneA),
                typeof(SupportOverrideOne),
                typeof(SupportOverrideBase),
                typeof(SupportQueueEnter),
                typeof(SupportQueueLeave),
                typeof(SupportBeanAtoFBase),
                typeof(SupportBeanAbstractSub),
                typeof(SupportBean_ST0),
                typeof(SupportBeanTwo),
                typeof(SupportCountAccessEvent),
                typeof(BookDesc),
                typeof(SupportBean_Container),
                typeof(SupportEventWithManyArray),
                typeof(SupportEventWithIntArray)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            IDictionary<string, object> outerMapInnerType = new Dictionary<string, object>();
            outerMapInnerType.Put("key", typeof(string));
            configuration.Common.AddEventType("InnerMap", outerMapInnerType);
            IDictionary<string, object> outerMap = new Dictionary<string, object>();
            outerMap.Put("innermap", "InnerMap");
            configuration.Common.AddEventType("OuterMap", outerMap);

            IDictionary<string, object> typesSimpleKeyValue = new Dictionary<string, object>();
            typesSimpleKeyValue.Put("key", typeof(string));
            typesSimpleKeyValue.Put("value", typeof(long));
            configuration.Common.AddEventType("MySimpleKeyValueMap", typesSimpleKeyValue);

            IDictionary<string, object> innerTypeOne = new Dictionary<string, object>();
            innerTypeOne.Put("i1", typeof(int));
            IDictionary<string, object> innerTypeTwo = new Dictionary<string, object>();
            innerTypeTwo.Put("i2", typeof(int));
            IDictionary<string, object> outerType = new Dictionary<string, object>();
            outerType.Put("one", "T1");
            outerType.Put("two", "T2");
            configuration.Common.AddEventType("T1", innerTypeOne);
            configuration.Common.AddEventType("T2", innerTypeTwo);
            configuration.Common.AddEventType("OuterType", outerType);

            IDictionary<string, object> types = new Dictionary<string, object>();
            types.Put("key", typeof(string));
            types.Put("primitive", typeof(long));
            types.Put("boxed", typeof(long?));
            configuration.Common.AddEventType("MyMapWithKeyPrimitiveBoxed", types);

            var dataType = BuildMap(
                new object[][] {
                    new object[] {"a", typeof(string)},
                    new object[] {"b", typeof(int)}
                });
            configuration.Common.AddEventType("MyMapAB", dataType);

            var legacy = new ConfigurationCommonEventTypeBean();
            legacy.CopyMethod = "MyCopyMethod";
            configuration.Common.AddEventType("SupportBeanCopyMethod", typeof(SupportBeanCopyMethod), legacy);

            configuration.Common.AddEventType("SimpleEventWithId", new string[]{"id"}, new object[]{ typeof(string) });

            configuration.Compiler.AddPlugInSingleRowFunction(
                "setBeanLongPrimitive999",
                typeof(InfraNamedWindowOnUpdate),
                "SetBeanLongPrimitive999");
            configuration.Compiler.AddPlugInSingleRowFunction(
                "increaseIntCopyDouble",
                typeof(InfraNamedWindowOnMerge),
                "IncreaseIntCopyDouble");

            var config = new ConfigurationCommonVariantStream();
            config.AddEventTypeName("SupportBean_A");
            config.AddEventTypeName("SupportBean_B");
            configuration.Common.AddVariantStream("VarStream", config);

            configuration.Common.Logging.IsEnableQueryPlan = true;
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowConsumer()
        {
            RegressionRunner.Run(_session, InfraNamedWindowConsumer.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowContainedEvent()
        {
            RegressionRunner.Run(_session, new InfraNamedWindowContainedEvent());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowIndex()
        {
            RegressionRunner.Run(_session, new InfraNamedWindowIndex());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowLateStartIndex()
        {
            RegressionRunner.Run(_session, new InfraNamedWindowLateStartIndex());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowOM()
        {
            RegressionRunner.Run(_session, InfraNamedWindowOM.Executions());
        }


        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowOnSelect()
        {
            RegressionRunner.Run(_session, InfraNamedWindowOnSelect.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowOutputrate()
        {
            RegressionRunner.Run(_session, new InfraNamedWindowOutputrate());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowRemoveStream()
        {
            RegressionRunner.Run(_session, new InfraNamedWindowRemoveStream());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowSubquery()
        {
            RegressionRunner.Run(_session, InfraNamedWindowSubquery.Executions());
        }

        /// <summary>
        /// Auto-test(s): InfraNamedWindowOnDelete
        /// <code>
        /// RegressionRunner.Run(_session, InfraNamedWindowOnDelete.Executions());
        /// </code>
        /// </summary>

        public class TestInfraNamedWindowOnDelete : AbstractTestBase
        {
            public TestInfraNamedWindowOnDelete() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithNamedWindowSilentDeleteOnDeleteMany() => RegressionRunner.Run(
                _session,
                InfraNamedWindowOnDelete.WithNamedWindowSilentDeleteOnDeleteMany());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowSilentDeleteOnDelete() => RegressionRunner.Run(
                _session,
                InfraNamedWindowOnDelete.WithNamedWindowSilentDeleteOnDelete());

            [Test, RunInApplicationDomain]
            public void WithCoercionKeyAndRangeMultiPropIndexes() => RegressionRunner.Run(
                _session,
                InfraNamedWindowOnDelete.WithCoercionKeyAndRangeMultiPropIndexes());

            [Test, RunInApplicationDomain]
            public void WithCoercionRangeMultiPropIndexes() => RegressionRunner.Run(
                _session,
                InfraNamedWindowOnDelete.WithCoercionRangeMultiPropIndexes());

            [Test, RunInApplicationDomain]
            public void WithCoercionKeyMultiPropIndexes() => RegressionRunner.Run(
                _session,
                InfraNamedWindowOnDelete.WithCoercionKeyMultiPropIndexes());

            [Test, RunInApplicationDomain]
            public void WithStaggeredNamedWindow() => RegressionRunner.Run(
                _session,
                InfraNamedWindowOnDelete.WithStaggeredNamedWindow());

            [Test, RunInApplicationDomain]
            public void WithFirstUnique() => RegressionRunner.Run(
                _session,
                InfraNamedWindowOnDelete.WithFirstUnique());
        }
        
        /// <summary>
        /// Auto-test(s): InfraNamedWindowOnMerge
        /// <code>
        /// RegressionRunner.Run(_session, InfraNamedWindowOnMerge.Executions());
        /// </code>
        /// </summary>

        public class TestInfraNamedWindowOnMerge : AbstractTestBase
        {
            public TestInfraNamedWindowOnMerge() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithOnMergeSetRHSEvent() => RegressionRunner.Run(_session, InfraNamedWindowOnMerge.WithOnMergeSetRHSEvent());

            [Test, RunInApplicationDomain]
            public void WithOnMergeNoWhereClauseInsertTranspose() => RegressionRunner.Run(_session, InfraNamedWindowOnMerge.WithOnMergeNoWhereClauseInsertTranspose());

            [Test, RunInApplicationDomain]
            public void WithOnMergeNoWhereClauseInsertSelectStar() => RegressionRunner.Run(_session, InfraNamedWindowOnMerge.WithOnMergeNoWhereClauseInsertSelectStar());

            [Test, RunInApplicationDomain]
            public void WithOnMergeWhere1Eq2InsertSelectStar() => RegressionRunner.Run(_session, InfraNamedWindowOnMerge.WithOnMergeWhere1Eq2InsertSelectStar());

            [Test, RunInApplicationDomain]
            public void WithDocExample() => RegressionRunner.Run(_session, InfraNamedWindowOnMerge.WithDocExample());

            [Test, RunInApplicationDomain]
            public void WithSubselect() => RegressionRunner.Run(_session, InfraNamedWindowOnMerge.WithSubselect());

            [Test, RunInApplicationDomain]
            public void WithPropertyInsertBean() => RegressionRunner.Run(_session, InfraNamedWindowOnMerge.WithPropertyInsertBean());

            [Test, RunInApplicationDomain]
            public void WithMergeTriggeredByAnotherWindow() => RegressionRunner.Run(_session, InfraNamedWindowOnMerge.WithMergeTriggeredByAnotherWindow());

            [Test, RunInApplicationDomain]
            public void WithUpdateNonPropertySet() => RegressionRunner.Run(_session, InfraNamedWindowOnMerge.WithUpdateNonPropertySet());
        }

        /// <summary>
        /// Auto-test(s): InfraNamedWindowTypes
        /// <code>
        /// RegressionRunner.Run(_session, InfraNamedWindowTypes.Executions());
        /// </code>
        /// </summary>

        public class TestInfraNamedWindowTypes : AbstractTestBase
        {
            public TestInfraNamedWindowTypes() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchemaModelAfter() => RegressionRunner.Run(_session, InfraNamedWindowTypes.WithCreateSchemaModelAfter());

            [Test, RunInApplicationDomain]
            public void WithEventTypeColumnDef() => RegressionRunner.Run(_session, InfraNamedWindowTypes.WithEventTypeColumnDef());

            [Test, RunInApplicationDomain]
            public void WithCreateTableArray() => RegressionRunner.Run(_session, InfraNamedWindowTypes.WithCreateTableArray());

            [Test, RunInApplicationDomain]
            public void WithWildcardWithFields() => RegressionRunner.Run(_session, InfraNamedWindowTypes.WithWildcardWithFields());

            [Test, RunInApplicationDomain]
            public void WithNoSpecificationBean() => RegressionRunner.Run(_session, InfraNamedWindowTypes.WithNoSpecificationBean());

            [Test, RunInApplicationDomain]
            public void WithWildcardInheritance() => RegressionRunner.Run(_session, InfraNamedWindowTypes.WithWildcardInheritance());

            [Test, RunInApplicationDomain]
            public void WithModelAfterMap() => RegressionRunner.Run(_session, InfraNamedWindowTypes.WithModelAfterMap());

            [Test, RunInApplicationDomain]
            public void WithWildcardNoFieldsNoAs() => RegressionRunner.Run(_session, InfraNamedWindowTypes.WithWildcardNoFieldsNoAs());

            [Test, RunInApplicationDomain]
            public void WithCreateTableSyntax() => RegressionRunner.Run(_session, InfraNamedWindowTypes.WithCreateTableSyntax());

            [Test, RunInApplicationDomain]
            public void WithConstantsAs() => RegressionRunner.Run(_session, InfraNamedWindowTypes.WithConstantsAs());

            [Test, RunInApplicationDomain]
            public void WithNoWildcardNoAs() => RegressionRunner.Run(_session, InfraNamedWindowTypes.WithNoWildcardNoAs());

            [Test, RunInApplicationDomain]
            public void WithNoWildcardWithAs() => RegressionRunner.Run(_session, InfraNamedWindowTypes.WithNoWildcardWithAs());

            [Test, RunInApplicationDomain]
            public void WithMapTranspose() => RegressionRunner.Run(_session, InfraNamedWindowTypes.WithMapTranspose());
        }
        
        /// <summary>
        /// Auto-test(s): InfraNamedWindowInsertFrom
        /// <code>
        /// RegressionRunner.Run(_session, InfraNamedWindowInsertFrom.Executions());
        /// </code>
        /// </summary>

        public class TestInfraNamedWindowInsertFrom : AbstractTestBase
        {
            public TestInfraNamedWindowInsertFrom() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithVariantStream() => RegressionRunner.Run(_session, InfraNamedWindowInsertFrom.WithVariantStream());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, InfraNamedWindowInsertFrom.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithInsertWhereOMStaggered() => RegressionRunner.Run(_session, InfraNamedWindowInsertFrom.WithInsertWhereOMStaggered());

            [Test, RunInApplicationDomain]
            public void WithInsertWhereTypeAndFilter() => RegressionRunner.Run(_session, InfraNamedWindowInsertFrom.WithInsertWhereTypeAndFilter());

            [Test, RunInApplicationDomain]
            public void WithCreateNamedAfterNamed() => RegressionRunner.Run(_session, InfraNamedWindowInsertFrom.WithCreateNamedAfterNamed());
        }
        
        /// <summary>
        /// Auto-test(s): InfraNamedWindowOnUpdate
        /// <code>
        /// RegressionRunner.Run(_session, InfraNamedWindowOnUpdate.Executions());
        /// </code>
        /// </summary>

        public class TestInfraNamedWindowOnUpdate : AbstractTestBase
        {
            public TestInfraNamedWindowOnUpdate() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithUpdateMultikeyWArrayTwoFields() => RegressionRunner.Run(_session, InfraNamedWindowOnUpdate.WithUpdateMultikeyWArrayTwoFields());

            [Test, RunInApplicationDomain]
            public void WithUpdateMultikeyWArrayPrimitiveArray() => RegressionRunner.Run(_session, InfraNamedWindowOnUpdate.WithUpdateMultikeyWArrayPrimitiveArray());

            [Test, RunInApplicationDomain]
            public void WithUpdateWrapper() => RegressionRunner.Run(_session, InfraNamedWindowOnUpdate.WithUpdateWrapper());

            [Test, RunInApplicationDomain]
            public void WithUpdateCopyMethodBean() => RegressionRunner.Run(_session, InfraNamedWindowOnUpdate.WithUpdateCopyMethodBean());

            [Test, RunInApplicationDomain]
            public void WithSubclass() => RegressionRunner.Run(_session, InfraNamedWindowOnUpdate.WithSubclass());

            [Test, RunInApplicationDomain]
            public void WithMultipleDataWindowUnion() => RegressionRunner.Run(_session, InfraNamedWindowOnUpdate.WithMultipleDataWindowUnion());

            [Test, RunInApplicationDomain]
            public void WithMultipleDataWindowIntersect() => RegressionRunner.Run(_session, InfraNamedWindowOnUpdate.WithMultipleDataWindowIntersect());

            [Test, RunInApplicationDomain]
            public void WithUpdateNonPropertySet() => RegressionRunner.Run(_session, InfraNamedWindowOnUpdate.WithUpdateNonPropertySet());
        }
        
        /// <summary>
        /// Auto-test(s): InfraNamedWindowProcessingOrder
        /// <code>
        /// RegressionRunner.Run(_session, InfraNamedWindowProcessingOrder.Executions());
        /// </code>
        /// </summary>

        public class TestInfraNamedWindowProcessingOrder : AbstractTestBase
        {
            public TestInfraNamedWindowProcessingOrder() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithOrderedDeleteAndSelect() => RegressionRunner.Run(_session, InfraNamedWindowProcessingOrder.WithOrderedDeleteAndSelect());

            [Test, RunInApplicationDomain]
            public void WithDispatchBackQueue() => RegressionRunner.Run(_session, InfraNamedWindowProcessingOrder.WithDispatchBackQueue());
        }

        /// <summary>
        /// Auto-test(s): InfraNamedWindowViews
        /// <code>
        /// RegressionRunner.Run(_session, InfraNamedWindowViews.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNamedWindowViews : AbstractTestBase
        {
            public TestInfraNamedWindowViews() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithOnInsertPremptiveTwoWindow() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithOnInsertPremptiveTwoWindow());

            [Test, RunInApplicationDomain]
            public void WithSelectGroupedViewLateStartVariableIterate() => RegressionRunner.Run(
                _session,
                InfraNamedWindowViews.WithSelectGroupedViewLateStartVariableIterate());

            [Test, RunInApplicationDomain]
            public void WithSelectStreamDotStarInsert() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithSelectStreamDotStarInsert());

            [Test, RunInApplicationDomain]
            public void WithExternallyTimedBatch() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithExternallyTimedBatch());

            [Test, RunInApplicationDomain]
            public void WithPattern() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithPattern());

            [Test, RunInApplicationDomain]
            public void WithLateConsumerJoin() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithLateConsumerJoin());

            [Test, RunInApplicationDomain]
            public void WithLateConsumer() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithLateConsumer());

            [Test, RunInApplicationDomain]
            public void WithPriorStats() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithPriorStats());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowInvalidConsumerDataWindow() => RegressionRunner.Run(
                _session,
                InfraNamedWindowViews.WithNamedWindowInvalidConsumerDataWindow());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowInvalidAlreadyExists() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithNamedWindowInvalidAlreadyExists());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithFilteringConsumerLateStart() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithFilteringConsumerLateStart());

            [Test, RunInApplicationDomain]
            public void WithSelectGroupedViewLateStart() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithSelectGroupedViewLateStart());

            [Test, RunInApplicationDomain]
            public void WithFilteringConsumer() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithFilteringConsumer());

            [Test, RunInApplicationDomain]
            public void WithWithDeleteNoAs() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithWithDeleteNoAs());

            [Test, RunInApplicationDomain]
            public void WithWithDeleteSecondAs() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithWithDeleteSecondAs());

            [Test, RunInApplicationDomain]
            public void WithWithDeleteFirstAs() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithWithDeleteFirstAs());

            [Test, RunInApplicationDomain]
            public void WithWithDeleteUseAs() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithWithDeleteUseAs());

            [Test, RunInApplicationDomain]
            public void WithDeepSupertypeInsert() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithDeepSupertypeInsert());

            [Test, RunInApplicationDomain]
            public void WithBeanSchemaBacked() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithBeanSchemaBacked());

            [Test, RunInApplicationDomain]
            public void WithIntersection() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithIntersection());

            [Test, RunInApplicationDomain]
            public void WithBeanContained() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithBeanContained());

            [Test, RunInApplicationDomain]
            public void WithFirstUnique() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithFirstUnique());

            [Test, RunInApplicationDomain]
            public void WithUniqueSceneTwo() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithUniqueSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithUnique() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithUnique());

            [Test, RunInApplicationDomain]
            public void WithFirstEvent() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithFirstEvent());

            [Test, RunInApplicationDomain]
            public void WithLastEventSceneTwo() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithLastEventSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithLastEvent() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithLastEvent());

            [Test, RunInApplicationDomain]
            public void WithDoubleInsertSameWindow() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithDoubleInsertSameWindow());

            [Test, RunInApplicationDomain]
            public void WithTimeBatchPerGroup() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithTimeBatchPerGroup());

            [Test, RunInApplicationDomain]
            public void WithLengthWindowPerGroup() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithLengthWindowPerGroup());

            [Test, RunInApplicationDomain]
            public void WithLengthWindowSceneThree() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithLengthWindowSceneThree());

            [Test, RunInApplicationDomain]
            public void WithTimeLengthBatchSceneTwo() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithTimeLengthBatchSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithTimeLengthBatch() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithTimeLengthBatch());

            [Test, RunInApplicationDomain]
            public void WithSortWindowSceneTwo() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithSortWindowSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithSortWindow() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithSortWindow());

            [Test, RunInApplicationDomain]
            public void WithLengthBatchSceneTwo() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithLengthBatchSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithLengthBatch() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithLengthBatch());

            [Test, RunInApplicationDomain]
            public void WithTimeBatchLateConsumer() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithTimeBatchLateConsumer());

            [Test, RunInApplicationDomain]
            public void WithTimeBatchSceneTwo() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithTimeBatchSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithTimeBatch() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithTimeBatch());

            [Test, RunInApplicationDomain]
            public void WithTimeAccumSceneTwo() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithTimeAccumSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithTimeAccum() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithTimeAccum());

            [Test, RunInApplicationDomain]
            public void WithLengthFirstWindow() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithLengthFirstWindow());

            [Test, RunInApplicationDomain]
            public void WithLengthWindowSceneTwo() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithLengthWindowSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithLengthWindow() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithLengthWindow());

            [Test, RunInApplicationDomain]
            public void WithTimeOrderSceneTwo() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithTimeOrderSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithTimeOrderWindow() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithTimeOrderWindow());

            [Test, RunInApplicationDomain]
            public void WithExtTimeWindowSceneThree() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithExtTimeWindowSceneThree());

            [Test, RunInApplicationDomain]
            public void WithExtTimeWindowSceneTwo() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithExtTimeWindowSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithExtTimeWindow() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithExtTimeWindow());

            [Test, RunInApplicationDomain]
            public void WithTimeFirstWindow() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithTimeFirstWindow());

            [Test, RunInApplicationDomain]
            public void WithTimeWindowSceneTwo() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithTimeWindowSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithTimeWindow() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithTimeWindow());

            [Test, RunInApplicationDomain]
            public void WithBeanBacked() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithBeanBacked());

            [Test, RunInApplicationDomain]
            public void WithKeepAllSceneTwo() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithKeepAllSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithKeepAllSimple() => RegressionRunner.Run(_session, InfraNamedWindowViews.WithKeepAllSimple());
        }
        
        /// <summary>
        /// Auto-test(s): InfraNamedWindowJoin
        /// <code>
        /// RegressionRunner.Run(_session, InfraNamedWindowJoin.Executions());
        /// </code>
        /// </summary>

        public class TestInfraNamedWindowJoin : AbstractTestBase
        {
            public TestInfraNamedWindowJoin() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithInnerJoinLateStart() => RegressionRunner.Run(_session, InfraNamedWindowJoin.WithInnerJoinLateStart());

            [Test, RunInApplicationDomain]
            public void WithWindowUnidirectionalJoin() => RegressionRunner.Run(_session, InfraNamedWindowJoin.WithWindowUnidirectionalJoin());

            [Test, RunInApplicationDomain]
            public void WithUnidirectional() => RegressionRunner.Run(_session, InfraNamedWindowJoin.WithUnidirectional());

            [Test, RunInApplicationDomain]
            public void WithJoinSingleInsertOneWindow() => RegressionRunner.Run(_session, InfraNamedWindowJoin.WithJoinSingleInsertOneWindow());

            [Test, RunInApplicationDomain]
            public void WithJoinBetweenSameNamed() => RegressionRunner.Run(_session, InfraNamedWindowJoin.WithJoinBetweenSameNamed());

            [Test, RunInApplicationDomain]
            public void WithJoinBetweenNamed() => RegressionRunner.Run(_session, InfraNamedWindowJoin.WithJoinBetweenNamed());

            [Test, RunInApplicationDomain]
            public void WithJoinNamedAndStream() => RegressionRunner.Run(_session, InfraNamedWindowJoin.WithJoinNamedAndStream());

            [Test, RunInApplicationDomain]
            public void WithFullOuterJoinNamedAggregationLateStart() => RegressionRunner.Run(_session, InfraNamedWindowJoin.WithFullOuterJoinNamedAggregationLateStart());

            [Test, RunInApplicationDomain]
            public void WithRightOuterJoinLateStart() => RegressionRunner.Run(_session, InfraNamedWindowJoin.WithRightOuterJoinLateStart());

            [Test, RunInApplicationDomain]
            public void WithJoinIndexChoice() => RegressionRunner.Run(_session, InfraNamedWindowJoin.WithJoinIndexChoice());
        }
    }
} // end of namespace