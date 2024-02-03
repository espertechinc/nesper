///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.@event.json.deserializers.core;
using com.espertech.esper.common.@internal.@event.json.deserializers.forge;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.@event.json.compiletime.StmtClassForgeableJsonUnderlying;

namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
    public class StmtClassForgeableJsonDeserializer : StmtClassForgeable
    {
        private readonly CodegenClassType _classType;
        private readonly string _className;
        private readonly CodegenNamespaceScope _namespaceScope;
        private readonly string _underlyingClassName;
        private readonly StmtClassForgeableJsonDesc _desc;

        public StmtClassForgeableJsonDeserializer(
            CodegenClassType classType,
            string className,
            CodegenNamespaceScope namespaceScope,
            string underlyingClassName,
            StmtClassForgeableJsonDesc desc)
        {
            _classType = classType;
            _className = className;
            _namespaceScope = namespaceScope;
            _underlyingClassName = underlyingClassName;
            _desc = desc;
        }

        public CodegenClass Forge(
            bool includeDebugSymbols,
            bool fireAndForget)
        {
            var makeVirtual = _desc.OptionalSupertype == null;

            var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, _className);

            // make members
            var members = new List<CodegenTypedParam>();

            // --------------------------------------------------------------------------------
            // Constructor
            // --------------------------------------------------------------------------------

            var ctorParams = new CodegenTypedParam[] { };
            var ctor = new CodegenCtor(typeof(StmtClassForgeableRSPFactoryProvider), classScope, ctorParams);

            // --------------------------------------------------------------------------------
            // Deserialize(JsonElement)
            // --------------------------------------------------------------------------------
            var deserializeMethod = CodegenMethod
                //.MakeParentNode(_underlyingClassName, GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .MakeParentNode(typeof(object), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam<JsonElement>(JsonDeserializeRefs.INSTANCE.ElementName);
            deserializeMethod = MakeDeserialize(deserializeMethod, classScope, makeVirtual);

            // --------------------------------------------------------------------------------
            // Properties (property)
            // --------------------------------------------------------------------------------

            var propertiesProp = CodegenProperty
                .MakePropertyNode(
                    typeof(ILookup<string, IJsonDeserializer>),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
            propertiesProp = makeVirtual
                ? propertiesProp.WithVirtual()
                : propertiesProp.WithOverride();

            propertiesProp.GetterBlock.BlockReturn(ConstantNull()); // TODO

            // --------------------------------------------------------------------------------

            var properties = new CodegenClassProperties();

            // walk methods
            var methods = new CodegenClassMethods();
            //CodegenStackGenerator.RecursiveBuildStack(getResultMethod, "GetResult", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(deserializeMethod, "Deserialize", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(propertiesProp, "Properties", methods, properties);

            var clazz = new CodegenClass(
                _classType,
                _className,
                classScope,
                members,
                ctor,
                methods,
                properties,
                EmptyList<CodegenInnerClass>.Instance);

            if (_desc.OptionalSupertype == null) {
                clazz.BaseList.AssignType(typeof(JsonCompositeDeserializer));
            }
            else {
                clazz.BaseList.AssignBaseType(_desc.OptionalSupertype.Detail.DeserializerClassName);
            }

            return clazz;
        }

        private CodegenMethod MakeDeserialize(
            CodegenMethod deserializeMethod,
            CodegenClassScope classScope,
            bool makeVirtual)
        {
            deserializeMethod = makeVirtual
                ? deserializeMethod.WithVirtual()
                : deserializeMethod.WithOverride();

            var elementRef = JsonDeserializeRefs.INSTANCE.Element;

            deserializeMethod
                .Block
                .IfCondition(Op(ExprDotName(elementRef, "ValueKind"), "==", Constant(JsonValueKind.Null)))
                .BlockReturn(ConstantNull());

            deserializeMethod
                .Block
                .DeclareVar(_underlyingClassName, "value", NewInstanceInner(_underlyingClassName))
                .DeclareVar<JsonElement.ObjectEnumerator>(
                    "propertyEnumerator",
                    ExprDotMethod(elementRef, "EnumerateObject"));

            var propertyNameRef = Ref("propertyName");
            var propertyValue = Ref("propertyValue");

            var propertiesThisType = _desc.PropertiesThisType
                .Where(_ => _.Value != null)
                .ToList();

            // This builds the "case XXX" which are supplied to the switch statement.  This does not include
            // the code to process the value, which we will do next.
            var caseOptions = propertiesThisType
                .Select(propertyPair => Constant(propertyPair.Key))
                .ToArray();

            var whileLoop = deserializeMethod
                .Block
                .WhileLoop(ExprDotMethod(Ref("propertyEnumerator"), "MoveNext"))
                .DeclareVar<JsonProperty>("property", ExprDotName(Ref("propertyEnumerator"), "Current"))
                .DeclareVar<string>("propertyName", ExprDotName(Ref("property"), "Name"))
                .DeclareVar<JsonElement>("propertyValue", ExprDotName(Ref("property"), "Value"));

            var switchBlock = whileLoop.SwitchBlockExpressions(
                propertyNameRef,
                caseOptions,
                false,
                false);

            var indx = 0;
            // Here we setup the "codeBlock" for each of the valid cases.
            foreach (var propertyPair in propertiesThisType) {
                var field = _desc.FieldDescriptorsInclSupertype.Get(propertyPair.Key);
                var fieldName = field.FieldName;
                var fieldType = field.PropertyType;
                var forge = _desc.Forges.Get(propertyPair.Key);

                // Get the switch block.  This only works because the order of the
                // properties is identical, so the index of the case block lines up
                // with this item.
                var caseExprBlock = switchBlock.Blocks[indx++];

                // Gets the property from the enclosing element
                // --------------------------------------------------------------------------------
                // Assign the field.  If the underlying type is a "value" with properties then this will
                // just use a standard assignment.  If the underlying is dynamic (as defined by IsDynamic)
                // then we believe we need to add this to a general dictionary that masks the data.  We
                // need more information to determine if this is the right thing to do.  Maybe we should
                // just use an IExpando for these kinds of objects?
                // --------------------------------------------------------------------------------

                caseExprBlock
                    .AssignMember(
                        "value." + fieldName.CodeInclusionName(),
                        Cast(
                            fieldType,
                            forge.DeserializerForge.CodegenDeserialize(
                                deserializeMethod,
                                classScope,
                                propertyValue)))
                    .BlockEnd();
            }

            if (_desc.IsDynamic) {
                // All other cases overflow into the "default" case if applicable.
                var caseExprBlock = switchBlock.DefaultBlock;

                caseExprBlock
                    .ExprDotMethod(
                        Ref("value." + DYNAMIC_PROP_FIELD),
                        "Put",
                        propertyNameRef,
                        StaticMethod(
                            typeof(JsonElementExtensions),
                            "ElementToValue",
                            ExprDotName(Ref("property"), "Value")))
                    .BlockEnd();
            }
            else {
                switchBlock
                    .DefaultBlock
                    .BlockEnd();
            }

            whileLoop.BlockEnd();

            deserializeMethod.Block.MethodReturn(Ref("value"));
            return deserializeMethod;
        }

        public string ClassName => _className;

        public StmtClassForgeableType ForgeableType => StmtClassForgeableType.JSON_DESERIALIZER;
    }
} // end of namespace