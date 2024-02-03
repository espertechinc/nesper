///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.view.previous
{
    public interface PreviousGetterStrategy
    {
        PreviousGetterStrategy GetStrategy(ExprEvaluatorContext ctx);
    }

    public static class PreviousGetterStrategyConstants
    {
        public static readonly PreviousGetterStrategy[] EMPTY_ARRAY = Array.Empty<PreviousGetterStrategy>();
    }
} // end of namespace