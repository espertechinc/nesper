///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;


namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Context condition that start/initiated or ends/terminates context partitions based on a pattern.
    /// </summary>
    public class ContextDescriptorConditionPattern : ContextDescriptorCondition
    {
        private PatternExpr pattern;
        private bool inclusive; // statements declaring the context are inclusive of the events matching the pattern
        private bool now; // statements declaring the context initiate now and matching the pattern
        private string asName;

        /// <summary>
        /// Ctor.
        /// </summary>
        public ContextDescriptorConditionPattern()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "pattern">pattern expression</param>
        /// <param name = "inclusive">if the events of the pattern should be included in the contextual statements</param>
        /// <param name = "now">indicator whether "now"</param>
        /// <param name = "asName">stream name, or null if not provided</param>
        public ContextDescriptorConditionPattern(
            PatternExpr pattern,
            bool inclusive,
            bool now,
            string asName)
        {
            this.pattern = pattern;
            this.inclusive = inclusive;
            this.now = now;
            this.asName = asName;
        }

        /// <summary>
        /// Return the inclusive flag, meaning events that constitute the pattern match should be considered for context-associated statements.
        /// </summary>
        /// <returns>inclusive flag</returns>
        public bool IsInclusive => inclusive;

        /// <summary>
        /// Returns "now" indicator
        /// </summary>
        /// <returns>"now" indicator</returns>
        public bool IsNow => now;

        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            if (now) {
                writer.Write("@now and");
            }

            writer.Write("pattern [");
            if (pattern != null) {
                pattern.ToEPL(writer, PatternExprPrecedenceEnum.MINIMUM, formatter);
            }

            writer.Write("]");
            if (inclusive) {
                writer.Write("@Inclusive");
            }

            if (asName != null) {
                writer.Write(" as ");
                writer.Write(asName);
            }
        }

        public PatternExpr Pattern {
            get => pattern;

            set => pattern = value;
        }

        public bool Inclusive {
            set => inclusive = value;
        }

        public bool Now {
            set => now = value;
        }

        public string AsName {
            get => asName;

            set => asName = value;
        }
    }
} // end of namespace