///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

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
        internal readonly FireAndForgetProcessorForge processor;
        internal readonly ExprNode whereClause;
        internal readonly QueryGraphForge queryGraph;
        internal readonly Attribute[] annotations;
        protected bool hasTableAccess;
        internal readonly IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> tableAccessForges;
        private readonly IDictionary<ExprSubselectNode, SubSelectFactoryForge> _subselectForges;
        private readonly IList<StmtClassForgeableFactory> _additionalForgeables = new List<StmtClassForgeableFactory>();

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
            this.annotations = spec.Annotations;
            this.hasTableAccess = spec.Raw.IntoTableSpec != null ||
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
                spec.SubselectNodes, services.TableCompileTimeResolver);

            // validate general FAF criteria
            FAFQueryMethodHelper.ValidateFAFQuery(spec);

            // obtain processor
            StreamSpecCompiled streamSpec = spec.StreamSpecs[0];
            processor = FireAndForgetProcessorForgeFactory.ValidateResolveProcessor(streamSpec);

            // obtain name and type
            string processorName = processor.NamedWindowOrTableName;
            EventType eventType = processor.EventTypeRspInputEvents;

            // determine alias
            string aliasName = processorName;
            if (streamSpec.OptionalStreamName != null) {
                aliasName = streamSpec.OptionalStreamName;
            }
            
            // activate subselect activations
            var @base = new StatementBaseInfo(compilable, spec, null, statementRawInfo, null);
            var subqueryNamedWindowConsumers = new List<NamedWindowConsumerStreamSpec>();
            var subSelectActivationDesc = SubSelectHelperActivations.CreateSubSelectActivation(
                EmptyList<FilterSpecCompiled>.Instance, subqueryNamedWindowConsumers, @base, services);
            var subselectActivation = subSelectActivationDesc.Subselects;
            _additionalForgeables.AddAll(subSelectActivationDesc.AdditionalForgeables);

            // plan subselects
            var namesPerStream = new[] {aliasName};
            var typesPerStream = new[] { processor.EventTypePublic };
            var eventTypeNames = new[] {typesPerStream[0].Name};
            SubSelectHelperForgePlan subSelectForgePlan = SubSelectHelperForgePlanner.PlanSubSelect(
                @base, subselectActivation, namesPerStream, typesPerStream, eventTypeNames, services);
            _subselectForges = subSelectForgePlan.Subselects;
            _additionalForgeables.AddAll(subSelectForgePlan.AdditionalForgeables);

            // compile filter to optimize access to named window
            StreamTypeServiceImpl typeService = new StreamTypeServiceImpl(
                new[] {eventType},
                new[] {aliasName},
                new[] {true},
                true,
                false);
            ExcludePlanHint excludePlanHint = ExcludePlanHint.GetHint(
                typeService.StreamNames,
                statementRawInfo,
                services);
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
            var forgeables = _additionalForgeables
                .Select(additional => additional.Make(namespaceScope, classPostfix))
                .ToList();
            forgeables.Add(new StmtClassForgeableQueryMethodProvider(queryMethodProviderClassName, namespaceScope, this));
            return forgeables;
        }

        public void MakeMethod(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenExpressionRef queryMethod = Ref("qm");
            method.Block
                .DeclareVar(TypeOfMethod(), queryMethod.Ref, NewInstance(TypeOfMethod()))
                .SetProperty(
                    queryMethod,
                    "Annotations",
                    annotations == null
                        ? ConstantNull()
                        : LocalMethod(MakeAnnotations(typeof(Attribute[]), annotations, method, classScope)))
                .SetProperty(
                    queryMethod, "Processor", processor.Make(method, symbols, classScope))
                .SetProperty(
                    queryMethod,
                    "QueryGraph",
                    queryGraph == null ? ConstantNull() : queryGraph.Make(method, symbols, classScope))
                .SetProperty(
                    queryMethod,
                    "InternalEventRouteDest",
                    ExprDotName(
                        symbols.GetAddInitSvc(method), EPStatementInitServicesConstants.INTERNALEVENTROUTEDEST))
                .SetProperty(
                    queryMethod,
                    "TableAccesses",
                    ExprTableEvalStrategyUtil.CodegenInitMap(
                        tableAccessForges,
                        this.GetType(),
                        method,
                        symbols,
                        classScope))
                .SetProperty(
                    queryMethod, "HasTableAccess", Constant(hasTableAccess))
                .SetProperty(
                    queryMethod, "Subselects", SubSelectFactoryForge.CodegenInitMap(
                        _subselectForges, GetType(), method, symbols, classScope));
                
            MakeInlineSpecificSetter(queryMethod, method, symbols, classScope);
            method.Block.MethodReturn(queryMethod);
        }
    }
} // end of namespace