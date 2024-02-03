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
            return new SerializerFactory(container);
        }
    }
}