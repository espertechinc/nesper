///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.datetime.reformatop;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.datetime.eval.reformat
{
    internal abstract class DTLocalEvaluatorReformatBase : DTLocalEvaluator
    {
        protected readonly ReformatOp ReformatOp;

        protected DTLocalEvaluatorReformatBase(ReformatOp reformatOp)
        {
            ReformatOp = reformatOp;
        }

        public abstract object Evaluate(object target, EvaluateParams evaluateParams);
    }
}
