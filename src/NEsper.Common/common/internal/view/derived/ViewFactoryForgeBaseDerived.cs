///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.view.derived
{
    public abstract class ViewFactoryForgeBaseDerived : ViewFactoryForgeBase
    {
        public IList<ExprNode> ViewParameters { get; internal set; }

        public StatViewAdditionalPropsForge AdditionalProps { get; internal set; }
    }
} // end of namespace