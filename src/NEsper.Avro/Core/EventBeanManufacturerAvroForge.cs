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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

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

                _indexPerWritable[i] = field ??
                    throw new IllegalStateException($"Failed to find property '{propertyName}' among the array indexes");
            }
        }

        public EventBeanManufacturer GetManufacturer(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new EventBeanManufacturerAvro(_eventType, eventBeanTypedEventFactory, _indexPerWritable);
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
                EventTypeUtility.ResolveTypeCodegen(_eventType, EPStatementInitServicesConstants.REF));

            var makeUndLambda = new CodegenExpressionLambda(codegenBlock)
                .WithParam<object[]>("properties")
                .WithBody(block => MakeUnderlyingCodegen(block, codegenClassScope));

            var manufacturer = NewInstance<ProxyAvroEventBeanManufacturer>(
                    eventType, factory, makeUndLambda);

            //var makeUndMethod = CodegenMethod.MakeMethod(typeof(GenericRecord), GetType(), codegenClassScope)
            //    .AddParam(typeof(object[]), "properties");
            //manufacturer.AddMethod("MakeUnderlying", makeUndMethod);
            //MakeUnderlyingCodegen(makeUndMethod, codegenClassScope);

            //var makeMethod = CodegenMethod
            //    .MakeMethod(typeof(EventBean), GetType(), codegenClassScope)
            //    .AddParam(typeof(object[]), "properties");
            //manufacturer.AddMethod("Make", makeMethod);
            //makeMethod.Block
            //    .DeclareVar<GenericRecord>("und", LocalMethod(makeUndMethod, Ref("properties")))
            //    .MethodReturn(ExprDotMethod(factory, "AdapterForTypedAvro", Ref("und"), beanType));

            return codegenClassScope.AddDefaultFieldUnshared(
                true, typeof(EventBeanManufacturer), manufacturer);
        }

        private void MakeUnderlyingCodegen(
            CodegenBlock block,
            CodegenClassScope codegenClassScope)
        {
            var schema = codegenClassScope.NamespaceScope.AddDefaultFieldUnshared(
                true,
                typeof(RecordSchema),
                StaticMethod(
                    typeof(AvroSchemaUtil),
                    "ResolveRecordSchema",
                    EventTypeUtility.ResolveTypeCodegen(_eventType, EPStatementInitServicesConstants.REF)));
            block
                .DeclareVar<GenericRecord>(
                    "record",
                    NewInstance(typeof(GenericRecord), schema));

            for (var i = 0; i < _indexPerWritable.Length; i++) {
                block.StaticMethod(
                    typeof(GenericRecordExtensions), "Put", 
                    Ref("record"),
                    Constant(_indexPerWritable[i].Name),
                    ArrayAtIndex(Ref("properties"), Constant(i)));
            }

            block.BlockReturn(Ref("record"));
        }
    }
} // end of namespace