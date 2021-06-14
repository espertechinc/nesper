///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.metrics.stmtmetrics
{
    /// <summary>Handle for statements metric reporting by runtime. </summary>
    public class StatementMetricHandle
    {
        /// <summary>Ctor. </summary>
        /// <param name="groupNum">group number, zero for default group</param>
        /// <param name="index">index slot</param>
        public StatementMetricHandle(
            int groupNum,
            int index)
        {
            GroupNum = groupNum;
            Index = index;
            IsEnabled = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementMetricHandle"/> class.
        /// </summary>
        /// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
        public StatementMetricHandle(
            bool isEnabled)
        {
            GroupNum = -1;
            Index = -1;
            IsEnabled = isEnabled;
        }

        /// <summary>Returns group number for statement. </summary>
        /// <returns>group number</returns>
        public int GroupNum { get; }

        /// <summary>Returns slot number of metric. </summary>
        /// <returns>metric index</returns>
        public int Index { get; }

        /// <summary>Gets or sets an indicator that if true is enabled for statement. </summary>
        /// <returns>enabled flag</returns>
        public bool IsEnabled { get; set; }
    }
}