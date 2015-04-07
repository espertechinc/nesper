///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Configuration information for legacy event types.
    /// </summary>

    [Serializable]
    public class ConfigurationEventTypeLegacy
    {
        /// <summary>
        /// Ctor.
        /// </summary>

        public ConfigurationEventTypeLegacy()
        {
            AccessorStyle = AccessorStyleEnum.NATIVE;
            CodeGeneration = CodeGenerationEnum.ENABLED;
            PropertyResolutionStyle = PropertyResolutionStyleHelper.DefaultPropertyResolutionStyle;

            FieldProperties = new List<LegacyFieldPropDesc>();
            MethodProperties = new List<LegacyMethodPropDesc>();
        }

        /// <summary>
        /// Gets or sets the accessor style.
        /// </summary>
        /// <value>The accessor style.</value>
        public AccessorStyleEnum AccessorStyle { get; set; }

        /// <summary>
        /// Gets or sets the code generation.  Thus controls whether or
        /// not the engine generates code for access to event property values.
        /// </summary>
        /// <value>The code generation.</value>
        public CodeGenerationEnum CodeGeneration { get; set; }

        /// <summary>
        /// Gets or sets the property resolution style.
        /// </summary>
        /// <value>The property resolution style.</value>
        public PropertyResolutionStyle PropertyResolutionStyle { get; set; }

        /// <summary>
        /// Returns a list of descriptors specifying explicitly configured method names
        /// and their property name.
        /// </summary>
        /// <returns> list of explicit method-access descriptors
        /// </returns>
        public IList<LegacyMethodPropDesc> MethodProperties { get; private set; }

        /// <summary> Returns a list of descriptors specifying explicitly configured field names
        /// and their property name.
        /// </summary>
        /// <returns> list of explicit field-access descriptors
        /// </returns>
        public IList<LegacyFieldPropDesc> FieldProperties { get; private set; }

        /// <summary>
        /// Gets or sets the start name of the timestamp property.
        /// </summary>
        /// <value>The start name of the timestamp property.</value>
        public String StartTimestampPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the end name of the timestamp property.
        /// </summary>
        /// <value>The end name of the timestamp property.</value>
        public String EndTimestampPropertyName { get; set; }

        /// <summary>
        /// Adds the named event property backed by the named accessor method.
        /// The accessor method is expected to be a public method with no parameters
        /// for simple event properties, or with a single integer parameter for indexed
        /// event properties, or with a single String parameter for mapped event properties.
        /// </summary>
        /// <param name="name">is the event property name</param>
        /// <param name="accessorMethod">is the accessor method name.</param>

        public virtual void AddMethodProperty(String name, String accessorMethod)
        {
            MethodProperties.Add(new LegacyMethodPropDesc(name, accessorMethod));
        }

        /// <summary>
        /// Adds the named event property backed by the named accessor field.
        /// </summary>
        /// <param name="name">is the event property name</param>
        /// <param name="accessorField">is the accessor field underlying the name</param>

        public virtual void AddFieldProperty(String name, String accessorField)
        {
            FieldProperties.Add(new LegacyFieldPropDesc(name, accessorField));
        }

        /// <summary>
        /// Gets or sets the the name of the factory method, either fully-qualified or just
        /// a method name if the method is on the same class as the configured class, to use
        /// when instantiating objects of the type.
        /// </summary>
        public string FactoryMethod { get; set; }

        /// <summary>
        /// Gets or sets the method name of the method to use to copy the underlying event object.
        /// </summary>
        public string CopyMethod { get; set; }

        /// <summary>
        /// Encapsulates information about an accessor field backing a named event property.
        /// </summary>

        [Serializable]
        public class LegacyFieldPropDesc
        {
            /// <summary> Returns the event property name.</summary>
            /// <returns> event property name
            /// </returns>
            public string Name { get; private set; }

            /// <summary> Returns the accessor field name.</summary>
            /// <returns> accessor field name
            /// </returns>
            public string AccessorFieldName { get; private set; }

            /// <summary> Ctor.</summary>
            /// <param name="name">is the event property name
            /// </param>
            /// <param name="accessorFieldName">is the accessor field name
            /// </param>
            public LegacyFieldPropDesc(String name, String accessorFieldName)
            {
                Name = name;
                AccessorFieldName = accessorFieldName;
            }
        }

        /// <summary>
        /// Encapsulates information about an accessor method backing a named event property.
        /// </summary>

        [Serializable]
        public class LegacyMethodPropDesc
        {
            /// <summary> Returns the event property name.</summary>
            /// <returns> event property name
            /// </returns>
            public string Name { get; private set; }

            /// <summary> Returns the accessor method name.</summary>
            /// <returns> accessor method name
            /// </returns>
            public string AccessorMethodName { get; private set; }

            /// <summary> Ctor.</summary>
            /// <param name="name">is the event method name
            /// </param>
            /// <param name="accessorMethodName">is the accessor method name
            /// </param>
            public LegacyMethodPropDesc(String name, String accessorMethodName)
            {
                Name = name;
                AccessorMethodName = accessorMethodName;
            }
        }
    }


    /// <summary>
    /// Accessor style defines the methods of a class that are automatically exposed via event property.
    /// </summary>

    public enum AccessorStyleEnum
    {
        /// <summary> Expose properties only, plus explicitly configured properties.</summary>
        NATIVE,
        /// <summary> Expose only the explicitly configured properties and public members as event properties.</summary>
        EXPLICIT,
        /// <summary> Expose all public properties and public members as event properties, plus explicitly configured properties.</summary>
        PUBLIC
    }

    /// <summary> Enum to control code generation.</summary>
    public enum CodeGenerationEnum
    {
        /// <summary> Enables code generation.</summary>
        ENABLED,
        /// <summary> Dispables code generation.</summary>
        DISABLED
    }
}
