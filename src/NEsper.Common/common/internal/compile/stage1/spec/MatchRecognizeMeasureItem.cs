///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    /// Specification for measure definition item within match_recognize.
    /// </summary>
    public class MatchRecognizeMeasureItem
    {
        /// <summary>Ctor. </summary>
        /// <param name="expr">expression</param>
        /// <param name="name">as name</param>
        public MatchRecognizeMeasureItem(
            ExprNode expr,
            string name)
        {
            Expr = expr;
            Name = name;
        }

        /// <summary>Returns the as-name. </summary>
        /// <value>name</value>
        public string Name { get; private set; }

        /// <summary>Gets or sets the validated expression. </summary>
        /// <value>expression</value>
        public ExprNode Expr { get; set; }
    }
}