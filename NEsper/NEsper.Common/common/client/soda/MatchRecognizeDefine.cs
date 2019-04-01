///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Define-clause in match-recognize expression.
    /// </summary>
    [Serializable]
    public class MatchRecognizeDefine
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public MatchRecognizeDefine() {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="name">variable name</param>
        /// <param name="expression">expression</param>
        public MatchRecognizeDefine(String name, Expression expression) {
            Name = name;
            Expression = expression;
        }

        /// <summary>
        /// Returns the variable name.
        /// </summary>
        /// <value>variable name</value>
        public string Name { get; set; }

        /// <summary>
        /// Returns the expression.
        /// </summary>
        /// <value>expression</value>
        public Expression Expression { get; set; }
    }
}
