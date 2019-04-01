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
    public class AggregationServiceFactoryMakeResult
    {
        public AggregationServiceFactoryMakeResult(CodegenMethod initMethod, IList<CodegenInnerClass> innerClasses)
        {
            InitMethod = initMethod;
            InnerClasses = innerClasses;
        }

        public CodegenMethod InitMethod { get; }

        public IList<CodegenInnerClass> InnerClasses { get; }
    }
} // end of namespace