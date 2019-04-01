///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationRowCtorDesc
    {
        public AggregationRowCtorDesc(
            CodegenClassScope classScope, CodegenCtor rowCtor, IList<CodegenTypedParam> rowMembers,
            CodegenNamedMethods namedMethods)
        {
            ClassScope = classScope;
            RowCtor = rowCtor;
            RowMembers = rowMembers;
            NamedMethods = namedMethods;
        }

        public CodegenClassScope ClassScope { get; }

        public CodegenCtor RowCtor { get; }

        public IList<CodegenTypedParam> RowMembers { get; }

        public CodegenNamedMethods NamedMethods { get; }
    }
} // end of namespace