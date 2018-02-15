///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportregression.bean
{
    [Serializable]
    public class ISupportBImpl : ISupportB
    {
        public virtual String B
        {
            get { return _valueB; }
        }

        public virtual String BaseAB
        {
            get { return _valueBaseAB; }
        }

        private readonly String _valueB;
        private readonly String _valueBaseAB;

        public ISupportBImpl(String valueB, String valueBaseAB)
        {
            _valueB = valueB;
            _valueBaseAB = valueBaseAB;
        }

        public override String ToString()
        {
            return "ISupportBImpl{" +
                   "valueB='" + _valueB + '\'' +
                   ", valueBaseAB='" + _valueBaseAB + '\'' +
                   '}';
        }
    }
}