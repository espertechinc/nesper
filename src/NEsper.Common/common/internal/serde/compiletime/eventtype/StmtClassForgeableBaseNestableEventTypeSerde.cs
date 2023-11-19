///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.serde;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.path;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.serde.compiletime.eventtype
{
    public class StmtClassForgeableBaseNestableEventTypeSerde : StmtClassForgeable
    {
        private const string OBJECT_NAME = "obj";
        private const string OUTPUT_NAME = "output";
        private const string INPUT_NAME = "input";
        private const string UNITKEY_NAME = "unitKey";
        private const string WRITER_NAME = "writer";
        private readonly string className;
        private readonly CodegenNamespaceScope namespaceScope;
        private readonly BaseNestableEventType eventType;
        private readonly DataInputOutputSerdeForge[] forges;

        public StmtClassForgeableBaseNestableEventTypeSerde(
            string className,
            CodegenNamespaceScope namespaceScope,
            BaseNestableEventType eventType,
            DataInputOutputSerdeForge[] forges)
        {
            this.className = className;
            this.namespaceScope = namespaceScope;
            this.eventType = eventType;
            this.forges = forges;
        }

        public CodegenClass Forge(
            bool includeDebugSymbols,
            bool fireAndForget)
        {
            var methods = new CodegenClassMethods();
            var properties = new CodegenClassProperties();
            var classScope = new CodegenClassScope(includeDebugSymbols, namespaceScope, className);
            
            var writeMethod = CodegenMethod
                .MakeParentNode(
                    typeof(void),
                    typeof(StmtClassForgeableBaseNestableEventTypeSerde),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<object>(OBJECT_NAME)
                .AddParam(typeof(DataOutput), OUTPUT_NAME)
                .AddParam<byte[]>(UNITKEY_NAME)
                .AddParam<EventBeanCollatedWriter>(WRITER_NAME);
            MakeWriteMethod(writeMethod);
            CodegenStackGenerator.RecursiveBuildStack(writeMethod, "Write", methods, properties);
            
            var readMethod = CodegenMethod
                .MakeParentNode(
                    typeof(object),
                    typeof(StmtClassForgeableBaseNestableEventTypeSerde),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(DataInput), INPUT_NAME)
                .AddParam<byte[]>(UNITKEY_NAME);
            MakeReadMethod(readMethod);
            CodegenStackGenerator.RecursiveBuildStack(readMethod, "Read", methods, properties);
            
            IList<CodegenTypedParam> members = new List<CodegenTypedParam>();
            for (var i = 0; i < forges.Length; i++) {
                members.Add(new CodegenTypedParam(forges[i].ForgeClassName, "s" + i));
            }

            var ctorParams =
                Collections.SingletonList(new CodegenTypedParam(typeof(EventTypeResolver), "resolver", false));
            var providerCtor = new CodegenCtor(GetType(), includeDebugSymbols, ctorParams);
            for (var i = 0; i < forges.Length; i++) {
                providerCtor.Block.AssignRef("s" + i, forges[i].Codegen(providerCtor, classScope, Ref("resolver")));
            }

            return new CodegenClass(
                CodegenClassType.EVENTSERDE,
                typeof(DataInputOutputSerde),
                className,
                classScope,
                members,
                providerCtor,
                methods,
                properties,
                EmptyList<CodegenInnerClass>.Instance);
        }

        public string ClassName => className;

        public StmtClassForgeableType ForgeableType => StmtClassForgeableType.MULTIKEY;
        
        private void MakeWriteMethod(CodegenMethod writeMethod)
        {
            var propertyNames = eventType.PropertyNames;
            if (eventType is MapEventType) {
                writeMethod.Block.DeclareVar(
                    typeof(IDictionary<string, object>),
                    "map",
                    Cast(typeof(IDictionary<string, object>), Ref(OBJECT_NAME)));
            }
            else if (eventType is ObjectArrayEventType) {
                writeMethod.Block.DeclareVar<object[]>("oa", Cast(typeof(object[]), Ref(OBJECT_NAME)));
            }
            else if (eventType is JsonEventType) {
                var jsonEventType = (JsonEventType)eventType;
                writeMethod.Block.DeclareVar(
                    jsonEventType.UnderlyingType,
                    "json",
                    Cast(jsonEventType.UnderlyingType, Ref(OBJECT_NAME)));
            }
            else {
                throw new IllegalStateException("Unrecognized event type " + eventType);
            }

            for (var i = 0; i < forges.Length; i++) {
                CodegenExpression serde = Ref("s" + i);
                CodegenExpression get;
                if (eventType is MapEventType) {
                    get = ExprDotMethod(Ref("map"), "Get", Constant(propertyNames[i]));
                }
                else if (eventType is ObjectArrayEventType) {
                    get = ArrayAtIndex(Ref("oa"), Constant(i));
                }
                else {
                    var jsonEventType = (JsonEventType)eventType;
                    var property = eventType.PropertyNames[i];
                    var field = jsonEventType.Detail.FieldDescriptors.Get(property);
                    if (field == null) {
                        throw new IllegalStateException("Unrecognized json event property " + property);
                    }

                    get = Ref("json." + field.FieldName);
                }

                writeMethod.Block.ExprDotMethod(
                    serde,
                    "Write",
                    get,
                    Ref(OUTPUT_NAME),
                    Ref(UNITKEY_NAME),
                    Ref(WRITER_NAME));
            }

            if (eventType is JsonEventType) {
                var jsonEventType = (JsonEventType)eventType;
                if (jsonEventType.Detail.IsDynamic) {
                    CodegenExpression get = Ref("json." + StmtClassForgeableJsonUnderlying.DYNAMIC_PROP_FIELD);
                    writeMethod.Block.ExprDotMethod(
                        PublicConstValue(typeof(DIOJsonObjectSerde), "INSTANCE"),
                        "Write",
                        get,
                        Ref(OUTPUT_NAME),
                        Ref(UNITKEY_NAME),
                        Ref(WRITER_NAME));
                }
            }
        }

        private void MakeReadMethod(CodegenMethod readMethod)
        {
            var propertyNames = eventType.PropertyNames;
            CodegenExpressionRef underlyingRef;
            if (eventType is MapEventType) {
                readMethod.Block.DeclareVar(
                    typeof(IDictionary<string, object>),
                    "map",
                    NewInstance(typeof(Dictionary<string, object>), Constant(CollectionUtil.CapacityHashMap(forges.Length))));
                underlyingRef = Ref("map");
            }
            else if (eventType is ObjectArrayEventType) {
                readMethod.Block.DeclareVar<object[]>(
                    "oa",
                    NewArrayByLength(typeof(object), Constant(forges.Length)));
                underlyingRef = Ref("oa");
            }
            else if (eventType is JsonEventType) {
                var jsonEventType = (JsonEventType)eventType;
                readMethod.Block.DeclareVar(
                    jsonEventType.UnderlyingType,
                    "json",
                    NewInstance(jsonEventType.UnderlyingType));
                underlyingRef = Ref("json");
            }
            else {
                throw new IllegalStateException("Unrecognized event type " + eventType);
            }

            for (var i = 0; i < forges.Length; i++) {
                CodegenExpression serde = Ref("s" + i);
                var read = ExprDotMethod(serde, "Read", Ref(INPUT_NAME), Ref(UNITKEY_NAME));
                if (eventType is MapEventType) {
                    readMethod.Block.ExprDotMethod(Ref("map"), "Put", Constant(propertyNames[i]), read);
                }
                else if (eventType is ObjectArrayEventType) {
                    readMethod.Block.AssignArrayElement(Ref("oa"), Constant(i), read);
                }
                else {
                    var jsonEventType = (JsonEventType)eventType;
                    var property = eventType.PropertyNames[i];
                    var field = jsonEventType.Detail.FieldDescriptors.Get(property);
                    if (field == null) {
                        throw new IllegalStateException("Unrecognized json event property " + property);
                    }

                    readMethod.Block.AssignRef(Ref("json." + field.FieldName), Cast(field.PropertyType, read));
                }
            }

            if (eventType is JsonEventType) {
                var jsonEventType = (JsonEventType)eventType;
                if (jsonEventType.Detail.IsDynamic) {
                    var read = ExprDotMethod(
                        PublicConstValue(typeof(DIOJsonObjectSerde), "INSTANCE"),
                        "Read",
                        Ref(INPUT_NAME),
                        Ref(UNITKEY_NAME));
                    readMethod.Block.AssignRef(
                        Ref("json." + StmtClassForgeableJsonUnderlying.DYNAMIC_PROP_FIELD),
                        read);
                }
            }

            readMethod.Block.MethodReturn(underlyingRef);
        }
    }
} // end of namespace