using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

#if NETCORE
using System.Runtime.Loader;
#endif

using com.espertech.esper.container;

using Microsoft.CodeAnalysis;

namespace com.espertech.esper.common.client.artifact
{
    public delegate IEnumerable<MetadataReference> MetadataReferenceProvider();
}