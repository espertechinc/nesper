///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.events
{
    /// <summary>
    ///     Provides metadata for event types.
    /// </summary>
    public class EventTypeMetadata
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="primaryName">the primary name by which the type became known.</param>
        /// <param name="secondaryNames">a list of additional names for the type, such as fully-qualified class name</param>
        /// <param name="typeClass">type of the type</param>
        /// <param name="applicationConfigured">true if configured by the application</param>
        /// <param name="applicationType">type of application class or null if not an application type</param>
        /// <param name="isPropertyAgnostic">true for types that accept any property name as a valid property (unchecked type)</param>
        /// <param name="applicationPreConfigured">preconfigured</param>
        /// <param name="isApplicationPreConfiguredStatic">preconfigured via static config</param>
        protected EventTypeMetadata(
            string primaryName,
            ICollection<string> secondaryNames,
            TypeClass typeClass,
            bool isApplicationPreConfiguredStatic,
            bool applicationPreConfigured,
            bool applicationConfigured,
            ApplicationType? applicationType,
            bool isPropertyAgnostic)
        {
            PublicName = primaryName;
            PrimaryName = primaryName;
            OptionalSecondaryNames = secondaryNames;
            TypeClass = typeClass;
            IsApplicationConfigured = applicationConfigured;
            IsApplicationPreConfigured = applicationPreConfigured;
            IsApplicationPreConfiguredStatic = isApplicationPreConfiguredStatic;
            OptionalApplicationType = applicationType;
            IsPropertyAgnostic = isPropertyAgnostic;
        }

        /// <summary>
        ///     Factory for a value-add type.
        /// </summary>
        /// <param name="name">type name</param>
        /// <param name="typeClass">type of type</param>
        /// <returns>instance</returns>
        public static EventTypeMetadata CreateValueAdd(string name, TypeClass typeClass)
        {
            if ((typeClass != TypeClass.VARIANT) && (typeClass != TypeClass.REVISION))
            {
                throw new ArgumentException("Type class " + typeClass + " invalid");
            }
            return new EventTypeMetadata(name, null, typeClass, true, true, true, null, false);
        }

        /// <summary>
        ///     Factory for a bean type.
        /// </summary>
        /// <param name="name">type name</param>
        /// <param name="clazz">java class</param>
        /// <param name="isConfigured">whether the class was made known or is discovered</param>
        /// <param name="typeClass">type of type</param>
        /// <param name="isPreConfigured">preconfigured</param>
        /// <param name="isPreConfiguredStatic">preconfigured via static config</param>
        /// <returns>instance</returns>
        public static EventTypeMetadata CreateBeanType(
            string name,
            Type clazz,
            bool isPreConfiguredStatic,
            bool isPreConfigured,
            bool isConfigured,
            TypeClass typeClass)
        {
            ISet<string> secondaryNames = null;
            if (name == null)
            {
                name = clazz.Name;
            }
            else
            {
                if (!name.Equals(clazz.Name))
                {
                    secondaryNames = new LinkedHashSet<string>();
                    secondaryNames.Add(clazz.Name);
                }
            }

            return new EventTypeMetadata(
                name, secondaryNames, typeClass, isPreConfiguredStatic, isPreConfigured, isConfigured,
                ApplicationType.CLASS, false);
        }

        /// <summary>
        ///     Factory for a XML type.
        /// </summary>
        /// <param name="name">type name</param>
        /// <param name="isPropertyAgnostic">true for types that accept any property name as a valid property (unchecked type)</param>
        /// <param name="isPreconfiguredStatic">preconfigured via static config</param>
        /// <returns>instance</returns>
        public static EventTypeMetadata CreateXMLType(string name, bool isPreconfiguredStatic, bool isPropertyAgnostic)
        {
            return new EventTypeMetadata(
                name, null, TypeClass.APPLICATION, isPreconfiguredStatic, true, true, ApplicationType.XML,
                isPropertyAgnostic);
        }

        /// <summary>
        ///     Factory for an anonymous type.
        /// </summary>
        /// <param name="associationName">what the type is associated with</param>
        /// <param name="applicationType">type INFO</param>
        /// <returns>instance</returns>
        public static EventTypeMetadata CreateAnonymous(string associationName, ApplicationType applicationType)
        {
            return new EventTypeMetadata(
                associationName, null, TypeClass.ANONYMOUS, false, false, false, applicationType, false);
        }

        /// <summary>
        ///     Factory for an table type.
        /// </summary>
        /// <param name="tableName">what the type is associated with</param>
        /// <returns>instance</returns>
        public static EventTypeMetadata CreateTable(string tableName)
        {
            return new EventTypeMetadata(tableName, null, TypeClass.TABLE, false, false, false, null, false);
        }

        /// <summary>
        ///     Factory for a wrapper type.
        /// </summary>
        /// <param name="eventTypeName">insert-into of create-window name</param>
        /// <param name="namedWindow">true for named window</param>
        /// <param name="insertInto">true for insert-into</param>
        /// <param name="isPropertyAgnostic">true for types that accept any property name as a valid property (unchecked type)</param>
        /// <returns>instance</returns>
        public static EventTypeMetadata CreateWrapper(
            string eventTypeName,
            bool namedWindow,
            bool insertInto,
            bool isPropertyAgnostic)
        {
            TypeClass typeClass;
            if (namedWindow)
            {
                typeClass = TypeClass.NAMED_WINDOW;
            }
            else if (insertInto)
            {
                typeClass = TypeClass.STREAM;
            }
            else
            {
                throw new IllegalStateException("Unknown Wrapper type, cannot create metadata");
            }
            return new EventTypeMetadata(eventTypeName, null, typeClass, false, false, false, null, isPropertyAgnostic);
        }

        /// <summary>
        ///     Factory for a map type.
        /// </summary>
        /// <param name="name">insert-into of create-window name</param>
        /// <param name="namedWindow">true for named window</param>
        /// <param name="insertInto">true for insert-into</param>
        /// <param name="configured">whether the made known or is discovered</param>
        /// <param name="preconfigured">preconfigured</param>
        /// <param name="preconfiguredStatic">preconfigured via static config</param>
        /// <param name="providedType">type</param>
        /// <returns>instance</returns>
        public static EventTypeMetadata CreateNonPonoApplicationType(
            ApplicationType providedType,
            string name,
            bool preconfiguredStatic,
            bool preconfigured,
            bool configured,
            bool namedWindow,
            bool insertInto)
        {
            TypeClass typeClass;
            ApplicationType? applicationType = null;
            if (configured)
            {
                typeClass = TypeClass.APPLICATION;
                applicationType = providedType;
            }
            else if (namedWindow)
            {
                typeClass = TypeClass.NAMED_WINDOW;
            }
            else if (insertInto)
            {
                typeClass = TypeClass.STREAM;
            }
            else
            {
                typeClass = TypeClass.ANONYMOUS;
            }
            return new EventTypeMetadata(
                name, null, typeClass, preconfiguredStatic, preconfigured, configured, applicationType, false);
        }

        /// <summary>
        ///     Returns the name.
        /// </summary>
        /// <value>name</value>
        public string PrimaryName { get; private set; }

        /// <summary>
        ///     Returns second names or null if none found.
        /// </summary>
        /// <value>further names</value>
        public ICollection<string> OptionalSecondaryNames { get; private set; }

        /// <summary>
        ///     Returns the type of the type.
        /// </summary>
        /// <value>meta type</value>
        public TypeClass TypeClass { get; private set; }

        /// <summary>
        ///     Returns true if the type originates in a configuration.
        /// </summary>
        /// <value>indicator whether configured or not</value>
        public bool IsApplicationConfigured { get; private set; }

        /// <summary>
        ///     The type of the application event type or null if not an application event type.
        /// </summary>
        /// <value>application event type</value>
        public ApplicationType? OptionalApplicationType { get; private set; }

        /// <summary>
        ///     Returns the name provided through #EventType.getName.
        /// </summary>
        /// <value>name or null if no public name</value>
        public string PublicName { get; private set; }

        /// <summary>
        ///     Returns true for types that accept any property name as a valid property (unchecked type).
        /// </summary>
        /// <value>indicator whether type is unchecked (agnostic to property)</value>
        public bool IsPropertyAgnostic { get; private set; }

        /// <summary>
        ///     Returns true to indicate the type is pre-configured, i.e. added through static or runtime configuration.
        /// </summary>
        /// <value>indicator</value>
        public bool IsApplicationPreConfigured { get; private set; }

        /// <summary>
        ///     Returns true to indicate the type is pre-configured, i.e. added through static configuration but not runtime
        ///     configuation.
        /// </summary>
        /// <value>indicator</value>
        public bool IsApplicationPreConfiguredStatic { get; private set; }
    }

    /// <summary>Metatype.</summary>
    public enum TypeClass
    {
        /// <summary>
        ///     A type that represents the information made available via insert-into.
        /// </summary>
        STREAM,

        /// <summary>A revision event type.</summary>
        REVISION,

        /// <summary>A variant stream event type.</summary>
        VARIANT,

        /// <summary>
        ///     An application-defined event type such as legacy object, XML or Map.
        /// </summary>
        APPLICATION,

        /// <summary>A type representing a named window.</summary>
        NAMED_WINDOW,

        /// <summary>A type representing a table.</summary>
        TABLE,

        /// <summary>An anonymous event type.</summary>
        ANONYMOUS
    }

    public static class TypeClassExtensions
    {
        public static bool IsPublic(this TypeClass typeClass)
        {
            switch (typeClass)
            {
                case TypeClass.STREAM:
                    return (true);
                case TypeClass.REVISION:
                    return (true);
                case TypeClass.VARIANT:
                    return (true);
                case TypeClass.APPLICATION:
                    return (true);
                case TypeClass.NAMED_WINDOW:
                    return (true);
                case TypeClass.ANONYMOUS:
                    return (false);
                case TypeClass.TABLE:
                    return (false);
            }

            throw new ArgumentException();
        }
    }


    /// <summary>Application type.</summary>
    public enum ApplicationType
    {
        /// <summary>Xml type.</summary>
        XML,

        /// <summary>Map type.</summary>
        MAP,

        /// <summary>Object Array type.</summary>
        OBJECTARR,

        /// <summary>Type type.</summary>
        CLASS,

        /// <summary>Avro type.</summary>
        AVRO,

        /// <summary>Wrapper type.</summary>
        WRAPPER
    }
} // end of namespace