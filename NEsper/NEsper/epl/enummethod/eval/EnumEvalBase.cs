///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalBase
    {
        protected int StreamNumLambda;

        public EnumEvalBase(ExprEvaluator innerExpression, int streamCountIncoming)
            : this(streamCountIncoming)
        {
            InnerExpression = innerExpression;
        }

        public EnumEvalBase(int streamCountIncoming)
        {
            StreamNumLambda = streamCountIncoming;
        }

        public int StreamNumSize
        {
            get { return StreamNumLambda + 1; }
        }

        protected internal ExprEvaluator InnerExpression;
    }
}
