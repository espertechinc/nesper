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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.@event.variant.VariantEventPropertyGetterAny;

namespace com.espertech.esper.common.@internal.@event.variant
{
	public class VariantEventPropertyGetterAnyWCast : EventPropertyGetterSPI {
	    private readonly VariantEventType variantEventType;
	    private readonly string propertyName;
	    private readonly SimpleTypeCaster caster;

	    public VariantEventPropertyGetterAnyWCast(VariantEventType variantEventType, string propertyName, SimpleTypeCaster caster) {
	        this.variantEventType = variantEventType;
	        this.propertyName = propertyName;
	        this.caster = caster;
	    }

	    public object Get(EventBean eventBean) {
	        object value = VariantEventPropertyGetterAny.VariantGet(eventBean, variantEventType.VariantPropertyGetterCache, propertyName);
	        if (value == null) {
	            return null;
	        }
	        return caster.Cast(value);
	    }

	    private CodegenMethod GetCodegen(CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        CodegenExpressionField cache = codegenClassScope.AddOrGetFieldSharable(new VariantPropertyGetterCacheCodegenField(variantEventType));
	        CodegenMethod method = codegenMethodScope.MakeChild(typeof(object), this.GetType(), codegenClassScope).AddParam(typeof(EventBean), "eventBean");
	        method.Block
	                .DeclareVar(typeof(object), "value", StaticMethod(typeof(VariantEventPropertyGetterAny), "variantGet", @Ref("eventBean"), cache, Constant(propertyName)))
	                .MethodReturn(caster.Codegen(@Ref("value"), typeof(object), method, codegenClassScope));
	        return method;
	    }

	    public bool IsExistsProperty(EventBean eventBean) {
	        return VariantEventPropertyGetterAny.VariantExists(eventBean, variantEventType.VariantPropertyGetterCache, propertyName);
	    }

	    public object GetFragment(EventBean eventBean) {
	        return null;
	    }

	    public CodegenExpression EventBeanGetCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return LocalMethod(GetCodegen(codegenMethodScope, codegenClassScope), beanExpression);
	    }

	    public CodegenExpression EventBeanExistsCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        CodegenExpressionField cache = codegenClassScope.AddOrGetFieldSharable(new VariantPropertyGetterCacheCodegenField(variantEventType));
	        return StaticMethod(typeof(VariantEventPropertyGetterAny), "variantExists", beanExpression, cache, Constant(propertyName));
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
	        throw VariantImplementationNotProvided();
	    }
	}
} // end of namespace