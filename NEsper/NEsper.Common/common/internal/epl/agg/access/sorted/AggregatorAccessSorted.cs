///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public interface AggregatorAccessSorted : AggregatorAccess
    {
        CodegenExpression GetLastValueCodegen(CodegenClassScope classScope, CodegenMethod parent);

        CodegenExpression GetFirstValueCodegen(CodegenClassScope classScope, CodegenMethod parent);

        CodegenExpression SizeCodegen();

        CodegenExpression ReverseIteratorCodegen { get; }

        CodegenExpression IteratorCodegen();

        CodegenExpression CollectionReadOnlyCodegen();
    }
} // end of namespace