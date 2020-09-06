﻿///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.Json;

using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
    /// <summary>
    /// Deserializes an entity.
    /// </summary>
    public interface IJsonDeserializer
    {
        /// <summary>
        /// Allocates a vanilla json composite object.
        /// </summary>
        public Func<object> Allocator { get; }

        /// <summary>
        /// Deserializes the entity.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public object Deserialize(JsonElement element);
    }

#if false
    /// <summary>
    /// Called to deserialize a JsonElement.
    /// </summary>
    /// <param name="element"></param>
    public delegate T JsonDeserializer<out T>(JsonElement element);
#endif

    public static class JsonDeserializerExtensions
    {
        public static object DeserializeProperty(
            IJsonDeserializer deserializer,
            JsonElement element,
            string propertyName)
        {
            return element.TryGetProperty(propertyName, out var propertyValue)
                ? deserializer.Deserialize(propertyValue)
                : null;
        }
        
        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="target">the dictionary into which the result will be placed</param>
        /// <param name="name">the name (key) to use</param>
        /// <param name="element">the value in its "json" element form</param> 
        public static void AddGeneralJson(
            IDictionary<string, object> target,
            string name,
            JsonElement element)
        {
            target[name] = element.ElementToValue();
        }
    }
}