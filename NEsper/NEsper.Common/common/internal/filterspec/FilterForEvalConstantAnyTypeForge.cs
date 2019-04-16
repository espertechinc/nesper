///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    /// Constant value in a list of values following an in-keyword.
    /// </summary>
    public class FilterForEvalConstantAnyTypeForge : FilterSpecParamInValueForge
    {
        private object _constant;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="constant">is the constant value</param>
        public FilterForEvalConstantAnyTypeForge(object constant)
        {
            this._constant = constant;
        }

        public Type ReturnType {
            get => _constant == null ? null : _constant.GetType();
        }

        public bool IsConstant {
            get { return true; }
        }

        /// <summary>
        /// Returns the constant value.
        /// </summary>
        /// <returns>constant</returns>
        public object Constant {
            get => _constant;
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            FilterForEvalConstantAnyTypeForge that = (FilterForEvalConstantAnyTypeForge) o;

            if (_constant != null ? !_constant.Equals(that._constant) : that._constant != null) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return _constant != null ? _constant.GetHashCode() : 0;
        }

        public object GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext evaluatorContext)
        {
            return _constant;
        }

        public CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            return Constant(_constant);
        }
    }
} // end of namespace