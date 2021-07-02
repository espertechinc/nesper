///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.faf;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.annotation;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    ///     Starts and provides the stop method for EPL statements.
    /// </summary>
    public class FAFQueryMethodSelectForge : FAFQueryMethodForge
    {
        private readonly string _classNameResultSetProcessor;
        private readonly FAFQueryMethodSelectDesc _desc;
        private readonly StatementRawInfo _statementRawInfo;

        public FAFQueryMethodSelectForge(
            FAFQueryMethodSelectDesc desc,
            string classNameResultSetProcessor,
            StatementRawInfo statementRawInfo)
        {
            _desc = desc;
            _classNameResultSetProcessor = classNameResultSetProcessor;
            _statementRawInfo = statementRawInfo;
        }

        public IList<StmtClassForgeable> MakeForgeables(
            string queryMethodProviderClassName,
            string classPostfix,
            CodegenNamespaceScope namespaceScope)
        {
            IList<StmtClassForgeable> forgeables = new List<StmtClassForgeable>();
            foreach (var additional in _desc.AdditionalForgeables) {
                forgeables.Add(additional.Make(namespaceScope, classPostfix));
            }

            //namespaceScopeP
            forgeables.Add(
                new StmtClassForgeableRSPFactoryProvider(
                    _classNameResultSetProcessor,
                    _desc.ResultSetProcessor,
                    namespaceScope,
                    _statementRawInfo));

            // generate faf-select
            forgeables.Add(new StmtClassForgeableQueryMethodProvider(queryMethodProviderClassName, namespaceScope, this));

            return forgeables;
        }

        public void MakeMethod(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var select = Ref("select");
            method.Block
                .DeclareVarNewInstance<FAFQueryMethodSelect>(select.Ref)
                .SetProperty(
                    select,
                    "Annotations",
                    AnnotationUtil.MakeAnnotations(typeof(Attribute[]), _desc.Annotations, method, classScope))
                .SetProperty(
                    select,
                    "Processors",
                    FireAndForgetProcessorForgeExtensions.MakeArray(_desc.Processors, method, symbols, classScope))
                .DeclareVar(
                    _classNameResultSetProcessor,
                    "rsp",
                    NewInstanceNamed(_classNameResultSetProcessor, symbols.GetAddInitSvc(method), Ref("statementFields")))
                .SetProperty(
                    select,
                    "ResultSetProcessorFactoryProvider",
                    Ref("rsp"))
                .SetProperty(
                    select,
                    "QueryGraph",
                    _desc.QueryGraph.Make(method, symbols, classScope))
                .SetProperty(
                    select,
                    "WhereClause",
                    _desc.WhereClause == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(
                            _desc.WhereClause.Forge,
                            method,
                            GetType(),
                            classScope))
                .SetProperty(
                    select,
                    "JoinSetComposerPrototype",
                    _desc.Joins == null ? ConstantNull() : _desc.Joins.Make(method, symbols, classScope))
                .SetProperty(
                    select,
                    "ConsumerFilters",
                    ExprNodeUtilityCodegen.CodegenEvaluators(
                        _desc.ConsumerFilters,
                        method,
                        GetType(),
                        classScope))
                .SetProperty(
                    select,
                    "ContextName",
                    Constant(_desc.ContextName))
                .SetProperty(
                    select,
                    "ContextModuleName",
                    Constant(_desc.ContextModuleName))
                .SetProperty(
                    select,
                    "TableAccesses",
                    ExprTableEvalStrategyUtil.CodegenInitMap(
                        _desc.TableAccessForges,
                        GetType(),
                        method,
                        symbols,
                        classScope))
                .SetProperty(
                    select,
                    "HasTableAccess",
                    Constant(_desc.HasTableAccess))
                .SetProperty(
                    select,
                    "DistinctKeyGetter",
                    MultiKeyCodegen.CodegenGetterEventDistinct(
                        _desc.IsDistinct,
                        _desc.ResultSetProcessor.ResultEventType,
                        _desc.DistinctMultiKey,
                        method,
                        classScope))
                .SetProperty(
                    select,
                    "Subselects",
                    SubSelectFactoryForge.CodegenInitMap(
                        _desc.SubselectForges,
                        GetType(),
                        method,
                        symbols,
                        classScope))
                .MethodReturn(select);
        }
    }
} // end of namespace