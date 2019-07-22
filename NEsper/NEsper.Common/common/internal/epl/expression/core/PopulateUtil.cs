///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.epl.expression.etc;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using Castle.Core.Internal;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class PopulateUtil
    {
        private const string CLASS_PROPERTY_NAME = "class";
        private static readonly ILog Log = LogManager.GetLogger(typeof(PopulateUtil));

        public static void PopulateSpecCheckParameters(
            PopulateFieldWValueDescriptor[] descriptors,
            IDictionary<string, object> jsonRaw,
            object spec,
            ExprNodeOrigin exprNodeOrigin,
            ExprValidationContext exprValidationContext)
        {
            // lowercase keys
            IDictionary<string, object> lowerCaseJsonRaw = new LinkedHashMap<string, object>();
            foreach (var entry in jsonRaw) {
                lowerCaseJsonRaw.Put(entry.Key.ToLowerInvariant(), entry.Value);
            }

            jsonRaw = lowerCaseJsonRaw;

            // apply values
            foreach (var desc in descriptors) {
                object value = jsonRaw.Remove(desc.PropertyName.ToLowerInvariant());
                var coerced = CoerceProperty(
                    desc.PropertyName,
                    desc.ContainerType,
                    value,
                    desc.FieldType,
                    exprNodeOrigin,
                    exprValidationContext,
                    desc.IsForceNumeric,
                    false);
                desc.Setter.Invoke(coerced);
            }

            // should not have remaining parameters
            if (!jsonRaw.IsEmpty()) {
                throw new ExprValidationException("Unrecognized parameter '" + jsonRaw.Keys.First() + "'");
            }
        }

        public static object CoerceProperty(
            string propertyName,
            Type containingType,
            object value,
            Type type,
            ExprNodeOrigin exprNodeOrigin,
            ExprValidationContext exprValidationContext,
            bool forceNumeric,
            bool includeClassNameInEx)
        {
            // handle system-property exception
            if (value is ExprNode) {
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
                    if (mappedProperty.PropertyNameAtomic.ToLowerInvariant()
                        .Equals(ExprEvalSystemProperty.SYSTEM_PROPETIES_NAME)) {
                        if (type == typeof(ExprNode)) {
                            return new ExprEvalSystemProperty(mappedProperty.Key);
                        }

                        return Environment.GetEnvironmentVariable(mappedProperty.Key);
                    }
                }
                else {
                    var exprNode = (ExprNode) value;
                    if (type == typeof(ExprNode)) {
                        return exprNode;
                    }

                    if (!exprNode.Forge.ForgeConstantType.IsCompileTimeConstant) {
                        throw new ExprValidationException(
                            "Failed to determine parameter for property '" +
                            propertyName +
                            "' as the parameter is not a compile-time constant expression");
                    }

                    value = exprNode.Forge.ExprEvaluator.Evaluate(null, true, null);
                }
            }

            if (value == null) {
                return null;
            }

            if (value.GetType() == type) {
                return value;
            }

            if (value.GetType().IsAssignmentCompatible(type)) {
                if (forceNumeric &&
                    value.GetType().GetBoxedType() != type.GetBoxedType() &&
                    type.IsNumeric() &&
                    value.GetType().IsNumeric()) {
                    value = TypeHelper.CoerceBoxed(value, type.GetBoxedType());
                }

                return value;
            }

            if (TypeHelper.IsSubclassOrImplementsInterface(value.GetType(), type)) {
                return value;
            }

            if (type.IsArray) {
                if (!value.GetType().IsGenericCollection()) {
                    var detail = "expects an array but receives a value of type " + value.GetType().Name;
                    throw new ExprValidationException(
                        GetExceptionText(propertyName, containingType, includeClassNameInEx, detail));
                }

                var items = value.UnwrapIntoArray<object>();
                var coercedArray = Array.CreateInstance(type.GetElementType(), items.Length);
                for (var i = 0; i < items.Length; i++) {
                    var coercedValue = CoerceProperty(
                        propertyName + " (array element)",
                        type,
                        items[i],
                        type.GetElementType(),
                        exprNodeOrigin,
                        exprValidationContext,
                        false,
                        includeClassNameInEx);
                    coercedArray.SetValue(coercedValue, i);
                }

                return coercedArray;
            }

            if (!(value is IDictionary<string, object>)) {
                var detail = "expects an " +
                             type.GetCleanName() +
                             " but receives a value of type " +
                             value.GetType().Name;
                throw new ExprValidationException(
                    GetExceptionText(propertyName, containingType, includeClassNameInEx, detail));
            }

            var props = (IDictionary<string, object>) value;
            return InstantiatePopulateObject(props, type, exprNodeOrigin, exprValidationContext);
        }

        public static object InstantiatePopulateObject(
            IDictionary<string, object> objectProperties,
            Type topClass,
            ExprNodeOrigin exprNodeOrigin,
            ExprValidationContext exprValidationContext)
        {
            var applicableClass = topClass;
            if (topClass.IsInterface) {
                applicableClass = FindInterfaceImplementation(
                    objectProperties,
                    topClass,
                    exprValidationContext.ImportService);
            }

            object top;
            try {
                top = TypeHelper.Instantiate(applicableClass);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                throw new ExprValidationException(
                    "Exception instantiating class " + applicableClass.Name + ": " + ex.Message,
                    ex);
            }

            PopulateObject(objectProperties, top, exprNodeOrigin, exprValidationContext);

            return top;
        }

        private static Type FindInterfaceImplementation(
            IDictionary<string, object> properties,
            Type topClass,
            ImportService importService)
        {
            var message = "Failed to find implementation for interface " + topClass.Name;

            // Allow to populate the special "class" field
            if (!properties.ContainsKey(CLASS_PROPERTY_NAME)) {
                throw new ExprValidationException(
                    message +
                    ", for interfaces please specified the '" +
                    CLASS_PROPERTY_NAME +
                    "' field that provides the class name either as a simple class name or fully qualified");
            }

            Type clazz = null;
            var className = (string) properties.Get(CLASS_PROPERTY_NAME);
            try {
                clazz = TypeHelper.GetClassForName(className, importService.ClassForNameProvider);
            }
            catch (TypeLoadException) {
                if (!className.Contains(".")) {
                    className = topClass.Namespace + "." + className;
                    try {
                        clazz = TypeHelper.GetClassForName(className, importService.ClassForNameProvider);
                    }
                    catch (TypeLoadException) {
                    }
                }

                if (clazz == null) {
                    throw new ExprValidationPropertyException(
                        message + ", could not find class by name '" + className + "'");
                }
            }

            if (!TypeHelper.IsSubclassOrImplementsInterface(clazz, topClass)) {
                throw new ExprValidationException(
                    message +
                    ", class " +
                    clazz.GetCleanName() +
                    " does not implement the interface");
            }

            return clazz;
        }

        public static void PopulateObject(
            string operatorName,
            int operatorNum,
            string dataFlowName,
            IDictionary<string, object> objectProperties,
            object top,
            ExprNodeOrigin exprNodeOrigin,
            ExprValidationContext exprValidationContext,
            EPDataFlowOperatorParameterProvider optionalParameterProvider,
            IDictionary<string, object> optionalParameterURIs)
        {
            var applicableClass = top.GetType();
            var writables = PropertyHelper.GetWritableProperties(applicableClass);
            var annotatedFields =
                TypeHelper.FindAnnotatedFields(top.GetType(), typeof(DataFlowOpParameterAttribute));
            var annotatedMethods =
                TypeHelper.FindAnnotatedMethods(top.GetType(), typeof(DataFlowOpParameterAttribute));

            // find catch-all methods
            ISet<MethodInfo> catchAllMethods = new LinkedHashSet<MethodInfo>();
            if (annotatedMethods != null) {
                foreach (var method in annotatedMethods) {
                    var anno = (DataFlowOpParameterAttribute) TypeHelper.GetAnnotations(
                        typeof(DataFlowOpParameterAttribute),
                        method.UnwrapAttributes())[0];
                    if (anno.IsAll) {
                        var parameterTypes = method.GetParameterTypes();
                        if (parameterTypes.Length == 2 &&
                            parameterTypes[0] == typeof(string) &&
                            parameterTypes[1] == typeof(ExprNode)) {
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
                        method.Invoke(top, new[] {propertyName, property.Value});
                    }
                    catch (MemberAccessException e) {
                        throw new ExprValidationException(
                            "Illegal access invoking method for property '" +
                            propertyName +
                            "' for class " +
                            applicableClass.Name +
                            " method " +
                            method.Name,
                            e);
                    }
                    catch (TargetException e) {
                        throw new ExprValidationException(
                            "Exception invoking method for property '" +
                            propertyName +
                            "' for class " +
                            applicableClass.Name +
                            " method " +
                            method.Name +
                            ": " +
                            e.InnerException.Message,
                            e);
                    }

                    found = true;
                }

                if (propertyName.ToLowerInvariant().Equals(CLASS_PROPERTY_NAME)) {
                    continue;
                }

                // use the writeable property descriptor (appropriate setter method) from writing the property
                var descriptor = FindDescriptor(applicableClass, propertyName, writables);
                if (descriptor != null) {
                    var coerceProperty = CoerceProperty(
                        propertyName,
                        applicableClass,
                        property.Value,
                        descriptor.PropertyType,
                        exprNodeOrigin,
                        exprValidationContext,
                        false,
                        true);

                    try {
                        descriptor.WriteMethod.Invoke(top, new[] {coerceProperty});
                    }
                    catch (ArgumentException e) {
                        throw new ExprValidationException(
                            "Illegal argument invoking setter method for property '" +
                            propertyName +
                            "' for class " +
                            applicableClass.Name +
                            " method " +
                            descriptor.WriteMethod.Name +
                            " provided value " +
                            coerceProperty,
                            e);
                    }
                    catch (MemberAccessException e) {
                        throw new ExprValidationException(
                            "Illegal access invoking setter method for property '" +
                            propertyName +
                            "' for class " +
                            applicableClass.Name +
                            " method " +
                            descriptor.WriteMethod.Name,
                            e);
                    }
                    catch (TargetException e) {
                        throw new ExprValidationException(
                            "Exception invoking setter method for property '" +
                            propertyName +
                            "' for class " +
                            applicableClass.Name +
                            " method " +
                            descriptor.WriteMethod.Name +
                            ": " +
                            e.InnerException.Message,
                            e);
                    }

                    continue;
                }

                // find the field annotated with {@link @GraphOpProperty}
                foreach (var annotatedField in annotatedFields) {
                    var anno = (DataFlowOpParameterAttribute) TypeHelper.GetAnnotations<DataFlowOpParameterAttribute>(
                        annotatedField.UnwrapAttributes())[0];
                    if (anno.Name.Equals(propertyName) || annotatedField.Name.Equals(propertyName)) {
                        var coerceProperty = CoerceProperty(
                            propertyName,
                            applicableClass,
                            property.Value,
                            annotatedField.FieldType,
                            exprNodeOrigin,
                            exprValidationContext,
                            true,
                            true);
                        try {
                            annotatedField.SetValue(top, coerceProperty);
                        }
                        catch (Exception e) {
                            throw new ExprValidationException(
                                "Failed to set field '" + annotatedField.Name + "': " + e.Message,
                                e);
                        }

                        found = true;
                        break;
                    }
                }

                if (found) {
                    continue;
                }

                throw new ExprValidationException(
                    "Failed to find writable property '" + propertyName + "' for class " + applicableClass.Name);
            }

            // second pass: if a parameter URI - value pairs were provided, check that
            if (optionalParameterURIs != null) {
                foreach (var annotatedField in annotatedFields) {
                    try {
                        var uri = operatorName + "/" + annotatedField.Name;
                        if (optionalParameterURIs.ContainsKey(uri)) {
                            var value = optionalParameterURIs.Get(uri);
                            annotatedField.SetValue(top, value);
                            if (Log.IsDebugEnabled) {
                                Log.Debug(
                                    "Found parameter '" +
                                    uri +
                                    "' for data flow " +
                                    dataFlowName +
                                    " setting " +
                                    value);
                            }
                        }
                        else {
                            if (Log.IsDebugEnabled) {
                                Log.Debug("Not found parameter '" + uri + "' for data flow " + dataFlowName);
                            }
                        }
                    }
                    catch (Exception e) {
                        throw new ExprValidationException(
                            "Failed to set field '" + annotatedField.Name + "': " + e.Message,
                            e);
                    }
                }

                foreach (var method in annotatedMethods) {
                    var anno = (DataFlowOpParameterAttribute) TypeHelper.GetAnnotations(
                        typeof(DataFlowOpParameterAttribute),
                        method.UnwrapAttributes())[0];
                    if (anno.IsAll) {
                        var parameters = method.GetParameters();

                        var parameterTypes = method.GetParameterTypes();
                        if (parameterTypes.Length == 2 &&
                            parameterTypes[0] == typeof(string) &&
                            parameterTypes[1] == typeof(object)) {
                            foreach (var entry in optionalParameterURIs) {
                                var elements = URIUtil.ParsePathElements(new Uri(entry.Key));
                                if (elements.Length < 2) {
                                    throw new ExprValidationException(
                                        "Failed to parse URI '" +
                                        entry.Key +
                                        "', expected " +
                                        "'operator_name/property_name' format");
                                }

                                if (elements[0].Equals(operatorName)) {
                                    try {
                                        method.Invoke(top, new[] {elements[1], entry.Value});
                                    }
                                    catch (MemberAccessException e) {
                                        throw new ExprValidationException(
                                            "Illegal access invoking method for property '" +
                                            entry.Key +
                                            "' for class " +
                                            applicableClass.Name +
                                            " method " +
                                            method.Name,
                                            e);
                                    }
                                    catch (TargetException e) {
                                        throw new ExprValidationException(
                                            "Exception invoking method for property '" +
                                            entry.Key +
                                            "' for class " +
                                            applicableClass.Name +
                                            " method " +
                                            method.Name +
                                            ": " +
                                            e.InnerException.Message,
                                            e);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void PopulateObject(
            IDictionary<string, object> objectProperties,
            object top,
            ExprNodeOrigin exprNodeOrigin,
            ExprValidationContext exprValidationContext)
        {
            var applicableClass = top.GetType();
            var writables = PropertyHelper.GetWritableProperties(applicableClass);
            var annotatedFields = TypeHelper.FindAnnotatedFields(top.GetType(), typeof(DataFlowOpParameterAttribute));
            var annotatedMethods = TypeHelper.FindAnnotatedMethods(top.GetType(), typeof(DataFlowOpParameterAttribute));

            // find catch-all methods
            ISet<MethodInfo> catchAllMethods = new LinkedHashSet<MethodInfo>();
            if (annotatedMethods != null) {
                foreach (var method in annotatedMethods) {
                    var anno = (DataFlowOpParameterAttribute) TypeHelper
                        .GetAnnotations<DataFlowOpParameterAttribute>(method.UnwrapAttributes())[0];
                    if (anno.IsAll) {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 2 &&
                            (parameters[0].ParameterType == typeof(string)) &&
                            (parameters[1].ParameterType == typeof(object))) {
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
                        method.Invoke(top, new[] {propertyName, property.Value});
                    }
                    catch (MemberAccessException e) {
                        throw new ExprValidationException(
                            "Illegal access invoking method for property '" +
                            propertyName +
                            "' for class " +
                            applicableClass.Name +
                            " method " +
                            method.Name,
                            e);
                    }
                    catch (TargetException e) {
                        throw new ExprValidationException(
                            "Exception invoking method for property '" +
                            propertyName +
                            "' for class " +
                            applicableClass.Name +
                            " method " +
                            method.Name +
                            ": " +
                            e.InnerException.Message,
                            e);
                    }

                    found = true;
                }

                if (propertyName.ToLowerInvariant().Equals(CLASS_PROPERTY_NAME)) {
                    continue;
                }

                // use the writeable property descriptor (appropriate setter method) from writing the property
                var descriptor = FindDescriptor(applicableClass, propertyName, writables);
                if (descriptor != null) {
                    var coerceProperty = CoerceProperty(
                        propertyName,
                        applicableClass,
                        property.Value,
                        descriptor.PropertyType,
                        exprNodeOrigin,
                        exprValidationContext,
                        false,
                        true);

                    try {
                        descriptor.WriteMethod.Invoke(top, new[] {coerceProperty});
                    }
                    catch (ArgumentException e) {
                        throw new ExprValidationException(
                            "Illegal argument invoking setter method for property '" +
                            propertyName +
                            "' for class " +
                            applicableClass.Name +
                            " method " +
                            descriptor.WriteMethod.Name +
                            " provided value " +
                            coerceProperty,
                            e);
                    }
                    catch (MemberAccessException e) {
                        throw new ExprValidationException(
                            "Illegal access invoking setter method for property '" +
                            propertyName +
                            "' for class " +
                            applicableClass.Name +
                            " method " +
                            descriptor.WriteMethod.Name,
                            e);
                    }
                    catch (TargetException e) {
                        throw new ExprValidationException(
                            "Exception invoking setter method for property '" +
                            propertyName +
                            "' for class " +
                            applicableClass.Name +
                            " method " +
                            descriptor.WriteMethod.Name +
                            ": " +
                            e.InnerException.Message,
                            e);
                    }

                    continue;
                }

                // find the field annotated with {@link @GraphOpProperty}
                foreach (var annotatedField in annotatedFields) {
                    var anno = (DataFlowOpParameterAttribute) TypeHelper.GetAnnotations(
                        typeof(DataFlowOpParameterAttribute),
                        annotatedField.UnwrapAttributes())[0];
                    if (anno.Name.Equals(propertyName) || annotatedField.Name.Equals(propertyName)) {
                        var coerceProperty = CoerceProperty(
                            propertyName,
                            applicableClass,
                            property.Value,
                            annotatedField.FieldType,
                            exprNodeOrigin,
                            exprValidationContext,
                            true,
                            true);
                        try {
                            annotatedField.SetValue(top, coerceProperty);
                        }
                        catch (Exception e) {
                            throw new ExprValidationException(
                                "Failed to set field '" + annotatedField.Name + "': " + e.Message,
                                e);
                        }

                        found = true;
                        break;
                    }
                }

                if (found) {
                    continue;
                }

                throw new ExprValidationException(
                    "Failed to find writable property '" + propertyName + "' for class " + applicableClass.Name);
            }
        }

        private static string GetExceptionText(
            string propertyName,
            Type containingType,
            bool includeClassNameInEx,
            string detailText)
        {
            var msg = "Property '" + propertyName + "'";
            if (includeClassNameInEx) {
                msg += " of class " + containingType.GetCleanName();
            }

            msg += " " + detailText;
            return msg;
        }

        private static WriteablePropertyDescriptor FindDescriptor(
            Type clazz,
            string propertyName,
            ISet<WriteablePropertyDescriptor> writables)
        {
            foreach (var desc in writables) {
                if (desc.PropertyName.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase)) {
                    return desc;
                }
            }

            return null;
        }

        private static string GetMessageExceptionInstantiating(Type clazz)
        {
            return "Exception instantiating class " +
                   clazz.Name +
                   ", please make sure the class has a public no-arg constructor (and for inner classes is declared static)";
        }
    }
} // end of namespace