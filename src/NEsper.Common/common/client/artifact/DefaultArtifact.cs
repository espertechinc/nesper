using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.compat.function;

using Microsoft.CodeAnalysis;

namespace com.espertech.esper.common.client.artifact
{
    /// <summary>
    /// The artifact produced by a compilation.
    /// </summary>
    public class DefaultArtifact : Artifact
    {
        private ISet<String> _typeNames;
        private Assembly _assembly;
        
        public DefaultArtifact(string id)
        {
            Id = id;
            _typeNames = new HashSet<string>();
        }
        
        /// <summary>
        /// A unique identifier for the artifact (within a given repository).
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// The byte array of the image for the assembly
        /// </summary>
        public byte[] Image { get; set; }

        /// <summary>
        /// Returns true if the artifact has materialized the assembly.
        /// </summary>
        public bool HasMaterializedAssembly {
            get {
                lock (this) {
                    return _assembly != null;
                }
            }
        }

        /// <summary>
        /// The assembly for this image.
        /// </summary>

        public Assembly Assembly {
            get {
                lock (this) {
                    if (_assembly == null) {
                        _assembly = AssemblySupplier.Invoke();
                    }
                }

                return _assembly;
            }
        }

        public Supplier<Assembly> AssemblySupplier { get; set; } 

        /// <summary>
        /// Internal use only
        /// </summary>
        public MetadataReference MetadataReference { get; set; }

        /// <summary>
        /// The set of unique type names exported by compilation.
        /// </summary>
        public ICollection<Type> ExportedTypes => Assembly.GetExportedTypes();

        public IEnumerable<String> TypeNames {
            get => _typeNames;
            set => _typeNames = new HashSet<string>(value);
        }
        
        /// <summary>
        /// Returns true if the artifact contains the given type name.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public bool Contains(string typeName)
        {
            return _typeNames.Contains(typeName);
        }

        protected bool Equals(DefaultArtifact other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return Equals((DefaultArtifact)obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }
}