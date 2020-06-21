///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using Castle.DynamicProxy;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.attributes;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

using DescriptionAttribute = com.espertech.esper.common.client.annotation.DescriptionAttribute;

namespace com.espertech.esper.common.@internal.epl.annotation
{
    /// <summary>
    ///     Utility to handle EPL statement annotations.
    /// </summary>
    public class AnnotationUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected internal static readonly string RootNamespace = typeof(NameAttribute).Namespace;

        public static IDictionary<string, IList<AnnotationDesc>> MapByNameLowerCase(IList<AnnotationDesc> annotations)
        {
            IDictionary<string, IList<AnnotationDesc>> map = new Dictionary<string, IList<AnnotationDesc>>();
            foreach (var desc in annotations) {
                var key = desc.Name.ToLowerInvariant();

                if (map.ContainsKey(key)) {
                    map.Get(key).Add(desc);
                    continue;
                }

                IList<AnnotationDesc> annos = new List<AnnotationDesc>(2);
                annos.Add(desc);
                map.Put(key, annos);
            }

            return map;
        }

        public static object GetValue(AnnotationDesc desc)
        {
            foreach (var pair in desc.Attributes) {
                if (string.Equals(pair.First, "value", StringComparison.InvariantCultureIgnoreCase)) {
                    return pair.Second;
                }
            }

            return null;
        }

        /// <summary>
        ///     Compile annotation objects from descriptors.
        /// </summary>
        /// <param name="annotationSpec">spec for annotations</param>
        /// <param name="importService">imports</param>
        /// <param name="compilable">statement expression</param>
        /// <returns>annotations</returns>
        /// <throws>StatementSpecCompileException compile exception</throws>
        public static Attribute[] CompileAnnotations(
            IList<AnnotationDesc> annotationSpec,
            ImportServiceCompileTime importService,
            Compilable compilable)
        {
            Attribute[] annotations;
            try {
                annotations = CompileAnnotations(annotationSpec, importService);
            }
            catch (AnnotationException e) {
                throw new StatementSpecCompileException(
                    "Failed to process statement annotations: " + e.Message,
                    e,
                    compilable.ToEPL());
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                var message =
                    "Unexpected exception compiling annotations in statement, please consult the log file and report the exception: " +
                    ex.Message;
                Log.Error(message, ex);
                throw new StatementSpecCompileException(message, ex, compilable.ToEPL());
            }

            return annotations;
        }

        public static CodegenMethod MakeAnnotations(
            Type arrayType,
            Attribute[] annotations,
            CodegenMethod parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(arrayType, typeof(AnnotationUtil), classScope);
            method.Block.DeclareVar(
                arrayType,
                "annotations",
                NewArrayByLength(arrayType.GetElementType(), Constant(annotations.Length)));
            for (var i = 0; i < annotations.Length; i++) {
                method.Block.AssignArrayElement(
                    "annotations",
                    Constant(i),
                    MakeAnnotation(annotations[i], parent, classScope));
            }

            method.Block.MethodReturn(Ref("annotations"));
            return method;
        }

        /// <summary>
        ///     Compiles annotations to an annotation array.
        /// </summary>
        /// <param name="desc">a list of descriptors</param>
        /// <param name="importService">for resolving the annotation class</param>
        /// <returns>annotations or empty array if none</returns>
        /// <throws>AnnotationException if annotations could not be created</throws>
        private static Attribute[] CompileAnnotations(
            IList<AnnotationDesc> desc,
            ImportServiceCompileTime importService)
        {
            var annotations = new Attribute[desc.Count];
            for (var i = 0; i < desc.Count; i++) {
                annotations[i] = CreateAttributeInstance(desc[i], importService);
                if (annotations[i] is HintAttribute) {
                    HintEnumExtensions.ValidateGetListed(annotations[i]);
                }
            }

            return annotations;
        }

