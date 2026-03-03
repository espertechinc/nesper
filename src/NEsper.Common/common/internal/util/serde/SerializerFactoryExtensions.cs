using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.util.serde
{
    public static class SerializerFactoryExtensions
    {
        public static SerializerFactory SerializerFactory(this IContainer container)
        {
            container.CheckContainer();

            lock (container) {
                if (container.DoesNotHave<SerializerFactory>()) {
                    container.Register<SerializerFactory>(
                        GetDefaultSerializerFactory,
                        Lifespan.Singleton);
                }
            }

            return container.Resolve<SerializerFactory>();
        }
        
        public static SerializerFactory GetDefaultSerializerFactory(IContainer container)
        {
            TypeResolver typeResolver;
            
            if (container.Has<TypeResolver>()) {
                typeResolver = container.Resolve<TypeResolver>();
            } else if (container.Has<TypeResolverProvider>()) {
                typeResolver = container.Resolve<TypeResolverProvider>().TypeResolver;
            } else {
                typeResolver = TypeResolverDefault.INSTANCE;
            }
            
            return new SerializerFactory(new ObjectSerializer(typeResolver));
        }
    }
}