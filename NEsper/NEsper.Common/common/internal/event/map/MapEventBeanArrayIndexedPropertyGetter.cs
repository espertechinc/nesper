///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
	/// <summary>
	/// Getter for array events.
	/// </summary>
	public class MapEventBeanArrayIndexedPropertyGetter : MapEventPropertyGetter {
	    private readonly string propertyName;
	    private readonly int index;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="propertyName">property name</param>
	    /// <param name="index">array index</param>
	    public MapEventBeanArrayIndexedPropertyGetter(string propertyName, int index) {
	        this.propertyName = propertyName;
	        this.index = index;
	    }

	    public object GetMap(IDictionary<string, object> map) {
	        // If the map does not contain the key, this is allowed and represented as null
	        EventBean[] wrapper = (EventBean[]) map.Get(propertyName);
	        return BaseNestableEventUtil.GetBNArrayPropertyUnderlying(wrapper, index);
	    }

	    private CodegenMethod GetMapCodegen(CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return codegenMethodScope.MakeChild(typeof(object), this.GetType(), codegenClassScope).AddParam(typeof(IDictionary<object, object>), "map").Block
	                .DeclareVar(typeof(EventBean[]), "wrapper", Cast(typeof(EventBean[]), ExprDotMethod(@Ref("map"), "get", Constant(propertyName))))
	                .MethodReturn(StaticMethod(typeof(BaseNestableEventUtil), "getBNArrayPropertyUnderlying", @Ref("wrapper"), Constant(index)));
	    }

	    public bool IsMapExistsProperty(IDictionary<string, object> map) {
	        return true;
	    }

	    public object Get(EventBean obj) {
	        return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(obj));
	    }

	    public bool IsExistsProperty(EventBean eventBean) {
	        return true; // Property exists as the property is not dynamic (unchecked)
	    }

	    public object GetFragment(EventBean obj) {
	        IDictionary<string, object> map = BaseNestableEventUtil.CheckedCastUnderlyingMap(obj);
	        EventBean[] wrapper = (EventBean[]) map.Get(propertyName);
	        return BaseNestableEventUtil.GetBNArrayPropertyBean(wrapper, index);
	    }

	    private CodegenMethod GetFragmentCodegen(CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return codegenMethodScope.MakeChild(typeof(object), this.GetType(), codegenClassScope).AddParam(typeof(IDictionary<object, object>), "map").Block
	                .DeclareVar(typeof(EventBean[]), "wrapper", Cast(typeof(EventBean[]), ExprDotMethod(@Ref("map"), "get", Constant(propertyName))))
	                .MethodReturn(StaticMethod(typeof(BaseNestableEventUtil), "getBNArrayPropertyBean", @Ref("wrapper"), Constant(index)));
	    }

	    public CodegenExpression EventBeanGetCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return UnderlyingGetCodegen(CastUnderlying(typeof(IDictionary<object, object>), beanExpression), codegenMethodScope, codegenClassScope);
	    }

	    public CodegenExpression EventBeanExistsCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ConstantTrue();
	    }

	    public CodegenExpression EventBeanFragmentCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return UnderlyingFragmentCodegen(CastUnderlying(typeof(IDictionary<object, object>), beanExpression), codegenMethodScope, codegenClassScope);
	    }

	    public CodegenExpression UnderlyingGetCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return LocalMethod(GetMapCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
	    }

	    public CodegenExpression UnderlyingExistsCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ConstantTrue();
	    }

	    public CodegenExpression UnderlyingFragmentCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return LocalMethod(GetFragmentCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
	    }
	}
} // end of namespace