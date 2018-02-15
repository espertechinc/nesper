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
    public class SupportChainChildTwo
    {
        public SupportChainChildTwo(String text, int value)
        {
            _text = text;
            _value = value;
        }

        private readonly string _text;
        private readonly int _value;

        public string GetText()
        {
            return _text;
        }

        public int GetValue()
        {
            return _value;
        }
    }
}
