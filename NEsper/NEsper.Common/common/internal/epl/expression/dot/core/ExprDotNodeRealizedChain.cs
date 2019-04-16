///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeRealizedChain
    {
        private readonly ExprDotForge[] chain;
        private readonly ExprDotForge[] chainWithUnpack;
        private readonly FilterExprAnalyzerAffector filterAnalyzerDesc;

        public ExprDotNodeRealizedChain(
            ExprDotForge[] chain,
            ExprDotForge[] chainWithUnpack,
            FilterExprAnalyzerAffector filterAnalyzerDesc)
        {
            this.chain = chain;
            this.chainWithUnpack = chainWithUnpack;
            this.filterAnalyzerDesc = filterAnalyzerDesc;
        }

        public ExprDotForge[] Chain {
            get => chain;
        }

        public ExprDotForge[] ChainWithUnpack {
            get => chainWithUnpack;
        }

        public FilterExprAnalyzerAffector FilterAnalyzerDesc {
            get => filterAnalyzerDesc;
        }
    }
} // end of namespace