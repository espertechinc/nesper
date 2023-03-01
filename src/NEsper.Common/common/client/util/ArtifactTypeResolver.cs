using System;
using System.Linq;

using com.espertech.esper.common.client.artifact;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.util
{
    public class ArtifactTypeResolver : TypeResolver
    {
        private readonly TypeResolver _parent;

        /// <summary>
        /// Artifacts that are relevant (and exposed) for this scope.
        /// </summary>
        private readonly IArtifactRepository _artifactRepository;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="artifactRepository">Repository of artifacts that are relevant (and exposed) for this classLoader scope</param>
        /// <param name="parent">The parent classLoader scope</param>
        public ArtifactTypeResolver(
            IArtifactRepository artifactRepository,
            TypeResolver parent)
        {
            _parent = parent;
            _artifactRepository = artifactRepository;
        }

        public virtual Type ResolveType(
            string typeName,
            bool resolve)
        {
            // First pass is to see if the class exists in the materialized space
            var materializedType = _artifactRepository.Artifacts
                .Where(_ => _.HasMaterializedAssembly)
                .Where(_ => _.Contains(typeName))
                .Select(_ => _.Assembly.GetType(typeName, false, false))
                .FirstOrDefault();
            if (materializedType != null) {
                return materializedType;
            }
            
            // However, if we do not find a materialized version, then check with
            // the parent to see if there is an existing materialized type
            var parentType = _parent.ResolveType(typeName, false);
            if (parentType != null) {
                return parentType;
            }

            // Attempt the same pass through the artifacts, but this time materialize any
            // assembly that contains the type
            return _artifactRepository.Artifacts
                .Where(_ => _.Contains(typeName))
                .Select(_ => _.Assembly.GetType(typeName, false, false))
                .FirstOrDefault();
        }
    }
}