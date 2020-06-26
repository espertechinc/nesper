using System;

using com.espertech.esper.common.@internal.epl.enummethod.eval;
using com.espertech.esper.common.@internal.epl.enummethod.eval.aggregate;
using com.espertech.esper.common.@internal.epl.enummethod.eval.plain.exceptintersectunion;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.allofanyof;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.arrayOf;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.average;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.countof;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.distinctof;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.firstoflastof;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.groupby;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.minmax;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.minmaxby;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.mostleastfreq;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@orderby;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.selectfrom;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.sumof;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.takewhile;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@where;
using com.espertech.esper.common.@internal.epl.enummethod.eval.twolambda.groupby;
using com.espertech.esper.common.@internal.epl.enummethod.eval.twolambda.tomap;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public static class EnumMethodBuiltinExtensions
    {
        public static  string GetNameCamel(this EnumMethodBuiltin value)
        {
            return value switch {
                EnumMethodBuiltin.AGGREGATE => "aggregate",
                EnumMethodBuiltin.ALLOF => "allOf",
                EnumMethodBuiltin.ANYOF => "anyOf",
                EnumMethodBuiltin.TOMAP => "toMap",
                EnumMethodBuiltin.GROUPBY => "groupBy",
                EnumMethodBuiltin.COUNTOF => "countOf",
                EnumMethodBuiltin.MIN => "min",
                EnumMethodBuiltin.MAX => "max",
                EnumMethodBuiltin.AVERAGE => "average",
                EnumMethodBuiltin.SUMOF => "sumOf",
                EnumMethodBuiltin.MOSTFREQUENT => "mostFrequent",
                EnumMethodBuiltin.LEASTFREQUENT => "leastFrequent",
                EnumMethodBuiltin.SELECTFROM => "selectFrom",
                EnumMethodBuiltin.FIRST => "firstOf",
                EnumMethodBuiltin.LAST => "lastOf",
                EnumMethodBuiltin.MINBY => "minBy",
                EnumMethodBuiltin.MAXBY => "maxBy",
                EnumMethodBuiltin.TAKE => "take",
                EnumMethodBuiltin.TAKELAST => "takeLast",
                EnumMethodBuiltin.TAKEWHILE => "takeWhile",
                EnumMethodBuiltin.TAKEWHILELAST => "takeWhileLast",
                EnumMethodBuiltin.ORDERBY => "orderBy",
                EnumMethodBuiltin.ORDERBYDESC => "orderByDesc",
                EnumMethodBuiltin.DISTINCT => "distinctOf",
                EnumMethodBuiltin.WHERE => "where",
                EnumMethodBuiltin.UNION => "union",
                EnumMethodBuiltin.EXCEPT => "except",
                EnumMethodBuiltin.INTERSECT => "intersect",
                EnumMethodBuiltin.REVERSE => "reverse",
                EnumMethodBuiltin.NOOP => "esperInternalNoop",
                EnumMethodBuiltin.SEQUENCE_EQUAL => "sequenceequal",
                EnumMethodBuiltin.ARRAYOF => "arrayOf",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        public static DotMethodFP[] GetFootprints(this EnumMethodBuiltin value)
        {
            return value switch {
                EnumMethodBuiltin.AGGREGATE => EnumMethodEnumParams.AGGREGATE_FP,
                EnumMethodBuiltin.ALLOF => EnumMethodEnumParams.ALLOF_ANYOF,
                EnumMethodBuiltin.ANYOF => EnumMethodEnumParams.ALLOF_ANYOF,
                EnumMethodBuiltin.TOMAP => EnumMethodEnumParams.TOMAP,
                EnumMethodBuiltin.GROUPBY => EnumMethodEnumParams.GROUP,
                EnumMethodBuiltin.COUNTOF => EnumMethodEnumParams.COUNTOF_FIRST_LAST,
                EnumMethodBuiltin.MIN => EnumMethodEnumParams.ORDERBY_DISTINCT_ARRAYOF_MOSTLEAST_MINMAX,
                EnumMethodBuiltin.MAX => EnumMethodEnumParams.ORDERBY_DISTINCT_ARRAYOF_MOSTLEAST_MINMAX,
                EnumMethodBuiltin.AVERAGE => EnumMethodEnumParams.AVERAGE_SUMOF,
                EnumMethodBuiltin.SUMOF => EnumMethodEnumParams.AVERAGE_SUMOF,
                EnumMethodBuiltin.MOSTFREQUENT => EnumMethodEnumParams.ORDERBY_DISTINCT_ARRAYOF_MOSTLEAST_MINMAX,
                EnumMethodBuiltin.LEASTFREQUENT => EnumMethodEnumParams.ORDERBY_DISTINCT_ARRAYOF_MOSTLEAST_MINMAX,
                EnumMethodBuiltin.SELECTFROM => EnumMethodEnumParams.SELECTFROM_MINMAXBY,
                EnumMethodBuiltin.FIRST => EnumMethodEnumParams.COUNTOF_FIRST_LAST,
                EnumMethodBuiltin.LAST => EnumMethodEnumParams.COUNTOF_FIRST_LAST,
                EnumMethodBuiltin.MINBY => EnumMethodEnumParams.SELECTFROM_MINMAXBY,
                EnumMethodBuiltin.MAXBY => EnumMethodEnumParams.SELECTFROM_MINMAXBY,
                EnumMethodBuiltin.TAKE => EnumMethodEnumParams.TAKE,
                EnumMethodBuiltin.TAKELAST => EnumMethodEnumParams.TAKELAST,
                EnumMethodBuiltin.TAKEWHILE => EnumMethodEnumParams.WHERE_FP,
                EnumMethodBuiltin.TAKEWHILELAST => EnumMethodEnumParams.WHERE_FP,
                EnumMethodBuiltin.ORDERBY => EnumMethodEnumParams.ORDERBY_DISTINCT_ARRAYOF_MOSTLEAST_MINMAX,
                EnumMethodBuiltin.ORDERBYDESC => EnumMethodEnumParams.ORDERBY_DISTINCT_ARRAYOF_MOSTLEAST_MINMAX,
                EnumMethodBuiltin.DISTINCT => EnumMethodEnumParams.ORDERBY_DISTINCT_ARRAYOF_MOSTLEAST_MINMAX,
                EnumMethodBuiltin.WHERE => EnumMethodEnumParams.WHERE_FP,
                EnumMethodBuiltin.UNION => EnumMethodEnumParams.SET_LOGIC_FP,
                EnumMethodBuiltin.EXCEPT => EnumMethodEnumParams.SET_LOGIC_FP,
                EnumMethodBuiltin.INTERSECT => EnumMethodEnumParams.SET_LOGIC_FP,
                EnumMethodBuiltin.REVERSE => EnumMethodEnumParams.NOOP_REVERSE,
                EnumMethodBuiltin.NOOP => EnumMethodEnumParams.NOOP_REVERSE,
                EnumMethodBuiltin.SEQUENCE_EQUAL => EnumMethodEnumParams.SEQ_EQUALS_FP,
                EnumMethodBuiltin.ARRAYOF => EnumMethodEnumParams.ORDERBY_DISTINCT_ARRAYOF_MOSTLEAST_MINMAX,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        public static ExprDotForgeEnumMethodFactory GetFactory(this EnumMethodBuiltin value)
        {
            return value switch {
                EnumMethodBuiltin.AGGREGATE => _ => new ExprDotForgeAggregate(),
                EnumMethodBuiltin.ALLOF => _ => new ExprDotForgeAllOfAnyOf(),
                EnumMethodBuiltin.ANYOF => _ => new ExprDotForgeAllOfAnyOf(),
                EnumMethodBuiltin.TOMAP => _ => new ExprDotForgeToMap(),
                EnumMethodBuiltin.GROUPBY => numParameters => {
                    if (numParameters == 1) {
                        return new ExprDotForgeGroupByOneParam();
                    }
                    else {
                        return new ExprDotForgeGroupByTwoParam();
                    }
                },
                EnumMethodBuiltin.COUNTOF => _ => new ExprDotForgeCountOf(),
                EnumMethodBuiltin.MIN => _ => new ExprDotForgeMinMax(),
                EnumMethodBuiltin.MAX => _ => new ExprDotForgeMinMax(),
                EnumMethodBuiltin.AVERAGE => _ => new ExprDotForgeAverage(),
                EnumMethodBuiltin.SUMOF => _ => new ExprDotForgeSumOf(),
                EnumMethodBuiltin.MOSTFREQUENT => _ => new ExprDotForgeMostLeastFrequent(),
                EnumMethodBuiltin.LEASTFREQUENT => _ => new ExprDotForgeMostLeastFrequent(),
                EnumMethodBuiltin.SELECTFROM => _ => new ExprDotForgeSelectFrom(),
                EnumMethodBuiltin.FIRST => _ => new ExprDotForgeFirstLastOf(),
                EnumMethodBuiltin.LAST => _ => new ExprDotForgeFirstLastOf(),
                EnumMethodBuiltin.MINBY => _ => new ExprDotForgeMinByMaxBy(),
                EnumMethodBuiltin.MAXBY => _ => new ExprDotForgeMinByMaxBy(),
                EnumMethodBuiltin.TAKE => _ => new ExprDotForgeTakeAndTakeLast(),
                EnumMethodBuiltin.TAKELAST => _ => new ExprDotForgeTakeAndTakeLast(),
                EnumMethodBuiltin.TAKEWHILE => _ => new ExprDotForgeTakeWhileAndLast(),
                EnumMethodBuiltin.TAKEWHILELAST => _ => new ExprDotForgeTakeWhileAndLast(),
                EnumMethodBuiltin.ORDERBY => _ => new ExprDotForgeOrderByAscDesc(),
                EnumMethodBuiltin.ORDERBYDESC => _ => new ExprDotForgeOrderByAscDesc(),
                EnumMethodBuiltin.DISTINCT => _ => new ExprDotForgeDistinctOf(),
                EnumMethodBuiltin.WHERE => _ => new ExprDotForgeWhere(),
                EnumMethodBuiltin.UNION => _ => new ExprDotForgeSetExceptIntersectUnion(),
                EnumMethodBuiltin.EXCEPT => _ => new ExprDotForgeSetExceptIntersectUnion(),
                EnumMethodBuiltin.INTERSECT => _ => new ExprDotForgeSetExceptIntersectUnion(),
                EnumMethodBuiltin.REVERSE => _ => new ExprDotForgeReverse(),
                EnumMethodBuiltin.NOOP => _ => new ExprDotForgeNoOp(),
                EnumMethodBuiltin.SEQUENCE_EQUAL => _ => new ExprDotForgeSequenceEqual(),
                EnumMethodBuiltin.ARRAYOF => _ => new ExprDotForgeArrayOf(),
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        public static EnumMethodDesc GetDescriptor(this EnumMethodBuiltin value)
        {
            var nameCamel = GetNameCamel(value);
            var footprints = GetFootprints(value);
            var factory = GetFactory(value);
            var enumMethodEnum = EnumHelper.Parse<EnumMethodEnum>(nameCamel);

            return new EnumMethodDesc(nameCamel, enumMethodEnum, factory, footprints);
        }
    }
}