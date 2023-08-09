using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using Microsoft.CodeAnalysis;

#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Loader;
#endif

namespace com.espertech.esper.common.client.artifact
{
    public abstract class BaseArtifactRepository : IArtifactRepository
    {
        /// <summary>
        /// Dictionary that maps ids to artifacts.
        /// </summary>
        private readonly IDictionary<string, IArtifact> _idToArtifact;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        protected BaseArtifactRepository()
        {
            _idToArtifact = new Dictionary<string, IArtifact>();
        }

        /// <summary>
        /// Returns an enumerable of all artifacts
        /// </summary>
        public IEnumerable<IArtifact> Artifacts {
            get {
                lock (_idToArtifact) {
                    return _idToArtifact.Values.ToList();
                }
            }
        }

        public IEnumerable<ICompileArtifact> CompileArtifacts => Artifacts.OfType<ICompileArtifact>();

        public IEnumerable<IRuntimeArtifact> RuntimeArtifacts => Artifacts.OfType<IRuntimeArtifact>();

        /// <summary>
        /// Performs cleanup on the repository.
        /// </summary>
        public virtual void Dispose()
        {
        }

        protected abstract Supplier<Assembly> MaterializeAssemblySupplier(byte[] image);
        
        
        /// <summary>
        /// Registers an image with the repository and returns a unique id for that artifact.
        /// </summary>
        /// <returns></returns>
        public ICompileArtifact Register(EPCompilationUnit compilationUnit)
        {
            var image = compilationUnit.Image;
            var id = compilationUnit.Name;
            var metadataReference = MetadataReference.CreateFromImage(image);
            var materializer = MaterializeAssemblySupplier(image);
            var artifact = new DefaultArtifact(id) {
                Image = image,
                MetadataReference = metadataReference,
                AssemblySupplier = materializer,
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
        public IRuntimeArtifact Resolve(string artifactId)
        {
            lock (_idToArtifact) {
                _idToArtifact.TryGetValue(artifactId, out var artifact);
                if (artifact is IRuntimeArtifact runtimeArtifact) {
                    return runtimeArtifact;
                } else if (artifact is ICompileArtifact compileArtifact) {
                    throw new NotSupportedException();
                }

                return null;
            }
        }
        
        /// <summary>
        /// Returns an enumeration of all metadata references.
        /// </summary>
        public IEnumerable<MetadataReference> AllMetadataReferences {
            get {
                lock (_idToArtifact) {
                    return _idToArtifact.Values.OfType<ICompileArtifact>().Select(_ => _.MetadataReference).ToList();
                }
            }
        }

        /// <summary>
        /// Takes an artifact that existed in one repository and deploys it in
        /// another.  Deployment materializes the assembly.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public ICompileArtifact Deploy(ICompileArtifact source)
        {
            // the artifact id, it does not change
            var artifactId = source.Id;
            // the source artifact image
            var artifactImage = source.Image;
            // materializer
            var materializer = MaterializeAssemblySupplier(artifactImage);
            // create the runtime artifact
            var target = new DefaultArtifact(artifactId) {
                Image = artifactImage,
                // no metadata references for runtime artifacts
                MetadataReference = null,
                AssemblySupplier = materializer,
                TypeNames = source.TypeNames
            };

            // register with this repository
            lock (_idToArtifact) {
                _idToArtifact[artifactId] = target;
            }

            return target;
        }

        /// <summary>
        /// Creates a classLoader.
        /// </summary>
        /// <value></value>
        public virtual TypeResolver TypeResolver => null;
    }
}