        private static Attribute CreateAttributeInstance(
            AnnotationDesc desc,
            ImportServiceCompileTime importService)
        {
            // resolve class
            Type annotationClass;
            try {
                annotationClass = importService.ResolveAnnotation(desc.Name);
            }
            catch (ImportException e) {
                throw new AnnotationException("Failed to resolve @-annotation class: " + e.Message);
            }

            // obtain Annotation class properties
            var annotationAttributeLists = GetAttributes(annotationClass);
            ISet<string> allAttributes = new HashSet<string>();
            ISet<string> requiredAttributes = new HashSet<string>();
            foreach (var annotationAttribute in annotationAttributeLists) {
                allAttributes.Add(annotationAttribute.Name);
                if (annotationAttribute.IsRequired) {
                    requiredAttributes.Add(annotationAttribute.Name);
                }
            }

            // get attribute values
            IList<string> providedValues = new List<string>();
            foreach (var annotationValuePair in desc.Attributes) {
                providedValues.Add(annotationValuePair.First);
            }

            // for all attributes determine value
            IDictionary<string, object> properties = new Dictionary<string, object>();
            foreach (var annotationAttribute in annotationAttributeLists) {
                // find value pair for this attribute
                var attributeName = annotationAttribute.Name;
                Pair<string, object> pairFound = null;
                foreach (var annotationValuePair in desc.Attributes) {
                    if (annotationValuePair.First.Equals(attributeName)) {
                        pairFound = annotationValuePair;
                    }
                }

                var valueProvided = pairFound?.Second;
                var value = GetFinalValue(annotationClass, annotationAttribute, valueProvided, importService);
                if (value != null) {
                    properties.Put(attributeName, value);
                }

                providedValues.Remove(attributeName);
                requiredAttributes.Remove(attributeName);
            }

            if (requiredAttributes.Count > 0) {
                var required = new List<string>(requiredAttributes);
                required.Sort();
                throw new AnnotationException(
                    "Annotation '" +
                    annotationClass.GetSimpleName() +
                    "' requires a value for attribute '" +
                    required[0] +
                    "'");
            }

            if (providedValues.Count > 0) {
                var provided = new List<string>(providedValues);
                provided.Sort();
                if (allAttributes.Contains(provided[0])) {
                    throw new AnnotationException(
                        "Annotation '" +
                        annotationClass.GetSimpleName() +
                        "' has duplicate attribute values for attribute '" +
                        provided[0] +
                        "'");
                }

                throw new AnnotationException(
                    "Annotation '" +
                    annotationClass.GetSimpleName() +
                    "' does not have an attribute '" +
                    provided[0] +
                    "'");
            }

            // Create a proxy of the attribute
            return (new EPLAnnotationInvocationHandler(annotationClass, properties)).CreateProxyInstance();
        }

        private static object GetFinalValue(
            Type annotationClass,
            AnnotationAttribute annotationAttribute,
            object value,
            ImportServiceCompileTime importService)
        {
            if (value == null) {
                if (annotationAttribute.DefaultValue == null && annotationAttribute.IsRequired) {
                    throw new AnnotationException(
                        "Annotation '" +
                        annotationClass.GetSimpleName() +
                        "' requires a value for attribute '" +
                        annotationAttribute.Name +
                        "'");
                }
                
                return annotationAttribute.DefaultValue;
            }

            // handle non-array
            if (!annotationAttribute.AnnotationType.IsArray) {
                // handle primitive value
                if (!annotationAttribute.AnnotationType.IsAttribute()) {
                    // if expecting an enumeration type, allow string value
                    if (annotationAttribute.AnnotationType.IsEnum && (value is string valueString)) {
                        valueString = valueString.Trim();

                        var annotationType = annotationAttribute.AnnotationType;
                        try {
                            return Enum.Parse(annotationType, valueString, true);
                        }
                        catch (ArgumentException) {
                            throw new AnnotationException(
                                "Annotation '" +
                                annotationClass.FullName +
                                "' requires an enum-value '" +
                                annotationAttribute.AnnotationType.FullName +
                                "' for attribute '" +
                                annotationAttribute.Name +
                                "' but received '" +
                                value +
                                "' which is not one of the enum choices");
                        }
                    }

                    // cast as required
                    var caster = SimpleTypeCasterFactory.GetCaster(value.GetType(), annotationAttribute.AnnotationType);
                    var finalValue = caster.Cast(value);
                    if (finalValue == null) {
                        throw new AnnotationException(
                            "Annotation '" +
                            annotationClass.Name +
                            "' requires a " +
                            annotationAttribute.AnnotationType.Name +
                            "-typed value for attribute '" +
                            annotationAttribute.Name +
                            "' but received " +
                            "a " +
                            value.GetType().Name +
                            "-typed value");
                    }

                    return finalValue;
                }

                // nested annotation
                if (!(value is AnnotationDesc)) {
                    throw new AnnotationException(
                        "Annotation '" +
                        annotationClass.GetSimpleName() +
                        "' requires a " +
                        annotationAttribute.AnnotationType.GetSimpleName() +
                        "-typed value for attribute '" +
                        annotationAttribute.Name +
                        "' but received " +
                        "a " +
                        value.GetType().GetSimpleName() +
                        "-typed value");
                }

                return CreateAttributeInstance((AnnotationDesc) value, importService);
            }

            if (!(value is Array valueAsArray)) {
                throw new AnnotationException(
                    "Annotation '" +
                    annotationClass.GetSimpleName() +
                    "' requires a " +
                    annotationAttribute.AnnotationType.GetSimpleName() +
                    "-typed value for attribute '" +
                    annotationAttribute.Name +
                    "' but received " +
                    "a " +
                    value.GetType().GetSimpleName() +
                    "-typed value");
            }

            var componentType = annotationAttribute.AnnotationType.GetElementType();
            var array = Arrays.CreateInstanceChecked(componentType, valueAsArray.Length);

            for (var i = 0; i < valueAsArray.Length; i++) {
                var arrayValue = valueAsArray.GetValue(i);
                if (arrayValue == null) {
                    throw new AnnotationException(
                        "Annotation '" +
                        annotationClass.GetSimpleName() +
                        "' requires a " +
                        "non-null value for array elements for attribute '" +
                        annotationAttribute.Name +
                        "'");
                }

                object finalValue;
                if (arrayValue is AnnotationDesc annotationDesc) {
                    var inner = CreateAttributeInstance(annotationDesc, importService);
                    if (!componentType.IsInstanceOfType(inner)) {
                        throw MakeArrayMismatchException(
                            annotationClass,
                            componentType,
                            annotationAttribute.Name,
                            inner.GetType());
                    }

                    finalValue = inner;
                }
                else {
                    var caster = SimpleTypeCasterFactory.GetCaster(
                        arrayValue.GetType(),
                        annotationAttribute.AnnotationType.GetElementType());
                    finalValue = caster.Cast(arrayValue);
                    if (finalValue == null) {
                        throw MakeArrayMismatchException(
                            annotationClass,
                            componentType,
                            annotationAttribute.Name,
                            arrayValue.GetType());
                    }
                }

                array.SetValue(finalValue, i);
            }

            return array;
        }

