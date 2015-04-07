///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.spec
{
    public class NewItem
    {
        public NewItem(String name, ExprNode optExpression)
        {
            Name = name;
            OptExpression = optExpression;
        }

        public String Name { get; private set; }

        public ExprNode OptExpression { get; private set; }
    }
}