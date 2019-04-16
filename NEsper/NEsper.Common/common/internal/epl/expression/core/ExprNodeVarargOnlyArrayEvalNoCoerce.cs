///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    internal class ExprNodeVarargOnlyArrayEvalNoCoerce : ExprEvaluator
    {
        private readonly ExprEvaluator[] evals;
        private readonly ExprNodeVarargOnlyArrayForge forge;

        public ExprNodeVarargOnlyArrayEvalNoCoerce(
            ExprNodeVarargOnlyArrayForge forge,
            ExprEvaluator[] evals)
        {
            this.forge = forge;
            this.evals = evals;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var array = Array.CreateInstance(forge.varargClass, evals.Length);
            for (var i = 0; i < evals.Length; i++) {
                var value = evals[i].Evaluate(eventsPerStream, isNewData, context);
                array.SetValue(value, i);
            }

            return array;
        }
    }
} // end of namespace