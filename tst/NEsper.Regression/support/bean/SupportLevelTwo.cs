///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportLevelTwo
    {
        public SupportLevelTwo(SupportLevelThree levelThree)
        {
            LevelThree = levelThree;
        }

        public SupportLevelThree LevelThree { get; }

        public string GetCustomLevelTwo(int val)
        {
            return "level2:" + val;
        }
    }
} // end of namespace