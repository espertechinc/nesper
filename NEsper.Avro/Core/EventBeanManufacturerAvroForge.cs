///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Core
{
    /// <summary>
    ///     Factory for Avro-underlying events.
    /// </summary>
    public class EventBeanManufacturerAvroForge : EventBeanManufacturerForge
    {
        private readonly AvroEventType _eventType;
        private readonly Field[] _indexPerWritable;

        public EventBeanManufacturerAvroForge(
            AvroSchemaEventType eventType,
            WriteablePropertyDescriptor[] properties)
        {
            _eventType = (AvroEventType) eventType;

            Schema schema = _eventType.SchemaAvro;
            _indexPerWritable = new Field[properties.Length];
            for (var i = 0; i < properties.Length; i++) {
                var propertyName = properties[i].PropertyName;

                var field = schema.GetField(propertyName);
                if (field == null) {
                    throw new IllegalStateException(
                        "Failed to find property '" + propertyName + "' among the array indexes");
                }

                _indexPerWritable[i] = field;
            }
        }

        public EventBeanManufacturer GetManufacturer(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new EventBeanManufacturerAvro(_eventType, eventBeanTypedEventFactory, _indexPerWritable);
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
                EventTypeUtility.ResolveTypeCodegen(_eventType, EPStatementInitServicesConstants.REF));

            var manufacturer = CodegenExpressionBuilder.NewAnonymousClass(init.Block, typeof(EventBeanManufacturer));

            var makeUndMethod = CodegenMethod.MakeParentNode(typeof(GenericRecord), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "properties");
            manufacturer.AddMethod("MakeUnderlying", makeUndMethod);
            MakeUnderlyingCodegen(makeUndMethod, codegenClassScope);

            var makeMethod = CodegenMethod
                .MakeParentNode(typeof(EventBean), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "properties");
            manufacturer
                .AddMethod("Make", makeMethod);
            makeMethod.Block
                .DeclareVar<GenericRecord>(
                    "und",
                    CodegenExpressionBuilder.LocalMethod(makeUndMethod, CodegenExpressionBuilder.Ref("properties")))
                .MethodReturn(
                    CodegenExpressionBuilder.ExprDotMethod(
                        factory,
                        "adapterForTypedAvro",
                        CodegenExpressionBuilder.Ref("und"),
                        beanType));

            return codegenClassScope.AddFieldUnshared(true, typeof(EventBeanManufacturer), manufacturer);
        }

        private void MakeUnderlyingCodegen(
            CodegenMethod method,
            CodegenClassScope codegenClassScope)
        {
            var schema = codegenClassScope.NamespaceScope.AddFieldUnshared(
                true,
                typeof(Schema),
                CodegenExpressionBuilder.StaticMethod(
                    typeof(AvroSchemaUtil),
                    "resolveAvroSchema",
                    EventTypeUtility.ResolveTypeCodegen(_eventType, EPStatementInitServicesConstants.REF)));
            method.Block
                .DeclareVar<GenericRecord>(
                    "record",
                    CodegenExpressionBuilder.NewInstance(typeof(GenericRecord), schema));

            for (var i = 0; i < _indexPerWritable.Length; i++) {
                method.Block.ExprDotMethod(
                    CodegenExpressionBuilder.Ref("record"),
                    "put",
                    CodegenExpressionBuilder.Constant(_indexPerWritable[i]),
                    CodegenExpressionBuilder.ArrayAtIndex(
                        CodegenExpressionBuilder.Ref("properties"),
                        CodegenExpressionBuilder.Constant(i)));
            }

            method.Block.MethodReturn(CodegenExpressionBuilder.Ref("record"));
        }
    }
} // end of namespace