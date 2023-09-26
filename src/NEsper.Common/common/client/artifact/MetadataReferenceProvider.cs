using System.Collections.Generic;

#if NETCOREAPP3_0_OR_GREATER
#endif

using Microsoft.CodeAnalysis;

namespace com.espertech.esper.common.client.artifact
{
    public delegate IEnumerable<MetadataReference> MetadataReferenceProvider();
}