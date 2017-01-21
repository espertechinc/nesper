///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Service supplying stream number and property type information.
    /// </summary>
    public interface StreamTypeService
    {
        /// <summary>
        /// Returns the offset of the stream and the type of the property for the given property name, by looking through the types offered and matching up.
        /// <para>
        /// This method considers only a property name and looks at all streams to resolve the property name.
        /// </para>
        /// </summary>
        /// <param name="propertyName">property name in event</param>
        /// <param name="obtainFragment">if set to <c>true</c> [obtain fragment].</param>
        /// <returns>
        /// descriptor with stream number, property type and property name
        /// </returns>
        /// <throws>DuplicatePropertyException to indicate property was found twice</throws>
        /// <throws>PropertyNotFoundException to indicate property could not be resolved</throws>
        PropertyResolutionDescriptor ResolveByPropertyName(String propertyName, bool obtainFragment);

        /// <summary>
        /// Returns the offset of the stream and the type of the property for the given property name, 
        /// by looking through the types offered considering only explicitly listed properties and matching up.
        /// <para>
        /// This method considers only a property name and looks at all streams to resolve the property name.
        /// </para>
        /// </summary>
        /// <param name="propertyName">property name in event</param>
        /// <param name="obtainFragment">if set to <c>true</c> [obtain fragment].</param>
        /// <returns>
        /// descriptor with stream number, property type and property name
        /// </returns>
        /// <throws>DuplicatePropertyException to indicate property was found twice</throws>
        /// <throws>PropertyNotFoundException to indicate property could not be resolved</throws>
        PropertyResolutionDescriptor ResolveByPropertyNameExplicitProps(String propertyName, bool obtainFragment);

        /// <summary>
        /// Returns the offset of the stream and the type of the property for the given property name, 
        /// by using the specified stream name to resolve the property. 
        /// <para>
        /// This method considers and explicit stream name and property name, both parameters are required.
        /// </para>
        /// </summary>
        /// <param name="streamName">name of stream, required</param>
        /// <param name="propertyName">property name in event, , required</param>
        /// <param name="obtainFragment">if set to <c>true</c> [obtain fragment].</param>
        /// <returns>
        /// descriptor with stream number, property type and property name
        /// </returns>
        /// <throws>PropertyNotFoundException to indicate property could not be resolved</throws>
        /// <throws>StreamNotFoundException to indicate stream name could not be resolved</throws>
        PropertyResolutionDescriptor ResolveByStreamAndPropName(String streamName, String propertyName, bool obtainFragment);

        /// <summary>
        /// Returns the offset of the stream and the type of the property for the given property name, by 
        /// using the specified stream name to resolve the property and considering only explicitly listed 
        /// properties. 
        /// <para>
        /// This method considers and explicit stream name and property name, both parameters are required.
        /// </para>
        /// </summary>
        /// <param name="streamName">name of stream, required</param>
        /// <param name="propertyName">property name in event, , required</param>
        /// <param name="obtainFragment">if set to <c>true</c> [obtain fragment].</param>
        /// <returns>
        /// descriptor with stream number, property type and property name
        /// </returns>
        /// <throws>PropertyNotFoundException to indicate property could not be resolved</throws>
        /// <throws>StreamNotFoundException to indicate stream name could not be resolved</throws>
        PropertyResolutionDescriptor ResolveByStreamAndPropNameExplicitProps(String streamName, String propertyName, bool obtainFragment);

        /// <summary>
        /// Returns the offset of the stream and the type of the property for the given property name, by looking through the types offered and matching up.
        /// <para>
        /// This method considers a single property name that may or may not be prefixed by a stream name. The resolution first attempts to find the property 
        /// name itself, then attempts to consider a stream name that may be part of the property name.  
        /// </para>
        /// </summary>
        /// <param name="streamAndPropertyName">stream name and property name (e.g. s0.p0) or just a property name (p0)</param>
        /// <param name="obtainFragment"></param>
        /// <returns>descriptor with stream number, property type and property name</returns>
        /// <throws>DuplicatePropertyException to indicate property was found twice</throws>
        /// <throws>PropertyNotFoundException to indicate property could not be resolved</throws>
        PropertyResolutionDescriptor ResolveByStreamAndPropName(String streamAndPropertyName, bool obtainFragment);

        /// <summary>
        /// Returns an array of event stream names in the order declared.
        /// </summary>
        /// <value>stream names</value>
        string[] StreamNames { get; }

        /// <summary>
        /// Returns an array of event types for each event stream in the order declared.
        /// </summary>
        /// <value>event types</value>
        EventType[] EventTypes { get; }

        /// <summary>
        /// Returns true for each stream without a data window.
        /// </summary>
        /// <value>true for non-windowed streams.</value>
        bool[] IsIStreamOnly { get; }

        int GetStreamNumForStreamName(String streamWildcard);

        bool IsOnDemandStreams { get; }

        string EngineURIQualifier { get; }

        bool HasPropertyAgnosticType { get; }

        bool HasTableTypes { get; }

        bool IsStreamZeroUnambigous { get; }
    }
}
