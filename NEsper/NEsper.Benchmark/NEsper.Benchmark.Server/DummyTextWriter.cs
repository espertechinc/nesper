///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NEsper.Benchmark.Server
{
    public class DummyTextWriter : TextWriter
    {
        private TextWriter _proxyTextWriter;

        public DummyTextWriter(TextWriter proxyTextWriter)
        {
            _proxyTextWriter = proxyTextWriter;
        }

        #region Overrides of TextWriter

        public override Encoding Encoding
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        public override void WriteLine(string value)
        {
            _proxyTextWriter.WriteLine(value);
        }

        public override void WriteLine(string format, object arg0)
        {
            _proxyTextWriter.WriteLine(format, arg0);
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            _proxyTextWriter.WriteLine(format, arg0, arg1);
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            _proxyTextWriter.WriteLine(format, arg0, arg1, arg2);
        }

        public override void WriteLine(string format, params object[] arg)
        {
            _proxyTextWriter.WriteLine(format, arg);
        }
    }
}
