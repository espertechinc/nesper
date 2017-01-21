///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>Context condition that start/initiated or ends/terminates context partitions based on a time period. </summary>
    [Serializable]
    public class ContextDescriptorConditionTimePeriod : ContextDescriptorCondition
    {
        /// <summary>Ctor. </summary>
        public ContextDescriptorConditionTimePeriod()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="timePeriod">time period expression</param>
        /// <param name="now">indicator whether "now"</param>
        public ContextDescriptorConditionTimePeriod(Expression timePeriod, bool now)
        {
            TimePeriod = timePeriod;
            IsNow = now;
        }

        /// <summary>Returns the time period expression </summary>
        /// <value>time period expression</value>
        public Expression TimePeriod { get; set; }

        /// <summary>Returns "now" indicator </summary>
        /// <value>&quot;now&quot; indicator</value>
        public bool IsNow { get; set; }

        public void ToEPL(TextWriter writer, EPStatementFormatter formatter)
        {
            if (IsNow)
            {
                writer.Write("@now and");
            }
            writer.Write("after ");
            TimePeriod.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
        }
    }
}