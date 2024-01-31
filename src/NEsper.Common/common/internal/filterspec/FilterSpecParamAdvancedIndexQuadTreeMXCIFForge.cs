///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterSpecParamAdvancedIndexQuadTreeMXCIFForge : FilterSpecParamForge
    {
        private readonly FilterSpecParamFilterForEvalDoubleForge _heightEval;
        private readonly FilterSpecParamFilterForEvalDoubleForge _widthEval;
        private readonly FilterSpecParamFilterForEvalDoubleForge _xEval;
        private readonly FilterSpecParamFilterForEvalDoubleForge _yEval;

        public FilterSpecParamAdvancedIndexQuadTreeMXCIFForge(
            ExprFilterSpecLookupableForge lookupable,
            FilterOperator filterOperator,
            FilterSpecParamFilterForEvalDoubleForge xEval,
            FilterSpecParamFilterForEvalDoubleForge yEval,
            FilterSpecParamFilterForEvalDoubleForge widthEval,
            FilterSpecParamFilterForEvalDoubleForge heightEval)
            :
            base(lookupable, filterOperator)
        {
            _xEval = xEval;
            _yEval = yEval;
            _widthEval = widthEval;
            _heightEval = heightEval;
        }

        public override CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols)
        {
            var method = parent.MakeChild(typeof(FilterSpecParamAdvancedIndexQuadTreeMXCIF), GetType(), classScope);
            method.Block
                .DeclareVar<ExprFilterSpecLookupable>(
                    "lookupable",
                    LocalMethod(lookupable.MakeCodegen(method, symbols, classScope)))
                .DeclareVar<FilterOperator>(
                    "filterOperator",
                    EnumValue(typeof(FilterOperator), filterOperator.GetName()))
                .DeclareVar<FilterSpecParamAdvancedIndexQuadTreeMXCIF>(
                    "fpai",
                    NewInstance<FilterSpecParamAdvancedIndexQuadTreeMXCIF>(Ref("lookupable"), Ref("filterOperator")))
                .SetProperty(
                    Ref("fpai"),
                    "XEval",
                    FilterSpecParamFilterForEvalDoubleForgeHelper.MakeAnonymous(_xEval, GetType(), classScope, method))
                .SetProperty(
                    Ref("fpai"),
                    "YEval",
                    FilterSpecParamFilterForEvalDoubleForgeHelper.MakeAnonymous(_yEval, GetType(), classScope, method))
                .SetProperty(
                    Ref("fpai"),
                    "WidthEval",
                    FilterSpecParamFilterForEvalDoubleForgeHelper.MakeAnonymous(
                        _widthEval,
                        GetType(),
                        classScope,
                        method))
                .SetProperty(
                    Ref("fpai"),
                    "HeightEval",
                    FilterSpecParamFilterForEvalDoubleForgeHelper.MakeAnonymous(
                        _heightEval,
                        GetType(),
                        classScope,
                        method))
                .MethodReturn(Ref("fpai"));

            return LocalMethod(method);
        }

        public override bool Equals(object obj)
        {
            if (this == obj) {
                return true;
            }

            if (!(obj is FilterSpecParamAdvancedIndexQuadTreeMXCIFForge other)) {
                return false;
            }

            if (!base.Equals(other)) {
                return false;
            }

            return _xEval.Equals(other._xEval) &&
                   _yEval.Equals(other._yEval) &&
                   _widthEval.Equals(other._widthEval) &&
                   _heightEval.Equals(other._heightEval);
        }

        protected bool Equals(FilterSpecParamAdvancedIndexQuadTreeMXCIFForge other)
        {
            return Equals(_heightEval, other._heightEval) &&
                   Equals(_widthEval, other._widthEval) &&
                   Equals(_xEval, other._xEval) &&
                   Equals(_yEval, other._yEval);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = _heightEval != null ? _heightEval.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (_widthEval != null ? _widthEval.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_xEval != null ? _xEval.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_yEval != null ? _yEval.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override void ValueExprToString(
            StringBuilder @out,
            int i)
        {
            @out.Append("MXCIF ");
            @out.Append("x ");
            _xEval.ValueToString(@out);
            @out.Append("y ");
            _yEval.ValueToString(@out);
            @out.Append("w ");
            _widthEval.ValueToString(@out);
            @out.Append("h ");
            _heightEval.ValueToString(@out);
        }
    }
} // end of namespace