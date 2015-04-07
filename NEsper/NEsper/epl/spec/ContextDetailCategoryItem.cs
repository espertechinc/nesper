///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.filter;

namespace com.espertech.esper.epl.spec
{
    [Serializable]
    public class ContextDetailCategoryItem
    {
        public ContextDetailCategoryItem(ExprNode expression, String name)
        {
            Expression = expression;
            Name = name;
        }

        public ExprNode Expression { get; private set; }

        public string Name { get; private set; }

        public FilterValueSetParam[][] CompiledFilterParam { get; private set; }

        public FilterSpecCompiled CompiledFilter
        {
            set { CompiledFilterParam = value.GetValueSet(null, null, null).Parameters; }
        }
    }
}
