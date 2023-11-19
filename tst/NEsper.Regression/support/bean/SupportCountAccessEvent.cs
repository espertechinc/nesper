///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportCountAccessEvent
    {
        private static int _countGetterCalled;

        private readonly string _p00;

        public SupportCountAccessEvent(
            int id,
            string p00)
        {
            Id = id;
            _p00 = p00;
        }

        public int Id { get; }

        public static int GetAndResetCountGetterCalled()
        {
            var value = _countGetterCalled;
            _countGetterCalled = 0;
            return value;
        }

        public string GetP00()
        {
            _countGetterCalled++;
            return _p00;
        }
    }
} // end of namespace