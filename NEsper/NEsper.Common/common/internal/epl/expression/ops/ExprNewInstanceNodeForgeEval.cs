///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.bean.manufacturer;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    ///     Represents the "new Class(...)" operator in an expression tree.
    /// </summary>
    public class ExprNewInstanceNodeForgeEval : ExprEvaluator
    {
        private readonly ExprNewInstanceNodeForge forge;
        private readonly InstanceManufacturer manufacturer;

        public ExprNewInstanceNodeForgeEval(ExprNewInstanceNodeForge forge, InstanceManufacturer manufacturer)
        {
            this.forge = forge;
            this.manufacturer = manufacturer;
        }

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return manufacturer.Make(eventsPerStream, isNewData, exprEvaluatorContext);
        }
    }
} // end of namespace