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

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.magic;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.annotation
{
    /// <summary>
    /// Utility to handle EPL statement annotations.
    /// </summary>
    public class AnnotationUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static IDictionary<String, IList<AnnotationDesc>> MapByNameLowerCase(IList<AnnotationDesc> annotations)
        {
            var map = new Dictionary<String, IList<AnnotationDesc>>();
            foreach (AnnotationDesc desc in annotations)
            {
                var key = desc.Name.ToLower();
                if (map.ContainsKey(key))
                {
                    map.Get(key).Add(desc);
                    continue;
                }

                var annos = new List<AnnotationDesc>(2) { desc };
                map.Put(key, annos);
            }
            return map;
        }

        public static Object GetValue(AnnotationDesc desc)
        {
            foreach (var pair in desc.Attributes)
            {
                if (pair.First.ToLower() == "value")
                {
                    return pair.Second;
                }
            }
            return null;
        }

        /// <summary>
        /// Compile annotation objects from descriptors.
        /// </summary>
        /// <param name="annotationSpec">spec for annotations</param>
        /// <param name="engineImportService">engine imports</param>
        /// <param name="eplStatement">statement expression</param>
        /// <returns>
        /// annotations
        /// </returns>
        public static Attribute[] CompileAnnotations(IList<AnnotationDesc> annotationSpec, EngineImportService engineImportService, String eplStatement)
        {
            try
            {
                return CompileAnnotations(annotationSpec, engineImportService);
            }
            catch (AttributeException ex)
            {
                throw new EPStatementException("Failed to process statement annotations: " + ex.Message, eplStatement, ex);
            }
            catch (Exception ex)
            {
                var message = "Unexpected exception compiling annotations in statement, please consult the log file and report the exception: " + ex.Message;
                Log.Error(message, ex);
                throw new EPStatementException(message, eplStatement, ex);
            }
        }

        /// <summary>
        /// Compiles attributes / annotations to an array.
        /// </summary>
        /// <param name="desc">a list of descriptors</param>
        /// <param name="engineImportService">for resolving the annotation/attribute class</param>
        /// <returns>
        /// attributes / annotations or empty array if none
        /// </returns>
        private static Attribute[] CompileAnnotations(IList<AnnotationDesc> desc, EngineImportService engineImportService)
        {
            if (desc == null)
                return new Attribute[0];

            var attributes = new Attribute[desc.Count];
            for (int ii = 0; ii < desc.Count; ii++)
            {
                attributes[ii] = CompileAnnotation(desc[ii], engineImportService);
            }
            return attributes;
        }

        /// <summary>
        /// Resolves the type of the attribute.
        /// </summary>
        /// <param name="desc">The desc.</param>
        /// <param name="engineImportService">The engine import service.</param>
        /// <returns></returns>
        public static Type ResolveAttributeType(AnnotationDesc desc, EngineImportService engineImportService)
        {
            // CLR attributes use a different notation that Java annotations.  Format
            // the attribute according to CLR conventions.
            var attributeTypeName = desc.Name;
            var attributeTypeNameForCLR =
                (attributeTypeName.EndsWith("Attribute"))
                    ? attributeTypeName
                    : String.Format("{0}Attribute", attributeTypeName);

            // resolve attribute type
            try
            {
                engineImportService.GetClassLoader(); // Currently unused
                return engineImportService.ResolveAnnotation(attributeTypeNameForCLR);
            }
            catch (EngineImportException e)
            {
                throw new AttributeException("Failed to resolve @-annotation class: " + e.Message);
            }
        }

        /// <summary>
        /// Compiles the attribute.
        /// </summary>
        /// <param name="desc">The desc.</param>
        /// <param name="engineImportService">The engine import service.</param>
        /// <returns></returns>
        public static Attribute CompileAnnotation(AnnotationDesc desc, EngineImportService engineImportService)
        {
            // CLR attributes use a different notation that Java annotations.  Format
            // the attribute according to CLR conventions.
            var attributeTypeName = desc.Name;
            var attributeType = ResolveAttributeType(desc, engineImportService);

            // Get the magic type for the attribute
            var attributeMagic = MagicType.GetCachedType(attributeType);
            if (!attributeMagic.ExtendsType(typeof(Attribute)))
            {
                throw new AttributeException(String.Format("Annotation '{0}' does not extends System.Attribute",
                                                            attributeTypeName));
            }

            // Search for a constructor that matches the constructor parameters
            var theConstructor = attributeType.GetConstructor(new Type[0]);
            if (theConstructor == null)
            {
                throw new AttributeException("Failed to find constructor for @-annotation class");
            }

            // Create the attribute
            var attributeInstance = (Attribute)theConstructor.Invoke(null);
            // Create a collection of properties that have been explicitly set for later
            var explicitPropertyTable = new HashSet<string>();

            // Set properties on the attribute
            foreach (var attributeValuePair in desc.Attributes)
            {
                var propertyName = attributeValuePair.First;

                // Check the explicitPropertyTable to see if we have already set
                // this property.  If we have, then throw an exception as we do not
                // allow a property to be set twice (i.e. duplicated)
                if (explicitPropertyTable.Contains(propertyName))
                {
                    throw new AttributeException(
                        "Annotation '" + attributeTypeName + "' has duplicate attribute values for attribute '" + propertyName + "'");
                }

                var magicProperty = attributeMagic.ResolveProperty(propertyName, PropertyResolutionStyle.CASE_SENSITIVE);
                if (magicProperty == null)
                {
                    throw new AttributeException(
                        String.Format("Failed to find property {0} in annotation type {1}", propertyName, attributeTypeName));
                }

                var propertyValue = attributeValuePair.Second;
                if (propertyValue is AnnotationDesc)
                {
                    propertyValue = CompileAnnotation((AnnotationDesc)propertyValue, engineImportService);
                }

                var magicPropertyType = magicProperty.PropertyType;
                if (magicPropertyType.IsArray)
                {
                    propertyValue = CheckArray(attributeTypeName, magicProperty, propertyName, propertyValue);
                }

                propertyValue = CheckTypeMismatch(attributeTypeName, magicPropertyType, propertyName, propertyValue);

                magicProperty.SetFunction(attributeInstance, propertyValue);

                explicitPropertyTable.Add(propertyName);
            }

            // Make sure all required attributes were set
            foreach (var property in attributeMagic.GetSimpleProperties(true))
            {
#if EXPLICIT_ATTRIBUTE_PROPERTIES
                // Explicit properties make this relatively painless if the value
                // was set through the property model.
                if (explicitPropertyTable.Contains(property.Name))
                    continue;
#endif

                // Not set explicitly which makes this somewhat ... painful
                if (property.Member is PropertyInfo)
                {
                    var propertyInfo = (PropertyInfo)property.Member;
                    var requiredAttributes = propertyInfo.GetCustomAttributes(typeof(RequiredAttribute), true);
                    if ((requiredAttributes != null) && (requiredAttributes.Length > 0))
                    {
                        var defaultValue = GetDefaultValue(property.PropertyType);
                        // Property is required ... unfortunately, properties can be set through
                        // constructors or late-bound properties.  It's honestly impossible for us
                        // to accurately determine if we have really bound the property.
                        var currentValue = property.GetMethod.Invoke(attributeInstance, new object[0]);
                        // Here's where it gets fuzzy, we're going to compare the currentValue against
                        // the defaultValue.  This is an inexact science ...
                        if (Equals(defaultValue, currentValue))
                        {
                            var propertyName = property.Name;
                            throw new AttributeException("Annotation '" + attributeTypeName + "' requires a value for attribute '" + propertyName + "'");
                        }
                    }
                }
            }

            if (attributeInstance is HintAttribute)
            {
                HintEnumExtensions.ValidateGetListed(attributeInstance);
            }

            return attributeInstance;
        }

        /// <summary>
        /// Checks the property type for a type mismatch.
        /// </summary>
        /// <param name="attributeTypeName">Name of the attribute type.</param>
        /// <param name="magicPropertyType">Type of the magic property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        private static object CheckTypeMismatch(string attributeTypeName, Type magicPropertyType, string propertyName, object propertyValue)
        {
            var isTypeMismatch = false;
            if (magicPropertyType.IsValueType)
            {
                if (propertyValue == null)
                {
                    isTypeMismatch = true;
                }
                else if (magicPropertyType.IsEnum && (propertyValue is string))
                {
                    return Enum.Parse(magicPropertyType, (string)propertyValue, true);
                }
                else if (!propertyValue.GetType().IsAssignableFrom(magicPropertyType))
                {
                    var typeCaster = CastHelper.GetCastConverter(magicPropertyType);
                    var newValue = typeCaster.Invoke(propertyValue);
                    if (newValue != null)
                    {
                        propertyValue = newValue;
                    }
                    else
                    {
                        isTypeMismatch = true;
                    }
                }
            }
            else if (propertyValue != null)
            {
                if (!propertyValue.GetType().IsAssignableFrom(magicPropertyType))
                {
                    isTypeMismatch = true;
                }
            }

            if (isTypeMismatch)
            {
                var propertyValueText =
                    propertyValue != null ? propertyValue.GetType().FullName : "null";

                throw new AttributeException(
                    "Annotation '" + attributeTypeName + "' requires a " +
                    magicPropertyType.Name + "-typed value for attribute '" +
                    propertyName + "' but received " +
                    "a " + propertyValueText + "-typed value");
            }

            return propertyValue;
        }

        /// <summary>
        /// Checks the property type for array semantics.
        /// </summary>
        /// <param name="attributeTypeName">Name of the attribute type.</param>
        /// <param name="propertyInfo">The property info.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <returns></returns>
        private static object CheckArray(string attributeTypeName, MagicPropertyInfo propertyInfo, string propertyName, object propertyValue)
        {
            if (propertyValue != null)
            {
                var actualElementType = propertyValue.GetType().GetElementType();
                var expectElementType = propertyInfo.PropertyType.GetElementType();

                // Did we actually receive an array as source?
                if (actualElementType == null)
                {
                    throw new AttributeException(
                        "Annotation '" + attributeTypeName + "' requires a " +
                        propertyInfo.PropertyType.GetCleanName() + "-typed value for attribute '" +
                        propertyName + "' but received a " +
                        propertyValue.GetType().GetCleanName() + "-typed value");
                }

                var array = (Array)propertyValue;
                var length = array.Length;
                for (var ii = 0; ii < length; ii++)
                {
                    if (array.GetValue(ii) == null)
                    {
                        throw new AttributeException(
                            "Annotation '" + attributeTypeName + "' requires a " +
                            "non-null value for array elements for attribute '" + propertyName + "'");
                    }
                }

                if (!Equals(actualElementType, expectElementType))
                {
                    var typeCaster = CastHelper.GetCastConverter(expectElementType);
                    var expectedArray = Array.CreateInstance(expectElementType, length);
                    for (var ii = 0; ii < length; ii++)
                    {
                        var oldValue = array.GetValue(ii);
                        var newValue = typeCaster.Invoke(oldValue);
                        if ((newValue == null) && (oldValue != null))
                        {
                            throw new AttributeException(
                                "Annotation '" + attributeTypeName + "' requires a " +
                                expectElementType.GetCleanName() + "-typed value for array elements for attribute '" +
                                propertyName + "' but received a " +
                                oldValue.GetType().GetCleanName() + "-typed value");
                        }

                        expectedArray.SetValue(newValue, ii);
                    }

                    propertyValue = expectedArray;
                }
            }

            return propertyValue;
        }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static object GetDefaultValue(Type t)
        {
            return t.IsValueType ? Activator.CreateInstance(t) : null;
        }

        public static Attribute FindAnnotation(IEnumerable<Attribute> attributes, Type attributeType)
        {
            if (!attributeType.IsSubclassOrImplementsInterface<Attribute>())
            {
                throw new ArgumentException("Type " + attributeType.FullName + " is not an attribute type");
            }
            if (attributes == null)
            {
                return null;
            }

            return attributes.FirstOrDefault(
                    attr => TypeHelper.IsSubclassOrImplementsInterface(attr.GetType(), (attributeType)));
        }

        public static Attribute FindAnnotation(Attribute[] annotations, Type annotationClass)
        {
            if (annotations == null || annotations.Length == 0)
            {
                return null;
            }

            return annotations.FirstOrDefault(anno =>
                anno.GetType() == annotationClass ||
                anno.GetType().IsSubclassOf(annotationClass));
        }

        public static List<Attribute> FindAnnotations(IEnumerable<Attribute> annotations, Type annotationClass)
        {
            if (!TypeHelper.IsSubclassOrImplementsInterface(annotationClass, typeof(Attribute)))
            {
                throw new ArgumentException("Class " + annotationClass.FullName + " is not an attribute class");
            }

            if (annotations == null || annotations.HasFirst() == false)
            {
                return null;
            }

            return annotations
                .Where(anno => TypeHelper.IsSubclassOrImplementsInterface(anno.GetType(), annotationClass))
                .ToList();
        }

        public static Attribute[] MergeAnnotations(Attribute[] first, Attribute[] second)
        {
            return first.Concat(second).ToArray();
        }

        public static String GetExpectSingleStringValue(String msgPrefix, IList<AnnotationDesc> annotationsSameName)
        {
            if (annotationsSameName.Count > 1)
            {
                throw new ExprValidationException(msgPrefix + " multiple annotations provided named '" + annotationsSameName[0].Name + "'");
            }
            var annotation = annotationsSameName[0];
            var value = AnnotationUtil.GetValue(annotation);
            if (value == null)
            {
                throw new ExprValidationException(msgPrefix + " no value provided for annotation '" + annotation.Name + "', expected a value");
            }
            if (!(value is String))
            {
                throw new ExprValidationException(msgPrefix + " string value expected for annotation '" + annotation.Name + "'");
            }
            return (String)value;
        }
    }
}
