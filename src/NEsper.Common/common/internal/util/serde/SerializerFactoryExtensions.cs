using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.util.serde
{
    public static class SerializerFactoryExtensions
    {
        public static SerializerFactory SerializerFactory(this IContainer container)
        {
            var serializerFactory = container.Resolve<SerializerFactory>();
            if (serializerFactory == null) {
                serializerFactory = new SerializerFactory(container);
            }

            return serializerFactory;
        }
    }
}