///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.join.@base;
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
        private const string OPVFACTORYPROVIDER = "opvFactoryProvider";

        private readonly JoinSetComposerPrototypeForge _joinSetComposerPrototypeForge;
        private readonly bool _orderByWithoutOutputRateLimit;
        private readonly string _outputProcessViewProviderClassName;
        private readonly string _resultSetProcessorProviderClassName;

        private readonly string[] _streamNames;
        private readonly IDictionary<ExprSubselectNode, SubSelectFactoryForge> _subselects;
        private readonly IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> _tableAccesses;
        private readonly bool _unidirectionalJoin;
        private readonly ViewableActivatorForge[] _viewableActivatorForges;
        private readonly ViewResourceDelegateDesc[] _viewResourceDelegates;
        private readonly IList<ViewFactoryForge>[] _views;
        private readonly ExprForge _whereClauseForge;

        public StatementAgentInstanceFactorySelectForge(
            string[] streamNames,
            ViewableActivatorForge[] viewableActivatorForges,
            string resultSetProcessorProviderClassName,
            IList<ViewFactoryForge>[] views,
            ViewResourceDelegateDesc[] viewResourceDelegates,
            ExprForge whereClauseForge,
            JoinSetComposerPrototypeForge joinSetComposerPrototypeForge,
            string outputProcessViewProviderClassName,
            IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselects,
            IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> tableAccesses,
            bool orderByWithoutOutputRateLimit,
            bool unidirectionalJoin)
        {
            _streamNames = streamNames;
            _viewableActivatorForges = viewableActivatorForges;
            _resultSetProcessorProviderClassName = resultSetProcessorProviderClassName;
            _views = views;
            _viewResourceDelegates = viewResourceDelegates;
            _whereClauseForge = whereClauseForge;
            _joinSetComposerPrototypeForge = joinSetComposerPrototypeForge;
            _outputProcessViewProviderClassName = outputProcessViewProviderClassName;
            _subselects = subselects;
            _tableAccesses = tableAccesses;
            _orderByWithoutOutputRateLimit = orderByWithoutOutputRateLimit;
            _unidirectionalJoin = unidirectionalJoin;
        }

        public CodegenMethod InitializeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols)
        {
            var method = parent.MakeChild(typeof(StatementAgentInstanceFactorySelect), GetType(), classScope);
            method.Block
                .DeclareVar<StatementAgentInstanceFactorySelect>(
                    "saiff",
                    NewInstance(typeof(StatementAgentInstanceFactorySelect)));

            // stream names
            method.Block.SetProperty(Ref("saiff"), "StreamNames", Constant(_streamNames));

            // activators
            method.Block.DeclareVar<ViewableActivator[]>(
                "activators",
                NewArrayByLength(typeof(ViewableActivator), Constant(_viewableActivatorForges.Length)));
            for (var i = 0; i < _viewableActivatorForges.Length; i++) {
                method.Block.AssignArrayElement(
                    "activators",
                    Constant(i),
                    _viewableActivatorForges[i].MakeCodegen(method, symbols, classScope));
            }

            method.Block.SetProperty(Ref("saiff"), "ViewableActivators", Ref("activators"));

            // views
            method.Block.DeclareVar<ViewFactory[][]>(
                "viewFactories",
                NewArrayByLength(typeof(ViewFactory[]), Constant(_views.Length)));
            for (var i = 0; i < _views.Length; i++) {
                if (_views[i] != null) {
                    var array = ViewFactoryForgeUtil.CodegenForgesWInit(
                        _views[i],
                        i,
                        null,
                        method,
                        symbols,
                        classScope);
                    method.Block.AssignArrayElement("viewFactories", Constant(i), array);
                }
            }

            method.Block.SetProperty(Ref("saiff"), "ViewFactories", Ref("viewFactories"));

            // view delegate information ('prior' and 'prev')
            method.Block.DeclareVar<ViewResourceDelegateDesc[]>(
                "viewResourceDelegates",
                NewArrayByLength(typeof(ViewResourceDelegateDesc), Constant(_viewResourceDelegates.Length)));
            for (var i = 0; i < _viewResourceDelegates.Length; i++) {
                method.Block.AssignArrayElement(
                    "viewResourceDelegates",
                    Constant(i),
                    _viewResourceDelegates[i].ToExpression());
            }

            method.Block.SetProperty(Ref("saiff"), "ViewResourceDelegates", Ref("viewResourceDelegates"));

            // result set processor
            method.Block.DeclareVar(
                    _resultSetProcessorProviderClassName,
                    RSPFACTORYPROVIDER,
                    NewInstanceInner(_resultSetProcessorProviderClassName, 
                        symbols.GetAddInitSvc(method), 
                        Ref(StmtClassForgeableAIFactoryProviderBase.MEMBERNAME_STATEMENT_FIELDS)))
                .SetProperty(Ref("saiff"), "ResultSetProcessorFactoryProvider", Ref(RSPFACTORYPROVIDER));

            // where-clause evaluator
            if (_whereClauseForge != null) {
                var whereEval = CodegenEvaluator(_whereClauseForge, method, GetType(), classScope);
                method.Block.SetProperty(Ref("saiff"), "WhereClauseEvaluator", whereEval);
                if (classScope.IsInstrumented) {
                    method.Block.SetProperty(
                        Ref("saiff"),
                        "WhereClauseEvaluatorTextForAudit",
                        Constant(ExprNodeUtilityPrint.ToExpressionStringMinPrecedence(_whereClauseForge)));
                }
            }

            // joins
            if (_joinSetComposerPrototypeForge != null) {
                method.Block.SetProperty(
                    Ref("saiff"),
                    "JoinSetComposerPrototype",
                    _joinSetComposerPrototypeForge.Make(method, symbols, classScope));
            }

            // output process view
            method.Block.DeclareVar(
                    _outputProcessViewProviderClassName,
                    OPVFACTORYPROVIDER,
                    NewInstanceInner(_outputProcessViewProviderClassName, symbols.GetAddInitSvc(method), Ref("statementFields")))
                .SetProperty(Ref("saiff"), "OutputProcessViewFactoryProvider", Ref(OPVFACTORYPROVIDER));

            // subselects
            if (!_subselects.IsEmpty()) {
                method.Block.SetProperty(
                    Ref("saiff"),
                    "Subselects",
                    SubSelectFactoryForge.CodegenInitMap(_subselects, GetType(), method, symbols, classScope));
            }

            // table-access
            if (!_tableAccesses.IsEmpty()) {
                method.Block.SetProperty(
                    Ref("saiff"),
                    "TableAccesses",
                    ExprTableEvalStrategyUtil.CodegenInitMap(_tableAccesses, GetType(), method, symbols, classScope));
            }

            // order-by with no output-limit
            method.Block.SetProperty(
                Ref("saiff"),
                "OrderByWithoutOutputRateLimit",
                Constant(_orderByWithoutOutputRateLimit));

            // unidirectional join
            method.Block.SetProperty(Ref("saiff"), "IsUnidirectionalJoin", Constant(_unidirectionalJoin));
            method.Block.MethodReturn(Ref("saiff"));

            return method;
        }
    }
} // end of namespace