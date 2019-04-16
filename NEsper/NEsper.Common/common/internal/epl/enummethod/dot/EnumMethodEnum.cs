///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.enummethod.eval;
using com.espertech.esper.common.@internal.epl.methodbase;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class EnumMethodEnum
    {
        public static readonly EnumMethodEnum AGGREGATE = new EnumMethodEnum(
            "aggregate", typeof(ExprDotForgeAggregate), EnumMethodEnumParams.AGGREGATE_FP);

        public static readonly EnumMethodEnum ALLOF = new EnumMethodEnum(
            "allOf", typeof(ExprDotForgeAllOfAnyOf), EnumMethodEnumParams.ALLOF_ANYOF);

        public static readonly EnumMethodEnum ANYOF = new EnumMethodEnum(
            "anyOf", typeof(ExprDotForgeAllOfAnyOf), EnumMethodEnumParams.ALLOF_ANYOF);

        public static readonly EnumMethodEnum TOMAP = new EnumMethodEnum(
            "toMap", typeof(ExprDotForgeToMap), EnumMethodEnumParams.MAP);

        public static readonly EnumMethodEnum GROUPBY = new EnumMethodEnum(
            "groupBy", typeof(ExprDotForgeGroupBy), EnumMethodEnumParams.GROUP);

        public static readonly EnumMethodEnum COUNTOF = new EnumMethodEnum(
            "countOf", typeof(ExprDotForgeCountOf), EnumMethodEnumParams.COUNTOF_FIRST_LAST);

        public static readonly EnumMethodEnum MIN = new EnumMethodEnum(
            "min", typeof(ExprDotForgeMinMax), EnumMethodEnumParams.MIN_MAX);

        public static readonly EnumMethodEnum MAX = new EnumMethodEnum(
            "max", typeof(ExprDotForgeMinMax), EnumMethodEnumParams.MIN_MAX);

        public static readonly EnumMethodEnum AVERAGE = new EnumMethodEnum(
            "average", typeof(ExprDotForgeAverage), EnumMethodEnumParams.AVERAGE_SUMOF);

        public static readonly EnumMethodEnum SUMOF = new EnumMethodEnum(
            "sumOf", typeof(ExprDotForgeSumOf), EnumMethodEnumParams.AVERAGE_SUMOF);

        public static readonly EnumMethodEnum MOSTFREQUENT = new EnumMethodEnum(
            "mostFrequent", typeof(ExprDotForgeMostLeastFrequent), EnumMethodEnumParams.MOST_LEAST_FREQ);

        public static readonly EnumMethodEnum LEASTFREQUENT = new EnumMethodEnum(
            "leastFrequent", typeof(ExprDotForgeMostLeastFrequent), EnumMethodEnumParams.MOST_LEAST_FREQ);

        public static readonly EnumMethodEnum SELECTFROM = new EnumMethodEnum(
            "selectFrom", typeof(ExprDotForgeSelectFrom), EnumMethodEnumParams.SELECTFROM_MINBY_MAXBY);

        public static readonly EnumMethodEnum FIRST = new EnumMethodEnum(
            "firstOf", typeof(ExprDotForgeFirstLastOf), EnumMethodEnumParams.COUNTOF_FIRST_LAST);

        public static readonly EnumMethodEnum LAST = new EnumMethodEnum(
            "lastOf", typeof(ExprDotForgeFirstLastOf), EnumMethodEnumParams.COUNTOF_FIRST_LAST);

        public static readonly EnumMethodEnum MINBY = new EnumMethodEnum(
            "minBy", typeof(ExprDotForgeMinByMaxBy), EnumMethodEnumParams.SELECTFROM_MINBY_MAXBY);

        public static readonly EnumMethodEnum MAXBY = new EnumMethodEnum(
            "maxBy", typeof(ExprDotForgeMinByMaxBy), EnumMethodEnumParams.SELECTFROM_MINBY_MAXBY);

        public static readonly EnumMethodEnum TAKE = new EnumMethodEnum(
            "take", typeof(ExprDotForgeTakeAndTakeLast), EnumMethodEnumParams.TAKE);

        public static readonly EnumMethodEnum TAKELAST = new EnumMethodEnum(
            "takeLast", typeof(ExprDotForgeTakeAndTakeLast), EnumMethodEnumParams.TAKELAST);

        public static readonly EnumMethodEnum TAKEWHILE = new EnumMethodEnum(
            "takeWhile", typeof(ExprDotForgeTakeWhileAndLast), EnumMethodEnumParams.WHERE_FP);

        public static readonly EnumMethodEnum TAKEWHILELAST = new EnumMethodEnum(
            "takeWhileLast", typeof(ExprDotForgeTakeWhileAndLast), EnumMethodEnumParams.WHERE_FP);

        public static readonly EnumMethodEnum ORDERBY = new EnumMethodEnum(
            "orderBy", typeof(ExprDotForgeOrderByAscDesc), EnumMethodEnumParams.ORDERBY_DISTINCT);

        public static readonly EnumMethodEnum ORDERBYDESC = new EnumMethodEnum(
            "orderByDesc", typeof(ExprDotForgeOrderByAscDesc), EnumMethodEnumParams.ORDERBY_DISTINCT);

        public static readonly EnumMethodEnum DISTINCT = new EnumMethodEnum(
            "distinctOf", typeof(ExprDotForgeDistinct), EnumMethodEnumParams.ORDERBY_DISTINCT);

        public static readonly EnumMethodEnum WHERE = new EnumMethodEnum(
            "where", typeof(ExprDotForgeWhere), EnumMethodEnumParams.WHERE_FP);

        public static readonly EnumMethodEnum UNION = new EnumMethodEnum(
            "union", typeof(ExprDotForgeSetExceptUnionIntersect), EnumMethodEnumParams.SET_LOGIC_FP);

        public static readonly EnumMethodEnum EXCEPT = new EnumMethodEnum(
            "except", typeof(ExprDotForgeSetExceptUnionIntersect), EnumMethodEnumParams.SET_LOGIC_FP);

        public static readonly EnumMethodEnum INTERSECT = new EnumMethodEnum(
            "intersect", typeof(ExprDotForgeSetExceptUnionIntersect), EnumMethodEnumParams.SET_LOGIC_FP);

        public static readonly EnumMethodEnum REVERSE = new EnumMethodEnum(
            "reverse", typeof(ExprDotForgeReverse), EnumMethodEnumParams.NOOP_REVERSE);

        public static readonly EnumMethodEnum NOOP = new EnumMethodEnum(
            "esperInternalNoop", typeof(ExprDotForgeNoOp), EnumMethodEnumParams.NOOP_REVERSE);

        public static readonly EnumMethodEnum SEQUENCE_EQUAL = new EnumMethodEnum(
            "sequenceequal", typeof(ExprDotForgeSequenceEqual), EnumMethodEnumParams.SEQ_EQUALS_FP);

        public static readonly ISet<EnumMethodEnum> Values = new HashSet<EnumMethodEnum>();


        private EnumMethodEnum(
            string nameCamel,
            Type implementation,
            DotMethodFP[] footprints)
        {
            NameCamel = nameCamel;
            Implementation = implementation;
            Footprints = footprints;
            Values.Add(this);
        }

        public string NameCamel { get; }

        public DotMethodFP[] Footprints { get; }

        public Type Implementation { get; }

        public static bool IsEnumerationMethod(string name)
        {
            foreach (EnumMethodEnum e in Values) {
                if (e.NameCamel.Equals(name, StringComparison.InvariantCultureIgnoreCase)) {
                    return true;
                }
            }

            return false;
        }

        public static EnumMethodEnum FromName(string name)
        {
            foreach (EnumMethodEnum e in Values) {
                if (e.NameCamel.Equals(name, StringComparison.InvariantCultureIgnoreCase)) {
                    return e;
                }
            }

            return null;
        }
    }
} // end of namespace