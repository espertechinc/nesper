using System;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.configuration.common
{
    [Serializable]
    public class ImportBuiltinAnnotations : Import
    {
        public static ImportBuiltinAnnotations Instance = new ImportBuiltinAnnotations();

        private ImportBuiltinAnnotations()
        {
        }

        public override Type Resolve(
            string providedTypeName,
            TypeResolver typeResolver)
        {
            return BuiltinAnnotation.BUILTIN.Get(providedTypeName.ToLowerInvariant());
        }

        public override string ToString()
        {
            return $"ImportBuiltinAnnotations";
        }
    }
}