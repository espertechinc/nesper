///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.rowrecog.expr;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Specification for match_recognize.
    /// </summary>
    public class MatchRecognizeSpec
    {
        private IList<MatchRecognizeDefineItem> defines;
        private MatchRecognizeInterval interval;
        private bool isAllMatches;
        private IList<MatchRecognizeMeasureItem> measures;
        private IList<ExprNode> partitionByExpressions;
        private RowRecogExprNode pattern;
        private MatchRecognizeSkip skip;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public MatchRecognizeSpec()
        {
            partitionByExpressions = new List<ExprNode>();
            measures = new List<MatchRecognizeMeasureItem>();
            defines = new List<MatchRecognizeDefineItem>();
            skip = new MatchRecognizeSkip(MatchRecognizeSkipEnum.PAST_LAST_ROW);
        }

        /// <summary>
        ///     Interval part of null.
        /// </summary>
        /// <returns>interval</returns>
        public MatchRecognizeInterval Interval {
            get => interval;
            set => interval = value;
        }

        /// <summary>
        ///     True for all-matches.
        /// </summary>
        /// <returns>indicator all-matches</returns>
        public bool IsAllMatches {
            get => isAllMatches;
            set => isAllMatches = value;
        }

        /// <summary>
        ///     Returns partition expressions.
        /// </summary>
        /// <returns>partition expressions</returns>
        public IList<ExprNode> PartitionByExpressions {
            get => partitionByExpressions;
            set => partitionByExpressions = value;
        }

        /// <summary>
        ///     Returns the define items.
        /// </summary>
        /// <returns>define items</returns>
        public IList<MatchRecognizeDefineItem> Defines {
            get => defines;
            set => defines = value;
        }

        /// <summary>
        ///     Returns measures.
        /// </summary>
        /// <returns>measures</returns>
        public IList<MatchRecognizeMeasureItem> Measures {
            get => measures;
            set => measures = value;
        }

        /// <summary>
        ///     Returns the pattern.
        /// </summary>
        /// <returns>pattern</returns>
        public RowRecogExprNode Pattern {
            get => pattern;
            set => pattern = value;
        }

        /// <summary>
        ///     Returns the skip.
        /// </summary>
        /// <returns>skip</returns>
        public MatchRecognizeSkip Skip {
            get => skip;
            set => skip = value;
        }

        /// <summary>
        ///     Add a measure item.
        /// </summary>
        /// <param name="item">to add</param>
        public void AddMeasureItem(MatchRecognizeMeasureItem item)
        {
            measures.Add(item);
        }

        /// <summary>
        ///     Adds a define item.
        /// </summary>
        /// <param name="define">to add</param>
        public void AddDefine(MatchRecognizeDefineItem define)
        {
            defines.Add(define);
        }
    }
} // end of namespace