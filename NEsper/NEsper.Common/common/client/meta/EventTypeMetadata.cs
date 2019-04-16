///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.client.meta
{
    /// <summary>
    ///     Provides metadata for an event type.
    /// </summary>
    public class EventTypeMetadata
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="name">event type name</param>
        /// <param name="moduleName">
        ///     module name that originated the event type or null if not provided or if the event type is
        ///     preconfigured
        /// </param>
        /// <param name="typeClass">information on the originator or use of the event type</param>
        /// <param name="applicationType">provides the type of the underlying</param>
        /// <param name="accessModifier">the access modifier defining how the event type is visible to other modules</param>
        /// <param name="busModifier">
        ///     the bus modifier defining how the event type is visible to applications calling send-event
        ///     methods
        /// </param>
        /// <param name="isPropertyAgnostic">
        ///     whether the type is property-agnostic (false for most typed, true for a type that
        ///     allows any property name)
        /// </param>
        /// <param name="eventTypeIdPair">the type id pair</param>
        public EventTypeMetadata(
            string name,
            string moduleName,
            EventTypeTypeClass typeClass,
            EventTypeApplicationType applicationType,
            NameAccessModifier accessModifier,
            EventTypeBusModifier busModifier,
            bool isPropertyAgnostic,
            EventTypeIdPair eventTypeIdPair)
        {
            Name = name;
            ModuleName = moduleName;
            TypeClass = typeClass;
            ApplicationType = applicationType;
            AccessModifier = accessModifier;
            BusModifier = busModifier;
            IsPropertyAgnostic = isPropertyAgnostic;
            EventTypeIdPair = eventTypeIdPair;
        }

        /// <summary>
        ///     Returns information on the originator or use of the event type
        /// </summary>
        /// <returns>type class</returns>
        public EventTypeTypeClass TypeClass { get; }

        /// <summary>
        ///     Returns the underlying type
        /// </summary>
        /// <returns>underling type</returns>
        public EventTypeApplicationType ApplicationType { get; }

        /// <summary>
        ///     Returns the access modifier
        /// </summary>
        /// <returns>access modifier</returns>
        public NameAccessModifier AccessModifier { get; }

        /// <summary>
        ///     Returns the event bus modifier.
        /// </summary>
        /// <returns>bus modifier</returns>
        public EventTypeBusModifier BusModifier { get; }

        /// <summary>
        ///     Returns event type ids
        /// </summary>
        /// <returns>event type ids</returns>
        public EventTypeIdPair EventTypeIdPair { get; }

        /// <summary>
        ///     Returns the event type name.
        /// </summary>
        /// <returns>event type name</returns>
        public string Name { get; }

        /// <summary>
        ///     Returns the module name or null when not provided.
        /// </summary>
        /// <returns>module name</returns>
        public string ModuleName { get; }

        /// <summary>
        ///     Returns indicator whether the type is property-agnostic, i.e. false for types that have a list of well-defined
        ///     property names and
        ///     true for a type that allows any property name
        /// </summary>
        /// <value>indicator</value>
        public bool IsPropertyAgnostic { get; }

        /// <summary>
        ///     Build an expression for the metadata (for internal use).
        /// </summary>
        /// <returns>exppression</returns>
        public CodegenExpression ToExpression()
        {
            return ToExpressionWPublicId(Constant(EventTypeIdPair.PublicId));
        }

        /// <summary>
        ///     Build an expression for the metadata (for internal use).
        /// </summary>
        /// <param name="expressionEventTypeIdPublic">id pair</param>
        /// <returns>exppression</returns>
        public CodegenExpression ToExpressionWPublicId(CodegenExpression expressionEventTypeIdPublic)
        {
            return NewInstance(
                typeof(EventTypeMetadata),
                Constant(Name), Constant(ModuleName),
                EnumValue(typeof(EventTypeTypeClass), TypeClass.GetName()),
                EnumValue(typeof(EventTypeApplicationType), ApplicationType.GetName()),
                EnumValue(typeof(NameAccessModifier), AccessModifier.GetName()),
                EnumValue(typeof(EventTypeBusModifier), BusModifier.GetName()),
                Constant(IsPropertyAgnostic),
                NewInstance(
                    typeof(EventTypeIdPair), expressionEventTypeIdPublic, Constant(EventTypeIdPair.ProtectedId)));
        }

        /// <summary>
        ///     Return metadata with the assigned ids
        /// </summary>
        /// <param name="eventTypeIdPublic">public id</param>
        /// <param name="eventTypeIdProtected">protected id</param>
        /// <returns>exppression</returns>
        public EventTypeMetadata WithIds(
            long eventTypeIdPublic,
            long eventTypeIdProtected)
        {
            return new EventTypeMetadata(
                Name, ModuleName, TypeClass, ApplicationType, AccessModifier, BusModifier, IsPropertyAgnostic,
                new EventTypeIdPair(eventTypeIdPublic, eventTypeIdProtected));
        }

        public override string ToString()
        {
            return "EventTypeMetadata{" +
                   "name='" + Name + '\'' +
                   ", typeClass=" + TypeClass +
                   ", applicationType=" + ApplicationType +
                   ", accessModifier=" + AccessModifier +
                   ", isPropertyAgnostic=" + IsPropertyAgnostic +
                   ", eventTypeIdPublic=" + EventTypeIdPair.PublicId +
                   ", eventTypeIdProtected=" + EventTypeIdPair.ProtectedId +
                   '}';
        }
    }
} // end of namespace