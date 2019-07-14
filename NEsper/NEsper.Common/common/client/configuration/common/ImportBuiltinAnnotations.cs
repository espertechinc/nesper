using System;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.configuration.common
{
    [Serializable]
    public class ImportBuiltinAnnotations : Import
    {
        public override Type Resolve(
            string providedTypeName,
            ClassForNameProvider classForNameProvider)
        {
            return BuiltinAnnotation.BUILTIN.Get(providedTypeName.ToLowerInvariant());
        }
    }
}