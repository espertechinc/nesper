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
    /// Specification for a "define" construct within a match_recognize.
    /// </summary>
    public class MatchRecognizeDefineItem
    {
        /// <summary>Ctor. </summary>
        /// <param name="identifier">variable name</param>
        /// <param name="expression">expression</param>
        public MatchRecognizeDefineItem(
            string identifier,
            ExprNode expression)
        {
            Identifier = identifier;
            Expression = expression;
        }

        /// <summary>Returns the variable name. </summary>
        /// <value>name</value>
        public string Identifier { get; private set; }

        /// <summary>Returns the expression. </summary>
        /// <value>expression</value>
        public ExprNode Expression { get; set; }
    }
}