///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.filterspec.FilterSpecParam;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     Filter parameter value defining the event property to filter, the filter operator, and the filter value.
    /// </summary>
    [Serializable]
    public class FilterValueSetParamImpl : FilterValueSetParam
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="lookupable">stuff to use to interrogate</param>
        /// <param name="filterOperator">operator to apply</param>
        /// <param name="filterValue">value to look for</param>
        public FilterValueSetParamImpl(
            ExprFilterSpecLookupable lookupable,
            FilterOperator filterOperator,
            object filterValue)
        {
            Lookupable = lookupable;
            FilterOperator = filterOperator;
            FilterForValue = filterValue;
        }

        public ExprFilterSpecLookupable Lookupable { get; }

        public FilterOperator FilterOperator { get; }

        public object FilterForValue { get; }

        public void AppendTo(TextWriter writer)
        {
            Lookupable.AppendTo(writer);
            writer.Write(FilterOperator.GetTextualOp());
            writer.Write(FilterForValue?.ToString() ?? "null");
        }

        public override string ToString()
        {
            return "FilterValueSetParamImpl{" +
                   "lookupable='" +
                   Lookupable +
                   '\'' +
                   ", filterOperator=" +
                   FilterOperator +
                   ", filterValue=" +
                   FilterForValue +
                   '}';
        }

        public static CodegenExpression CodegenNew(CodegenExpression filterForValue)
        {
            return NewInstance<FilterValueSetParamImpl>(REF_LOOKUPABLE, REF_FILTEROPERATOR, filterForValue);
        }
    }
} // end of namespace