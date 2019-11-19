///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.compat.collections
{
    public class Tuple<TA,TB>
    {
        public TA A { get; set; }
        public TB B { get; set; }
    }

    public class Tuple<TA,TB,TC>
    {
        public TA A { get; set; }
        public TB B { get; set; }
        public TC C { get; set; }
    }
}
