///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
    /// <summary>
    ///     Factory for ObjectArray-underlying events.
    /// </summary>
    public class EventBeanManufacturerObjectArrayForge : EventBeanManufacturerForge
    {
        private readonly ObjectArrayEventType eventType;
        private readonly int[] indexPerWritable;
        private readonly bool oneToOne;

        /// <summary>
        ///     Ctor.
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
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var init = codegenClassScope.NamespaceScope.InitMethod;

            var factory = codegenClassScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var eventType = codegenClassScope.AddFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(this.eventType, EPStatementInitServicesConstants.REF));

            var manufacturer = NewAnonymousClass(init.Block, typeof(EventBeanManufacturer));

            var makeUndMethod = CodegenMethod.MakeParentNode(typeof(object[]), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "properties");
            manufacturer.AddMethod("MakeUnderlying", makeUndMethod);
            MakeUnderlyingCodegen(makeUndMethod, codegenClassScope);

            var makeMethod = CodegenMethod.MakeParentNode(typeof(EventBean), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "properties");
            manufacturer.AddMethod("Make", makeMethod);
            makeMethod.Block
                .DeclareVar<object[]>("und", LocalMethod(makeUndMethod, Ref("properties")))
                .MethodReturn(ExprDotMethod(factory, "AdapterForTypedObjectArray", Ref("und"), eventType));

            return codegenClassScope.AddFieldUnshared(true, typeof(EventBeanManufacturer), manufacturer);
        }

        private void MakeUnderlyingCodegen(
            CodegenMethod method,
            CodegenClassScope codegenClassScope)
        {
            if (oneToOne) {
                method.Block.MethodReturn(Ref("properties"));
                return;
            }

            method.Block.DeclareVar<object[]>(
                "cols",
                NewArrayByLength(typeof(object), Constant(eventType.PropertyNames.Length)));
            for (var i = 0; i < indexPerWritable.Length; i++) {
                method.Block.AssignArrayElement(
                    Ref("cols"),
                    Constant(indexPerWritable[i]),
                    ArrayAtIndex(Ref("properties"), Constant(i)));
            }

            method.Block.MethodReturn(Ref("cols"));
        }
    }
} // end of namespace