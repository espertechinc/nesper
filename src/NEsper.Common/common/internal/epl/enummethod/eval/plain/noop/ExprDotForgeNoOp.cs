///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class ExprDotForgeNoOp : ExprDotForgeEnumMethodBase
    {
        public override EnumForgeDescFactory GetForgeFactory(
            DotMethodFP footprint,
            IList<ExprNode> parameters,
            EnumMethodEnum enumMethod,
            string enumMethodUsedName,
            EventType inputEventType,
            Type collectionComponentType,
            ExprValidationContext validationContext)
        {
            return new ProxyEnumForgeDescFactory() {
                ProcGetLambdaStreamTypesForParameter = parameterNum => new EnumForgeLambdaDesc(new EventType[0], new string[0]),
                ProcMakeEnumForgeDesc = (
                    bodiesAndParameters,
                    streamCountIncoming,
                    services) => {
                    var type = EPChainableTypeHelper.CollectionOfEvents(inputEventType);
                    return new EnumForgeDesc(type, new EnumForgeNoOp(streamCountIncoming));
                }
            };
        }
    }
} // end of namespace