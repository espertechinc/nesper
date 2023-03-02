using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using Microsoft.CodeAnalysis;

#if NETCORE
using System.Runtime.Loader;
#endif

namespace com.espertech.esper.common.client.artifact
{
    public abstract class BaseArtifactRepository : IArtifactRepository
    {
        /// <summary>
        /// Dictionary that maps ids to artifacts.
        /// </summary>
        private readonly IDictionary<string, Artifact> _idToArtifact;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        protected BaseArtifactRepository()
        {
            _idToArtifact = new Dictionary<string, Artifact>();
        }

        /// <summary>
        /// Returns an enumerable of all artifacts
        /// </summary>
        public IEnumerable<Artifact> Artifacts {
            get {
                lock (_idToArtifact) {
                    return _idToArtifact.Values.ToList();
                }
            }
        }

        /// <summary>
        /// Performs cleanup on the repository.
        /// </summary>
        public virtual void Dispose()
        {
        }

        protected abstract Assembly MaterializeAssembly(byte[] image);

        /// <summary>
        /// Registers an image with the repository and returns a unique id for that artifact.
        /// </summary>
        /// <returns></returns>
        public Artifact Register(EPCompilationUnit compilationUnit)
        {
            var image = compilationUnit.Image;
            var id = compilationUnit.Name;
            var metadataReference = MetadataReference.CreateFromImage(image);
            var artifact = new DefaultArtifact(id) {
                Image = image,
                MetadataReference = metadataReference,
                AssemblySupplier = () => MaterializeAssembly(image),
                TypeNames = compilationUnit.TypeNames
            };

            lock (_idToArtifact) {
                _idToArtifact[id] = artifact;
            }

            return artifact;
        }

        /// <summary>
        /// Resolves an artifact from the repository.
        /// </summary>
        /// <param name="artifactId"></param>
        /// <returns></returns>
        public Artifact Resolve(string artifactId)
        {
            lock (_idToArtifact) {
                _idToArtifact.TryGetValue(artifactId, out var artifact);
                return artifact;
            }
        }
        
        /// <summary>
        /// Returns an enumeration of all metadata references.
        /// </summary>
        public IEnumerable<MetadataReference> AllMetadataReferences {
            get {
                lock (_idToArtifact) {
                    return _idToArtifact.Values.Select(_ => _.MetadataReference).ToList();
                }
            }
        }

        /// <summary>
        /// Creates a classLoader.
        /// </summary>
        /// <value></value>
        public virtual TypeResolver TypeResolver => null;
    }
}