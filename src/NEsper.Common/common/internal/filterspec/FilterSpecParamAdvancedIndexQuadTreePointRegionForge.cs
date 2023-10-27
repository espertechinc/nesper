///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterSpecParamAdvancedIndexQuadTreePointRegionForge : FilterSpecParamForge
    {
        private readonly FilterSpecParamFilterForEvalDoubleForge _xEval;
        private readonly FilterSpecParamFilterForEvalDoubleForge _yEval;

        public FilterSpecParamAdvancedIndexQuadTreePointRegionForge(
            ExprFilterSpecLookupableForge lookupable,
            FilterOperator filterOperator,
            FilterSpecParamFilterForEvalDoubleForge xEval,
            FilterSpecParamFilterForEvalDoubleForge yEval)
            : base(
                lookupable,
                filterOperator)
        {
            _xEval = xEval;
            _yEval = yEval;
        }

        public override CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols)
        {
            var method = parent.MakeChild(
                typeof(FilterSpecParamAdvancedIndexQuadTreePointRegion),
                GetType(),
                classScope);
            method.Block
                .DeclareVar<ExprFilterSpecLookupable>(
                    "lookupable",
                    LocalMethod(lookupable.MakeCodegen(method, symbols, classScope)))
                .DeclareVar<FilterOperator>(
                    "filterOperator",
                    EnumValue(typeof(FilterOperator), filterOperator.GetName()))
                .DeclareVar<FilterSpecParamAdvancedIndexQuadTreePointRegion>(
                    "fpai",
                    NewInstance<FilterSpecParamAdvancedIndexQuadTreePointRegion>(
                        Ref("lookupable"),
                        Ref("filterOperator")))
                .SetProperty(
                    Ref("fpai"),
                    "XEval",
                    FilterSpecParamFilterForEvalDoubleForgeHelper.MakeAnonymous(_xEval, GetType(), classScope, method))
                .SetProperty(
                    Ref("fpai"),
                    "YEval",
                    FilterSpecParamFilterForEvalDoubleForgeHelper.MakeAnonymous(_yEval, GetType(), classScope, method))
                .MethodReturn(Ref("fpai"));
            return LocalMethod(method);
        }

        public override bool Equals(object obj)
        {
            if (this == obj) {
                return true;
            }

            if (!(obj is FilterSpecParamAdvancedIndexQuadTreePointRegionForge other)) {
                return false;
            }

            if (!base.Equals(other)) {
                return false;
            }

            return _xEval.Equals(other._xEval) &&
                   _yEval.Equals(other._yEval);
        }

        protected bool Equals(FilterSpecParamAdvancedIndexQuadTreePointRegionForge other)
        {
            return Equals(_xEval, other._xEval) && Equals(_yEval, other._yEval);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_xEval, _yEval);
        }

        public override void ValueExprToString(
            StringBuilder @out,
            int i)
        {
            @out.Append("Point-Region ");
            @out.Append("x ");
            _xEval.ValueToString(@out);
            @out.Append("y ");
            _yEval.ValueToString(@out);
        }
    }
} // end of namespace