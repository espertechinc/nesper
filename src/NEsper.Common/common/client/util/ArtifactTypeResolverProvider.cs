using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.util
{
    public class ArtifactTypeResolverProvider : TypeResolverProvider
    {
        private readonly IArtifactRepositoryManager _artifactRepositoryManager;
        private ArtifactTypeResolver typeResolver;

        public ArtifactTypeResolverProvider(IArtifactRepositoryManager artifactRepositoryManager)
        {
            _artifactRepositoryManager = artifactRepositoryManager;
        }

        public TypeResolver TypeResolver {
            get {
                lock (this) {
                    if (typeResolver == null) {
                        var parentClassLoader = TypeResolverDefault.INSTANCE;
                        var defaultArtifactRepository = _artifactRepositoryManager.DefaultRepository;
                        typeResolver = new ArtifactTypeResolver(defaultArtifactRepository, parentClassLoader);
                    }

                    return typeResolver;
                }
            }
        }
    }
}