        public static object GetDefaultValue(PropertyInfo propertyInfo)
        {
            var defaultValueAttributes = propertyInfo.GetCustomAttributes<DefaultValueAttribute>();
            foreach (var defaultValueAttribute in defaultValueAttributes) {
                // We should probably do some type assignment checking.
                return defaultValueAttribute.Value;
            }

            return null;
            //return GetDefaultValue(propertyInfo.PropertyType);
        }

        private static IList<AnnotationAttribute> GetAttributes(Type annotationClass)
        {
            var props = new List<AnnotationAttribute>();

#if true
            var clazzProperties = annotationClass.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (clazzProperties.Length == 0) {
                return Collections.GetEmptyList<AnnotationAttribute>();
            }

            foreach (var clazzProperty in clazzProperties
                .Where(c => c.DeclaringType != typeof(Attribute))
                .Where(c => c.CanRead)) {

                var isRequired = clazzProperty
                    .GetCustomAttributes()
                    .OfType<RequiredAttribute>()
                    .Any();
                
                var annotationAttribute = new AnnotationAttribute(
                    clazzProperty.Name,
                    clazzProperty.PropertyType,
                    GetDefaultValue(clazzProperty),
                    isRequired);

                props.Add(annotationAttribute);
            }
#else
            var methods = annotationClass.GetMethods();
            if (methods.Length == 0) {
                return Collections.GetEmptyList<AnnotationAttribute>();
            }

            for (var i = 0; i < methods.Length; i++) {
                var method = methods[i];
                if (method.ReturnType == typeof(void)) {
                    continue;
                }

                var parameters = method.GetParameters();
                if (parameters.Length > 0) {
                    continue;
                }

                var methodName = method.Name;
                if (methodName.Equals("GetType") ||
                    methodName.Equals("ToString") ||
                    methodName.Equals("AnnotationType") ||
                    methodName.Equals("GetHashCode")) {
                    continue;
                }

                props.Add(new AnnotationAttribute(method.Name, method.ReturnType, null)); // TBD: method.DefaultValue
            }
#endif

            props.Sort(
                (
                    o1,
                    o2) => o1.Name.CompareTo(o2.Name));
            return props;
        }

        public static bool HasAnnotation<T>(Attribute[] annotations)
            where T : Attribute
        {
            return FindAnnotation(annotations, typeof(T)) != null;
        }

        public static bool HasAnnotation(
            Attribute[] annotations,
            Type annotationClass)
        {
            return FindAnnotation(annotations, annotationClass) != null;
        }
        
