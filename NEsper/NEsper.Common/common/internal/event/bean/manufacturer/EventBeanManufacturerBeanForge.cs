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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.instantiator;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
	/// <summary>
	/// Factory for event beans created and populate anew from a set of values.
	/// </summary>
	public class EventBeanManufacturerBeanForge : EventBeanManufacturerForge {
	    private readonly static ILog log = LogManager.GetLogger(typeof(EventBeanManufacturerBeanForge));

	    private readonly BeanInstantiatorForge beanInstantiator;
	    private readonly BeanEventType beanEventType;
	    private readonly WriteablePropertyDescriptor[] properties;
	    private readonly ImportService _importService;
	    private readonly MethodInfo[] writeMethodsReflection;
	    private readonly bool hasPrimitiveTypes;
	    private readonly bool[] primitiveType;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="beanEventType">target type</param>
	    /// <param name="properties">written properties</param>
	    /// <param name="importService">for resolving write methods</param>
	    /// <throws>EventBeanManufactureException if the write method lookup fail</throws>
	    public EventBeanManufacturerBeanForge(BeanEventType beanEventType,
	                                          WriteablePropertyDescriptor[] properties,
	                                          ImportService importService
	    )
	            {
	        this.beanEventType = beanEventType;
	        this.properties = properties;
	        this._importService = importService;

	        beanInstantiator = BeanInstantiatorFactory.MakeInstantiator(beanEventType, importService);

	        writeMethodsReflection = new MethodInfo[properties.Length];

	        bool primitiveTypeCheck = false;
	        primitiveType = new bool[properties.Length];
	        for (int i = 0; i < properties.Length; i++) {
	            writeMethodsReflection[i] = properties[i].WriteMethod;
	            primitiveType[i] = properties[i].Type.IsPrimitive;
	            primitiveTypeCheck |= primitiveType[i];
	        }
	        hasPrimitiveTypes = primitiveTypeCheck;
	    }

	    public EventBeanManufacturer GetManufacturer(EventBeanTypedEventFactory eventBeanTypedEventFactory) {
	        return new EventBeanManufacturerBean(beanEventType, eventBeanTypedEventFactory, properties, _importService);
	    }

	    public CodegenExpression Make(CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        CodegenMethod init = codegenClassScope.PackageScope.InitMethod;

	        CodegenExpressionField factory = codegenClassScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
	        CodegenExpressionField beanType = codegenClassScope.AddFieldUnshared(true, typeof(EventType), EventTypeUtility.ResolveTypeCodegen(beanEventType, EPStatementInitServicesConstants.REF));

	        CodegenExpressionNewAnonymousClass manufacturer = NewAnonymousClass(init.Block, typeof(EventBeanManufacturer));

	        CodegenMethod makeUndMethod = CodegenMethod.MakeParentNode(typeof(object), this.GetType(), codegenClassScope).AddParam(typeof(object[]), "properties");
	        manufacturer.AddMethod("makeUnderlying", makeUndMethod);
	        MakeUnderlyingCodegen(makeUndMethod, codegenClassScope);

	        CodegenMethod makeMethod = CodegenMethod.MakeParentNode(typeof(EventBean), this.GetType(), codegenClassScope).AddParam(typeof(object[]), "properties");
	        manufacturer.AddMethod("make", makeMethod);
	        makeMethod.Block
	                .DeclareVar(typeof(object), "und", LocalMethod(makeUndMethod, @Ref("properties")))
	                .MethodReturn(ExprDotMethod(factory, "adapterForTypedBean", @Ref("und"), beanType));

	        return codegenClassScope.AddFieldUnshared(true, typeof(EventBeanManufacturer), manufacturer);
	    }

	    private void MakeUnderlyingCodegen(CodegenMethod method, CodegenClassScope codegenClassScope) {
	        method.Block
	                .DeclareVar(beanEventType.UnderlyingType, "und", Cast(beanEventType.UnderlyingType, beanInstantiator.Make(method, codegenClassScope)))
	                .DeclareVar(typeof(object), "value", ConstantNull());

	        for (int i = 0; i < writeMethodsReflection.Length; i++) {
	            method.Block.AssignRef("value", ArrayAtIndex(@Ref("properties"), Constant(i)));

	            Type targetType = writeMethodsReflection[i].ParameterTypes[0];
	            CodegenExpression value;
	            if (targetType.IsPrimitive) {
	                SimpleTypeCaster caster = SimpleTypeCasterFactory.GetCaster(typeof(object), targetType);
	                value = caster.Codegen(@Ref("value"), typeof(object), method, codegenClassScope);
	            } else {
	                value = Cast(targetType, @Ref("value"));
	            }
	            CodegenExpression set = ExprDotMethod(@Ref("und"), writeMethodsReflection[i].Name, value);
	            if (primitiveType[i]) {
	                method.Block.IfRefNotNull("value").Expression(set).BlockEnd();
	            } else {
	                method.Block.Expression(set);
	            }
	        }
	        method.Block.MethodReturn(@Ref("und"));
	    }
	}
} // end of namespace