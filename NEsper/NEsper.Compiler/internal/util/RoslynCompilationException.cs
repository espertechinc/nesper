///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Immutable;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

using Microsoft.CodeAnalysis;

namespace com.espertech.esper.compiler.@internal.util
{
    public class RoslynCompilationException : EPException
    {
        private readonly ImmutableArray<Diagnostic> _diagnosticErrors;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoslynCompilationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <exception cref="NotImplementedException"></exception>
        public RoslynCompilationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoslynCompilationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="diagnosticErrors">The diagnostic errors.</param>
        public RoslynCompilationException(
            string message,
            ImmutableArray<Diagnostic> diagnosticErrors) : base(message)
        {
            _diagnosticErrors = diagnosticErrors;
        }

        public override string ToString()
        {
            var diagnosticErrorMsg = _diagnosticErrors.RenderAny();
            return $"{base.ToString()}, {nameof(_diagnosticErrors)}: {diagnosticErrorMsg}";
        }
    }
}