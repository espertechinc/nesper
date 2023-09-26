using System;

#if NETCOREAPP3_0_OR_GREATER
#endif

using com.espertech.esper.container;

using Microsoft.CodeAnalysis;

namespace com.espertech.esper.common.client.artifact
{
    public static class MetadataReferenceProviderExtensions
    {
        public static MetadataReferenceProvider MetadataReferenceProvider(this IContainer container)
        {
            container.CheckContainer();

            lock (container) {
                if (container.DoesNotHave<MetadataReferenceProvider>()) {
                    container.Register<MetadataReferenceProvider>(Array.Empty<MetadataReference>, Lifespan.Singleton);
                }
            }

            return container.Resolve<MetadataReferenceProvider>();
        }
    }
}