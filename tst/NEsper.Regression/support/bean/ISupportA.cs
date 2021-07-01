///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public interface ISupportA : ISupportBaseAB
    {
        string A { get; }
    }

    public class ISupportAConstants
    {
        public const int VALUE_1 = 1;
        public const int VALUE_2 = 2;
    }
} // end of namespace