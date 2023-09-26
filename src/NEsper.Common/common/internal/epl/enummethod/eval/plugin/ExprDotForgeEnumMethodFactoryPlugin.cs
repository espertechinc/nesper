///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.enummethod;
using com.espertech.esper.common.@internal.epl.enummethod.dot;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.plugin
{
    public class ExprDotForgeEnumMethodFactoryPlugin
    {
        private readonly EnumMethodForgeFactory _forgeFactory;

        public ExprDotForgeEnumMethodFactoryPlugin(EnumMethodForgeFactory forgeFactory)
        {
            _forgeFactory = forgeFactory;
        }

        public ExprDotForgeEnumMethod Make(int numParameters)
        {
            return new ExprDotForgeEnumMethodPlugin(_forgeFactory);
        }

        public ExprDotForgeEnumMethodFactory EnumMethodFactory => Make;
    }
} // end of namespace