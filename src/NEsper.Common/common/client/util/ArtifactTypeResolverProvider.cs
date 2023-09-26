using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.container;

namespace com.espertech.esper.common.client.util
{
    public class ArtifactTypeResolverProvider : TypeResolverProvider
    {
        private readonly IContainer _container;
        private ArtifactTypeResolver typeResolver;

        public ArtifactTypeResolverProvider(IContainer container)
        {
            _container = container;
        }

        public TypeResolver TypeResolver {
            get {
                lock (this) {
                    if (typeResolver == null) {
                        var parentClassLoader = TypeResolverDefault.INSTANCE;
                        var defaultArtifactRepository = _container.ArtifactRepositoryManager().DefaultRepository;
                        typeResolver = new ArtifactTypeResolver(defaultArtifactRepository, parentClassLoader);
                    }

                    return typeResolver;
                }
            }
        }
    }
}