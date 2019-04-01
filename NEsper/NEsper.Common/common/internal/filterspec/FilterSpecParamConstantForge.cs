///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
	/// <summary>
	/// This class represents a single, constant value filter parameter in an <seealso cref="FilterSpecActivatable" /> filter specification.
	/// </summary>
	public sealed class FilterSpecParamConstantForge : FilterSpecParamForge {
	    private readonly object _filterConstant;

	    public FilterSpecParamConstantForge(ExprFilterSpecLookupableForge lookupable, FilterOperator filterOperator, object filterConstant)
	           
	           	 : base(lookupable, filterOperator)
	           {
	        this.filterConstant = filterConstant;

	        if (filterOperator.IsRangeOperator) {
	            throw new ArgumentException("Illegal filter operator " + filterOperator + " supplied to " +
	                    "constant filter parameter");
	        }
	    }

	    public override CodegenMethod MakeCodegen(CodegenClassScope classScope, CodegenMethodScope parent, SAIFFInitializeSymbolWEventType symbols) {
	        CodegenMethod method = parent.MakeChild(typeof(FilterSpecParam), typeof(FilterSpecParamConstantForge), classScope);
	        method.Block
	                .DeclareVar(typeof(ExprFilterSpecLookupable), "lookupable", LocalMethod(lookupable.MakeCodegen(method, symbols, classScope)))
	                .DeclareVar(typeof(FilterOperator), "op", EnumValue(typeof(FilterOperator), filterOperator.Name()));

	        CodegenExpressionNewAnonymousClass inner = NewAnonymousClass(method.Block, typeof(FilterSpecParam), Arrays.AsList(@Ref("lookupable"), @Ref("op")));
	        CodegenMethod getFilterValue = CodegenMethod.MakeParentNode(typeof(object), this.GetType(), classScope).AddParam(FilterSpecParam.GET_FILTER_VALUE_FP);
	        inner.AddMethod("getFilterValue", getFilterValue);
	        getFilterValue.Block.MethodReturn(Constant(filterConstant));

	        method.Block.MethodReturn(inner);
	        return method;
	    }

	    /// <summary>
	    /// Returns the constant value.
	    /// </summary>
	    /// <returns>constant value</returns>
	    public object FilterConstant {
	        get => filterConstant;	    }

	    public override string ToString() {
	        return base.ToString() + " filterConstant=" + filterConstant;
	    }

	    public override bool Equals(object o) {
	        if (this == o) return true;
	        if (o == null || GetType() != o.GetType()) return false;
	        if (!base.Equals(o)) return false;

	        FilterSpecParamConstantForge that = (FilterSpecParamConstantForge) o;

	        if (filterConstant != null ? !filterConstant.Equals(that.filterConstant) : that.filterConstant != null)
	            return false;

	        return true;
	    }

	    public override int GetHashCode() {
	        int result = base.GetHashCode();
	        result = 31 * result + (filterConstant != null ? filterConstant.GetHashCode() : 0);
	        return result;
	    }
	}
} // end of namespace