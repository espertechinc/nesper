///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class SupportSelectorWithListEvent
    {
        public SupportSelectorWithListEvent(string selector)
        {
            Selector = selector;

            TheList = new List<string>();
            TheList.Add("1");
            TheList.Add("2");
            TheList.Add("3");
        }

        public string Selector { get; }

        public IList<string> TheList { get; }

        public string[] TheArray => TheList.ToArray();

        public IList<string> GetTheList()
        {
            return TheList;
        }

        public string[] GetTheArray()
        {
            return TheArray;
        }
        
        public SupportStringListEvent NestedMyEvent => new SupportStringListEvent(TheList);

        public static string[] ConvertToArray(IList<string> list)
        {
            return list.ToArray();
        }
    }
} // end of namespace