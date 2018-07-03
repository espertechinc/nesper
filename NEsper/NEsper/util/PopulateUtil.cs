///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.property;

namespace com.espertech.esper.util
{
    public class PopulateUtil
    {
        private const string CLASS_PROPERTY_NAME = "class";
        private const string SYSTEM_PROPETIES_NAME = "systemProperties";

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public static Object InstantiatePopulateObject(IDictionary<string, Object> objectProperties, Type topClass, ExprNodeOrigin exprNodeOrigin, ExprValidationContext exprValidationContext) {
    
            var applicableClass = topClass;
            if (topClass.IsInterface) {
                applicableClass = FindInterfaceImplementation(objectProperties, topClass, exprValidationContext.EngineImportService);
            }
    
            Object top;
            try {
                top = TypeHelper.Instantiate(applicableClass);
            }
            catch (TypeLoadException e)
            {
                throw new ExprValidationException(
                    GetMessageExceptionInstantiating(applicableClass), e);
            }
            catch (TypeInstantiationException e)
            {
                if (e.InnerException is MissingMethodException)
                {
                    throw new ExprValidationException(
                        GetMessageExceptionInstantiating(applicableClass), e);
                }
                else if (e.InnerException is MemberAccessException)
                {
                    throw new ExprValidationException(
                        "Illegal access to construct class " + applicableClass.FullName + ": " + e.InnerException.Message, e.InnerException);
                }
                else if (e.InnerException is TargetInvocationException)
                {
                    throw new ExprValidationException(
                        "Exception instantiating class " + applicableClass.FullName + ": " + e.InnerException.InnerException.Message, e.InnerException.InnerException);
                }
                else
                {
                    throw new ExprValidationException(
                        "Exception instantiating class " + applicableClass.FullName + ": " + e.InnerException.Message, e.InnerException);
                }
            }
            catch (MemberAccessException e)
            {
                throw new ExprValidationException("Illegal access to construct class " + applicableClass.FullName + ": " + e.Message, e);
            }
            catch (Exception e)
            {
                throw new ExprValidationException("Exception instantiating class " + applicableClass.FullName + ": " + e.Message, e);
            }
    
            PopulateObject(topClass.Name, 0, topClass.Name, objectProperties, top, exprNodeOrigin, exprValidationContext, null, null);
    
            return top;
        }

