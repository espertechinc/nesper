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
    public sealed class Tracer : IDisposable
    {
        private readonly ILog _log;
        private readonly string _context;

        public Tracer(ILog log, string context)
        {
            _context = context;
            _log = log;
            _log.Info(_context + " - starting");
        }

        public void Dispose()
        {
            _log.Info(_context + " - ending");
        }
    }
}
