///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecPlanForge;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecPlanPathTripletForge
    {
        public FilterSpecPlanPathTripletForge(FilterSpecParamForge param,
            ExprNode tripletConfirm)
        {
            Param = param;
            TripletConfirm = tripletConfirm;
        }

        public FilterSpecParamForge Param { get; }

        public ExprNode TripletConfirm { get; set; }

        public bool EqualsFilter(FilterSpecPlanPathTripletForge other)
        {
            if (!ExprNodeUtilityCompare.DeepEqualsNullChecked(TripletConfirm, other.TripletConfirm, true)) {
                return false;
            }

            return Param.Equals(other.Param);
        }

        public CodegenMethod Codegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(FilterSpecPlanPathTriplet), typeof(FilterSpecParamForge), classScope);
            method.Block
                .DeclareVar(typeof(FilterSpecPlanPathTriplet), "triplet", NewInstance(typeof(FilterSpecPlanPathTriplet)))
                .ExprDotMethod(Ref("triplet"), "setParam", LocalMethod(Param.MakeCodegen(classScope, method, symbols)))
                .ExprDotMethod(Ref("triplet"), "setTripletConfirm", OptionalEvaluator(TripletConfirm, method, classScope))
                .MethodReturn(Ref("triplet"));
            return method;
        }

        protected void AppendFilterPlanTriplet(
            int indexForge,
            StringBuilder stringBuilder)
        {
            stringBuilder
                .Append("    -triplet #")
                .Append(indexForge)
                .Append(NEWLINE);

            if (TripletConfirm != null) {
                stringBuilder
                    .Append("      -triplet-confirm-expression: ")
                    .Append(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(TripletConfirm))
                    .Append(NEWLINE);
            }

            Param.AppendFilterPlanParam(stringBuilder);
        }
    }
} // end of namespace