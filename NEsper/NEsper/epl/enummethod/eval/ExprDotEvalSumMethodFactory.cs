///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.enummethod.eval
{
    public interface ExprDotEvalSumMethodFactory
    {
        ExprDotEvalSumMethod SumAggregator { get; }
        Type ValueType { get; }
    }

    public class ProxyExprDotEvalSumMethodFactory : ExprDotEvalSumMethodFactory
    {
        public Func<ExprDotEvalSumMethod> ProcSumAggregator { get; set; }
        public Func<Type> ProcValueType { get; set; }

        public ExprDotEvalSumMethod SumAggregator
        {
            get { return ProcSumAggregator(); }
        }

        public Type ValueType
        {
            get { return ProcValueType(); }
        }
    }
}
