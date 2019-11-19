///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.@join.analyze;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeRealizedChain
    {
        public ExprDotNodeRealizedChain(
            ExprDotForge[] chain,
            ExprDotForge[] chainWithUnpack,
            FilterExprAnalyzerAffector filterAnalyzerDesc)
        {
            Chain = chain;
            ChainWithUnpack = chainWithUnpack;
            FilterAnalyzerDesc = filterAnalyzerDesc;
        }

        public ExprDotForge[] Chain { get; }

        public ExprDotForge[] ChainWithUnpack { get; }

        public FilterExprAnalyzerAffector FilterAnalyzerDesc { get; }
    }
} // end of namespace