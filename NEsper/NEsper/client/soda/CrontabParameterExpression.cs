///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Parameter expression such as last/lastweek/weekday/wildcard for use in crontab expressions.
    /// </summary>
    [Serializable]
    public class CrontabParameterExpression : ExpressionBase
    {
        /// <summary>Ctor. </summary>
        public CrontabParameterExpression()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="type">of crontab parameter</param>
        public CrontabParameterExpression(ScheduleItemType type)
        {
            ItemType = type;
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.UNARY; }
        }

        /// <summary>Returns crontab parameter type. </summary>
        /// <value>crontab parameter type</value>
        public ScheduleItemType ItemType { get; set; }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (!Children.IsEmpty())
            {
                Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(' ');
            }
            writer.Write(ItemType.GetSyntax());
        }
    }
}