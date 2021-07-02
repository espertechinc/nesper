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
        private readonly ObjectArrayEventType _eventType;
        private readonly int[] _indexPerWritable;
        private readonly bool _oneToOne;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventType">type to create</param>
        /// <param name="properties">written properties</param>
        public EventBeanManufacturerObjectArrayForge(
            ObjectArrayEventType eventType,
            WriteablePropertyDescriptor[] properties)
        {
            this._eventType = eventType;

            var indexes = eventType.PropertiesIndexes;
            _indexPerWritable = new int[properties.Length];
            var oneToOneMapping = true;
            for (var i = 0; i < properties.Length; i++) {
                var propertyName = properties[i].PropertyName;
                if (!indexes.TryGetValue(propertyName, out var index)) {
                    throw new IllegalStateException(
                        "Failed to find property '" + propertyName + "' among the array indexes");
                }

                _indexPerWritable[i] = index;
                if (index != i) {
                    oneToOneMapping = false;
                }
            }

            _oneToOne = oneToOneMapping && properties.Length == eventType.PropertyNames.Length;
        }

        public EventBeanManufacturer GetManufacturer(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new EventBeanManufacturerObjectArray(
                _eventType,
                eventBeanTypedEventFactory,
                _indexPerWritable,
                _oneToOne);
        }

        public CodegenExpression Make(
            CodegenBlock codegenBlock,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var factory = codegenClassScope.AddOrGetDefaultFieldSharable(
                EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var eventType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(this._eventType, EPStatementInitServicesConstants.REF));

            var makeUndLambda = new CodegenExpressionLambda(codegenBlock)
                .WithParam<object[]>("properties")
                .WithBody(block => MakeUnderlyingCodegen(block, codegenClassScope));

            var manufacturer = NewInstance<ProxyObjectArrayEventBeanManufacturer>(
                eventType, factory, makeUndLambda);

            //var makeUndProc = CodegenMethod.MakeMethod(typeof(object[]), GetType(), codegenClassScope)
            //    .AddParam(typeof(object[]), "properties");
            //manufacturer.AddMethod("MakeUnderlying", makeUndProc);
            //MakeUnderlyingCodegen(makeUndProc, codegenClassScope);

            //var makeProc = CodegenMethod.MakeMethod(typeof(EventBean), GetType(), codegenClassScope)
            //    .AddParam(typeof(object[]), "properties");
            //manufacturer.AddMethod("Make", makeProc);

            return codegenClassScope.AddDefaultFieldUnshared(true, typeof(EventBeanManufacturer), manufacturer);
        }

        private void MakeUnderlyingCodegen(
            CodegenBlock block,
            CodegenClassScope codegenClassScope)
        {
            if (_oneToOne) {
                block.ReturnMethodOrBlock(Ref("properties"));
                return;
            }

            block.DeclareVar<object[]>(
                "cols",
                NewArrayByLength(typeof(object), Constant(_eventType.PropertyNames.Length)));
            for (var i = 0; i < _indexPerWritable.Length; i++) {
                block.AssignArrayElement(
                    Ref("cols"),
                    Constant(_indexPerWritable[i]),
                    ArrayAtIndex(Ref("properties"), Constant(i)));
            }

            block.ReturnMethodOrBlock(Ref("cols"));
        }
    }
} // end of namespace