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
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.filterspec
{
    public abstract class FilterSpecParamForge
    {
        public static readonly FilterSpecParamForge[] EMPTY_PARAM_ARRAY = new FilterSpecParamForge[0];

        internal readonly FilterOperator filterOperator;

        /// <summary>
        ///     The property name of the filter parameter.
        /// </summary>
        internal readonly ExprFilterSpecLookupableForge lookupable;

        protected FilterSpecParamForge(
            ExprFilterSpecLookupableForge lookupable,
            FilterOperator filterOperator)
        {
            this.lookupable = lookupable;
            this.filterOperator = filterOperator;
        }

        public ExprFilterSpecLookupableForge Lookupable => lookupable;

        public FilterOperator FilterOperator => filterOperator;

        public abstract CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols);

        public abstract void ValueExprToString(StringBuilder @out, int indent);

        public void AppendFilterPlanParam(StringBuilder buf)
        {
            buf.Append("      -lookupable: ")
                .Append(lookupable.Expression)
                .Append(FilterSpecCompiler.NEWLINE);
            buf.Append("      -operator: ")
                .Append(filterOperator.GetTextualOp())
                .Append(FilterSpecCompiler.NEWLINE);
            buf.Append("      -value-expression: ");
            ValueExprToString(buf, 8);
            buf.Append(FilterSpecCompiler.NEWLINE);
        }
    }
} // end of namespace