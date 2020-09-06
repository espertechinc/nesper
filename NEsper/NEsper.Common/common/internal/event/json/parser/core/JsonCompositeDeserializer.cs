using System;
using System.Linq;

using com.espertech.esper.common.@internal.@event.json.core;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
    /// <summary>
    /// Deserializes an entity that is composed of constituent parts.
    /// </summary>
    public interface JsonCompositeDeserializer : IJsonDeserializer
    {
        /// <summary>
        /// Returns a lookup that allows the caller to see all properties being deserialized
        /// by the composer.
        /// </summary>
        public ILookup<string, IJsonDeserializer> Properties { get; }
    }
}