        public static void PopulateObject(
            string operatorName,
            int operatorNum,
            string dataFlowName,
            IDictionary<string, Object> objectProperties,
            Object top,
            ExprNodeOrigin exprNodeOrigin,
            ExprValidationContext exprValidationContext,
            EPDataFlowOperatorParameterProvider optionalParameterProvider,
            IDictionary<string, Object> optionalParameterURIs)
        {
            var applicableClass = top.GetType();
            var writables = PropertyHelper.GetWritableProperties(applicableClass);
            var annotatedFields = TypeHelper.FindAnnotatedFields(top.GetType(), typeof(DataFlowOpParameterAttribute));
            var annotatedMethods = TypeHelper.FindAnnotatedMethods(top.GetType(), typeof(DataFlowOpParameterAttribute));
    
            // find catch-all methods
            var catchAllMethods = new LinkedHashSet<MethodInfo>();
            if (annotatedMethods != null) {
                foreach (var method in annotatedMethods)
                {
                    var anno = (DataFlowOpParameterAttribute) TypeHelper.GetAnnotations(
                        typeof (DataFlowOpParameterAttribute),
                        method.GetCustomAttributes(true).Cast<Attribute>().ToArray())[0];
                    if (anno.All) {
                        var parameterTypes = method.GetParameterTypes();
                        if ((parameterTypes.Length == 2) && 
                            (parameterTypes[0] == typeof(string)) &&
                            (parameterTypes[1] == typeof(object)))
                        {
                            catchAllMethods.Add(method);
                            continue;
                        }
                        throw new ExprValidationException("Invalid annotation for catch-call");
                    }
                }
            }
    
            // map provided values
            foreach (var property in objectProperties) {
                var found = false;
                var propertyName = property.Key;
    
                // invoke catch-all setters
                foreach (var method in catchAllMethods) {
                    try {
                        object value = CoerceProperty(property.Value, method.GetParameters()[0].ParameterType);
                        method.Invoke(top, new Object[] { propertyName, value });
                    }
                    catch (MemberAccessException e)
                    {
                        throw new ExprValidationException("Illegal access invoking method for property '" + propertyName + "' for class " + applicableClass.Name + " method " + method.Name, e);
                    }
                    catch (TargetInvocationException e)
                    {
                        throw new ExprValidationException("Exception invoking method for property '" + propertyName + "' for class " + applicableClass.Name + " method " + method.Name + ": " + e.InnerException.Message, e);
                    }
                    found = true;
                }
    
                if (propertyName.ToLowerInvariant() == CLASS_PROPERTY_NAME) {
                    continue;
                }
    
                // use the writeable property descriptor (appropriate setter method) from writing the property
                var descriptor = FindDescriptor(applicableClass, propertyName, writables);
                if (descriptor != null) {
                    var coerceProperty = CoerceProperty(propertyName, applicableClass, property.Value, descriptor.PropertyType, exprNodeOrigin, exprValidationContext, false, true);
    
                    try {
                        descriptor.WriteMethod.Invoke(top, new Object[]{coerceProperty});
                    }
                    catch (ArgumentException e)
                    {
                        throw new ExprValidationException("Illegal argument invoking setter method for property '" + propertyName + "' for class " + applicableClass.Name + " method " + descriptor.WriteMethod.Name + " provided value " + coerceProperty, e);
                    }
                    catch (MemberAccessException e)
                    {
                        throw new ExprValidationException("Illegal access invoking setter method for property '" + propertyName + "' for class " + applicableClass.Name + " method " + descriptor.WriteMethod.Name, e);
                    }
                    catch (TargetInvocationException e)
                    {
                        throw new ExprValidationException("Exception invoking setter method for property '" + propertyName + "' for class " + applicableClass.Name + " method " + descriptor.WriteMethod.Name + ": " + e.InnerException.Message, e);
                    }
                    continue;
                }

                // in .NET, it's common to name fields with an underscore prefix, this modified
                // notation is preserved in the modPropertyName
                var modPropertyName = "_" + propertyName;
                // find the field annotated with <seealso cref="GraphOpProperty" />
                foreach (var annotatedField in annotatedFields)
                {
                    var anno = (DataFlowOpParameterAttribute) TypeHelper.GetAnnotations(
                        typeof (DataFlowOpParameterAttribute),
                        annotatedField.GetCustomAttributes(true).Cast<Attribute>().ToArray())[0];
                    if ((anno.Name == propertyName) || (annotatedField.Name == propertyName) || (annotatedField.Name == modPropertyName))
                    {
                        var coerceProperty = CoerceProperty(propertyName, applicableClass, property.Value, annotatedField.FieldType, exprNodeOrigin, exprValidationContext, true, true);
                        try
                        {
                            annotatedField.SetValue(top, coerceProperty);
                        }
                        catch (Exception e)
                        {
                            throw new ExprValidationException(
                                "Failed to set field '" + annotatedField.Name + "': " + e.Message, e);
                        }
                        found = true;
                        break;
                    }
                }
                if (found) {
                    continue;
                }
    
                throw new ExprValidationException("Failed to find writable property '" + propertyName + "' for class " + applicableClass.FullName);
            }
    
            // second pass: if a parameter URI - value pairs were provided, check that
            if (optionalParameterURIs != null)
            {
                foreach (var annotatedField in annotatedFields)
                {
                    try
                    {
                        var uri = operatorName + "/" + annotatedField.Name;
                        if (optionalParameterURIs.ContainsKey(uri))
                        {
                            var value = optionalParameterURIs.Get(uri);
                            annotatedField.SetValue(top, value);
                            if (Log.IsDebugEnabled)
                            {
                                Log.Debug(
                                    "Found parameter '" + uri + "' for data flow " + dataFlowName + " setting " + value);
                            }
                        }
                        else
                        {
                            if (Log.IsDebugEnabled)
                            {
                                Log.Debug("Not found parameter '" + uri + "' for data flow " + dataFlowName);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ExprValidationException(
                            "Failed to set field '" + annotatedField.Name + "': " + e.Message, e);
                    }
                }

                foreach (var method in annotatedMethods)
                {
                    var anno = method.GetCustomAttributes(typeof (DataFlowOpParameterAttribute), false)
                        .Cast<DataFlowOpParameterAttribute>()
                        .First();

                    if (anno.All)
                    {
                        var parameterTypes = method.GetParameterTypes();
                        if (parameterTypes.Length == 2 && parameterTypes[0] == typeof (string) &&
                            parameterTypes[1] == typeof (Object))
                        {
                            foreach (var entry in optionalParameterURIs)
                            {
                                var uri = new Uri(entry.Key, UriKind.RelativeOrAbsolute);
                                var elements = URIUtil.ParsePathElements(uri);
                                if (elements.Length < 2)
                                {
                                    throw new ExprValidationException(
                                        "Failed to parse URI '" + entry.Key + "', expected " +
                                        "'operator_name/property_name' format");
                                }
                                if (elements[0].Equals(operatorName))
                                {
                                    try
                                    {
                                        method.Invoke(
                                            top, new Object[]
                                            {
                                                elements[1],
                                                entry.Value
                                            });
                                    }
                                    catch (ArgumentException e)
                                    {
                                        throw new ExprValidationException(
                                            "Illegal argument invoking setter method for property '" + entry.Key +
                                            "' for class " + applicableClass.Name + " method " + method.Name, e);
                                    }
                                    catch (MemberAccessException e)
                                    {
                                        throw new ExprValidationException(
                                            "Illegal access invoking setter method for property '" + entry.Key +
                                            "' for class " + applicableClass.Name + " method " + method.Name, e);
                                    }
                                    catch (TargetInvocationException e)
                                    {
                                        throw new ExprValidationException(
                                            "Exception invoking setter method for property '" + entry.Key +
                                            "' for class " + applicableClass.Name + " method " + method.Name + ": " +
                                            e.InnerException.Message, e);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // third pass: if a parameter provider is provided, use that
            if (optionalParameterProvider != null)
            {
                foreach (var annotatedField in annotatedFields)
                {
                    try
                    {
                        var provided = annotatedField.GetValue(top);
                        var value = optionalParameterProvider.Provide(new EPDataFlowOperatorParameterProviderContext(operatorName, annotatedField.Name, top, operatorNum, provided, dataFlowName));
                        if (value != null)
                        {
                            annotatedField.SetValue(top, value);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ExprValidationException(
                            "Failed to set field '" + annotatedField.Name + "': " + e.Message, e);
                    }
                }
            }
        }

        private static Type FindInterfaceImplementation(
            IDictionary<string, Object> properties,
            Type topClass,
            EngineImportService engineImportService)
        {
            var message = "Failed to find implementation for interface " + Name.Clean(topClass);

            // Allow to populate the special "class" field
            if (!properties.ContainsKey(CLASS_PROPERTY_NAME))
            {
                throw new ExprValidationException(
                    message + ", for interfaces please specified the '" + CLASS_PROPERTY_NAME +
                    "' field that provides the class name either as a simple class name or fully qualified");
            }

            Type clazz = null;
            var className = (string) properties.Get(CLASS_PROPERTY_NAME);
            try
            {
                clazz = TypeHelper.GetClassForName(className, engineImportService.GetClassForNameProvider());
            }
            catch (TypeLoadException)
            {

                if (!className.Contains("."))
                {
                    className = topClass.Namespace + "." + className;
                    try
                    {
                        clazz = TypeHelper.GetClassForName(className, engineImportService.GetClassForNameProvider());
                    }
                    catch (TypeLoadException)
                    {
                    }
                }

                if (clazz == null)
                {
                    throw new ExprValidationPropertyException(message + ", could not find class by name '" + className + "'");
                }
            }

            if (!TypeHelper.IsSubclassOrImplementsInterface(clazz, topClass))
            {
                throw new ExprValidationException(
                    message + ", class " + clazz.GetCleanName() +
                    " does not implement the interface");
            }
            return clazz;
        }

        public static void PopulateSpecCheckParameters(
            PopulateFieldWValueDescriptor[] descriptors,
            IDictionary<string, Object> jsonRaw,
            Object spec,
            ExprNodeOrigin exprNodeOrigin,
            ExprValidationContext exprValidationContext)
        {
            // lowercase keys
            var lowerCaseJsonRaw = new LinkedHashMap<string, Object>();
            foreach (var entry in jsonRaw)
            {
                lowerCaseJsonRaw.Put(entry.Key.ToLowerInvariant(), entry.Value);
            }
            jsonRaw = lowerCaseJsonRaw;

            // apply values
            foreach (var desc in descriptors)
            {
                var value = jsonRaw.Delete(desc.PropertyName.ToLowerInvariant());
                var coerced = CoerceProperty(
                    desc.PropertyName, desc.ContainerType, value, desc.FieldType, exprNodeOrigin, exprValidationContext,
                    desc.IsForceNumeric, false);
                desc.Setter.Invoke(coerced);
            }

            // should not have remaining parameters
            if (!jsonRaw.IsEmpty())
            {
                throw new ExprValidationException(
                    "Unrecognized parameter '" + jsonRaw.Keys.First() + "'");
            }
        }

        private static object CoerceProperty(object value, Type parameterType)
        {
            if (value == null)
                return null;

            var valueType = value.GetType();
            if (valueType.IsAssignmentCompatible(parameterType))
            {
                if ((valueType.GetBoxedType() != parameterType.GetBoxedType())
                    && parameterType.IsNumeric()
                    && valueType.IsNumeric())
                {
                    value = CoercerFactory.CoerceBoxed(value, parameterType.GetBoxedType());
                }
                return value;
            }

            return value;
        }

        public static Object CoerceProperty(
            string propertyName,
            Type containingType,
            Object value,
            Type type,
            ExprNodeOrigin exprNodeOrigin,
            ExprValidationContext exprValidationContext,
            bool forceNumeric,
            bool includeClassNameInEx)
        {
            if (value is ExprNode && type != typeof(ExprNode)) {
                if (value is ExprIdentNode) {
                    var identNode = (ExprIdentNode) value;
                    Property prop;
                    try {
                        prop = PropertyParser.ParseAndWalkLaxToSimple(identNode.FullUnresolvedName);
                    }
                    catch (Exception) {
                        throw new ExprValidationException(
                            "Failed to parse property '" + identNode.FullUnresolvedName + "'");
                    }

                    if (!(prop is MappedProperty)) {
                        throw new ExprValidationException(
                            "Unrecognized property '" + identNode.FullUnresolvedName + "'");
                    }

                    var mappedProperty = (MappedProperty) prop;
                    if (string.Equals(
                        mappedProperty.PropertyNameAtomic, SYSTEM_PROPETIES_NAME,
                        StringComparison.InvariantCultureIgnoreCase)) {
                        return Environment.GetEnvironmentVariable(mappedProperty.Key);
                    }
                }
                else {
                    var exprNode = (ExprNode) value;
                    var validated = ExprNodeUtility.GetValidatedSubtree(
                        exprNodeOrigin, exprNode, exprValidationContext);
                    exprValidationContext.VariableService.SetLocalVersion();
                    var evaluator = validated.ExprEvaluator;
                    if (evaluator == null) {
                        throw new ExprValidationException(
                            "Failed to evaluate expression '" +
                            ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(exprNode) + "'");
                    }

                    value = evaluator.Evaluate(EvaluateParams.EmptyTrue);
                }
            }

            if (value == null) {
                return null;
            }

            var valueType = value.GetType();
            if (valueType == type) {
                return value;
            }

            if (valueType.IsAssignmentCompatible(type)) {
                if (forceNumeric
                    && (valueType.GetBoxedType() != type.GetBoxedType())
                    && type.IsNumeric()
                    && valueType.IsNumeric()) {
                    value = CoercerFactory.CoerceBoxed(value, type.GetBoxedType());
                }

                return value;
            }

#if false
            // numerical coercion between non-boxed types and incompatible boxed types
            if ((valueType.IsNumeric()) &&
                (valueType.IsNullable() == false) &&
                (type.IsNumeric()) &&
                (type.IsNullable())) {
                var typeNonGeneric = Nullable.GetUnderlyingType(type);
                if (valueType.IsAssignmentCompatible(typeNonGeneric)) {
                    value = CoercerFactory.CoerceBoxed(value, type);
                }

                return value;
            }
#endif

            if (TypeHelper.IsSubclassOrImplementsInterface(valueType, type)) {
                return value;
            }

            if (type.IsArray) {
                if (!(valueType.IsGenericCollection())) {
                    var detail = "expects an array but receives a value of type " + valueType.GetCleanName();
                    throw new ExprValidationException(
                        GetExceptionText(propertyName, containingType, includeClassNameInEx, detail));
                }

                var items = value.UnwrapIntoArray<object>();
                var coercedArray = Array.CreateInstance(type.GetElementType(), items.Length);
                for (var i = 0; i < items.Length; i++) {
                    var coercedValue = CoerceProperty(
                        propertyName + " (array element)", type, items[i], type.GetElementType(), exprNodeOrigin,
                        exprValidationContext, false, includeClassNameInEx);
                    coercedArray.SetValue(coercedValue, i);
                }

                return coercedArray;
            }

            if (type.IsNullable() && !valueType.IsNullable()) {
                var typeNonGeneric = Nullable.GetUnderlyingType(type);
                if (valueType.IsAssignmentCompatible(typeNonGeneric)) {
                    return CoercerFactory.CoerceBoxed(value, type);
                }
            }

            if (!(value is IDictionary<String, Object>)) {
                var detail = "expects an " + type.GetCleanName() + " but receives a value of type " +
                             valueType.GetCleanName();
                throw new ExprValidationException(
                    GetExceptionText(propertyName, containingType, includeClassNameInEx, detail));
            }

            var props = (IDictionary<string, Object>) value;
            return InstantiatePopulateObject(props, type, exprNodeOrigin, exprValidationContext);
        }

        private static string GetExceptionText(
            string propertyName,
            Type containingType,
            bool includeClassNameInEx,
            string detailText)
        {
            var msg = "Property '" + propertyName + "'";
            if (includeClassNameInEx)
            {
                msg += " of class " + containingType.GetCleanName();
            }
            msg += " " + detailText;
            return msg;
        }

        private static WriteablePropertyDescriptor FindDescriptor(
            Type clazz,
            string propertyName,
            IEnumerable<WriteablePropertyDescriptor> writables)
        {
            return writables
                .FirstOrDefault(desc => string.Equals(desc.PropertyName, propertyName, StringComparison.InvariantCultureIgnoreCase));
        }

        private static string GetMessageExceptionInstantiating(Type clazz)
        {
            return "Exception instantiating class " + clazz.FullName +
                   ", please make sure the class has a public no-arg constructor (and for inner classes is declared static)";
        }
    }
} // end of namespace
