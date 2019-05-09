///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    [Serializable]
    public class ExpressionDeclDesc
    {
        public IList<ExpressionDeclItem> Expressions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionDeclDesc"/> class.
        /// </summary>
        public ExpressionDeclDesc()
        {
            Expressions = new List<ExpressionDeclItem>();
        }

        public void Add(ExpressionDeclItem declNode)
        {
            Expressions.Add(declNode);
        }
    }
}