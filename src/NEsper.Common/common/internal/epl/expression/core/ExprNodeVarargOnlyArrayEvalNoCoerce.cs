///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    internal class ExprNodeVarargOnlyArrayEvalNoCoerce : ExprEvaluator
    {
        private readonly ExprEvaluator[] _evals;
        private readonly ExprNodeVarargOnlyArrayForge _forge;

        public ExprNodeVarargOnlyArrayEvalNoCoerce(
            ExprNodeVarargOnlyArrayForge forge,
            ExprEvaluator[] evals)
        {
            this._forge = forge;
            this._evals = evals;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var array = Arrays.CreateInstanceChecked(_forge.varargClass, _evals.Length);
            for (var i = 0; i < _evals.Length; i++) {
                var value = _evals[i].Evaluate(eventsPerStream, isNewData, context);
                array.SetValue(value, i);
            }

            return array;
        }
    }
} // end of namespace