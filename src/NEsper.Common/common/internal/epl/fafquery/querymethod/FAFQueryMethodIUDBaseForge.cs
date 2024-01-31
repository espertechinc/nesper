///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.faf;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.join.hint;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.statement.helper;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.annotation.AnnotationUtil;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public abstract class FAFQueryMethodIUDBaseForge : FAFQueryMethodForge
    {
        protected readonly FireAndForgetProcessorForge processor;
        protected readonly ExprNode whereClause;
        protected readonly QueryGraphForge queryGraph;
        protected readonly Attribute[] annotations;
        protected bool hasTableAccess;
        protected readonly IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> tableAccessForges;
        private readonly IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselectForges;
        private readonly IList<StmtClassForgeableFactory> additionalForgeables = new List<StmtClassForgeableFactory>(2);

        protected abstract void InitExec(
            string aliasName,
            StatementSpecCompiled spec,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services);

        protected abstract Type TypeOfMethod();

        protected abstract void MakeInlineSpecificSetter(
            CodegenExpressionRef queryMethod,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        public FAFQueryMethodIUDBaseForge(
            StatementSpecCompiled spec,
            Compilable compilable,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            annotations = spec.Annotations;
            hasTableAccess = spec.Raw.IntoTableSpec != null ||
                             (spec.TableAccessNodes != null && spec.TableAccessNodes.Count > 0);
            if (spec.Raw.InsertIntoDesc != null &&
                services.TableCompileTimeResolver.Resolve(spec.Raw.InsertIntoDesc.EventTypeName) != null) {
                hasTableAccess = true;
            }

            if (spec.Raw.FireAndForgetSpec is FireAndForgetSpecUpdate ||
                spec.Raw.FireAndForgetSpec is FireAndForgetSpecDelete) {
                hasTableAccess |= spec.StreamSpecs[0] is TableQueryStreamSpec;
            }

            hasTableAccess |= StatementLifecycleSvcUtil.IsSubqueryWithTable(
                spec.SubselectNodes,
                services.TableCompileTimeResolver);

            // validate general FAF criteria
            FAFQueryMethodHelper.ValidateFAFQuery(spec);

            // obtain processor
            var streamSpec = spec.StreamSpecs[0];
            var @base = new StatementBaseInfo(compilable, spec, null, statementRawInfo, null);
            processor = FireAndForgetProcessorForgeFactory.ValidateResolveProcessor(
                streamSpec,
                spec,
                @base.StatementRawInfo,
                services);

            // obtain name and type
            var processorName = processor.ProcessorName;
            var eventType = processor.EventTypeRSPInputEvents;

            // determine alias
            var aliasName = processorName;
            if (streamSpec.OptionalStreamName != null) {
                aliasName = streamSpec.OptionalStreamName;
            }

            // activate subselect activations
            IList<NamedWindowConsumerStreamSpec> subqueryNamedWindowConsumers =
                new List<NamedWindowConsumerStreamSpec>();
            var subSelectActivationDesc = SubSelectHelperActivations.CreateSubSelectActivation(
                false,
                EmptyList<FilterSpecTracked>.Instance, 
                subqueryNamedWindowConsumers,
                @base,
                services);
            var subselectActivation = subSelectActivationDesc.Subselects;
            additionalForgeables.AddAll(subSelectActivationDesc.AdditionalForgeables);

            // plan subselects
            var namesPerStream = new string[] { aliasName };
            var typesPerStream = new EventType[] { processor.EventTypePublic };
            var eventTypeNames = new string[] { typesPerStream[0].Name };
            var subSelectForgePlan = SubSelectHelperForgePlanner.PlanSubSelect(
                @base,
                subselectActivation,
                namesPerStream,
                typesPerStream,
                eventTypeNames,
                services);
            subselectForges = subSelectForgePlan.Subselects;
            additionalForgeables.AddAll(subSelectForgePlan.AdditionalForgeables);

            // compile filter to optimize access to named window
            var typeService = new StreamTypeServiceImpl(
                new EventType[] { eventType },
                new string[] { aliasName },
                new bool[] { true },
                true,
                false);
            var excludePlanHint = ExcludePlanHint.GetHint(typeService.StreamNames, statementRawInfo, services);
            if (spec.Raw.WhereClause != null) {
                queryGraph = new QueryGraphForge(1, excludePlanHint, false);
                EPLValidationUtil.ValidateFilterWQueryGraphSafe(
                    queryGraph,
                    spec.Raw.WhereClause,
                    typeService,
                    statementRawInfo,
                    services);
            }
            else {
                queryGraph = null;
            }

            // validate expressions
            whereClause = EPStatementStartMethodHelperValidate.ValidateNodes(
                spec.Raw,
                typeService,
                null,
                statementRawInfo,
                services);

            // get executor
            InitExec(aliasName, spec, statementRawInfo, services);

            // plan table access
            tableAccessForges = ExprTableEvalHelperPlan.PlanTableAccess(spec.Raw.TableExpressions);
        }

        public IList<StmtClassForgeable> MakeForgeables(
            string queryMethodProviderClassName,
            string classPostfix,
            CodegenNamespaceScope namespaceScope)
        {
            IList<StmtClassForgeable> forgeables = new List<StmtClassForgeable>();
            foreach (var additional in additionalForgeables) {
                forgeables.Add(additional.Make(namespaceScope, classPostfix));
            }

            forgeables.Add(
                new StmtClassForgeableQueryMethodProvider(queryMethodProviderClassName, namespaceScope, this));
            return forgeables;
        }

        public void MakeMethod(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var queryMethod = Ref("qm");
            method.Block
                .DeclareVar(TypeOfMethod(), queryMethod.Ref, NewInstance(TypeOfMethod()))
                .SetProperty(
                    queryMethod,
                    "Annotations",
                    annotations == null
                        ? ConstantNull()
                        : MakeAnnotations(typeof(Attribute[]), annotations, method, classScope))
                .SetProperty(queryMethod, "Processor", processor.Make(method, symbols, classScope))
                .SetProperty(
                    queryMethod,
                    "QueryGraph",
                    queryGraph == null ? ConstantNull() : queryGraph.Make(method, symbols, classScope))
                .SetProperty(
                    queryMethod,
                    "InternalEventRouteDest",
                    ExprDotName(symbols.GetAddInitSvc(method), EPStatementInitServicesConstants.INTERNALEVENTROUTEDEST))
                .SetProperty(
                    queryMethod,
                    "TableAccesses",
                    ExprTableEvalStrategyUtil.CodegenInitMap(tableAccessForges, GetType(), method, symbols, classScope))
                .SetProperty(queryMethod, "HasTableAccess", Constant(hasTableAccess))
                .SetProperty(
                    queryMethod,
                    "Subselects",
                    SubSelectFactoryForge.CodegenInitMap(subselectForges, GetType(), method, symbols, classScope));
            MakeInlineSpecificSetter(queryMethod, method, symbols, classScope);
            method.Block.MethodReturn(queryMethod);
        }
    }
} // end of namespace