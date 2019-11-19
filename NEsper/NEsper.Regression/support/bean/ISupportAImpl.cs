///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class ISupportAImpl : ISupportA
    {
        public ISupportAImpl(
            string valueA,
            string valueBaseAB)
        {
            A = valueA;
            BaseAB = valueBaseAB;
        }

        public string A { get; }

        public string BaseAB { get; }
    }
} // end of namespace