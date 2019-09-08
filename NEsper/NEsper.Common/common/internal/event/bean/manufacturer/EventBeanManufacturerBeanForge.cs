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
        private readonly MemberInfo[] writeMembersReflection;

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

            writeMembersReflection = new MemberInfo[properties.Length];

            var primitiveTypeCheck = false;
            primitiveType = new bool[properties.Length];
            for (var i = 0; i < properties.Length; i++) {
                writeMembersReflection[i] = properties[i].WriteMember;
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

            var makeUndLambda = new CodegenExpressionLambda(init.Block)
                .WithParam<object[]>("properties");
            init.Block.DeclareVar<ProxyEventBeanManufacturer.MakeUnderlyingFunc>(
                "makeUndFunc",
                makeUndLambda);
            MakeUnderlyingCodegen(
                codegenMethodScope,
                makeUndLambda.Block,
                codegenClassScope);

            var makeLambda = new CodegenExpressionLambda(init.Block)
                .WithParam<object[]>("properties");
            makeLambda.Block
                .DeclareVar<object>("und", ExprDotMethod(Ref("makeUndFunc"), "Invoke", Ref("properties")))
                .ReturnMethodOrBlock(ExprDotMethod(factory, "AdapterForTypedBean", Ref("und"), beanType));

            init.Block.DeclareVar<ProxyEventBeanManufacturer.MakeFunc>(
                "makeFunc",
                makeLambda);

            var manufacturer = NewInstance<ProxyEventBeanManufacturer>(Ref("makeFunc"), Ref("makeUndFunc"));

            //var manufacturer = NewAnonymousClass(init.Block, typeof(EventBeanManufacturer));

            //var makeUndMethod = CodegenMethod.MakeMethod(typeof(object), GetType(), codegenClassScope)
            //    .AddParam(typeof(object[]), "properties");
            //manufacturer.AddMethod("MakeUnderlying", makeUndMethod);
            //MakeUnderlyingCodegen(makeUndMethod, codegenClassScope);

            //var makeMethod = CodegenMethod.MakeMethod(typeof(EventBean), GetType(), codegenClassScope)
            //    .AddParam(typeof(object[]), "properties");
            //manufacturer.AddMethod("Make", makeMethod);
            //makeMethod.Block
            //    .DeclareVar<object>("und", LocalMethod(makeUndMethod, Ref("properties")))
            //    .MethodReturn(ExprDotMethod(factory, "AdapterForTypedBean", Ref("und"), beanType));

            return codegenClassScope.AddFieldUnshared(true, typeof(EventBeanManufacturer), manufacturer);
        }

        private void MakeUnderlyingCodegen(
            CodegenMethodScope method,
            CodegenBlock block,
            CodegenClassScope codegenClassScope)
        {
            block
                .DeclareVar(
                    beanEventType.UnderlyingType,
                    "und",
                    Cast(beanEventType.UnderlyingType, beanInstantiator.Make(method, codegenClassScope)))
                .DeclareVar<object>("value", ConstantNull());

            for (var i = 0; i < writeMembersReflection.Length; i++) {
                block.AssignRef("value", ArrayAtIndex(Ref("properties"), Constant(i)));

                var writeMember = writeMembersReflection[i];
                Type targetType;

                if (writeMember is MethodInfo writeMethod) {
                    targetType = writeMethod.GetParameters()[0].ParameterType;
                }
                else if (writeMember is PropertyInfo writeProperty) {
                    targetType = writeProperty.PropertyType;
                }
                else {
                    throw new IllegalStateException("writeMember of invalid type");
                }

                CodegenExpression value;
                //if (targetType.IsPrimitive) {
                //    var caster = SimpleTypeCasterFactory.GetCaster(typeof(object), targetType);
                //    value = caster.Codegen(Ref("value"), typeof(object), method, codegenClassScope);
                //}
                //else {
                    value = Cast(targetType, Ref("value"));
                //}

                CodegenExpression set = null;

                if (writeMember is MethodInfo) {
                    set = ExprDotMethod(Ref("und"), writeMember.Name, value);
                }
                else if (writeMember is PropertyInfo) {
                    set = SetProperty(Ref("und"), writeMember.Name, value);
                }

                if (primitiveType[i]) {
                    block.IfRefNotNull("value").Expression(set).BlockEnd();
                }
                else {
                    block.Expression(set);
                }
            }

            block.ReturnMethodOrBlock(Ref("und"));
        }
    }
} // end of namespace