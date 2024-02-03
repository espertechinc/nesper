using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

#if NETCOREAPP3_0_OR_GREATER
#endif

using com.espertech.esper.container;

namespace com.espertech.esper.common.client.artifact
{
    public static class ArtifactRepositoryExtensions
    {
        public static IArtifactRepositoryManager ArtifactRepositoryManager(this IContainer container)
        {
            container.CheckContainer();

            lock (container) {
                if (container.DoesNotHave<IArtifactRepositoryManager>()) {
                    container.Register<IArtifactRepositoryManager>(
                        GetDefaultArtifactRepositoryManager,
                        Lifespan.Singleton);
                }
            }

            return container.Resolve<IArtifactRepositoryManager>();
        }

        public static IArtifactRepositoryManager GetDefaultArtifactRepositoryManager(IContainer container)
        {
            var baseTypeResolver = container.Has<TypeResolver>()
                ? container.Resolve<TypeResolver>()
                : TypeResolverDefault.INSTANCE;
            var assemblyResolver = container.Has<AssemblyResolver>()
                ? container.Resolve<AssemblyResolver>()
                : null;
            return new DefaultArtifactRepositoryManager(baseTypeResolver, assemblyResolver);
        }
    }
}