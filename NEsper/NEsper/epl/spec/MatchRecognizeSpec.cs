///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.rowregex;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Specification for match_recognize.
    /// </summary>
    [Serializable]
    public class MatchRecognizeSpec : MetaDefItem
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public MatchRecognizeSpec()
        {
            PartitionByExpressions = new List<ExprNode>();
            Measures = new List<MatchRecognizeMeasureItem>();
            Defines = new List<MatchRecognizeDefineItem>();
            Skip = new MatchRecognizeSkip(MatchRecognizeSkipEnum.PAST_LAST_ROW);
        }

        /// <summary>
        /// Interval part of null.
        /// </summary>
        /// <returns>
        /// interval
        /// </returns>
        public MatchRecognizeInterval Interval { get; set; }

        /// <summary>
        /// True for all-matches.
        /// </summary>
        /// <returns>
        /// indicator all-matches
        /// </returns>
        public bool IsAllMatches { get; set; }

        /// <summary>
        /// Returns partition expressions.
        /// </summary>
        /// <returns>
        /// partition expressions
        /// </returns>
        public IList<ExprNode> PartitionByExpressions { get; set; }

        /// <summary>
        /// Returns the define items.
        /// </summary>
        /// <returns>
        /// define items
        /// </returns>
        public IList<MatchRecognizeDefineItem> Defines { get; set; }

        /// <summary>
        /// Returns measures.
        /// </summary>
        /// <returns>
        /// measures
        /// </returns>
        public IList<MatchRecognizeMeasureItem> Measures { get; set; }

        /// <summary>
        /// Returns the pattern.
        /// </summary>
        /// <returns>
        /// pattern
        /// </returns>
        public RowRegexExprNode Pattern { get; set; }

        /// <summary>
        /// Returns the skip.
        /// </summary>
        /// <returns>
        /// skip
        /// </returns>
        public MatchRecognizeSkip Skip { get; set; }

        /// <summary>
        /// Add a measure item.
        /// </summary>
        /// <param name="item">to add</param>
        public void AddMeasureItem(MatchRecognizeMeasureItem item)
        {
            Measures.Add(item);
        }

        /// <summary>
        /// Adds a define item.
        /// </summary>
        /// <param name="define">to add</param>
        public void AddDefine(MatchRecognizeDefineItem define)
        {
            Defines.Add(define);
        }
    }
}
