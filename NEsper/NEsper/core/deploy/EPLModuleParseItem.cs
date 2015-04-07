///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.core.deploy
{
    /// <summary>Item parsing an EPL module file. </summary>
    public class EPLModuleParseItem
    {
        /// <summary>Ctor. </summary>
        /// <param name="expression">EPL</param>
        /// <param name="lineNum">line number</param>
        /// <param name="startChar">start character number total file</param>
        /// <param name="endChar">end character number</param>
        public EPLModuleParseItem(String expression, int lineNum, int startChar, int endChar)
        {
            Expression = expression;
            LineNum = lineNum;
            StartChar = startChar;
            EndChar = endChar;
        }

        /// <summary>Returns line number of expression. </summary>
        /// <value>line number</value>
        public int LineNum { get; private set; }

        /// <summary>Returns the expression. </summary>
        /// <value>expression</value>
        public string Expression { get; private set; }

        /// <summary>Returns the position of the start character. </summary>
        /// <value>start char position</value>
        public int StartChar { get; private set; }

        /// <summary>Returns the position of the end character. </summary>
        /// <value>end char position</value>
        public int EndChar { get; private set; }
    }
}
