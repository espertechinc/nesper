///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.variant
{
	public class VariantEventPropertyGetterAny : EventPropertyGetterSPI {
	    private readonly VariantEventType variantEventType;
	    private readonly string propertyName;

	    public VariantEventPropertyGetterAny(VariantEventType variantEventType, string propertyName) {
	        this.variantEventType = variantEventType;
	        this.propertyName = propertyName;
	    }

	    /// <summary>
	    /// NOTE: Code-generation-invoked method, method name and parameter order matters
	    /// </summary>
	    /// <param name="eventBean">bean</param>
	    /// <param name="propertyGetterCache">cache</param>
	    /// <param name="propertyName">name</param>
	    /// <returns>value</returns>
	    /// <throws>PropertyAccessException ex</throws>
	    public static object VariantGet(EventBean eventBean, VariantPropertyGetterCache propertyGetterCache, string propertyName) {
	        VariantEvent variant = (VariantEvent) eventBean;
	        EventPropertyGetter getter = propertyGetterCache.GetGetter(propertyName, variant.UnderlyingEventBean.EventType);
	        if (getter == null) {
	            return null;
	        }
	        object result = getter.Get(variant.UnderlyingEventBean);
	        return result;
	    }

	    /// <summary>
	    /// NOTE: Code-generation-invoked method, method name and parameter order matters
	    /// </summary>
	    /// <param name="eventBean">bean</param>
	    /// <param name="propertyGetterCache">cache</param>
	    /// <param name="propertyName">name</param>
	    /// <returns>value</returns>
	    public static bool VariantExists(EventBean eventBean, VariantPropertyGetterCache propertyGetterCache, string propertyName) {
	        VariantEvent variant = (VariantEvent) eventBean;
	        EventPropertyGetter getter = propertyGetterCache.GetGetter(propertyName, variant.UnderlyingEventBean.EventType);
	        return getter != null && getter.IsExistsProperty(variant.UnderlyingEventBean);
	    }

	    public object Get(EventBean eventBean) {
	        return VariantGet(eventBean, variantEventType.VariantPropertyGetterCache, propertyName);
	    }

	    public bool IsExistsProperty(EventBean eventBean) {
	        return VariantExists(eventBean, variantEventType.VariantPropertyGetterCache, propertyName);
	    }

	    public object GetFragment(EventBean eventBean) {
	        return null; // no fragments provided as the type is not known in advance
	    }

	    public CodegenExpression EventBeanGetCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        CodegenExpressionField cache = codegenClassScope.AddOrGetFieldSharable(new VariantPropertyGetterCacheCodegenField(variantEventType));
	        return StaticMethod(this.GetType(), "variantGet", beanExpression, cache, Constant(propertyName));
	    }

	    public CodegenExpression EventBeanExistsCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        CodegenExpressionField cache = codegenClassScope.AddOrGetFieldSharable(new VariantPropertyGetterCacheCodegenField(variantEventType));
	        return StaticMethod(this.GetType(), "variantExists", beanExpression, cache, Constant(propertyName));
	    }

	    public CodegenExpression EventBeanFragmentCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ConstantNull();
	    }

	    public CodegenExpression UnderlyingGetCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        throw VariantImplementationNotProvided();
	    }

	    public CodegenExpression UnderlyingExistsCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        throw VariantImplementationNotProvided();
	    }

	    public CodegenExpression UnderlyingFragmentCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ConstantNull();
	    }

	    protected internal static UnsupportedOperationException VariantImplementationNotProvided() {
	        return new UnsupportedOperationException("Variant event type does not provide an implementation for underlying get");
	    }
	}
} // end of namespace