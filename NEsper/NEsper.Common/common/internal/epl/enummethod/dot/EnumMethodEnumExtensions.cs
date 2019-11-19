///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.enummethod.eval;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public static class EnumMethodEnumExtensions
    {
        public static string GetNameCamel(this EnumMethodEnum value)
        {
            switch (value) {
                case EnumMethodEnum.AGGREGATE:
                    return "aggregate";

                case EnumMethodEnum.ALLOF:
                    return "allOf";

                case EnumMethodEnum.ANYOF:
                    return "anyOf";

                case EnumMethodEnum.TOMAP:
                    return "toMap";

                case EnumMethodEnum.GROUPBY:
                    return "groupBy";

                case EnumMethodEnum.COUNTOF:
                    return "countOf";

                case EnumMethodEnum.MIN:
                    return "min";

                case EnumMethodEnum.MAX:
                    return "max";

                case EnumMethodEnum.AVERAGE:
                    return "average";

                case EnumMethodEnum.SUMOF:
                    return "sumOf";

                case EnumMethodEnum.MOSTFREQUENT:
                    return "mostFrequent";

                case EnumMethodEnum.LEASTFREQUENT:
                    return "leastFrequent";

                case EnumMethodEnum.SELECTFROM:
                    return "selectFrom";

                case EnumMethodEnum.FIRST:
                    return "firstOf";

                case EnumMethodEnum.LAST:
                    return "lastOf";

                case EnumMethodEnum.MINBY:
                    return "minBy";

                case EnumMethodEnum.MAXBY:
                    return "maxBy";

                case EnumMethodEnum.TAKE:
                    return "take";

                case EnumMethodEnum.TAKELAST:
                    return "takeLast";

                case EnumMethodEnum.TAKEWHILE:
                    return "takeWhile";

                case EnumMethodEnum.TAKEWHILELAST:
                    return "takeWhileLast";

                case EnumMethodEnum.ORDERBY:
                    return "orderBy";

                case EnumMethodEnum.ORDERBYDESC:
                    return "orderByDesc";

                case EnumMethodEnum.DISTINCT:
                    return "distinctOf";

                case EnumMethodEnum.WHERE:
                    return "where";

                case EnumMethodEnum.UNION:
                    return "union";

                case EnumMethodEnum.EXCEPT:
                    return "except";

                case EnumMethodEnum.INTERSECT:
                    return "intersect";

                case EnumMethodEnum.REVERSE:
                    return "reverse";

                case EnumMethodEnum.NOOP:
                    return "esperInternalNoop";

                case EnumMethodEnum.SEQUENCE_EQUAL:
                    return "sequenceequal";
            }

            throw new ArgumentException("invalid value", nameof(value));
        }

        public static DotMethodFP[] GetFootprints(this EnumMethodEnum value)
        {
            switch (value) {
                case EnumMethodEnum.AGGREGATE:
                    return EnumMethodEnumParams.AGGREGATE_FP;

                case EnumMethodEnum.ALLOF:
                    return EnumMethodEnumParams.ALLOF_ANYOF;

                case EnumMethodEnum.ANYOF:
                    return EnumMethodEnumParams.ALLOF_ANYOF;

                case EnumMethodEnum.TOMAP:
                    return EnumMethodEnumParams.MAP;

                case EnumMethodEnum.GROUPBY:
                    return EnumMethodEnumParams.GROUP;

                case EnumMethodEnum.COUNTOF:
                    return EnumMethodEnumParams.COUNTOF_FIRST_LAST;

                case EnumMethodEnum.MIN:
                    return EnumMethodEnumParams.MIN_MAX;

                case EnumMethodEnum.MAX:
                    return EnumMethodEnumParams.MIN_MAX;

                case EnumMethodEnum.AVERAGE:
                    return EnumMethodEnumParams.AVERAGE_SUMOF;

                case EnumMethodEnum.SUMOF:
                    return EnumMethodEnumParams.AVERAGE_SUMOF;

                case EnumMethodEnum.MOSTFREQUENT:
                    return EnumMethodEnumParams.MOST_LEAST_FREQ;

                case EnumMethodEnum.LEASTFREQUENT:
                    return EnumMethodEnumParams.MOST_LEAST_FREQ;

                case EnumMethodEnum.SELECTFROM:
                    return EnumMethodEnumParams.SELECTFROM_MINBY_MAXBY;

                case EnumMethodEnum.FIRST:
                    return EnumMethodEnumParams.COUNTOF_FIRST_LAST;

                case EnumMethodEnum.LAST:
                    return EnumMethodEnumParams.COUNTOF_FIRST_LAST;

                case EnumMethodEnum.MINBY:
                    return EnumMethodEnumParams.SELECTFROM_MINBY_MAXBY;

                case EnumMethodEnum.MAXBY:
                    return EnumMethodEnumParams.SELECTFROM_MINBY_MAXBY;

                case EnumMethodEnum.TAKE:
                    return EnumMethodEnumParams.TAKE;

                case EnumMethodEnum.TAKELAST:
                    return EnumMethodEnumParams.TAKELAST;

                case EnumMethodEnum.TAKEWHILE:
                    return EnumMethodEnumParams.WHERE_FP;

                case EnumMethodEnum.TAKEWHILELAST:
                    return EnumMethodEnumParams.WHERE_FP;

                case EnumMethodEnum.ORDERBY:
                    return EnumMethodEnumParams.ORDERBY_DISTINCT;

                case EnumMethodEnum.ORDERBYDESC:
                    return EnumMethodEnumParams.ORDERBY_DISTINCT;

                case EnumMethodEnum.DISTINCT:
                    return EnumMethodEnumParams.ORDERBY_DISTINCT;

                case EnumMethodEnum.WHERE:
                    return EnumMethodEnumParams.WHERE_FP;

                case EnumMethodEnum.UNION:
                    return EnumMethodEnumParams.SET_LOGIC_FP;

                case EnumMethodEnum.EXCEPT:
                    return EnumMethodEnumParams.SET_LOGIC_FP;

                case EnumMethodEnum.INTERSECT:
                    return EnumMethodEnumParams.SET_LOGIC_FP;

                case EnumMethodEnum.REVERSE:
                    return EnumMethodEnumParams.NOOP_REVERSE;

                case EnumMethodEnum.NOOP:
                    return EnumMethodEnumParams.NOOP_REVERSE;

                case EnumMethodEnum.SEQUENCE_EQUAL:
                    return EnumMethodEnumParams.SEQ_EQUALS_FP;
            }

            throw new ArgumentException("invalid value", nameof(value));
        }

        public static Type GetImplementation(this EnumMethodEnum value)
        {
            switch (value) {
                case EnumMethodEnum.AGGREGATE:
                    return typeof(ExprDotForgeAggregate);

                case EnumMethodEnum.ALLOF:
                    return typeof(ExprDotForgeAllOfAnyOf);

                case EnumMethodEnum.ANYOF:
                    return typeof(ExprDotForgeAllOfAnyOf);

                case EnumMethodEnum.TOMAP:
                    return typeof(ExprDotForgeToMap);

                case EnumMethodEnum.GROUPBY:
                    return typeof(ExprDotForgeGroupBy);

                case EnumMethodEnum.COUNTOF:
                    return typeof(ExprDotForgeCountOf);

                case EnumMethodEnum.MIN:
                    return typeof(ExprDotForgeMinMax);

                case EnumMethodEnum.MAX:
                    return typeof(ExprDotForgeMinMax);

                case EnumMethodEnum.AVERAGE:
                    return typeof(ExprDotForgeAverage);

                case EnumMethodEnum.SUMOF:
                    return typeof(ExprDotForgeSumOf);

                case EnumMethodEnum.MOSTFREQUENT:
                    return typeof(ExprDotForgeMostLeastFrequent);

                case EnumMethodEnum.LEASTFREQUENT:
                    return typeof(ExprDotForgeMostLeastFrequent);

                case EnumMethodEnum.SELECTFROM:
                    return typeof(ExprDotForgeSelectFrom);

                case EnumMethodEnum.FIRST:
                    return typeof(ExprDotForgeFirstLastOf);

                case EnumMethodEnum.LAST:
                    return typeof(ExprDotForgeFirstLastOf);

                case EnumMethodEnum.MINBY:
                    return typeof(ExprDotForgeMinByMaxBy);

                case EnumMethodEnum.MAXBY:
                    return typeof(ExprDotForgeMinByMaxBy);

                case EnumMethodEnum.TAKE:
                    return typeof(ExprDotForgeTakeAndTakeLast);

                case EnumMethodEnum.TAKELAST:
                    return typeof(ExprDotForgeTakeAndTakeLast);

                case EnumMethodEnum.TAKEWHILE:
                    return typeof(ExprDotForgeTakeWhileAndLast);

                case EnumMethodEnum.TAKEWHILELAST:
                    return typeof(ExprDotForgeTakeWhileAndLast);

                case EnumMethodEnum.ORDERBY:
                    return typeof(ExprDotForgeOrderByAscDesc);

                case EnumMethodEnum.ORDERBYDESC:
                    return typeof(ExprDotForgeOrderByAscDesc);

                case EnumMethodEnum.DISTINCT:
                    return typeof(ExprDotForgeDistinct);

                case EnumMethodEnum.WHERE:
                    return typeof(ExprDotForgeWhere);

                case EnumMethodEnum.UNION:
                    return typeof(ExprDotForgeSetExceptUnionIntersect);

                case EnumMethodEnum.EXCEPT:
                    return typeof(ExprDotForgeSetExceptUnionIntersect);

                case EnumMethodEnum.INTERSECT:
                    return typeof(ExprDotForgeSetExceptUnionIntersect);

                case EnumMethodEnum.REVERSE:
                    return typeof(ExprDotForgeReverse);

                case EnumMethodEnum.NOOP:
                    return typeof(ExprDotForgeNoOp);

                case EnumMethodEnum.SEQUENCE_EQUAL:
                    return typeof(ExprDotForgeSequenceEqual);
            }

            throw new ArgumentException("invalid value", nameof(value));
        }

        public static bool IsEnumerationMethod(string name)
        {
            foreach (EnumMethodEnum e in EnumHelper.GetValues<EnumMethodEnum>()) {
                if (GetNameCamel(e).Equals(name, StringComparison.InvariantCultureIgnoreCase)) {
                    return true;
                }
            }

            return false;
        }

        public static EnumMethodEnum? FromName(string name)
        {
            foreach (EnumMethodEnum e in EnumHelper.GetValues<EnumMethodEnum>()) {
                if (GetNameCamel(e).Equals(name, StringComparison.InvariantCultureIgnoreCase)) {
                    return e;
                }
            }

            return null;
        }
    }
}