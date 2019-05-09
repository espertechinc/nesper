///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    ///     Hold parameters for timer:at.
    /// </summary>
    [Serializable]
    public class CronParameter
    {
        /// <summary>Ctor. </summary>
        /// <param name="operator">is the operator as text</param>
        /// <param name="day">is the day text</param>
        public CronParameter(
            CronOperatorEnum @operator,
            int? day)
        {
            Operator = @operator;
            Day = day;
        }

        /// <summary>Ctor. </summary>
        /// <param name="operator">is the operator as text</param>
        /// <param name="day">is the day text</param>
        public CronParameter(
            CronOperatorEnum @operator,
            int? day,
            int? month)
        {
            Operator = @operator;
            Day = day;
            Month = month;
        }

        public CronOperatorEnum Operator { get; }

        public int? Day { get; }

        public int? Month { get; set; }

        public string Formatted()
        {
            return string.Format(
                "{0}(day {1} month {2})",
                Operator,
                Day.FormatInt(),
                Month.FormatInt()
            );
        }

        public CodegenExpression Make()
        {
            return NewInstance(typeof(CronParameter), Constant(Operator), Constant(Day), Constant(Month));
        }
    }
}