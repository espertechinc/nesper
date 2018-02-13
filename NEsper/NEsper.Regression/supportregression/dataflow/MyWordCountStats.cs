///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportregression.dataflow
{
    public class MyWordCountStats
    {
        public MyWordCountStats()
        {
            Lines = 0;
            Words = 0;
            Chars = 0;
        }

        public int Lines { get; private set; }

        public int Words { get; private set; }

        public int Chars { get; private set; }

        public void Add(int lines, int words, int chars)
        {
            Lines += lines;
            Words += words;
            Chars += chars;
        }

        public override String ToString()
        {
            return "WordCountStats{" +
                    "lines=" + Lines +
                    ", words=" + Words +
                    ", chars=" + Chars +
                    '}';
        }
    }
}