        public static Attribute FindAnnotation(
            Attribute[] annotations,
            Type annotationClass)
        {
            if (!annotationClass.IsAttribute()) {
                throw new ArgumentException("Class " + annotationClass.Name + " is not an annotation class");
            }

            if (annotations == null || annotations.Length == 0) {
                return null;
            }

            foreach (var anno in annotations) {
                if (TypeHelper.IsSubclassOrImplementsInterface(anno.GetType(), annotationClass)) {
                    return anno;
                }

                if (anno is IProxyTargetAccessor proxyTargetAccessor) {
                    var interceptor = proxyTargetAccessor.GetInterceptors()[0] as EPLAnnotationInvocationHandler;
                    if (interceptor != null) {
                        var declaredClass = interceptor.AnnotationClass;
                        var declaredAttributes = declaredClass.GetCustomAttributes(true).OfType<Attribute>().ToArray();
                        var depthSearch = FindAnnotation(declaredAttributes, annotationClass);
                        if (depthSearch != null) {
                            return depthSearch;
                        }
                    }
                }
            }

            return null;
        }

        public static IList<Attribute> FindAnnotations(
            Attribute[] annotations,
            Type annotationClass)
        {
            if (!annotationClass.IsAttribute()) {
                throw new ArgumentException("Class " + annotationClass.Name + " is not an annotation class");
            }

            if (annotations == null || annotations.Length == 0) {
                return null;
            }

            IList<Attribute> annotationsList = new List<Attribute>();
            foreach (var anno in annotations) {
                if (TypeHelper.IsSubclassOrImplementsInterface(anno.GetType(), annotationClass)) {
                    annotationsList.Add(anno);
                }
            }

            return annotationsList;
        }

        public static Attribute[] MergeAnnotations(
            Attribute[] first,
            Attribute[] second)
        {
            return (Attribute[]) CollectionUtil.AddArrays(first, second);
        }

        public static string GetExpectSingleStringValue(
            string msgPrefix,
            IList<AnnotationDesc> annotationsSameName)
        {
            if (annotationsSameName.Count > 1) {
                throw new ExprValidationException(
                    msgPrefix + " multiple annotations provided named '" + annotationsSameName[0].Name + "'");
            }

            var annotation = annotationsSameName[0];
            var value = GetValue(annotation);
            if (value == null) {
                throw new ExprValidationException(
                    msgPrefix + " no value provided for annotation '" + annotation.Name + "', expected a value");
            }

            if (!(value is string)) {
                throw new ExprValidationException(
                    msgPrefix + " string value expected for annotation '" + annotation.Name + "'");
            }

            return (string) value;
        }

        private static AnnotationException MakeArrayMismatchException(
            Type annotationClass,
            Type componentType,
            string attributeName,
            Type unexpected)
        {
            return new AnnotationException(
                "Annotation '" +
                annotationClass.GetSimpleName() +
                "' requires a " +
                componentType.GetSimpleName() +
                "-typed value for array elements for attribute '" +
                attributeName +
                "' but received " +
                "a " +
                unexpected.GetSimpleName() +
                "-typed value");
        }

