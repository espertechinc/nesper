///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>Statistics holder for data flow instances. </summary>
    public class EPDataFlowInstanceOperatorStat
    {
        /// <summary>Ctor. </summary>
        /// <param name="operatorName">operator name</param>
        /// <param name="operatorPrettyPrint">operator pretty print</param>
        /// <param name="operatorNumber">operator number</param>
        /// <param name="submittedOverallCount">count of submitted events</param>
        /// <param name="submittedPerPortCount">count of events submitted per port</param>
        /// <param name="timeOverall">time spent submitting events</param>
        /// <param name="timePerPort">time spent submitting events per port</param>
        public EPDataFlowInstanceOperatorStat(
            String operatorName,
            String operatorPrettyPrint,
            int operatorNumber,
            long submittedOverallCount,
            long[] submittedPerPortCount,
            long timeOverall,
            long[] timePerPort)
        {
            OperatorName = operatorName;
            OperatorPrettyPrint = operatorPrettyPrint;
            OperatorNumber = operatorNumber;
            SubmittedOverallCount = submittedOverallCount;
            SubmittedPerPortCount = submittedPerPortCount;
            TimeOverall = timeOverall;
            TimePerPort = timePerPort;
        }

        /// <summary>Returns operator name. </summary>
        /// <value>op name</value>
        public string OperatorName { get; private set; }

        /// <summary>Returns count of submitted events. </summary>
        /// <value>count</value>
        public long SubmittedOverallCount { get; private set; }

        /// <summary>Returns count of submitted events per port. </summary>
        /// <value>count per port</value>
        public long[] SubmittedPerPortCount { get; private set; }

        /// <summary>Returns operator pretty print </summary>
        /// <value>textual representation of op</value>
        public string OperatorPrettyPrint { get; private set; }

        /// <summary>Returns the operator number. </summary>
        /// <value>op number</value>
        public int OperatorNumber { get; private set; }

        /// <summary>Returns total time spent submitting events </summary>
        /// <value>time</value>
        public long TimeOverall { get; private set; }

        /// <summary>Returns total time spent submitting events per port </summary>
        /// <value>time per port</value>
        public long[] TimePerPort { get; private set; }
    }
}