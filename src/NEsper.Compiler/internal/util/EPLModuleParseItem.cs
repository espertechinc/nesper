///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.compiler.@internal.util
{
    /// <summary>
    ///     Item parsing an EPL module file.
    /// </summary>
    public class EPLModuleParseItem
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="expression">EPL</param>
        /// <param name="lineNum">line number</param>
        /// <param name="startChar">start character number total file</param>
        /// <param name="endChar">end character number</param>
        public EPLModuleParseItem(
            string expression,
            int lineNum,
            int startChar,
            int endChar,
            int lineNumEnd,
            int lineNumContent,
            int lineNumContentEnd)
        {
            Expression = expression;
            LineNum = lineNum;
            StartChar = startChar;
            EndChar = endChar;
            LineNumEnd = lineNumEnd;
            LineNumContent = lineNumContent;
            LineNumContentEnd = lineNumContentEnd;
        }

        /// <summary>
        ///     Returns line number of expression.
        /// </summary>
        /// <returns>line number</returns>
        public int LineNum { get; }

        /// <summary>
        ///     Returns the expression.
        /// </summary>
        /// <returns>expression</returns>
        public string Expression { get; }

        /// <summary>
        ///     Returns the position of the start character.
        /// </summary>
        /// <returns>start char position</returns>
        public int StartChar { get; }

        /// <summary>
        ///     Returns the position of the end character.
        /// </summary>
        /// <returns>end char position</returns>
        public int EndChar { get; }
        
        /// <summary>
        ///     Returns the line number where the expression ends.
        /// </summary>
        public int LineNumEnd { get; }
        
        /// <summary>
        ///     Returns the line number starting the EPL excluding comments.
        /// </summary>
        public int LineNumContent { get; }
        
        /// <summary>
        ///     Returns the line number ending the EPL excluding comments.
        /// </summary>
        public int LineNumContentEnd { get; }
    }
} // end of namespace