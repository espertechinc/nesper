using System;

using com.espertech.esper.compat.collections;

#if NETCORE
using System.Runtime.Loader;
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
                    container.Register<IArtifactRepositoryManager>(GetDefaultArtifactRepositoryManager(), Lifespan.Singleton);
                }
            }

            return container.Resolve<IArtifactRepositoryManager>();
        }

        public static IArtifactRepositoryManager GetDefaultArtifactRepositoryManager()
        {
            return new DefaultArtifactRepositoryManager();
        }

#if NETCORE
#endif
    }
}