///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Net;
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
    public class PopulateUtil {
        private static readonly string CLASS_PROPERTY_NAME = "class";
        private static readonly string SYSTEM_PROPETIES_NAME = "systemProperties".ToLowerInvariant();
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public static Object InstantiatePopulateObject(IDictionary<string, Object> objectProperties, Type topClass, ExprNodeOrigin exprNodeOrigin, ExprValidationContext exprValidationContext) {
    
            Type applicableClass = topClass;
            if (topClass.IsInterface) {
                applicableClass = FindInterfaceImplementation(objectProperties, topClass, exprValidationContext.EngineImportService);
            }
    
            Object top;
            try {
                top = applicableClass.NewInstance();
            } catch (RuntimeException e) {
                throw new ExprValidationException("Exception instantiating class " + applicableClass.Name + ": " + e.Message, e);
            } catch (InstantiationException e) {
                throw new ExprValidationException(GetMessageExceptionInstantiating(applicableClass), e);
            } catch (IllegalAccessException e) {
                throw new ExprValidationException("Illegal access to construct class " + applicableClass.Name + ": " + e.Message, e);
            }
    
            PopulateObject(topClass.SimpleName, 0, topClass.SimpleName, objectProperties, top, exprNodeOrigin, exprValidationContext, null, null);
    
            return top;
        }
    
        public static void PopulateObject(string operatorName, int operatorNum, string dataFlowName, IDictionary<string, Object> objectProperties, Object top, ExprNodeOrigin exprNodeOrigin, ExprValidationContext exprValidationContext, EPDataFlowOperatorParameterProvider optionalParameterProvider, IDictionary<string, Object> optionalParameterURIs)
                {
            Type applicableClass = top.Class;
            ISet<WriteablePropertyDescriptor> writables = PropertyHelper.GetWritableProperties(applicableClass);
            ISet<Field> annotatedFields = TypeHelper.FindAnnotatedFields(top.Class, typeof(DataFlowOpParameter));
            ISet<Method> annotatedMethods = TypeHelper.FindAnnotatedMethods(top.Class, typeof(DataFlowOpParameter));
    
            // find catch-all methods
            var catchAllMethods = new LinkedHashSet<Method>();
            if (annotatedMethods != null) {
                foreach (Method method in annotatedMethods) {
                    DataFlowOpParameter anno = (DataFlowOpParameter) TypeHelper.GetAnnotations(typeof(DataFlowOpParameter), method.DeclaredAnnotations)[0];
                    if (anno.All()) {
                        if (method.ParameterTypes.Length == 2 && method.ParameterTypes[0] == typeof(string) && method.ParameterTypes[1] == typeof(Object)) {
                            catchAllMethods.Add(method);
                            continue;
                        }
                        throw new ExprValidationException("Invalid annotation for catch-call");
                    }
                }
            }
    
            // map provided values
            foreach (var property in objectProperties) {
                bool found = false;
                string propertyName = property.Key;
    
                // invoke catch-all setters
                foreach (Method method in catchAllMethods) {
                    try {
                        method.Invoke(top, new Object[]{propertyName, property.Value});
                    } catch (IllegalAccessException e) {
                        throw new ExprValidationException("Illegal access invoking method for property '" + propertyName + "' for class " + applicableClass.Name + " method " + method.Name, e);
                    } catch (InvocationTargetException e) {
                        throw new ExprValidationException("Exception invoking method for property '" + propertyName + "' for class " + applicableClass.Name + " method " + method.Name + ": " + e.TargetException.Message, e);
                    }
                    found = true;
                }
    
                if (propertyName.ToLowerInvariant().Equals(CLASS_PROPERTY_NAME)) {
                    continue;
                }
    
                // use the writeable property descriptor (appropriate setter method) from writing the property
                WriteablePropertyDescriptor descriptor = FindDescriptor(applicableClass, propertyName, writables);
                if (descriptor != null) {
                    Object coerceProperty = CoerceProperty(propertyName, applicableClass, property.Value, descriptor.Type, exprNodeOrigin, exprValidationContext, false, true);
    
                    try {
                        descriptor.WriteMethod.Invoke(top, new Object[]{coerceProperty});
                    } catch (ArgumentException e) {
                        throw new ExprValidationException("Illegal argument invoking setter method for property '" + propertyName + "' for class " + applicableClass.Name + " method " + descriptor.WriteMethod.Name + " provided value " + coerceProperty, e);
                    } catch (IllegalAccessException e) {
                        throw new ExprValidationException("Illegal access invoking setter method for property '" + propertyName + "' for class " + applicableClass.Name + " method " + descriptor.WriteMethod.Name, e);
                    } catch (InvocationTargetException e) {
                        throw new ExprValidationException("Exception invoking setter method for property '" + propertyName + "' for class " + applicableClass.Name + " method " + descriptor.WriteMethod.Name + ": " + e.TargetException.Message, e);
                    }
                    continue;
                }
    
                // find the field annotated with {@link @GraphOpProperty}
                foreach (Field annotatedField in annotatedFields) {
                    DataFlowOpParameter anno = (DataFlowOpParameter) TypeHelper.GetAnnotations(typeof(DataFlowOpParameter), annotatedField.DeclaredAnnotations)[0];
                    if (anno.Name().Equals(propertyName) || annotatedField.Name.Equals(propertyName)) {
                        Object coerceProperty = CoerceProperty(propertyName, applicableClass, property.Value, annotatedField.Type, exprNodeOrigin, exprValidationContext, true, true);
                        try {
                            annotatedField.Accessible = true;
                            annotatedField.Set(top, coerceProperty);
                        } catch (Exception e) {
                            throw new ExprValidationException("Failed to set field '" + annotatedField.Name + "': " + e.Message, e);
                        }
                        found = true;
                        break;
                    }
                }
                if (found) {
                    continue;
                }
    
                throw new ExprValidationException("Failed to find writable property '" + propertyName + "' for class " + applicableClass.Name);
            }
    
            // second pass: if a parameter URI - value pairs were provided, check that
            if (optionalParameterURIs != null) {
                foreach (Field annotatedField in annotatedFields) {
                    try {
                        annotatedField.Accessible = true;
                        string uri = operatorName + "/" + annotatedField.Name;
                        if (optionalParameterURIs.ContainsKey(uri)) {
                            Object value = optionalParameterURIs.Get(uri);
                            annotatedField.Set(top, value);
                            if (Log.IsDebugEnabled) {
                                Log.Debug("Found parameter '" + uri + "' for data flow " + dataFlowName + " setting " + value);
                            }
                        } else {
                            if (Log.IsDebugEnabled) {
                                Log.Debug("Not found parameter '" + uri + "' for data flow " + dataFlowName);
                            }
                        }
                    } catch (Exception e) {
                        throw new ExprValidationException("Failed to set field '" + annotatedField.Name + "': " + e.Message, e);
                    }
                }
    
                foreach (Method method in annotatedMethods) {
                    DataFlowOpParameter anno = (DataFlowOpParameter) TypeHelper.GetAnnotations(typeof(DataFlowOpParameter), method.DeclaredAnnotations)[0];
                    if (anno.All()) {
                        if (method.ParameterTypes.Length == 2 && method.ParameterTypes[0] == typeof(string) && method.ParameterTypes[1] == typeof(Object)) {
                            foreach (var entry in optionalParameterURIs) {
                                string[] elements = URIUtil.ParsePathElements(URI.Create(entry.Key));
                                if (elements.Length < 2) {
                                    throw new ExprValidationException("Failed to parse URI '" + entry.Key + "', expected " +
                                            "'operator_name/property_name' format");
                                }
                                if (elements[0].Equals(operatorName)) {
                                    try {
                                        method.Invoke(top, new Object[]{elements[1], entry.Value});
                                    } catch (IllegalAccessException e) {
                                        throw new ExprValidationException("Illegal access invoking method for property '" + entry.Key + "' for class " + applicableClass.Name + " method " + method.Name, e);
                                    } catch (InvocationTargetException e) {
                                        throw new ExprValidationException("Exception invoking method for property '" + entry.Key + "' for class " + applicableClass.Name + " method " + method.Name + ": " + e.TargetException.Message, e);
                                    }
                                }
                            }
                        }
                    }
                }
            }
    
            // third pass: if a parameter provider is provided, use that
            if (optionalParameterProvider != null) {
    
                foreach (Field annotatedField in annotatedFields) {
                    try {
                        annotatedField.Accessible = true;
                        Object provided = annotatedField.Get(top);
                        Object value = optionalParameterProvider.Provide(new EPDataFlowOperatorParameterProviderContext(operatorName, annotatedField.Name, top, operatorNum, provided, dataFlowName));
                        if (value != null) {
                            annotatedField.Set(top, value);
                        }
                    } catch (Exception e) {
                        throw new ExprValidationException("Failed to set field '" + annotatedField.Name + "': " + e.Message, e);
                    }
                }
            }
        }
    
        private static Type FindInterfaceImplementation(IDictionary<string, Object> properties, Type topClass, EngineImportService engineImportService) {
            string message = "Failed to find implementation for interface " + topClass.Name;
    
            // Allow to populate the special "class" field
            if (!properties.ContainsKey(CLASS_PROPERTY_NAME)) {
                throw new ExprValidationException(message + ", for interfaces please specified the '" + CLASS_PROPERTY_NAME + "' field that provides the class name either as a simple class name or fully qualified");
            }
    
            Type clazz = null;
            string className = (string) properties.Get(CLASS_PROPERTY_NAME);
            try {
                clazz = TypeHelper.GetClassForName(className, engineImportService.ClassForNameProvider);
            } catch (ClassNotFoundException e) {
    
                if (!className.Contains(".")) {
                    className = topClass.Package.Name + "." + className;
                    try {
                        clazz = TypeHelper.GetClassForName(className, engineImportService.ClassForNameProvider);
                    } catch (ClassNotFoundException ex) {
                    }
                }
    
                if (clazz == null) {
                    throw new ExprValidationPropertyException(message + ", could not find class by name '" + className + "'");
                }
            }
    
            if (!TypeHelper.IsSubclassOrImplementsInterface(clazz, topClass)) {
                throw new ExprValidationException(message + ", class " + TypeHelper.GetTypeNameFullyQualPretty(clazz) + " does not implement the interface");
            }
            return clazz;
        }
    
        public static void PopulateSpecCheckParameters(PopulateFieldWValueDescriptor[] descriptors, IDictionary<string, Object> jsonRaw, Object spec, ExprNodeOrigin exprNodeOrigin, ExprValidationContext exprValidationContext)
                {
            // lowercase keys
            var lowerCaseJsonRaw = new LinkedHashMap<string, Object>();
            foreach (var entry in jsonRaw) {
                lowerCaseJsonRaw.Put(entry.Key.ToLowerInvariant(), entry.Value);
            }
            jsonRaw = lowerCaseJsonRaw;
    
            // apply values
            foreach (PopulateFieldWValueDescriptor desc in descriptors) {
                Object value = jsonRaw.Remove(desc.PropertyName.ToLowerInvariant());
                Object coerced = CoerceProperty(desc.PropertyName, desc.ContainerType, value, desc.FieldType, exprNodeOrigin, exprValidationContext, desc.IsForceNumeric, false);
                desc.Setter.Set(coerced);
            }
    
            // should not have remaining parameters
            if (!jsonRaw.IsEmpty()) {
                throw new ExprValidationException("Unrecognized parameter '" + jsonRaw.KeySet().GetEnumerator().Next() + "'");
            }
        }
    
        public static Object CoerceProperty(string propertyName, Type containingType, Object value, Type type, ExprNodeOrigin exprNodeOrigin, ExprValidationContext exprValidationContext, bool forceNumeric, bool includeClassNameInEx) {
            if (value is ExprNode && type != typeof(ExprNode)) {
                if (value is ExprIdentNode) {
                    ExprIdentNode identNode = (ExprIdentNode) value;
                    Property prop;
                    try {
                        prop = PropertyParser.ParseAndWalkLaxToSimple(identNode.FullUnresolvedName);
                    } catch (Exception ex) {
                        throw new ExprValidationException("Failed to parse property '" + identNode.FullUnresolvedName + "'");
                    }
                    if (!(prop is MappedProperty)) {
                        throw new ExprValidationException("Unrecognized property '" + identNode.FullUnresolvedName + "'");
                    }
                    MappedProperty mappedProperty = (MappedProperty) prop;
                    if (mappedProperty.PropertyNameAtomic.ToLowerInvariant().Equals(SYSTEM_PROPETIES_NAME)) {
                        return System.GetProperty(mappedProperty.Key);
                    }
                } else {
                    ExprNode exprNode = (ExprNode) value;
                    ExprNode validated = ExprNodeUtility.GetValidatedSubtree(exprNodeOrigin, exprNode, exprValidationContext);
                    exprValidationContext.VariableService.SetLocalVersion();
                    ExprEvaluator evaluator = validated.ExprEvaluator;
                    if (evaluator == null) {
                        throw new ExprValidationException("Failed to evaluate expression '" + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(exprNode) + "'");
                    }
                    value = evaluator.Evaluate(null, true, null);
                }
            }
    
            if (value == null) {
                return null;
            }
            if (value.Class == type) {
                return value;
            }
            if (TypeHelper.IsAssignmentCompatible(value.Class, type)) {
                if (forceNumeric && TypeHelper.GetBoxedType(value.Class) != TypeHelper.GetBoxedType(type) && TypeHelper.IsNumeric(type) && TypeHelper.IsNumeric(value.Class)) {
                    value = TypeHelper.CoerceBoxed((Number) value, TypeHelper.GetBoxedType(type));
                }
                return value;
            }
            if (TypeHelper.IsSubclassOrImplementsInterface(value.Class, type)) {
                return value;
            }
            if (type.IsArray) {
                if (!(value is Collection)) {
                    string detail = "expects an array but receives a value of type " + value.Class.Name;
                    throw new ExprValidationException(GetExceptionText(propertyName, containingType, includeClassNameInEx, detail));
                }
                Object[] items = ((Collection) value).ToArray();
                Object coercedArray = Array.NewInstance(type.ComponentType, items.Length);
                for (int i = 0; i < items.Length; i++) {
                    Object coercedValue = CoerceProperty(propertyName + " (array element)", type, items[i], type.ComponentType, exprNodeOrigin, exprValidationContext, false, includeClassNameInEx);
                    Array.Set(coercedArray, i, coercedValue);
                }
                return coercedArray;
            }
            if (!(value is Map)) {
                string detail = "expects an " + TypeHelper.GetTypeNameFullyQualPretty(type) + " but receives a value of type " + value.Class.Name;
                throw new ExprValidationException(GetExceptionText(propertyName, containingType, includeClassNameInEx, detail));
            }
            IDictionary<string, Object> props = (IDictionary<string, Object>) value;
            return InstantiatePopulateObject(props, type, exprNodeOrigin, exprValidationContext);
        }
    
        private static string GetExceptionText(string propertyName, Type containingType, bool includeClassNameInEx, string detailText) {
            string msg = "Property '" + propertyName + "'";
            if (includeClassNameInEx) {
                msg += " of class " + TypeHelper.GetTypeNameFullyQualPretty(containingType);
            }
            msg += " " + detailText;
            return msg;
        }
    
        private static WriteablePropertyDescriptor FindDescriptor(Type clazz, string propertyName, ISet<WriteablePropertyDescriptor> writables)
                {
            foreach (WriteablePropertyDescriptor desc in writables) {
                if (desc.PropertyName.ToLowerInvariant().Equals(propertyName.ToLowerInvariant())) {
                    return desc;
                }
            }
            return null;
        }
    
        private static string GetMessageExceptionInstantiating(Type clazz) {
            return "Exception instantiating class " + clazz.Name + ", please make sure the class has a public no-arg constructor (and for inner classes is declared static)";
        }
    }
} // end of namespace
