///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.script
{
    public class ScriptCompilationException : ExprValidationException
    {
#if NETFRAMEWORK
        /// <summary>
        /// Gets or sets the compiler errors.
        /// </summary>
        /// <value>The compiler errors.</value>
        public ICollection<CompilerError> CompilerErrors { get; set; }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptCompilationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ScriptCompilationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptCompilationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ScriptCompilationException(string message, Exception innerException) : base(message, innerException)
        {
        }

#if NETFRAMEWORK
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptCompilationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="compilerErrors">The compiler errors.</param>
        public ScriptCompilationException(string message, ICollection<CompilerError> compilerErrors) : base(message)
        {
            CompilerErrors = compilerErrors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptCompilationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="compilerErrors">The compiler errors.</param>
        public ScriptCompilationException(string message, Exception innerException, ICollection<CompilerError> compilerErrors) : base(message, innerException)
        {
            CompilerErrors = compilerErrors;
        }
#endif
    }
}
