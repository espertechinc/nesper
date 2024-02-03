///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.plain.reverse
{
    public partial class ExprDotForgeReverse : ExprDotForgeEnumMethodBase
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
            EPChainableType type;
            if (inputEventType != null) {
                type = EPChainableTypeHelper.CollectionOfEvents(inputEventType);
            }
            else {
                type = EPChainableTypeHelper.CollectionOfSingleValue(collectionComponentType);
            }

            return new EnumForgeDescFactoryReverse(type, inputEventType == null);
        }
    }
} // end of namespace