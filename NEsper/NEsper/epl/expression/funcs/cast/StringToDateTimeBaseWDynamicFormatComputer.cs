///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.funcs.cast
{
    public abstract class StringToDateTimeBaseWDynamicFormat<T> : StringToDateTimeBaseComputer<T>
    {
        private readonly ExprEvaluator _dateFormatEval;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringToDateTimeBaseWDynamicFormat{T}"/> class.
        /// </summary>
        /// <param name="dateFormatEval">The date format eval.</param>
        protected StringToDateTimeBaseWDynamicFormat(ExprEvaluator dateFormatEval)
        {
            _dateFormatEval = dateFormatEval;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is constant for constant input.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is constant for constant input; otherwise, <c>false</c>.
        /// </value>
        public override bool IsConstantForConstInput
        {
            get { return false; }
        }

        /// <summary>
        /// Returns the date format that should be used for a given invocation.
        /// </summary>
        /// <param name="evaluateParams">The evaluate parameters.</param>
        /// <returns></returns>
        /// <exception cref="EPException">
        /// Null date format returned by 'dateformat' expression
        /// or
        /// DateFormat returned by expression was of incorrect type
        /// </exception>
        protected override string GetDateFormat(EvaluateParams evaluateParams)
        {
            var dateFormat = _dateFormatEval.Evaluate(evaluateParams);
            if (dateFormat == null)
            {
                throw new EPException("Null date format returned by 'dateformat' expression");
            }

            if (dateFormat is string)
            {
                return (string) dateFormat;
            }

            throw new EPException("DateFormat returned by expression was of incorrect type");
        }
    }
}
