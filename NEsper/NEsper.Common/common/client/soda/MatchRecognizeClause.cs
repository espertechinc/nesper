///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Match-recognize clause.
    /// </summary>
    [Serializable]
    public class MatchRecognizeClause
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public MatchRecognizeClause()
        {
            Defines = new List<MatchRecognizeDefine>();
            SkipClause = MatchRecognizeSkipClause.PAST_LAST_ROW;
            Measures = new List<SelectClauseExpression>();
            PartitionExpressions = new List<Expression>();
        }

        /// <summary>
        /// Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        public void ToEPL(TextWriter writer)
        {
            string delimiter;

            writer.Write(" match_recognize (");
    
            if (PartitionExpressions.Count > 0) {
                delimiter = "";
                writer.Write(" partition by ");
                foreach (Expression part in PartitionExpressions) {
                    writer.Write(delimiter);
                    part.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                    delimiter = ", ";
                }
            }
    
            delimiter = "";
            writer.Write(" measures ");
            foreach (SelectClauseExpression part in Measures) {
                writer.Write(delimiter);
                part.ToEPLElement(writer);
                delimiter = ", ";
            }
    
            if (IsAll) {
                writer.Write(" all matches");
            }
    
            if (SkipClause != MatchRecognizeSkipClause.PAST_LAST_ROW) {
                writer.Write(" after match skip " + SkipClause.GetText());
            }
    
            writer.Write(" pattern (");
            Pattern.WriteEPL(writer);
            writer.Write(")");
    
            if ((IntervalClause != null) && (IntervalClause.Expression != null)){
                writer.Write(" interval ");
                IntervalClause.Expression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                if (IntervalClause.IsOrTerminated) {
                    writer.Write(" or terminated");
                }
            }
    
            delimiter = "";
            if (!Defines.IsEmpty()) {
                writer.Write(" define ");
                foreach (MatchRecognizeDefine def in Defines) {
                    writer.Write(delimiter);
                    writer.Write(def.Name);
                    writer.Write(" as ");
                    def.Expression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                    delimiter = ", ";
                }
            }
    
            writer.Write(")");
        }

        /// <summary>
        /// Get partition expressions.
        /// </summary>
        /// <value>partition expressions</value>
        public IList<Expression> PartitionExpressions { get; set; }

        /// <summary>
        /// Returns measures.
        /// </summary>
        /// <value>measures</value>
        public IList<SelectClauseExpression> Measures { get; set; }

        /// <summary>
        /// Indicator whether all matches.
        /// </summary>
        /// <value>all matches</value>
        public bool IsAll { get; set; }

        /// <summary>
        /// Returns skip-clause.
        /// </summary>
        /// <value>skip-clause</value>
        public MatchRecognizeSkipClause SkipClause { get; set; }

        /// <summary>
        /// Returns the defines-clause
        /// </summary>
        /// <value>defines-clause</value>
        public IList<MatchRecognizeDefine> Defines { get; set; }

        /// <summary>
        /// Returns the interval clause.
        /// </summary>
        /// <value>interval clause</value>
        public MatchRecognizeIntervalClause IntervalClause { get; set; }

        /// <summary>
        /// Returns regex-pattern.
        /// </summary>
        /// <value>pattern</value>
        public MatchRecognizeRegEx Pattern { get; set; }
    }
}
