///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client.soda;

namespace com.espertech.esper.common.client.module
{
    /// <summary>
    /// Represents an EPL statement as part of a <see cref="Module" />.
    /// <para/>
    /// Character position start and end are only available for non-comment only.
    /// </summary>
    [Serializable]
    public class ModuleItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleItem"/> class.
        /// </summary>
        /// <param name="expression">EPL</param>
        /// <param name="commentOnly">true if the statement consists only of comments or whitespace</param>
        /// <param name="lineNumber">line number</param>
        /// <param name="charPosStart">character position of start of segment</param>
        /// <param name="charPosEnd">character position of end of segment</param>
        public ModuleItem(
            string expression,
            bool commentOnly,
            int lineNumber,
            int charPosStart,
            int charPosEnd)
        {
            Expression = expression;
            IsCommentOnly = commentOnly;
            LineNumber = lineNumber;
            CharPosStart = charPosStart;
            CharPosEnd = charPosEnd;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleItem"/> class.
        /// </summary>
        /// <param name="model">The statement object model.</param>
        /// <param name="isCommentOnly">if set to <c>true</c> [is comment only].</param>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="charPosStart">The character position start.</param>
        /// <param name="charPosEnd">The character position end.</param>
        public ModuleItem(EPStatementObjectModel model,
            bool isCommentOnly,
            int lineNumber,
            int charPosStart,
            int charPosEnd)
        {
            Model = model;
            IsCommentOnly = isCommentOnly;
            LineNumber = lineNumber;
            CharPosStart = charPosStart;
            CharPosEnd = charPosEnd;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleItem"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public ModuleItem(string expression)
        {
            Expression = expression;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleItem"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public ModuleItem(EPStatementObjectModel model)
        {
            Model = model;
        }

        /// <summary>Returns the EPL. </summary>
        /// <value>expression</value>
        public string Expression { get; set; }

        /// <summary>Returns true to indicate comments-only expression. </summary>
        /// <value>comments-only indicator</value>
        public bool IsCommentOnly { get; set; }

        /// <summary>Returns the line number of item. </summary>
        /// <value>item line num</value>
        public int LineNumber { get; set; }

        /// <summary>Returns item char position in line. </summary>
        /// <value>char position</value>
        public int CharPosStart { get; set; }

        /// <summary>Returns end position of character on line for the item. </summary>
        /// <value>position</value>
        public int CharPosEnd { get; set; }

        /// <summary>Returns the statement object model when provided</summary>
        /// <value>the statement object model</value>
        public EPStatementObjectModel Model { get; private set; }
    }
}