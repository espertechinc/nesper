///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBeanVariantTwo
    {
        public SupportBeanVariantTwo()
        {
            Indexed = new[] {10, 20, 30};
            Mapped = new Dictionary<string, string>();
            Mapped.Put("a", "val2");
            Inneritem = new SupportBeanVariantOne.SupportBeanVariantOneInner("i2");
        }

        public ISupportBaseAB P0 { get; }

        public ISupportAImplSuperGImplPlus P1 { get; }

        public LinkedList<object> P2 { get; }

        public IList<object> P3 { get; }

        public IList<object> P4 { get; }

        public ICollection<object> P5 { get; }

        public int[] Indexed { get; }

        public IDictionary<string, string> Mapped { get; }

        public SupportBeanVariantOne.SupportBeanVariantOneInner Inneritem { get; }

        public int GetIndexArr(int index)
        {
            return Indexed[index];
        }

        public string GetMappedKey(string key)
        {
            return Mapped.Get(key);
        }
    }
} // end of namespace