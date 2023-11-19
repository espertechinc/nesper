///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// Represents an EPL statement as part of a <seealso cref = "Module"/>.
    /// <para/>Character position start and end are only available for non-comment only.
    /// </summary>
    public class ModuleItem
    {
        private string expression;
        private EPStatementObjectModel model;
        private bool commentOnly;
        private int lineNumber;
        private int charPosStart;
        private int charPosEnd;
        private int lineNumberEnd;
        private int lineNumberContent;
        private int lineNumberContentEnd;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "expression">EPL</param>
        /// <param name = "commentOnly">true if the statement consists only of comments or whitespace</param>
        /// <param name = "lineNumber">line number</param>
        /// <param name = "charPosStart">character position of start of segment</param>
        /// <param name = "charPosEnd">character position of end of segment</param>
        /// <param name = "lineNumberEnd">line number of the line that ends the statement</param>
        /// <param name = "lineNumberContent">line number of the line that starts the statement excluding comments, or -1 if comments-only</param>
        /// <param name = "lineNumberContentEnd">line number of the line that ends the statement excluding comments, or -1 if comments-only</param>
        public ModuleItem(
            string expression,
            bool commentOnly,
            int lineNumber,
            int charPosStart,
            int charPosEnd,
            int lineNumberEnd,
            int lineNumberContent,
            int lineNumberContentEnd)
        {
            this.expression = expression;
            this.commentOnly = commentOnly;
            this.lineNumber = lineNumber;
            this.charPosStart = charPosStart;
            this.charPosEnd = charPosEnd;
            this.lineNumberEnd = lineNumberEnd;
            this.lineNumberContent = lineNumberContent;
            this.lineNumberContentEnd = lineNumberContentEnd;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "expression">expression</param>
        public ModuleItem(string expression)
        {
            this.expression = expression;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "model">statement object model</param>
        public ModuleItem(EPStatementObjectModel model)
        {
            this.model = model;
        }

        /// <summary>
        /// Returns true to indicate comments-only expression.
        /// </summary>
        /// <returns>comments-only indicator</returns>
        public bool IsCommentOnly => commentOnly;

        public string Expression {
            get => expression;

            set => expression = value;
        }

        public bool CommentOnly {
            set => commentOnly = value;
        }

        public int LineNumber {
            get => lineNumber;

            set => lineNumber = value;
        }

        public int LineNumberEnd {
            get => lineNumberEnd;

            set => lineNumberEnd = value;
        }

        public int CharPosStart {
            get => charPosStart;

            set => charPosStart = value;
        }

        public int CharPosEnd {
            get => charPosEnd;

            set => charPosEnd = value;
        }

        public EPStatementObjectModel Model => model;

        public int LineNumberContent {
            get => lineNumberContent;

            set => lineNumberContent = value;
        }

        public int LineNumberContentEnd {
            get => lineNumberContentEnd;

            set => lineNumberContentEnd = value;
        }
    }
} // end of namespace