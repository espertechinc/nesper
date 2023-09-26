///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;


namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    public class FireAndForgetSpecInsert : FireAndForgetSpec
    {
        private readonly bool useValuesKeyword;
        private readonly IList<IList<ExprNode>> multirow;

        public FireAndForgetSpecInsert(
            bool useValuesKeyword,
            IList<IList<ExprNode>> multirow)
        {
            this.useValuesKeyword = useValuesKeyword;
            this.multirow = multirow;
        }

        public bool IsUseValuesKeyword => useValuesKeyword;

        public IList<IList<ExprNode>> Multirow => multirow;
    }
} // end of namespace