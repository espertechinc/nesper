using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.CodeAnalysis;

namespace com.espertech.esper.common.client.artifact
{
    /// <summary>
    /// The artifact produced by a compilation.
    /// </summary>
    public interface Artifact
    {
        /// <summary>
        /// A unique identifier for the artifact (within a given repository).
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The byte array of the image for the assembly
        /// </summary>
        public byte[] Image { get; }

        /// <summary>
        /// Returns true if the artifact has materialized the assembly.
        /// </summary>
        public bool HasMaterializedAssembly { get; }
        
        /// <summary>
        /// The assembly for this image.
        /// </summary>
        public Assembly Assembly { get; }

        /// <summary>
        /// Internal use only
        /// </summary>
        public MetadataReference MetadataReference { get; }
        
        /// <summary>
        /// The set of unique types exported by compilation.
        /// </summary>
        public IEnumerable<string> TypeNames { get; }

        public bool Contains(string typeName);
    }
}