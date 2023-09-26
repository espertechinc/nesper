///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     A Double-typed value as a filter parameter representing a range.
    /// </summary>
    public class FilterForEvalConstantDoubleForge : FilterSpecParamFilterForEvalDoubleForge
    {
        private readonly double _doubleValue;

        public FilterForEvalConstantDoubleForge(double doubleValue)
        {
            _doubleValue = doubleValue;
        }

        public CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            return Constant(_doubleValue);
        }

        public object GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext evaluatorContext)
        {
            return _doubleValue;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) {
                return true;
            }

            if (!(obj is FilterForEvalConstantDoubleForge other)) {
                return false;
            }

            return other._doubleValue == _doubleValue;
        }

        public override int GetHashCode()
        {
            return _doubleValue.GetHashCode();
        }


        public void ValueToString(StringBuilder @out)
        {
            @out.Append("double-value ")
                .Append(_doubleValue);
        }
    }
} // end of namespace