///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>Context condition that start/initiated or ends/terminates context partitions based on a pattern. </summary>
    [Serializable]
    public class ContextDescriptorConditionPattern : ContextDescriptorCondition
    {
        /// <summary>Ctor. </summary>
        public ContextDescriptorConditionPattern()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="pattern">pattern expression</param>
        /// <param name="inclusive">if the events of the pattern should be included in the contextual statements</param>
        /// <param name="now">indicator whether "now"</param>
        /// <param name="asName">stream name, or null if not provided</param>
        public ContextDescriptorConditionPattern(
            PatternExpr pattern,
            bool inclusive,
            bool now,
            string asName)
        {
            Pattern = pattern;
            IsInclusive = inclusive;
            IsNow = now;
            AsName = asName;
        }

        /// <summary>Returns the pattern expression. </summary>
        /// <value>pattern</value>
        public PatternExpr Pattern { get; set; }

        /// <summary>
        ///     Return the inclusive flag, meaning events that constitute the pattern match should be considered for
        ///     context-associated statements.
        /// </summary>
        /// <value>inclusive flag</value>
        public bool IsInclusive { get; set; }

        /// <summary>Returns "now" indicator </summary>
        /// <value>&quot;now&quot; indicator</value>
        public bool IsNow { get; set; }
        
        /// <summary>
        /// Gets or sets the "as" name.
        /// </summary>
        public string AsName { get; set; }

        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            if (IsNow) {
                writer.Write("@now and");
            }

            writer.Write("pattern [");
            Pattern?.ToEPL(writer, PatternExprPrecedenceEnum.MINIMUM, formatter);

            writer.Write("]");
            if (IsInclusive) {
                writer.Write("@Inclusive");
            }

            if (AsName != null) {
                writer.Write(" as ");
                writer.Write(AsName);
            }
        }
    }
}