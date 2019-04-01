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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
	/// <summary>
	/// Getter for Map-entries with well-defined fragment type.
	/// </summary>
	public class MapArrayPropertyGetter : MapEventPropertyGetter, MapEventPropertyGetterAndIndexed {
	    private readonly string propertyName;
	    private readonly int index;
	    private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
	    private readonly EventType fragmentType;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="propertyNameAtomic">property name</param>
	    /// <param name="index">array index</param>
	    /// <param name="eventBeanTypedEventFactory">factory for event beans and event types</param>
	    /// <param name="fragmentType">type of the entry returned</param>
	    public MapArrayPropertyGetter(string propertyNameAtomic, int index, EventBeanTypedEventFactory eventBeanTypedEventFactory, EventType fragmentType) {
	        this.propertyName = propertyNameAtomic;
	        this.index = index;
	        this.fragmentType = fragmentType;
	        this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
	    }

	    public bool IsMapExistsProperty(IDictionary<string, object> map) {
	        return true;
	    }

	    public object GetMap(IDictionary<string, object> map) {
	        return GetMapInternal(map, index);
	    }

	    public object Get(EventBean eventBean, int index) {
	        IDictionary<string, object> map = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
	        return GetMapInternal(map, index);
	    }

	    public object Get(EventBean obj) {
	        IDictionary<string, object> map = BaseNestableEventUtil.CheckedCastUnderlyingMap(obj);
	        return GetMap(map);
	    }

	    private object GetMapInternal(IDictionary<string, object> map, int index) {
	        object value = map.Get(propertyName);
	        return BaseNestableEventUtil.GetBNArrayValueAtIndexWithNullCheck(value, index);
	    }

	    private CodegenMethod GetMapInternalCodegen(CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return codegenMethodScope.MakeChild(typeof(object), this.GetType(), codegenClassScope).AddParam(typeof(IDictionary<object, object>), "map").AddParam(typeof(int), "index").Block
	                .DeclareVar(typeof(object), "value", ExprDotMethod(@Ref("map"), "get", Constant(propertyName)))
	                .MethodReturn(StaticMethod(typeof(BaseNestableEventUtil), "getBNArrayValueAtIndexWithNullCheck", @Ref("value"), @Ref("index")));
	    }

	    public bool IsExistsProperty(EventBean eventBean) {
	        return true;
	    }

	    public object GetFragment(EventBean obj) {
	        object value = Get(obj);
	        return BaseNestableEventUtil.GetBNFragmentNonPojo(value, fragmentType, eventBeanTypedEventFactory);
	    }

	    private CodegenMethod GetFragmentCodegen(CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        CodegenExpressionField factory = codegenClassScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
	        CodegenExpressionField eventType = codegenClassScope.AddFieldUnshared(true, typeof(EventType), EventTypeUtility.ResolveTypeCodegen(fragmentType, EPStatementInitServicesConstants.REF));
	        return codegenMethodScope.MakeChild(typeof(object), this.GetType(), codegenClassScope).AddParam(typeof(IDictionary<object, object>), "map").Block
	                .DeclareVar(typeof(object), "value", UnderlyingGetCodegen(@Ref("map"), codegenMethodScope, codegenClassScope))
	                .MethodReturn(StaticMethod(typeof(BaseNestableEventUtil), "getBNFragmentNonPojo", @Ref("value"), eventType, factory));
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
	        return LocalMethod(GetMapInternalCodegen(codegenMethodScope, codegenClassScope), underlyingExpression, Constant(index));
	    }

	    public CodegenExpression UnderlyingExistsCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ConstantTrue();
	    }

	    public CodegenExpression UnderlyingFragmentCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return LocalMethod(GetFragmentCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
	    }

	    public CodegenExpression EventBeanGetIndexedCodegen(CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpression beanExpression, CodegenExpression key) {
	        return LocalMethod(GetMapInternalCodegen(codegenMethodScope, codegenClassScope), CastUnderlying(typeof(IDictionary<object, object>), beanExpression), key);
	    }
	}
} // end of namespace