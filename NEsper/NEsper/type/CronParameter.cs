///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.type
{
    /// <summary>
    /// Hold parameters for timer:at.
    /// </summary>
    [Serializable]
    public class CronParameter
    {
        private readonly CronOperatorEnum _operator;
        private readonly int? _day;
        private int? _month;

        /// <summary>Ctor. </summary>
        /// <param name="operator">is the operator as text</param>
        /// <param name="day">is the day text</param>
        public CronParameter(CronOperatorEnum @operator, int? day)
        {
            _operator = @operator;
            _day = day;
        }

        public CronOperatorEnum Operator
        {
            get { return _operator; }
        }

        public int? Day
        {
            get { return _day; }
        }

        public int? Month
        {
            get { return _month; }
            set { _month = value; }
        }

        public String Formatted()
        {
            return string.Format("{0}(day {1} month {2})", 
                _operator, 
                _day.FormatInt(), 
                _month.FormatInt()
                );
        }
    }
}