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
    ///     Factory for event beans created and populate anew from a set of values.
    /// </summary>
    public class EventBeanManufacturerBeanForge : EventBeanManufacturerForge
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(EventBeanManufacturerBeanForge));

        private readonly ImportService _importService;
        private readonly BeanEventType _beanEventType;

        private readonly BeanInstantiatorForge _beanInstantiator;
        private readonly bool[] _primitiveType;
        private readonly WriteablePropertyDescriptor[] _properties;
        private readonly MemberInfo[] _writeMembersReflection;

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
            _beanEventType = beanEventType;
            _properties = properties;
            _importService = importService;

            _beanInstantiator = BeanInstantiatorFactory.MakeInstantiator(beanEventType, importService);

            _writeMembersReflection = new MemberInfo[properties.Length];

            _primitiveType = new bool[properties.Length];
            for (var i = 0; i < properties.Length; i++) {
                _writeMembersReflection[i] = properties[i].WriteMember;
                _primitiveType[i] = properties[i].PropertyType.IsValueType && properties[i].PropertyType.CanNotBeNull();
            }
        }

        public EventBeanManufacturer GetManufacturer(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new EventBeanManufacturerBean(
                _beanEventType,
                eventBeanTypedEventFactory,
                _properties,
                _importService);
        }

        public CodegenExpression Make(
            CodegenBlock codegenBlock,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var factory = codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var eventType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(_beanEventType, EPStatementInitServicesConstants.REF));

            var makeUndLambda = new CodegenExpressionLambda(codegenBlock)
                .WithParam<object[]>("properties")
                .WithBody(block => MakeUnderlyingCodegen(codegenMethodScope, block, codegenClassScope));

            var manufacturer = NewInstance<ProxyBeanEventBeanManufacturer>(
                eventType, factory, makeUndLambda);

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
            //    .MethodReturn(ExprDotMethod(factory, "AdapterForTypedObject", Ref("und"), beanType));

            return codegenClassScope.AddDefaultFieldUnshared(true, typeof(EventBeanManufacturer), manufacturer);
        }

        private void MakeUnderlyingCodegen(
            CodegenMethodScope method,
            CodegenBlock block,
            CodegenClassScope codegenClassScope)
        {
            block
                .DeclareVar(
                    _beanEventType.UnderlyingType,
                    "und",
                    Cast(_beanEventType.UnderlyingType, _beanInstantiator.Make(method, codegenClassScope)))
                .DeclareVar<object>("value", ConstantNull());

            for (var i = 0; i < _writeMembersReflection.Length; i++) {
                block.AssignRef("value", ArrayAtIndex(Ref("properties"), Constant(i)));

                Type targetType;

                var writeMember = _writeMembersReflection[i];
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
                //if (targetType.IsValueType) {
                //    var caster = SimpleTypeCasterFactory.GetCaster(typeof(object), targetType);
                //    value = caster.Codegen(Ref("value"), typeof(object), method, codegenClassScope);
                //}
                //else {

                var caster = SimpleTypeCasterFactory.GetCaster(typeof(object), targetType);
                value = caster.Codegen(Ref("value"), typeof(object), method, codegenClassScope);

                //}

                CodegenExpression set = null;

                if (writeMember is MethodInfo) {
                    set = ExprDotMethod(Ref("und"), writeMember.Name, value);
                }
                else if (writeMember is PropertyInfo) {
                    set = SetProperty(Ref("und"), writeMember.Name, value);
                }

                if (_primitiveType[i]) {
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