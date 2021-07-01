///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportLevelOne
    {
        public SupportLevelOne(SupportLevelTwo levelTwo)
        {
            LevelTwo = levelTwo;
        }

        public SupportLevelTwo LevelTwo { get; }

        public string GetCustomLevelOne(int val)
        {
            return "level1:" + val;
        }
    }
} // end of namespace