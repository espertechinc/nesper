///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.datetime.eval
{
    public class ExprDotDTMethodDesc
    {
        private readonly ExprDotForge forge;
        private readonly EPChainableType returnType;
        private readonly FilterExprAnalyzerAffector intervalFilterDesc;

        public ExprDotDTMethodDesc(
            ExprDotForge forge,
            EPChainableType returnType,
            FilterExprAnalyzerAffector intervalFilterDesc)
        {
            this.forge = forge;
            this.returnType = returnType;
            this.intervalFilterDesc = intervalFilterDesc;
        }

        public ExprDotForge Forge => forge;

        public EPChainableType ReturnType => returnType;

        public FilterExprAnalyzerAffector IntervalFilterDesc => intervalFilterDesc;
    }
} // end of namespace