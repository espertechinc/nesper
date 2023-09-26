///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.ontrigger.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.join.hint;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.ontrigger;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.epl.updatehelper;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;


namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.ontrigger
{
    public class OnTriggerWindowUtil
    {
        private static readonly ILog QUERY_PLAN_LOG = LogManager.GetLogger(AuditPath.QUERYPLAN_LOG);

        public static OnTriggerPlan HandleContextFactoryOnTrigger(
            string className,
            CodegenNamespaceScope namespaceScope,
            string classPostfix,
            NamedWindowMetaData namedWindow,
            TableMetaData table,
            OnTriggerWindowPlan planDesc,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            // validate context
            var infraName = planDesc.OnTriggerDesc.WindowName;
            var infraTitle = (namedWindow != null ? "Named window" : "Table") + " '" + infraName + "'";
            var infraContextName = namedWindow != null ? namedWindow.ContextName : table.OptionalContextName;
            var infraModuleName = namedWindow != null ? namedWindow.NamedWindowModuleName : table.TableModuleName;
            var infraEventType = namedWindow != null ? namedWindow.EventType : table.InternalEventType;
            var resultEventType = namedWindow != null ? namedWindow.EventType : table.PublicEventType;
            var infraVisibility = namedWindow != null
                ? namedWindow.EventType.Metadata.AccessModifier
                : table.TableVisibility;
            ValidateOnExpressionContext(planDesc.ContextName, infraContextName, infraTitle);
            IList<StmtClassForgeableFactory> additionalForgeables = new List<StmtClassForgeableFactory>(1);
            var fabricCharge = services.StateMgmtSettingsProvider.NewCharge();

            // validate expressions and plan subselects
            var validationResult = OnTriggerPlanValidator.ValidateOnTriggerPlan(
                infraEventType,
                planDesc.OnTriggerDesc,
                planDesc.StreamSpec,
                planDesc.ActivatorResult,
                planDesc.SubselectActivation,
                @base,
                services);
            additionalForgeables.AddAll(validationResult.AdditionalForgeables);
            fabricCharge.Add(validationResult.FabricCharge);

            var validatedJoin = validationResult.ValidatedJoin;
            var activatorResultEventType = planDesc.ActivatorResult.ActivatorResultEventType;

            var pair = IndexHintPair.GetIndexHintPair(
                planDesc.OnTriggerDesc,
                @base.StatementSpec.StreamSpecs[0].OptionalStreamName,
                @base.StatementRawInfo,
                services);
            var indexHint = pair.IndexHint;
            var excludePlanHint = pair.ExcludePlanHint;

            var enabledSubqueryIndexShare = namedWindow != null && namedWindow.IsEnableIndexShare;
            var isVirtualWindow = namedWindow != null && namedWindow.IsVirtualDataWindow;
            var indexMetadata = namedWindow != null ? namedWindow.IndexMetadata : table.IndexMetadata;
            var optionalUniqueKeySet = namedWindow != null ? namedWindow.UniquenessAsSet : table.UniquenessAsSet;

            // query plan
            var onlyUseExistingIndexes = table != null;
            var planResult = SubordinateQueryPlanner.PlanOnExpression(
                validatedJoin,
                activatorResultEventType,
                indexHint,
                enabledSubqueryIndexShare,
                -1,
                excludePlanHint,
                isVirtualWindow,
                indexMetadata,
                infraEventType,
                optionalUniqueKeySet,
                onlyUseExistingIndexes,
                @base.StatementRawInfo,
                services);
            var queryPlan = planResult.Forge;
            additionalForgeables.AddAll(planResult.AdditionalForgeables);
            fabricCharge.Add(planResult.FabricCharge);

            // indicate index dependencies
            if (queryPlan.Indexes != null && infraVisibility == NameAccessModifier.PUBLIC) {
                foreach (var index in queryPlan.Indexes) {
                    services.ModuleDependenciesCompileTime.AddPathIndex(
                        namedWindow != null,
                        infraName,
                        infraModuleName,
                        index.IndexName,
                        index.IndexModuleName,
                        services.NamedWindowCompileTimeRegistry,
                        services.TableCompileTimeRegistry);
                }
            }

            var onTriggerType = planDesc.OnTriggerDesc.OnTriggerType;
            var activator = planDesc.ActivatorResult.Activator;
            var subselectForges = validationResult.SubselectForges;
            var tableAccessForges = validationResult.TableAccessForges;

            IList<StmtClassForgeable> forgeables = new List<StmtClassForgeable>(2);
            StatementAgentInstanceFactoryOnTriggerInfraBaseForge forge;
            var classNameRSP = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(ResultSetProcessorFactoryProvider),
                classPostfix);
            ResultSetProcessorDesc resultSetProcessor;

            if (onTriggerType == OnTriggerType.ON_SELECT) {
                resultSetProcessor = validationResult.ResultSetProcessorPrototype;
                var outputEventType = resultSetProcessor.ResultEventType;

                var insertInto = false;
                TableMetaData optionalInsertIntoTable = null;
                var insertIntoDesc = @base.StatementSpec.Raw.InsertIntoDesc;
                var addToFront = false;
                ExprNode eventPrecedence = null;
                if (insertIntoDesc != null) {
                    insertInto = true;
                    optionalInsertIntoTable = services.TableCompileTimeResolver.Resolve(insertIntoDesc.EventTypeName);
                    var optionalInsertIntoNamedWindow =
                        services.NamedWindowCompileTimeResolver.Resolve(insertIntoDesc.EventTypeName);
                    addToFront = optionalInsertIntoNamedWindow != null || optionalInsertIntoTable != null;
                    if (insertIntoDesc.EventPrecedence != null) {
                        eventPrecedence = EPLValidationUtil.ValidateEventPrecedence(
                            optionalInsertIntoTable != null,
                            insertIntoDesc.EventPrecedence,
                            resultSetProcessor.ResultEventType,
                            @base.StatementRawInfo,
                            services);
                    }
                }

                var selectAndDelete = planDesc.OnTriggerDesc.IsDeleteAndSelect;
                var distinct = @base.StatementSpec.SelectClauseCompiled.IsDistinct;
                var distinctMultiKeyPlan = MultiKeyPlanner.PlanMultiKeyDistinct(
                    distinct,
                    outputEventType,
                    @base.StatementRawInfo,
                    services.SerdeResolver);
                additionalForgeables.AddAll(distinctMultiKeyPlan.MultiKeyForgeables);
                forge = new StatementAgentInstanceFactoryOnTriggerInfraSelectForge(
                    activator,
                    outputEventType,
                    subselectForges,
                    tableAccessForges,
                    namedWindow,
                    table,
                    queryPlan,
                    classNameRSP,
                    insertInto,
                    addToFront,
                    optionalInsertIntoTable,
                    selectAndDelete,
                    distinct,
                    distinctMultiKeyPlan.ClassRef,
                    eventPrecedence);
            }
            else {
                var defaultSelectAllSpec = new StatementSpecCompiled();
                defaultSelectAllSpec.SelectClauseCompiled.SelectExprList = new[] { new SelectClauseElementWildcard() };
                defaultSelectAllSpec.Raw.SelectStreamDirEnum = SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH;
                StreamTypeService typeService = new StreamTypeServiceImpl(
                    new EventType[] { resultEventType },
                    new string[] { infraName },
                    new bool[] { false },
                    false,
                    false);
                resultSetProcessor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                    ResultSetProcessorAttributionKeyStatement.INSTANCE,
                    new ResultSetSpec(defaultSelectAllSpec),
                    typeService,
                    null,
                    new bool[1],
                    false,
                    @base.ContextPropertyRegistry,
                    false,
                    false,
                    @base.StatementRawInfo,
                    services);

                if (onTriggerType == OnTriggerType.ON_DELETE) {
                    forge = new StatementAgentInstanceFactoryOnTriggerInfraDeleteForge(
                        activator,
                        resultEventType,
                        subselectForges,
                        tableAccessForges,
                        classNameRSP,
                        namedWindow,
                        table,
                        queryPlan);
                }
                else if (onTriggerType == OnTriggerType.ON_UPDATE) {
                    var updateDesc = (OnTriggerWindowUpdateDesc)planDesc.OnTriggerDesc;
                    var updateHelper = EventBeanUpdateHelperForgeFactory.Make(
                        infraName,
                        (EventTypeSPI)infraEventType,
                        updateDesc.Assignments,
                        validationResult.ZeroStreamAliasName,
                        activatorResultEventType,
                        namedWindow != null,
                        @base.StatementName,
                        services.EventTypeAvroHandler);
                    forge = new StatementAgentInstanceFactoryOnTriggerInfraUpdateForge(
                        activator,
                        resultEventType,
                        subselectForges,
                        tableAccessForges,
                        classNameRSP,
                        namedWindow,
                        table,
                        queryPlan,
                        updateHelper);
                }
                else if (onTriggerType == OnTriggerType.ON_MERGE) {
                    var onMergeTriggerDesc = (OnTriggerMergeDesc)planDesc.OnTriggerDesc;
                    var onMergeHelper = new InfraOnMergeHelperForge(
                        onMergeTriggerDesc,
                        activatorResultEventType,
                        planDesc.StreamSpec.OptionalStreamName,
                        infraName,
                        (EventTypeSPI)infraEventType,
                        @base.StatementRawInfo,
                        services,
                        table);
                    forge = new StatementAgentInstanceFactoryOnTriggerInfraMergeForge(
                        activator,
                        resultEventType,
                        subselectForges,
                        tableAccessForges,
                        classNameRSP,
                        namedWindow,
                        table,
                        queryPlan,
                        onMergeHelper);
                }
                else {
                    throw new IllegalStateException("Unrecognized trigger type " + onTriggerType);
                }
            }

            forgeables.Add(
                new StmtClassForgeableRSPFactoryProvider(
                    classNameRSP,
                    resultSetProcessor,
                    namespaceScope,
                    @base.StatementRawInfo,
                    services.SerdeResolver.IsTargetHA));

            var queryPlanLogging = services.Configuration.Common.Logging.IsEnableQueryPlan;
            SubordinateQueryPlannerUtil.QueryPlanLogOnExpr(
                queryPlanLogging,
                QUERY_PLAN_LOG,
                queryPlan,
                @base.StatementSpec.Annotations,
                services.ImportServiceCompileTime);

            var onTrigger = new StmtClassForgeableAIFactoryProviderOnTrigger(className, namespaceScope, forge);
            return new OnTriggerPlan(
                onTrigger,
                forgeables,
                resultSetProcessor.SelectSubscriberDescriptor,
                additionalForgeables,
                fabricCharge);
        }

        internal static void ValidateOnExpressionContext(
            string onExprContextName,
            string desiredContextName,
            string title)
        {
            if (onExprContextName == null) {
                if (desiredContextName != null) {
                    throw new ExprValidationException(
                        "Cannot create on-trigger expression: " +
                        title +
                        " was declared with context '" +
                        desiredContextName +
                        "', please declare the same context name");
                }

                return;
            }

            if (!onExprContextName.Equals(desiredContextName)) {
                var text = desiredContextName == null
                    ? "without a context"
                    : "with context '" + desiredContextName + "', please use the same context instead";
                throw new ExprValidationException(
                    "Cannot create on-trigger expression: " + title + " was declared " + text);
            }
        }
    }
} // end of namespace