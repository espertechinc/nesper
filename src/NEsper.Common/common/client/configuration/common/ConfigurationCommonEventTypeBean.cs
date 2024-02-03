///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Configuration information for legacy event types.
    /// </summary>
    public class ConfigurationCommonEventTypeBean
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public ConfigurationCommonEventTypeBean()
        {
            AccessorStyle = AccessorStyle.NATIVE;
            MethodProperties = new List<LegacyMethodPropDesc>();
            FieldProperties = new List<LegacyFieldPropDesc>();
            PropertyResolutionStyle = PropertyResolutionStyle.CASE_SENSITIVE;
        }

        /// <summary>
        ///     Returns the accessor style.
        /// </summary>
        /// <returns>accessor style</returns>
        public AccessorStyle AccessorStyle { get; set; }

        /// <summary>
        ///     Returns the type's property resolution style to use.
        /// </summary>
        /// <returns>property resolution style</returns>
        public PropertyResolutionStyle PropertyResolutionStyle { get; set; }

        /// <summary>
        ///     Returns the name of the factory method, either fully-qualified or just a method name if the
        ///     method is on the same class as the configured class, to use when instantiating
        ///     objects of the type.
        /// </summary>
        /// <returns>factory methods</returns>
        public string FactoryMethod { get; set; }

        /// <summary>
        ///     Returns the method name of the method to use to copy the underlying event object.
        /// </summary>
        /// <returns>method name</returns>
        public string CopyMethod { get; set; }

        /// <summary>
        ///     Returns the property name of the property providing the start timestamp value.
        /// </summary>
        /// <returns>start timestamp property name</returns>
        public string StartTimestampPropertyName { get; set; }

        /// <summary>
        ///     Returns the property name of the property providing the end timestamp value.
        /// </summary>
        /// <returns>end timestamp property name</returns>
        public string EndTimestampPropertyName { get; set; }

        /// <summary>
        ///     Returns a list of descriptors specifying explicitly configured method names
        ///     and their property name.
        /// </summary>
        /// <value>list of explicit method-access descriptors</value>
        public IList<LegacyMethodPropDesc> MethodProperties { get; }

        /// <summary>
        ///     Returns a list of descriptors specifying explicitly configured field names
        ///     and their property name.
        /// </summary>
        /// <value>list of explicit field-access descriptors</value>
        public IList<LegacyFieldPropDesc> FieldProperties { get; }

        /// <summary>
        ///     Adds the named event property backed by the named accessor method.
        ///     <para />
        ///     The accessor method is expected to be a public method with no parameters
        ///     for simple event properties, or with a single integer parameter for indexed
        ///     event properties, or with a single String parameter for mapped event properties.
        /// </summary>
        /// <param name="name">is the event property name</param>
        /// <param name="accessorMethod">is the accessor method name.</param>
        public void AddMethodProperty(
            string name,
            string accessorMethod)
        {
            MethodProperties.Add(new LegacyMethodPropDesc(name, accessorMethod));
        }

        /// <summary>
        ///     Adds the named event property backed by the named accessor field.
        /// </summary>
        /// <param name="name">is the event property name</param>
        /// <param name="accessorField">is the accessor field underlying the name</param>
        public void AddFieldProperty(
            string name,
            string accessorField)
        {
            FieldProperties.Add(new LegacyFieldPropDesc(name, accessorField));
        }

        /// <summary>
        ///     Encapsulates information about an accessor field backing a named event property.
        /// </summary>
        public class LegacyFieldPropDesc
        {
            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="name">is the event property name</param>
            /// <param name="accessorFieldName">is the accessor field name</param>
            public LegacyFieldPropDesc(
                string name,
                string accessorFieldName)
            {
                Name = name;
                AccessorFieldName = accessorFieldName;
            }

            /// <summary>
            ///     Returns the event property name.
            /// </summary>
            /// <returns>event property name</returns>
            public string Name { get; }

            /// <summary>
            ///     Returns the accessor field name.
            /// </summary>
            /// <returns>accessor field name</returns>
            public string AccessorFieldName { get; }
        }

        /// <summary>
        ///     Encapsulates information about an accessor method backing a named event property.
        /// </summary>
        public class LegacyMethodPropDesc
        {
            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="name">is the event property name</param>
            /// <param name="accessorMethodName">is the name of the accessor method</param>
            public LegacyMethodPropDesc(
                string name,
                string accessorMethodName)
            {
                Name = name;
                AccessorMethodName = accessorMethodName;
            }

            /// <summary>
            ///     Returns the event property name.
            /// </summary>
            /// <returns>event property name</returns>
            public string Name { get; }

            /// <summary>
            ///     Returns the accessor method name.
            /// </summary>
            /// <returns>accessor method name</returns>
            public string AccessorMethodName { get; }
        }
    }
} // end of namespace