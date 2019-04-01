///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
	/// <summary>
	/// Property getter for fields using Java's vanilla reflection.
	/// </summary>
	public class ReflectionPropFieldGetter : BaseNativePropertyGetter , BeanEventPropertyGetter {
	    private readonly FieldInfo field;

	    public ReflectionPropFieldGetter(
	        FieldInfo field, 
	        EventBeanTypedEventFactory eventBeanTypedEventFactory, 
	        BeanEventTypeFactory beanEventTypeFactory) 
	        : base(eventBeanTypedEventFactory, beanEventTypeFactory, field.FieldType, TypeHelper.GetGenericFieldType(field, true))
	    {
	        this.field = field;
	    }

	    public object GetBeanProp(object @object) {
	        try {
	            return field.GetValue(@object);
	        } catch (ArgumentException e) {
	            throw PropertyUtility.GetArgumentException(field, e);
	        } catch (MemberAccessException e) {
	            throw PropertyUtility.GetMemberAccessException(field, e);
	        }
	    }

	    public bool IsBeanExistsProperty(object @object) {
	        return true; // Property exists as the property is not dynamic (unchecked)
	    }

	    public override object Get(EventBean obj) {
	        object underlying = obj.Underlying;
	        return GetBeanProp(underlying);
	    }

	    public override string ToString() {
	        return "ReflectionPropFieldGetter " +
	                "field=" + field.ToGenericString();
	    }

	    public override bool IsExistsProperty(EventBean eventBean) {
	        return true; // Property exists as the property is not dynamic (unchecked)
	    }

	    public override Type BeanPropType
	    {
	        get => field.FieldType;
	    }

	    public override Type TargetType
	    {
	        get => field.DeclaringType;
	    }

	    public override CodegenExpression EventBeanGetCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return UnderlyingGetCodegen(CastUnderlying(TargetType, beanExpression), codegenMethodScope, codegenClassScope);
	    }

	    public override CodegenExpression EventBeanExistsCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ConstantTrue();
	    }

	    public override CodegenExpression UnderlyingGetCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ExprDotName(underlyingExpression, field.Name);
	    }

	    public override CodegenExpression UnderlyingExistsCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ConstantTrue();
	    }
	}
} // end of namespace