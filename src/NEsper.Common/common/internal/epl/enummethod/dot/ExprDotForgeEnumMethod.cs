///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public interface ExprDotForgeEnumMethod : ExprDotForge
    {
        void Init(
            int? streamOfProviderIfApplicable,
            EnumMethodDesc lambda,
            string lambdaUsedName,
            EPChainableType currentInputType,
            IList<ExprNode> parameters,
            ExprValidationContext validationContext);
    }
} // end of namespace