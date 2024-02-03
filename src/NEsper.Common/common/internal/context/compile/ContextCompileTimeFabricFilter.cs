///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;

namespace com.espertech.esper.common.@internal.context.compile
{
    internal interface ContextCompileTimeFabricFilter
    {
        void Accept(
            FilterOperator @operator,
            string expression,
            DataInputOutputSerdeForge forge);
    }
} // end of namespace