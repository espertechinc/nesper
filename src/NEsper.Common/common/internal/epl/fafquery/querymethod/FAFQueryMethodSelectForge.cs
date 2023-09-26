///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
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
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class FAFQueryMethodSelectForge : FAFQueryMethodForge
    {
        private readonly FAFQueryMethodSelectDesc desc;
        private readonly string classNameResultSetProcessor;
        private readonly StatementRawInfo statementRawInfo;
        private readonly ModuleCompileTimeServices services;

        public FAFQueryMethodSelectForge(
            FAFQueryMethodSelectDesc desc,
            string classNameResultSetProcessor,
            StatementRawInfo statementRawInfo,
            ModuleCompileTimeServices services)
        {
            this.desc = desc;
            this.classNameResultSetProcessor = classNameResultSetProcessor;
            this.statementRawInfo = statementRawInfo;
            this.services = services;
        }

        public IList<StmtClassForgeable> MakeForgeables(
            string queryMethodProviderClassName,
            string classPostfix,
            CodegenNamespaceScope namespaceScope)
        {
            IList<StmtClassForgeable> forgeables = new List<StmtClassForgeable>();
            foreach (var additional in desc.AdditionalForgeables) {
                forgeables.Add(additional.Make(namespaceScope, classPostfix));
            }

            // generate RSP
            forgeables.Add(
                new StmtClassForgeableRSPFactoryProvider(
                    classNameResultSetProcessor,
                    desc.ResultSetProcessor,
                    namespaceScope,
                    statementRawInfo,
                    false));

            // generate faf-select
            forgeables.Add(
                new StmtClassForgeableQueryMethodProvider(queryMethodProviderClassName, namespaceScope, this));

            return forgeables;
        }

        public void MakeMethod(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var select = Ref("select");
            method.Block
                .DeclareVarNewInstance(typeof(FAFQueryMethodSelect), select.Ref)
                .SetProperty(
                    select,
                    "Annotations",
                    AnnotationUtil.MakeAnnotations(
                        typeof(Attribute[]),
                        desc.Annotations,
                        method,
                        classScope))
                .SetProperty(
                    select,
                    "Processors",
                    FireAndForgetProcessorForgeExtensions.MakeArray(desc.Processors, method, symbols, classScope))
                .DeclareVar(
                    classNameResultSetProcessor,
                    "rsp",
                    NewInstanceInner(classNameResultSetProcessor, symbols.GetAddInitSvc(method)))
                .SetProperty(
                    select,
                    "ResultSetProcessorFactoryProvider",
                    Ref("rsp"))
                .SetProperty(
                    select,
                    "QueryGraph",
                    desc.QueryGraph.Make(method, symbols, classScope))
                .SetProperty(
                    select,
                    "WhereClause",
                    desc.WhereClause == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(
                            desc.WhereClause.Forge,
                            method,
                            GetType(),
                            classScope))
                .SetProperty(
                    select,
                    "JoinSetComposerPrototype",
                    desc.Joins == null ? ConstantNull() : desc.Joins.Make(method, symbols, classScope))
                .SetProperty(
                    select,
                    "ConsumerFilters",
                    ExprNodeUtilityCodegen.CodegenEvaluators(desc.ConsumerFilters, method, GetType(), classScope))
                .SetProperty(
                    select,
                    "ContextName",
                    Constant(desc.ContextName))
                .SetProperty(
                    select,
                    "ContextModuleName",
                    Constant(desc.ContextModuleName))
                .SetProperty(
                    select,
                    "TableAccesses",
                    ExprTableEvalStrategyUtil.CodegenInitMap(
                        desc.TableAccessForges,
                        GetType(),
                        method,
                        symbols,
                        classScope))
                .SetProperty(
                    select,
                    "HasTableAccess",
                    Constant(desc.HasTableAccess))
                .SetProperty(
                    select,
                    "DistinctKeyGetter",
                    MultiKeyCodegen.CodegenGetterEventDistinct(
                        desc.IsDistinct,
                        desc.ResultSetProcessor.ResultEventType,
                        desc.DistinctMultiKey,
                        method,
                        classScope))
                .SetProperty(
                    select,
                    "Subselects",
                    SubSelectFactoryForge.CodegenInitMap(
                        desc.SubselectForges,
                        GetType(),
                        method,
                        symbols,
                        classScope))
                .MethodReturn(select);
        }
    }
} // end of namespace