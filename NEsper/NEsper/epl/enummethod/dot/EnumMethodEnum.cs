///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.epl.enummethod.eval;
using com.espertech.esper.epl.methodbase;

namespace com.espertech.esper.epl.enummethod.dot
{
    public enum EnumMethodEnum
    {
        AGGREGATE,
        ALLOF,
        ANYOF,
        TOMAP,
        GROUPBY,
        COUNTOF,
        MIN,
        MAX,
        AVERAGE,
        SUMOF,
        MOSTFREQUENT,
        LEASTFREQUENT,
        SELECTFROM,
        FIRST,
        LAST,
        MINBY,
        MAXBY,
        TAKE,
        TAKELAST,
        TAKEWHILE,
        TAKEWHILELAST,
        ORDERBY,
        ORDERBYDESC,
        DISTINCT,
        WHERE,
        UNION,
        EXCEPT,
        INTERSECT,
        REVERSE,
        NOOP,
        SEQUENCE_EQUAL
    } ;

    public static class EnumMethodEnumExtensions
    {
        public static string GetNameCamel(this EnumMethodEnum value)
        {
            switch (value)
            {
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

            throw new ArgumentException();
        }

        public static Type GetImplementation(this EnumMethodEnum value)
        {
            switch (value)
            {
                case EnumMethodEnum.AGGREGATE:
                    return typeof(ExprDotEvalAggregate);
                case EnumMethodEnum.ALLOF:
                    return typeof(ExprDotEvalAllOfAnyOf);
                case EnumMethodEnum.ANYOF:
                    return typeof(ExprDotEvalAllOfAnyOf);
                case EnumMethodEnum.TOMAP:
                    return typeof(ExprDotEvalToMap);
                case EnumMethodEnum.GROUPBY:
                    return typeof(ExprDotEvalGroupBy);
                case EnumMethodEnum.COUNTOF:
                    return typeof(ExprDotEvalCountOf);
                case EnumMethodEnum.MIN:
                    return typeof(ExprDotEvalMinMax);
                case EnumMethodEnum.MAX:
                    return typeof(ExprDotEvalMinMax);
                case EnumMethodEnum.AVERAGE:
                    return typeof(ExprDotEvalAverage);
                case EnumMethodEnum.SUMOF:
                    return typeof(ExprDotEvalSumOf);
                case EnumMethodEnum.MOSTFREQUENT:
                    return typeof(ExprDotEvalMostLeastFrequent);
                case EnumMethodEnum.LEASTFREQUENT:
                    return typeof(ExprDotEvalMostLeastFrequent);
                case EnumMethodEnum.SELECTFROM:
                    return typeof(ExprDotEvalSelectFrom);
                case EnumMethodEnum.FIRST:
                    return typeof(ExprDotEvalFirstLastOf);
                case EnumMethodEnum.LAST:
                    return typeof(ExprDotEvalFirstLastOf);
                case EnumMethodEnum.MINBY:
                    return typeof(ExprDotEvalMinByMaxBy);
                case EnumMethodEnum.MAXBY:
                    return typeof(ExprDotEvalMinByMaxBy);
                case EnumMethodEnum.TAKE:
                    return typeof(ExprDotEvalTakeAndTakeLast);
                case EnumMethodEnum.TAKELAST:
                    return typeof(ExprDotEvalTakeAndTakeLast);
                case EnumMethodEnum.TAKEWHILE:
                    return typeof(ExprDotEvalTakeWhileAndLast);
                case EnumMethodEnum.TAKEWHILELAST:
                    return typeof(ExprDotEvalTakeWhileAndLast);
                case EnumMethodEnum.ORDERBY:
                    return typeof(ExprDotEvalOrderByAscDesc);
                case EnumMethodEnum.ORDERBYDESC:
                    return typeof(ExprDotEvalOrderByAscDesc);
                case EnumMethodEnum.DISTINCT:
                    return typeof(ExprDotEvalDistinct);
                case EnumMethodEnum.WHERE:
                    return typeof(ExprDotEvalWhere);
                case EnumMethodEnum.UNION:
                    return typeof(ExprDotEvalSetExceptUnionIntersect);
                case EnumMethodEnum.EXCEPT:
                    return typeof(ExprDotEvalSetExceptUnionIntersect);
                case EnumMethodEnum.INTERSECT:
                    return typeof(ExprDotEvalSetExceptUnionIntersect);
                case EnumMethodEnum.REVERSE:
                    return typeof(ExprDotEvalReverse);
                case EnumMethodEnum.NOOP:
                    return typeof(ExprDotEvalNoOp);
                case EnumMethodEnum.SEQUENCE_EQUAL:
                    return typeof(ExprDotEvalSequenceEqual);
            }

            throw new ArgumentException();
        }

        public static DotMethodFP[] GetFootprints(this EnumMethodEnum value)
        {
            switch (value)
            {
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

            throw new ArgumentException();
        }

        public static bool IsEnumerationMethod(this String name)
        {
            return EnumHelper
                .GetValues<EnumMethodEnum>()
                .Any(e => String.Equals(name, e.GetNameCamel(), StringComparison.InvariantCultureIgnoreCase));
        }

        public static EnumMethodEnum FromName(this String name)
        {
            return EnumHelper
                .GetValues<EnumMethodEnum>()
                .FirstOrDefault(e => String.Equals(name, e.GetNameCamel(), StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
