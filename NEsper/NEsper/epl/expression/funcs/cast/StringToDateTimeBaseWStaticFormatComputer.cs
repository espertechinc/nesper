///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.funcs.cast
{
    public abstract class StringToDateTimeBaseWStaticFormatComputer<T> : StringToDateTimeBaseComputer<T>
    {
        private readonly string _dateFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringToDateTimeBaseWStaticFormatComputer{T}"/> class.
        /// </summary>
        /// <param name="dateFormat">The date format.</param>
        protected StringToDateTimeBaseWStaticFormatComputer(string dateFormat)
        {
            _dateFormat = dateFormat;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is constant for constant input.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is constant for constant input; otherwise, <c>false</c>.
        /// </value>
        public override bool IsConstantForConstInput
        {
            get { return true; }
        }

        /// <summary>
        /// Returns the date format that should be used for a given invocation.
        /// </summary>
        /// <param name="evaluateParams">The evaluate parameters.</param>
        /// <returns></returns>
        protected override string GetDateFormat(EvaluateParams evaluateParams)
        {
            return _dateFormat;
        }
    }
}
