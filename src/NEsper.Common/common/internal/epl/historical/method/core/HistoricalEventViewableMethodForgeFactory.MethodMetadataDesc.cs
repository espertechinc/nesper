namespace com.espertech.esper.common.@internal.epl.historical.method.core
{
    public partial class HistoricalEventViewableMethodForgeFactory
    {
        public class MethodMetadataDesc
        {
            public MethodMetadataDesc(
                string typeName,
                object typeMetadata)
            {
                TypeName = typeName;
                TypeMetadata = typeMetadata;
            }

            public string TypeName { get; }

            public object TypeMetadata { get; }
        }
    }
}