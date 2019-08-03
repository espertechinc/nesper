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
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
    /// <summary>
    ///     Factory for event beans created and populate anew from a set of values.
    /// </summary>
    public class EventBeanManufacturerBeanForge : EventBeanManufacturerForge
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(EventBeanManufacturerBeanForge));
        private readonly ImportService _importService;
        private readonly BeanEventType beanEventType;

        private readonly BeanInstantiatorForge beanInstantiator;
        private readonly bool hasPrimitiveTypes;
        private readonly bool[] primitiveType;
        private readonly WriteablePropertyDescriptor[] properties;
        private readonly MethodInfo[] writeMethodsReflection;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="beanEventType">target type</param>
        /// <param name="properties">written properties</param>
        /// <param name="importService">for resolving write methods</param>
        /// <throws>EventBeanManufactureException if the write method lookup fail</throws>
        public EventBeanManufacturerBeanForge(
            BeanEventType beanEventType,
            WriteablePropertyDescriptor[] properties,
            ImportService importService
        )
        {
            this.beanEventType = beanEventType;
            this.properties = properties;
            _importService = importService;

            beanInstantiator = BeanInstantiatorFactory.MakeInstantiator(beanEventType, importService);

            writeMethodsReflection = new MethodInfo[properties.Length];

            var primitiveTypeCheck = false;
            primitiveType = new bool[properties.Length];
            for (var i = 0; i < properties.Length; i++) {
                writeMethodsReflection[i] = properties[i].WriteMethod;
                primitiveType[i] = properties[i].PropertyType.IsPrimitive;
                primitiveTypeCheck |= primitiveType[i];
            }

            hasPrimitiveTypes = primitiveTypeCheck;
        }

        public EventBeanManufacturer GetManufacturer(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new EventBeanManufacturerBean(beanEventType, eventBeanTypedEventFactory, properties, _importService);
        }

        public CodegenExpression Make(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var init = codegenClassScope.NamespaceScope.InitMethod;

            var factory = codegenClassScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var beanType = codegenClassScope.AddFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(beanEventType, EPStatementInitServicesConstants.REF));

            var manufacturer = NewAnonymousClass(init.Block, typeof(EventBeanManufacturer));

            var makeUndMethod = CodegenMethod.MakeParentNode(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "properties");
            manufacturer.AddMethod("MakeUnderlying", makeUndMethod);
            MakeUnderlyingCodegen(makeUndMethod, codegenClassScope);

            var makeMethod = CodegenMethod.MakeParentNode(typeof(EventBean), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "properties");
            manufacturer.AddMethod("Make", makeMethod);
            makeMethod.Block
                .DeclareVar<object>("und", LocalMethod(makeUndMethod, Ref("properties")))
                .MethodReturn(ExprDotMethod(factory, "AdapterForTypedBean", Ref("und"), beanType));

            return codegenClassScope.AddFieldUnshared(true, typeof(EventBeanManufacturer), manufacturer);
        }

        private void MakeUnderlyingCodegen(
            CodegenMethod method,
            CodegenClassScope codegenClassScope)
        {
            method.Block
                .DeclareVar(
                    beanEventType.UnderlyingType,
                    "und",
                    Cast(beanEventType.UnderlyingType, beanInstantiator.Make(method, codegenClassScope)))
                .DeclareVar<object>("value", ConstantNull());

            for (var i = 0; i < writeMethodsReflection.Length; i++) {
                method.Block.AssignRef("value", ArrayAtIndex(Ref("properties"), Constant(i)));

                Type targetType = writeMethodsReflection[i].GetParameters()[0].ParameterType;
                CodegenExpression value;
                if (targetType.IsPrimitive) {
                    var caster = SimpleTypeCasterFactory.GetCaster(typeof(object), targetType);
                    value = caster.Codegen(Ref("value"), typeof(object), method, codegenClassScope);
                }
                else {
                    value = Cast(targetType, Ref("value"));
                }

                var set = ExprDotMethod(Ref("und"), writeMethodsReflection[i].Name, value);
                if (primitiveType[i]) {
                    method.Block.IfRefNotNull("value").Expression(set).BlockEnd();
                }
                else {
                    method.Block.Expression(set);
                }
            }

            method.Block.MethodReturn(Ref("und"));
        }
    }
} // end of namespace