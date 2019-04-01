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
	/// Property getter for methods using Java's vanilla reflection.
	/// </summary>
	public class ReflectionPropMethodGetter : BaseNativePropertyGetter , BeanEventPropertyGetter {
	    private readonly MethodInfo method;

	    public ReflectionPropMethodGetter(MethodInfo method, EventBeanTypedEventFactory eventBeanTypedEventFactory, BeanEventTypeFactory beanEventTypeFactory) : base(eventBeanTypedEventFactory, beanEventTypeFactory, method.ReturnType, TypeHelper.GetGenericReturnType(method, false))
	        {
	        this.method = method;
	    }

	    public object GetBeanProp(object @object) {
	        try {
	            return method.Invoke(@object, (object[]) null);
	        } catch (ArgumentException e) {
	            throw PropertyUtility.GetArgumentException(method, e);
	        } catch (MemberAccessException e) {
	            throw PropertyUtility.GetMemberAccessException(method, e);
	        } catch (TargetException e) {
	            throw PropertyUtility.GetTargetException(method, e);
	        }
	    }

	    public bool IsBeanExistsProperty(object @object) {
	        return true;
	    }

	    public override object Get(EventBean obj) {
	        object underlying = obj.Underlying;
	        return GetBeanProp(underlying);
	    }

	    public override string ToString() {
	        return "ReflectionPropMethodGetter " +
	                "method=" + method.ToGenericString();
	    }

	    public override bool IsExistsProperty(EventBean eventBean) {
	        return true; // Property exists as the property is not dynamic (unchecked)
	    }

	    public override Type BeanPropType
	    {
	        get => method.ReturnType;
	    }

	    public override Type TargetType
	    {
	        get => method.DeclaringType;
	    }

	    public override CodegenExpression EventBeanGetCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return UnderlyingGetCodegen(CastUnderlying(TargetType, beanExpression), codegenMethodScope, codegenClassScope);
	    }

	    public override CodegenExpression EventBeanExistsCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ConstantTrue();
	    }

	    public override CodegenExpression UnderlyingGetCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ExprDotMethod(underlyingExpression, method.Name);
	    }

	    public override CodegenExpression UnderlyingExistsCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ConstantTrue();
	    }
	}
} // end of namespace