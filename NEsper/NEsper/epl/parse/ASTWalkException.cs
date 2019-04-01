///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Antlr4.Runtime;

namespace com.espertech.esper.epl.parse
{
    /// <summary>
    /// This exception is thrown to indicate a problem in statement creation.
    /// </summary>
    [Serializable]
    public class ASTWalkException : Exception
    {
        public static ASTWalkException From(String message, Exception ex) {
            return new ASTWalkException(message, ex);
        }
    
        public static ASTWalkException From(String message) {
            return new ASTWalkException(message);
        }
    
        public static ASTWalkException From(String message, String parseTreeTextMayHaveNoWhitespace) {
            return new ASTWalkException(message + " in text '" + parseTreeTextMayHaveNoWhitespace + "'");
        }
    
        public static ASTWalkException From(String message, CommonTokenStream tokenStream, RuleContext parseTree) {
            return new ASTWalkException(message + " in text '" + tokenStream.GetText(parseTree) + "'");
        }
    
        public static ASTWalkException From(String message, IToken token) {
            return new ASTWalkException(message + " in text '" + token.Text + "'");
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="message">is the error message</param>
        private ASTWalkException(String message)
            : base(message)
        {
        }
    
        public ASTWalkException(String message, Exception cause)
            : base(message, cause)
        {
        }
    }
}
