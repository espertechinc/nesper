///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Specification for a "define" construct within a match_recognize.
    /// </summary>
    [Serializable]
    public class MatchRecognizeDefineItem : MetaDefItem
    {
        /// <summary>Ctor. </summary>
        /// <param name="identifier">variable name</param>
        /// <param name="expression">expression</param>
        public MatchRecognizeDefineItem(String identifier, ExprNode expression) {
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
