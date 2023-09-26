///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
    /// <summary>
    /// Factory for Map-underlying events.
    /// </summary>
    public class EventBeanManufacturerJsonForge : EventBeanManufacturerForge
    {
        private readonly JsonEventType _jsonEventType;
        private readonly WriteablePropertyDescriptor[] _writables;

        public EventBeanManufacturerJsonForge(
            JsonEventType jsonEventType,
            WriteablePropertyDescriptor[] writables)
        {
            this._jsonEventType = jsonEventType;
            this._writables = writables;
        }

        public CodegenExpression Make(
            CodegenBlock codegenBlock,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var init = codegenClassScope.NamespaceScope.InitMethod;

            var factory = codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var eventType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(_jsonEventType, EPStatementInitServicesConstants.REF));

            // var makeUndMethod = CodegenMethod.MakeParentNode(typeof(object), GetType(), codegenClassScope)
            //     .AddParam<object[]>("properties");
            // manufacturer.AddMethod("makeUnderlying", makeUndMethod);
            // MakeUnderlyingCodegen(makeUndMethod, codegenClassScope);

            var makeUnderlyingLambda = new CodegenExpressionLambda(codegenBlock)
                .WithParam<object[]>("properties")
                .WithBody(block => MakeUnderlyingCodegen(block, codegenClassScope));

            // CodegenExpressionNewAnonymousClass manufacturer = NewAnonymousClass(
            //     init.Block,
            //     typeof(EventBeanManufacturer));

            var manufacturer = NewInstance<ProxyJsonEventBeanManufacturer>(eventType, factory, makeUnderlyingLambda);

            // Make(): this is provided by ProxyJsonEventBeanManufacturer
            // 
            // var makeMethod = CodegenMethod.MakeParentNode(typeof(EventBean), GetType(), codegenClassScope)
            //     .AddParam<object[]>("properties");
            // manufacturer.AddMethod("make", makeMethod);
            // makeMethod.Block
            //     .DeclareVar(typeof(object), "und", LocalMethod(makeUndMethod, Ref("properties")))
            //     .MethodReturn(ExprDotMethod(factory, "adapterForTypedJson", Ref("und"), eventType));

            return codegenClassScope.AddDefaultFieldUnshared(true, typeof(EventBeanManufacturer), manufacturer);
        }

        public EventBeanManufacturer GetManufacturer(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            var nativeNums = EventBeanManufacturerJson.FindPropertyIndexes(_jsonEventType, _writables);
            return new EventBeanManufacturerJson(_jsonEventType, eventBeanTypedEventFactory, nativeNums);
        }

        private void MakeUnderlyingCodegen(
            CodegenBlock block,
            CodegenClassScope codegenClassScope)
        {
            block.DeclareVar(_jsonEventType.UnderlyingType, "und", NewInstance(_jsonEventType.UnderlyingType));
            for (var i = 0; i < _writables.Length; i++) {
                var field = _jsonEventType.Detail.FieldDescriptors.Get(_writables[i].PropertyName);
                var rhs = ArrayAtIndex(Ref("properties"), Constant(i));
                var fieldTypeBoxed = field.PropertyType.GetBoxedType();
                if (field.PropertyType.IsPrimitive) {
                    block
                        .IfCondition(NotEqualsNull(rhs))
                        .AssignRef(Ref("und." + field.FieldName), Cast(fieldTypeBoxed, rhs));
                }
                else {
                    block.AssignRef(
                        Ref("und." + field.FieldName),
                        Cast(fieldTypeBoxed, rhs));
                }
            }

            block.BlockReturn(Ref("und"));
        }
    }
} // end of namespace