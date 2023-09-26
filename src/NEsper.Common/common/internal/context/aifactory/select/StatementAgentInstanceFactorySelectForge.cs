///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.join.@base;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public class StatementAgentInstanceFactorySelectForge : StatementAgentInstanceFactoryForge
    {
        private const string RSPFACTORYPROVIDER = "rspFactoryProvider";

        private readonly string[] streamNames;
        private readonly ViewableActivatorForge[] viewableActivatorForges;
        private readonly string resultSetProcessorProviderClassName;
        private readonly IList<ViewFactoryForge>[] views;
        private readonly ViewResourceDelegateDesc[] viewResourceDelegates;
        private readonly ExprForge whereClauseForge;
        private readonly JoinSetComposerPrototypeForge joinSetComposerPrototypeForge;
        private readonly string outputProcessViewProviderClassName;
        private readonly bool outputProcessViewDirectSimple;
        private readonly IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselects;
        private readonly IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> tableAccesses;
        private readonly bool orderByWithoutOutputRateLimit;
        private readonly bool unidirectionalJoin;

        public StatementAgentInstanceFactorySelectForge(
            string[] streamNames,
            ViewableActivatorForge[] viewableActivatorForges,
            string resultSetProcessorProviderClassName,
            IList<ViewFactoryForge>[] views,
            ViewResourceDelegateDesc[] viewResourceDelegates,
            ExprForge whereClauseForge,
            JoinSetComposerPrototypeForge joinSetComposerPrototypeForge,
            string outputProcessViewProviderClassName,
            bool outputProcessViewDirectSimple,
            IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselects,
            IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> tableAccesses,
            bool orderByWithoutOutputRateLimit,
            bool unidirectionalJoin)
        {
            this.streamNames = streamNames;
            this.viewableActivatorForges = viewableActivatorForges;
            this.resultSetProcessorProviderClassName = resultSetProcessorProviderClassName;
            this.views = views;
            this.viewResourceDelegates = viewResourceDelegates;
            this.whereClauseForge = whereClauseForge;
            this.joinSetComposerPrototypeForge = joinSetComposerPrototypeForge;
            this.outputProcessViewProviderClassName = outputProcessViewProviderClassName;
            this.outputProcessViewDirectSimple = outputProcessViewDirectSimple;
            this.subselects = subselects;
            this.tableAccesses = tableAccesses;
            this.orderByWithoutOutputRateLimit = orderByWithoutOutputRateLimit;
            this.unidirectionalJoin = unidirectionalJoin;
        }

        public CodegenMethod InitializeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols)
        {
            var method = parent.MakeChild(typeof(StatementAgentInstanceFactorySelect), GetType(), classScope);
            method.Block
                .DeclareVarNewInstance(typeof(StatementAgentInstanceFactorySelect), "saiff");

            // stream names
            method.Block.ExprDotMethod(Ref("saiff"), "setStreamNames", Constant(streamNames));

            // activators
            method.Block.DeclareVar<ViewableActivator[]>(
                "activators",
                NewArrayByLength(typeof(ViewableActivator), Constant(viewableActivatorForges.Length)));
            for (var i = 0; i < viewableActivatorForges.Length; i++) {
                method.Block.AssignArrayElement(
                    "activators",
                    Constant(i),
                    viewableActivatorForges[i].MakeCodegen(method, symbols, classScope));
            }

            method.Block.ExprDotMethod(Ref("saiff"), "setViewableActivators", Ref("activators"));

            // views
            if (views.Length == 1 && views[0].IsEmpty()) {
                method.Block.ExprDotMethod(
                    Ref("saiff"),
                    "setViewFactories",
                    PublicConstValue(typeof(ViewFactory), "SINGLE_ELEMENT_ARRAY"));
            }
            else {
                method.Block.DeclareVar<ViewFactory[][]>(
                    "viewFactories",
                    NewArrayByLength(typeof(ViewFactory[]), Constant(views.Length)));
                for (var i = 0; i < views.Length; i++) {
                    if (views[i] != null) {
                        var array = ViewFactoryForgeUtil.CodegenForgesWInit(
                            views[i],
                            i,
                            null,
                            method,
                            symbols,
                            classScope);
                        method.Block.AssignArrayElement("viewFactories", Constant(i), array);
                    }
                }

                method.Block.ExprDotMethod(Ref("saiff"), "setViewFactories", Ref("viewFactories"));
            }

            // view delegate information ('prior' and 'prev')
            method.Block.ExprDotMethod(
                Ref("saiff"),
                "setViewResourceDelegates",
                ViewResourceDelegateDesc.ToExpression(viewResourceDelegates));

            // result set processor
            method.Block.DeclareVar(
                    resultSetProcessorProviderClassName,
                    RSPFACTORYPROVIDER,
                    CodegenExpressionBuilder.NewInstanceInner(
                        resultSetProcessorProviderClassName,
                        symbols.GetAddInitSvc(method)))
                .ExprDotMethod(Ref("saiff"), "setResultSetProcessorFactoryProvider", Ref(RSPFACTORYPROVIDER));

            // where-clause evaluator
            if (whereClauseForge != null) {
                var whereEval = CodegenEvaluator(whereClauseForge, method, GetType(), classScope);
                method.Block.ExprDotMethod(Ref("saiff"), "setWhereClauseEvaluator", whereEval);
                if (classScope.IsInstrumented) {
                    method.Block.ExprDotMethod(
                        Ref("saiff"),
                        "setWhereClauseEvaluatorTextForAudit",
                        Constant(ExprNodeUtilityPrint.ToExpressionStringMinPrecedence(whereClauseForge)));
                }
            }

            // joins
            if (joinSetComposerPrototypeForge != null) {
                method.Block.ExprDotMethod(
                    Ref("saiff"),
                    "setJoinSetComposerPrototype",
                    joinSetComposerPrototypeForge.Make(method, symbols, classScope));
            }

            // output process view
            CodegenExpression opv;
            if (outputProcessViewDirectSimple) {
                opv = PublicConstValue(typeof(OutputProcessViewDirectSimpleFactoryProvider), "INSTANCE");
            }
            else {
                opv = CodegenExpressionBuilder.NewInstanceInner(
                    outputProcessViewProviderClassName,
                    symbols.GetAddInitSvc(method));
            }

            method.Block.ExprDotMethod(Ref("saiff"), "setOutputProcessViewFactoryProvider", opv);

            // subselects
            if (!subselects.IsEmpty()) {
                method.Block.ExprDotMethod(
                    Ref("saiff"),
                    "setSubselects",
                    SubSelectFactoryForge.CodegenInitMap(subselects, GetType(), method, symbols, classScope));
            }

            // table-access
            if (!tableAccesses.IsEmpty()) {
                method.Block.ExprDotMethod(
                    Ref("saiff"),
                    "setTableAccesses",
                    ExprTableEvalStrategyUtil.CodegenInitMap(tableAccesses, GetType(), method, symbols, classScope));
            }

            // order-by with no output-limit
            if (orderByWithoutOutputRateLimit) {
                method.Block.ExprDotMethod(
                    Ref("saiff"),
                    "setOrderByWithoutOutputRateLimit",
                    Constant(orderByWithoutOutputRateLimit));
            }

            // unidirectional join
            if (unidirectionalJoin) {
                method.Block.ExprDotMethod(Ref("saiff"), "setUnidirectionalJoin", Constant(unidirectionalJoin));
            }

            method.Block.MethodReturn(Ref("saiff"));

            return method;
        }
    }
} // end of namespace