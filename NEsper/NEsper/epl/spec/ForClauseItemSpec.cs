///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    [Serializable]
    public class ForClauseItemSpec : MetaDefItem
    {
        public ForClauseItemSpec(String keyword, IList<ExprNode> expressions)
        {
            Keyword = keyword;
            Expressions = expressions;
        }

        public string Keyword { get; set; }

        public IList<ExprNode> Expressions { get; set; }
    }
}
