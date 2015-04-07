///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.deploy
{
    /// <summary>Inner exception to <seealso cref="DeploymentActionException" /> available on statement level. </summary>
    [Serializable]
    public class DeploymentItemException : DeploymentException 
    {
        /// <summary>Ctor. </summary>
        /// <param name="message">exception text</param>
        /// <param name="expression">EPL</param>
        /// <param name="inner">compile or start exception</param>
        /// <param name="lineNumber">line number</param>
        public DeploymentItemException(String message, String expression, Exception inner, int lineNumber)
            : base(message, inner)
        {
            Expression = expression;
            Inner = inner;
            LineNumber = lineNumber;
        }

        /// <summary>Returns EPL expression. </summary>
        /// <value>expression</value>
        public string Expression { get; private set; }

        /// <summary>Returns EPL compile or start exception. </summary>
        /// <value>exception</value>
        public Exception Inner { get; private set; }

        /// <summary>Returns line number. </summary>
        /// <value>line number</value>
        public int LineNumber { get; private set; }
    }
}
