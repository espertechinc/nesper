///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.chain;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public interface ExprNodeWithChainSpec
    {
        IList<Chainable> ChainSpec {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
    }
}