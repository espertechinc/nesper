///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.join.hint;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.statement.helper;
using com.espertech.esper.compat;
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
            if (spec.Raw.InsertIntoDesc != null && services.TableCompileTimeResolver.Resolve(spec.Raw.InsertIntoDesc.EventTypeName) != null) {
                hasTableAccess = true;
            }

            if (spec.Raw.FireAndForgetSpec is FireAndForgetSpecUpdate ||
                spec.Raw.FireAndForgetSpec is FireAndForgetSpecDelete) {
                hasTableAccess |= spec.StreamSpecs[0] is TableQueryStreamSpec;
            }

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

            // compile filter to optimize access to named window
            StreamTypeServiceImpl typeService = new StreamTypeServiceImpl(
                new EventType[] {eventType}, new string[] {aliasName}, new bool[] {true}, true, false);
            ExcludePlanHint excludePlanHint = ExcludePlanHint.GetHint(typeService.StreamNames, statementRawInfo, services);
            if (spec.Raw.WhereClause != null) {
                queryGraph = new QueryGraphForge(1, excludePlanHint, false);
                EPLValidationUtil.ValidateFilterWQueryGraphSafe(queryGraph, spec.Raw.WhereClause, typeService, statementRawInfo, services);
            }
            else {
                queryGraph = null;
            }

            // validate expressions
            whereClause = EPStatementStartMethodHelperValidate.ValidateNodes(spec.Raw, typeService, null, statementRawInfo, services);

            // get executor
            InitExec(aliasName, spec, statementRawInfo, services);

            // plan table access
            tableAccessForges = ExprTableEvalHelperPlan.PlanTableAccess(spec.Raw.TableExpressions);
        }

        public IList<StmtClassForgable> MakeForgables(
            string queryMethodProviderClassName,
            string classPostfix,
            CodegenPackageScope packageScope)
        {
            return Collections.SingletonList<StmtClassForgable>(
                new StmtClassForgableQueryMethodProvider(queryMethodProviderClassName, packageScope, this));
        }

        public void MakeMethod(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenExpressionRef queryMethod = @Ref("qm");
            method.Block
                .DeclareVar(TypeOfMethod(), queryMethod.Ref, NewInstance(TypeOfMethod()))
                .ExprDotMethod(
                    queryMethod, "setAnnotations",
                    annotations == null ? ConstantNull() : LocalMethod(MakeAnnotations(typeof(Attribute[]), annotations, method, classScope)))
                .ExprDotMethod(queryMethod, "setProcessor", processor.Make(method, symbols, classScope))
                .ExprDotMethod(queryMethod, "setQueryGraph", queryGraph == null ? ConstantNull() : queryGraph.Make(method, symbols, classScope))
                .ExprDotMethod(
                    queryMethod, "setInternalEventRouteDest",
                    ExprDotMethod(symbols.GetAddInitSvc(method), EPStatementInitServicesConstants.GETINTERNALEVENTROUTEDEST))
                .ExprDotMethod(
                    queryMethod, "setTableAccesses",
                    ExprTableEvalStrategyUtil.CodegenInitMap(tableAccessForges, this.GetType(), method, symbols, classScope))
                .ExprDotMethod(queryMethod, "setHasTableAccess", Constant(hasTableAccess));
            MakeInlineSpecificSetter(queryMethod, method, symbols, classScope);
            method.Block.MethodReturn(queryMethod);
        }
    }
} // end of namespace