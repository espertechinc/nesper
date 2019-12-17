///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat.datetime
{
    public interface DateTimeParser
    {
        DateTimeEx Parse(string dateTimeInputText);
    }

    public class ProxyDateTimeParser : DateTimeParser
    {
        public Func<string, DateTimeEx> ProcParse;

        public ProxyDateTimeParser()
        {
        }

        public ProxyDateTimeParser(Func<string, DateTimeEx> procParse)
        {
            ProcParse = procParse;
        }

        public DateTimeEx Parse(string dateTimeInputText)
        {
            if (ProcParse == null) {
                throw new NotImplementedException();
            }

            return ProcParse.Invoke(dateTimeInputText);
        }
    }
}