        private static CodegenExpression MakeAnnotation(
            Attribute annotation,
            CodegenMethod parent,
            CodegenClassScope codegenClassScope)
        {
            if (annotation == null) {
                return ConstantNull();
            }

            if (annotation is NameAttribute name) {
                return NewInstance<AnnotationName>(
                    Constant(name.Value));
            }

            if (annotation is PriorityAttribute priority) {
                return NewInstance<AnnotationPriority>(
                    Constant(priority.Value));
            }

            if (annotation is TagAttribute tag) {
                return NewInstance<AnnotationTag>(
                    Constant(tag.Name),
                    Constant(tag.Value));
            }

            if (annotation is DropAttribute) {
                return NewInstance<AnnotationDrop>();
            }

            if (annotation is DescriptionAttribute description) {
                return NewInstance<AnnotationDescription>(
                    Constant(description.Value));
            }

            if (annotation is HintAttribute hint) {
                return NewInstance<AnnotationHint>(
                    Constant(hint.Value),
                    Constant(hint.Applies),
                    Constant(hint.Model));
            }

            if (annotation is NoLockAttribute) {
                return NewInstance<AnnotationNoLock>();
            }

            if (annotation is AuditAttribute audit) {
                return NewInstance<AnnotationAudit>(Constant(audit.Value));
            }

            if (annotation is EventRepresentationAttribute anno) {
                return NewInstance<AnnotationEventRepresentation>(
                    EnumValue(
                        anno.Value.GetType(),
                        anno.Value.GetName()));
            }

            if (annotation is IterableUnboundAttribute) {
                return NewInstance<AnnotationIterableUnbound>();
            }

            if (annotation is HookAttribute hook) {
                return NewInstance<AnnotationHook>(
                    EnumValue(typeof(HookType), hook.HookType.GetName()),
                    Constant(hook.Hook));
            }

            if (annotation is AvroSchemaFieldAttribute field) {
                return NewInstance<AvroSchemaFieldHook>(
                    Constant(field.Name),
                    Constant(field.Schema));
            }

            if (annotation is PrivateAttribute) {
                return NewInstance<AnnotationPrivate>();
            }

            if (annotation is ProtectedAttribute) {
                return NewInstance<AnnotationProtected>();
            }

            if (annotation is PublicAttribute) {
                return NewInstance<AnnotationPublic>();
            }

            if (annotation is BusEventTypeAttribute) {
                return NewInstance<AnnotationBusEventType>();
            }

            if (annotation is JsonSchemaAttribute jsonSchema) {
                return NewInstance<AnnotationJsonSchema>(Constant(jsonSchema.Dynamic), Constant(jsonSchema.ClassName));
            }

            if (annotation is JsonSchemaFieldAttribute jsonSchemaField) {
                return NewInstance<AnnotationJsonSchemaField>(Constant(jsonSchemaField.Name), Constant(jsonSchemaField.Adapter));
            }

            if (annotation is XMLSchemaAttribute xmlSchema) {
                return AnnotationXMLSchema.ToExpression(xmlSchema, parent, codegenClassScope);
            }

            if (annotation is XMLSchemaNamespacePrefixAttribute schemaNamespacePrefix) {
                return AnnotationXMLSchemaNamespacePrefix.ToExpression(schemaNamespacePrefix, parent, codegenClassScope);
            }

            if (annotation is XMLSchemaFieldAttribute xmlSchemaField) {
                return AnnotationXMLSchemaField.ToExpression(xmlSchemaField, parent, codegenClassScope);
            }
            
            if (annotation.GetType().Namespace == RootNamespace) {
                throw new IllegalStateException(
                    "Unrecognized annotation residing in the '" +
                    RootNamespace +
                    " namespace having type" +
                    annotation.GetType().Name);
            }

            // application-provided annotation
            if (annotation is CustomAttribute) {
                return ((CustomAttribute) annotation).MakeCodegenExpression(
                    parent,
                    codegenClassScope);
            }

            if (annotation is IProxyTargetAccessor proxyTargetAccessor) {
                var interceptor = (EPLAnnotationInvocationHandler) proxyTargetAccessor.GetInterceptors()[0];
                var methodNode = parent.MakeChild(typeof(Attribute), typeof(AnnotationUtil), codegenClassScope);

                methodNode.Block.DeclareVar(
                    interceptor.AnnotationClass,
                    "annotation",
                    NewInstance(interceptor.AnnotationClass));

                //var annotationType = CodegenMethod.MakeMethod(
                //    typeof(Type), typeof(AnnotationUtil), codegenClassScope);
                //clazz.AddMethod("annotationType", annotationType);
                //annotationType.Block.MethodReturn(Clazz(interceptor.AnnotationClass));

                foreach (var property in interceptor.AnnotationClass.GetProperties())
                {
                    if (!property.CanWrite) {
                        continue;
                    }

                    CodegenExpression valueExpression;

                    var value = interceptor.Attributes.Get(property.Name);
                    if (value == null)
                    {
                        valueExpression = ConstantNull();
                    }
                    else if (property.PropertyType == typeof(Type))
                    {
                        valueExpression = Clazz((Type) value);
                    }
                    else if (property.PropertyType.IsArray && property.PropertyType.GetElementType().IsAttribute())
                    {
                        valueExpression = LocalMethod(
                            MakeAnnotations(
                                property.PropertyType, (Attribute[]) value, methodNode, codegenClassScope));
                    }
                    else if (!property.PropertyType.IsAttribute())
                    {
                        valueExpression = Constant(value);
                    }
                    else
                    {
                        valueExpression = FlexCast(
                            property.PropertyType, 
                            MakeAnnotation((Attribute) value, methodNode, codegenClassScope));
                    }

                    methodNode.Block.SetProperty(
                        Ref("annotation"), property.Name, valueExpression);
                }

                methodNode.Block.MethodReturn(Ref("annotation"));
                return LocalMethod(methodNode);
            }

            throw new IllegalStateException(
                "Unrecognized annotation having type " + annotation.GetType().FullName);
        }
    }
} // end of namespacecom