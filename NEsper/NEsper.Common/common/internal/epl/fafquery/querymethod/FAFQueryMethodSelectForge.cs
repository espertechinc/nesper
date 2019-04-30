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
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.faf;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.annotation;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
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

        public FAFQueryMethodSelectForge(
            FAFQueryMethodSelectDesc desc,
            string classNameResultSetProcessor,
            StatementRawInfo statementRawInfo)
        {
            this.desc = desc;
            this.classNameResultSetProcessor = classNameResultSetProcessor;
            this.statementRawInfo = statementRawInfo;
        }

        public IList<StmtClassForgable> MakeForgables(
            string queryMethodProviderClassName,
            string classPostfix,
            CodegenNamespaceScope namespaceScope)
        {
            IList<StmtClassForgable> forgables = new List<StmtClassForgable>();

            //namespaceScopeP
            forgables.Add(
                new StmtClassForgableRSPFactoryProvider(
                    classNameResultSetProcessor, 
                    desc.ResultSetProcessor, 
                    namespaceScope,
                    statementRawInfo));

            // generate faf-select
            forgables.Add(new StmtClassForgableQueryMethodProvider(queryMethodProviderClassName, namespaceScope, this));

            return forgables;
        }

        public void MakeMethod(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenExpressionRef select = @Ref("select");
            method.Block
                .DeclareVar(typeof(FAFQueryMethodSelect), select.Ref, NewInstance(typeof(FAFQueryMethodSelect)))
                .SetProperty(select, "Annotations", LocalMethod(AnnotationUtil.MakeAnnotations(typeof(Attribute[]), desc.Annotations, method, classScope)))
                .SetProperty(select, "Processors", FireAndForgetProcessorForge.MakeArray(desc.Processors, method, symbols, classScope))
                .DeclareVar(
                    classNameResultSetProcessor, "rsp",
                    CodegenExpressionBuilder.NewInstance(classNameResultSetProcessor, symbols.GetAddInitSvc(method)))
                .SetProperty(select, "ResultSetProcessorFactoryProvider", @Ref("rsp"))
                .SetProperty(select, "QueryGraph", desc.QueryGraph.Make(method, symbols, classScope))
                .SetProperty(select, "WhereClause",
                    desc.WhereClause == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(desc.WhereClause.Forge, method, this.GetType(), classScope))
                .SetProperty(select, "JoinSetComposerPrototype", desc.Joins == null ? ConstantNull() : desc.Joins.Make(method, symbols, classScope))
                .SetProperty(select, "ConsumerFilters", ExprNodeUtilityCodegen.CodegenEvaluators(desc.ConsumerFilters, method, this.GetType(), classScope))
                .SetProperty(select, "ContextName", Constant(desc.ContextName))
                .SetProperty(select, "TableAccesses",
                    ExprTableEvalStrategyUtil.CodegenInitMap(desc.TableAccessForges, this.GetType(), method, symbols, classScope))
                .SetProperty(select, "HasTableAccess", Constant(desc.HasTableAccess))
                .SetProperty(select, "Distinct", Constant(desc.IsDistinct))
                .MethodReturn(select);
        }
    }
} // end of namespace