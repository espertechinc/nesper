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
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
    /// <summary>
    /// Factory for ObjectArray-underlying events.
    /// </summary>
    public class EventBeanManufacturerObjectArrayForge : EventBeanManufacturerForge
    {
        private readonly ObjectArrayEventType eventType;
        private readonly int[] indexPerWritable;
        private readonly bool oneToOne;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventType">type to create</param>
        /// <param name="properties">written properties</param>
        public EventBeanManufacturerObjectArrayForge(
            ObjectArrayEventType eventType,
            WriteablePropertyDescriptor[] properties)
        {
            this.eventType = eventType;

            var indexes = eventType.PropertiesIndexes;
            indexPerWritable = new int[properties.Length];
            var oneToOneMapping = true;
            for (var i = 0; i < properties.Length; i++) {
                var propertyName = properties[i].PropertyName;
                if (!indexes.TryGetValue(propertyName, out var index)) {
                    throw new IllegalStateException(
                        "Failed to find property '" + propertyName + "' among the array indexes");
                }

                indexPerWritable[i] = index;
                if (index != i) {
                    oneToOneMapping = false;
                }
            }

            oneToOne = oneToOneMapping && properties.Length == eventType.PropertyNames.Length;
        }

        public EventBeanManufacturer GetManufacturer(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new EventBeanManufacturerObjectArray(
                eventType,
                eventBeanTypedEventFactory,
                indexPerWritable,
                oneToOne);
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
                EventTypeUtility.ResolveTypeCodegen(this.eventType, EPStatementInitServicesConstants.REF));

            // var makeUndMethod = CodegenMethod.MakeParentNode(typeof(object[]), GetType(), codegenClassScope)
            //     .AddParam<object[]>("properties");
            // manufacturer.AddMethod("makeUnderlying", makeUndMethod);
            // MakeUnderlyingCodegen(makeUndMethod, codegenClassScope);

            var makeUndFunc = new CodegenExpressionLambda(codegenBlock)
                .WithParam<object[]>("properties")
                .WithBody(block => MakeUnderlyingCodegen(block, codegenClassScope));

            // CodegenExpressionNewAnonymousClass manufacturer = NewAnonymousClass(
            //     init.Block,
            //     typeof(EventBeanManufacturer));

            var manufacturer = NewInstance<ProxyObjectArrayEventBeanManufacturer>(eventType, factory, makeUndFunc);

            // Make(): this is provided by ProxyJsonEventBeanManufacturer
            // 
            // var makeMethod = CodegenMethod.MakeParentNode(typeof(EventBean), GetType(), codegenClassScope)
            //     .AddParam<object[]>("properties");
            // manufacturer.AddMethod("make", makeMethod);
            // makeMethod.Block
            //     .DeclareVar<object[]>("und", LocalMethod(makeUndMethod, Ref("properties")))
            //     .MethodReturn(ExprDotMethod(factory, "AdapterForTypedObjectArray", Ref("und"), eventType));

            return codegenClassScope.AddDefaultFieldUnshared(true, typeof(EventBeanManufacturer), manufacturer);
        }

        private void MakeUnderlyingCodegen(
            CodegenBlock block,
            CodegenClassScope codegenClassScope)
        {
            if (oneToOne) {
                block.ReturnMethodOrBlock(Ref("properties"));
                return;
            }

            block.DeclareVar<object[]>(
                "cols",
                NewArrayByLength(typeof(object), Constant(eventType.PropertyNames.Length)));
            for (var i = 0; i < indexPerWritable.Length; i++) {
                block.AssignArrayElement(
                            Ref("cols"),
                    Constant(indexPerWritable[i]),
                    ArrayAtIndex(Ref("properties"), Constant(i)));
            }

            block.ReturnMethodOrBlock(Ref("cols"));
        }
    }
} // end of namespace