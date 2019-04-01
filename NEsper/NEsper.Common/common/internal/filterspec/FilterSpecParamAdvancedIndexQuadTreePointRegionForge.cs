///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterSpecParamAdvancedIndexQuadTreePointRegionForge : FilterSpecParamForge
    {
        private readonly FilterSpecParamFilterForEvalDoubleForge xEval;
        private readonly FilterSpecParamFilterForEvalDoubleForge yEval;

        public FilterSpecParamAdvancedIndexQuadTreePointRegionForge(
            ExprFilterSpecLookupableForge lookupable, FilterOperator filterOperator,
            FilterSpecParamFilterForEvalDoubleForge xEval, FilterSpecParamFilterForEvalDoubleForge yEval) : base(
            lookupable, filterOperator)
        {
            this.xEval = xEval;
            this.yEval = yEval;
        }

        public override CodegenMethod MakeCodegen(
            CodegenClassScope classScope, CodegenMethodScope parent, SAIFFInitializeSymbolWEventType symbols)
        {
            var method = parent.MakeChild(
                typeof(FilterSpecParamAdvancedIndexQuadTreePointRegion), GetType(), classScope);
            method.Block
                .DeclareVar(
                    typeof(ExprFilterSpecLookupable), "lookupable",
                    LocalMethod(lookupable.MakeCodegen(method, symbols, classScope)))
                .DeclareVar(typeof(FilterOperator), "op", EnumValue(typeof(FilterOperator), filterOperator.Name()))
                .DeclareVar(
                    typeof(FilterSpecParamAdvancedIndexQuadTreePointRegion), "fpai",
                    NewInstance(typeof(FilterSpecParamAdvancedIndexQuadTreePointRegion), Ref("lookupable"), Ref("op")))
                .ExprDotMethod(Ref("fpai"), "setxEval", MakeAnonymous(xEval, GetType(), classScope, method))
                .ExprDotMethod(Ref("fpai"), "setyEval", MakeAnonymous(yEval, GetType(), classScope, method))
                .MethodReturn(Ref("fpai"));
            return method;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) {
                return true;
            }

            if (!(obj is FilterSpecParamAdvancedIndexQuadTreePointRegionForge)) {
                return false;
            }

            var other = (FilterSpecParamAdvancedIndexQuadTreePointRegionForge) obj;
            if (!base.Equals(other)) {
                return false;
            }

            return xEval.Equals(other.xEval) &&
                   yEval.Equals(other.yEval);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
} // end of namespace