///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompiler;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecPlanForge
    {
        public static readonly FilterSpecPlanForge EMPTY = new FilterSpecPlanForge(
            Array.Empty<FilterSpecPlanPathForge>(),
            null,
            null,
            null);

        public FilterSpecPlanForge(
            FilterSpecPlanPathForge[] paths,
            ExprNode filterConfirm,
            ExprNode filterNegate,
            MatchedEventConvertorForge convertorForge)
        {
            Paths = paths;
            FilterConfirm = filterConfirm;
            FilterNegate = filterNegate;
            ConvertorForge = convertorForge;
        }

        public FilterSpecPlanPathForge[] Paths { get; }

        public ExprNode FilterConfirm { get; set; }

        public ExprNode FilterNegate { get; }

        public MatchedEventConvertorForge ConvertorForge { get; }

        public bool EqualsFilter(FilterSpecPlanForge other)
        {
            if (Paths.Length != other.Paths.Length) {
                return false;
            }

            for (var i = 0; i < Paths.Length; i++) {
                var myPath = Paths[i];
                var otherPath = other.Paths[i];
                if (!myPath.EqualsFilter(otherPath)) {
                    return false;
                }
            }

            if (!ExprNodeUtilityCompare.DeepEqualsNullChecked(FilterConfirm, other.FilterConfirm, true)) {
                return false;
            }

            return ExprNodeUtilityCompare.DeepEqualsNullChecked(FilterNegate, other.FilterNegate, true);
        }

        public CodegenExpression CodegenWithEventType(
            CodegenMethodScope parent,
            CodegenExpression eventType,
            CodegenExpression stmtInitSvc,
            CodegenClassScope classScope)
        {
            if (Paths.Length == 0) {
                return PublicConstValue(typeof(FilterSpecPlan), "EMPTY_PLAN");
            }

            var method = CodegenWithEventType(parent, classScope);
            return LocalMethod(method, eventType, stmtInitSvc);
        }

        private CodegenMethod CodegenWithEventType(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var symbolsWithType = new SAIFFInitializeSymbolWEventType();
            var method = parent
                .MakeChildWithScope(typeof(FilterSpecPlan), typeof(FilterSpecParamForge), symbolsWithType, classScope)
                .AddParam<EventType>(SAIFFInitializeSymbolWEventType.REF_EVENTTYPE.Ref)
                .AddParam<EPStatementInitServices>(SAIFFInitializeSymbol.REF_STMTINITSVC.Ref);
            if (Paths.Length == 0) {
                method.Block.MethodReturn(PublicConstValue(typeof(FilterSpecPlan), "EMPTY_PLAN"));
                return method;
            }

            method.Block.DeclareVar<FilterSpecPlanPath[]>(
                "paths",
                NewArrayByLength(typeof(FilterSpecPlanPath), Constant(Paths.Length)));
            for (var i = 0; i < Paths.Length; i++) {
                method.Block.AssignArrayElement(
                    "paths",
                    Constant(i),
                    LocalMethod(Paths[i].Codegen(method, symbolsWithType, classScope)));
            }

            method.Block
                .DeclareVarNewInstance(typeof(FilterSpecPlan), "plan")
                .ExprDotMethod(Ref("plan"), "setPaths", Ref("paths"))
                .ExprDotMethod(Ref("plan"), "setFilterConfirm", OptionalEvaluator(FilterConfirm, method, classScope))
                .ExprDotMethod(Ref("plan"), "setFilterNegate", OptionalEvaluator(FilterNegate, method, classScope))
                .ExprDotMethod(
                    Ref("plan"),
                    "setConvertor",
                    ConvertorForge == null ? ConstantNull() : ConvertorForge.MakeAnonymous(method, classScope))
                .ExprDotMethod(Ref("plan"), "initialize")
                .MethodReturn(Ref("plan"));
            return method;
        }

        internal static CodegenExpression OptionalEvaluator(
            ExprNode node,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return node == null
                ? ConstantNull()
                : ExprNodeUtilityCodegen.CodegenEvaluator(node.Forge, method, typeof(FilterSpecPlanForge), classScope);
        }

        public static FilterSpecPlanPathForge MakePathFromTriplets(
            ICollection<FilterSpecPlanPathTripletForge> tripletsColl,
            ExprNode control)
        {
            var triplets = tripletsColl.ToArray();
            return new FilterSpecPlanPathForge(triplets, control);
        }

        public static FilterSpecPlanForge MakePlanFromTriplets(
            ICollection<FilterSpecPlanPathTripletForge> triplets,
            ExprNode topLevelNegation,
            FilterSpecCompilerArgs args)
        {
            var path = MakePathFromTriplets(triplets, null);
            var convertor = new MatchedEventConvertorForge(
                args.taggedEventTypes,
                args.arrayEventTypes,
                args.allTagNamesOrdered,
                null,
                true);
            return new FilterSpecPlanForge(new[] { path }, null, topLevelNegation, convertor);
        }

        public void AppendPlan(StringBuilder buf)
        {
            if (FilterNegate != null) {
                LogFilterPlanExpr(buf, "filter-negate-expression", FilterNegate);
            }

            if (FilterConfirm != null) {
                LogFilterPlanExpr(buf, "filter-confirm-expression", FilterConfirm);
            }

            for (var i = 0; i < Paths.Length; i++) {
                Paths[i].AppendFilterPlanPath(i, buf);
            }
        }

        private static void LogFilterPlanExpr(
            StringBuilder buf,
            string name,
            ExprNode exprNode)
        {
            buf.Append("  -")
                .Append(name)
                .Append(": ")
                .Append(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(exprNode))
                .Append(NEWLINE);
        }
    }
} // end of namespace