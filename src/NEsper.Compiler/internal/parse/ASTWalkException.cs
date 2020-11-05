///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

using Antlr4.Runtime;

namespace com.espertech.esper.compiler.@internal.parse
{
    /// <summary>
    ///     This exception is thrown to indicate a problem in statement creation.
    /// </summary>
    [Serializable]
    public class ASTWalkException : Exception
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="message">is the error message</param>
        private ASTWalkException(string message)
            : base(message)
        {
        }

        public ASTWalkException(
            string message,
            Exception cause)
            : base(message, cause)
        {
        }

        protected ASTWalkException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public static ASTWalkException From(
            string message,
            Exception ex)
        {
            return new ASTWalkException(message, ex);
        }

        public static ASTWalkException From(string message)
        {
            return new ASTWalkException(message);
        }

        public static ASTWalkException From(
            string message,
            string parseTreeTextMayHaveNoWhitespace)
        {
            return new ASTWalkException(message + " in text '" + parseTreeTextMayHaveNoWhitespace + "'");
        }

        public static ASTWalkException From(
            string message,
            CommonTokenStream tokenStream,
            RuleContext parseTree)
        {
            return new ASTWalkException(message + " in text '" + tokenStream.GetText(parseTree) + "'");
        }

        public static ASTWalkException From(
            string message,
            IToken token)
        {
            return new ASTWalkException(message + " in text '" + token.Text + "'");
        }
    }
} // end of namespace