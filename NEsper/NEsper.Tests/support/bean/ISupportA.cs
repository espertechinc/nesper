///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.support.bean
{
    public interface ISupportA : ISupportBaseAB
    {
        String A { get; }
    }

    public class ISupportAConst
    {
        public const int VALUE_1 = 1;
        public const int VALUE_2 = 2;
    }
}
