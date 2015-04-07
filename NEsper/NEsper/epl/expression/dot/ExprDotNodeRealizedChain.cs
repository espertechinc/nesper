///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.datetime.eval;

namespace com.espertech.esper.epl.expression.dot
{
    public class ExprDotNodeRealizedChain
    {
        public ExprDotEval[] Chain { get; set; }
        public ExprDotEval[] ChainWithUnpack { get; set; }
        public ExprDotNodeFilterAnalyzerDesc FilterAnalyzerDesc { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExprDotNodeRealizedChain"/> class.
        /// </summary>
        /// <param name="chain">The chain.</param>
        /// <param name="chainWithUnpack">The chain with unpack.</param>
        /// <param name="filterAnalyzerDesc">The filter analyzer desc.</param>
        public ExprDotNodeRealizedChain(ExprDotEval[] chain, ExprDotEval[] chainWithUnpack, ExprDotNodeFilterAnalyzerDesc filterAnalyzerDesc)
        {
            Chain = chain;
            ChainWithUnpack = chainWithUnpack;
            FilterAnalyzerDesc = filterAnalyzerDesc;
        }
    }
}
