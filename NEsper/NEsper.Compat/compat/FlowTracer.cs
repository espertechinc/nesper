///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Can be used to TRACE flow through a process.
    /// </summary>
    public class FlowTracer : IDisposable
    {
        [ThreadStatic]
        private static string _indent;

        private readonly ILog _log;
        private readonly string _id;
        private readonly string _saved;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowTracer"/> class.
        /// </summary>
        public FlowTracer()
            : this(DefaultLog, CurrentMethodName())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowTracer"/> class.
        /// </summary>
        /// <param name="log">The log.</param>
        public FlowTracer(ILog log)
            : this(log, CurrentMethodName())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowTracer"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        public FlowTracer(String id)
            : this(DefaultLog, id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowTracer"/> class.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="id">The id.</param>
        public FlowTracer(ILog log, string id)
        {
            if (String.IsNullOrEmpty(_indent))
            {
                _indent = "";
            }

            _id = id;
            _saved = _indent;
            _log = log;

            _indent = _indent + '>';
            _log.Debug("{0} Enter > {1}", _indent, _id);
        }

        private static string CurrentMethodName()
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            var stackFrame = stackTrace.GetFrame(2);
            return stackFrame.GetMethod().Name;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _log.Debug("{0} Leave > {1}", _indent, _id);
            _indent = _saved;
        }

        private static readonly ILog DefaultLog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    }
}
