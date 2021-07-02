///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     Context condition that start/initiated or ends/terminates context partitions based on a crontab expression.
    /// </summary>
    [Serializable]
    public class ContextDescriptorConditionCrontab : ContextDescriptorCondition
    {
        /// <summary>Ctor. </summary>
        public ContextDescriptorConditionCrontab()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="crontabExpressions">crontab expressions returning number sets for each crontab position</param>
        /// <param name="now">indicator whethet to include "now"</param>
        public ContextDescriptorConditionCrontab(
            IList<IList<Expression>> crontabExpressions,
            bool now)
        {
            CrontabExpressions = crontabExpressions;
            IsNow = now;
        }

        /// <summary>Returns the crontab expressions. </summary>
        /// <value>crontab</value>
        public IList<IList<Expression>> CrontabExpressions { get; set; }

        /// <summary>Returns "now" indicator </summary>
        /// <value>&quot;now&quot; indicator</value>
        public bool IsNow { get; set; }

        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            Write(writer, CrontabExpressions, IsNow);
        }

        private static void Write(
            TextWriter writer,
            IList<IList<Expression>> crontabs,
            bool now)
        {
            if (now) {
                writer.Write("@now and ");
            }
            string delimiter = "";
            foreach (IList<Expression> crontab in crontabs) {
                writer.Write(delimiter);
                Write(writer, crontab);
                delimiter = ", ";
            }
        }
        
        private static void Write(
            TextWriter writer,
            IList<Expression> expressions)
        {
            writer.Write("(");
            string delimiter = "";
            foreach (Expression e in expressions)
            {
                writer.Write(delimiter);
                e.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                delimiter = ", ";
            }

            writer.Write(")");
        }
    }
}