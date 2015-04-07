///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.metric
{
    /// <summary>Handle for statements metric reporting by runtime. </summary>
    public class StatementMetricHandle
    {
        private readonly int groupNum;
        private readonly int index;

        /// <summary>Ctor. </summary>
        /// <param name="groupNum">group number, zero for default group</param>
        /// <param name="index">index slot</param>
        public StatementMetricHandle(int groupNum, int index)
        {
            this.groupNum = groupNum;
            this.index = index;
            this.IsEnabled = true;
        }

        /// <summary>Returns group number for statement. </summary>
        /// <returns>group number</returns>
        public int GroupNum
        {
            get { return groupNum; }
        }

        /// <summary>Returns slot number of metric. </summary>
        /// <returns>metric index</returns>
        public int Index
        {
            get { return index; }
        }

        /// <summary>Gets or sets an indicator that if true is enabled for statement. </summary>
        /// <returns>enabled flag</returns>
        public bool IsEnabled { get; set; }
    }
}
