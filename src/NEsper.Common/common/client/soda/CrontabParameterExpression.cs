///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     Parameter expression such as last/lastweek/weekday/wildcard for use in crontab expressions.
    /// </summary>
    [Serializable]
    public class CrontabParameterExpression : ExpressionBase
    {
        private ScheduleItemType type;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public CrontabParameterExpression()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="type">of crontab parameter</param>
        public CrontabParameterExpression(ScheduleItemType type)
        {
            this.type = type;
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.UNARY;

        /// <summary>
        ///     Returns crontab parameter type.
        /// </summary>
        /// <returns>crontab parameter type</returns>
        public ScheduleItemType Type
        {
            get => type;
            set => type = value;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (!Children.IsEmpty())
            {
                Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(' ');
            }

            writer.Write(type.GetSyntax());
        }
    }
} // end of namespace