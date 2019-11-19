///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// DefaultTypeCasterFactory is a class that : the methods required to
    /// transform objects from one type to another type.  This specific class allows
    /// the developer to override the behavior that occurs during creation of new
    /// TypeCasters.
    /// </summary>
    public class DefaultTypeCasterFactory
    {
        private static readonly Dictionary<TypePair, TypeCaster> typePairConverterTable =
            new Dictionary<TypePair, TypeCaster>();

        /// <summary>
        /// Gets or creates a typeCaster for the specified pair of types.
        /// </summary>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        public TypeCaster GetTypeCaster(Type sourceType, Type targetType)
        {
            // If the target is nullable then it's because of boxing.  From the perspective
            // of how we put items on the stack its not that much different from an unboxed
            // value since the return value must be boxed for return anyway.
            sourceType = GetTrueType(sourceType);
            targetType = GetTrueType(targetType);

            var typePair = new TypePair(sourceType, targetType);
            var typePairConverter = typePairConverterTable.Get(typePair);
            if (typePairConverter == null)
            {
                typePairConverter = EmitTypePairConverter(typePair);
                typePairConverterTable[typePair] = typePairConverter;
            }

            return typePairConverter;
        }

        /// <summary>
        /// Returns the source as the target; this is used when the source type and
        /// target types are identical.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        private static Object IdentityTypePairConverter(Object source)
        {
            return source;
        }

        /// <summary>
        /// Returns the source as a string.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        private static Object StringTypePairConverter(Object source)
        {
            return source != null ? source.ToString() : null;
        }

        /// <summary>
        /// Emits a type caster for the type pair.
        /// </summary>
        /// <param name="typePair">The type pair.</param>
        /// <returns></returns>
        public TypeCaster EmitTypePairConverter(TypePair typePair)
        {
            // Are the source type and target types identical
            if (typePair.TypeA == typePair.TypeB) return IdentityTypePairConverter;
            // Are the type pairs assignable
            if (typePair.TypeB.IsAssignableFrom(typePair.TypeA)) return IdentityTypePairConverter;
            // Is the target type a string
            if (typePair.TypeB == typeof (string)) return StringTypePairConverter;
            // Are the source and target types primitives
            // Dynamically emit a typeCaster
            var eParam = Expression.Parameter(typeof (object), "source");
            var eCastA = Expression.ConvertChecked(eParam, typePair.TypeA); // Cast to source type
            var eCastB = Expression.ConvertChecked(eCastA, typePair.TypeB); // Cast to target type
            var eCastC = Expression.Convert(eCastB, typeof(object)); // Cast to return type
            var eCheck = Expression.Equal(eParam, Expression.Constant(null));
            var eCondition = Expression.Condition(eCheck, Expression.Constant(null), eCastC);
            var eLambda = Expression.Lambda<Func<object, object>>(eCondition, eParam);
            Func<object, object> tempFunc = eLambda.Compile();
            return tempFunc.Invoke;
        }

        /// <summary>
        /// Gets the true underlying type of the provided type.  Basically it unmasks
        /// nullables.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Type GetTrueType(Type type)
        {
            var baseT = Nullable.GetUnderlyingType(type);
            return baseT ?? type;
        }
    }
}
