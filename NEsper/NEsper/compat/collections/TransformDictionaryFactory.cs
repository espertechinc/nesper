using System.Collections.Generic;

using com.espertech.esper.util;

namespace com.espertech.esper.compat.collections
{
    public class TransformDictionaryFactory
    {
        public static TransformDictionary<TK1, TV1, TK2, TV2> Create<TK1, TV1, TK2, TV2>(object value)
        {
            var sourceDictionary = (IDictionary<TK2, TV2>) value;

            var key1To2Coercer = CoercerFactory.GetCoercer(typeof(TK1), typeof(TK2));
            var key2To1Coercer = CoercerFactory.GetCoercer(typeof(TK2), typeof(TK1));
            var val1To2Coercer = CoercerFactory.GetCoercer(typeof(TV1), typeof(TV2));
            var val2To1Coercer = CoercerFactory.GetCoercer(typeof(TV2), typeof(TV1));

            return new TransformDictionary<TK1, TV1, TK2, TV2>(
                sourceDictionary,
                k2 => (TK1) key2To1Coercer(k2),
                k1 => (TK2) key1To2Coercer(k1),
                v2 => (TV1) val2To1Coercer(v2),
                v1 => (TV2) val1To2Coercer(v1));
        }
    }
}
