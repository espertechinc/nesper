///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    public class SupportBeanVariantOne
    {
        private readonly ISupportB p0;
        private readonly ISupportAImplSuperG p1;

        public SupportBeanVariantOne()
        {
            Indexed = new[] { 1, 2, 3 };
            Mapped = new Dictionary<string, string>();
            Mapped.Put("a", "val1");
            Inneritem = new SupportBeanVariantOneInner("i1");
        }

        public ISupportB P0 => new ISupportABCImpl("a", "b", "baseAB", "c");

        public ISupportAImplSuperG P1 => new ISupportAImplSuperGImpl("g", "a", "baseAB");

        public LinkedList<object> P2 { get; }

        public IList<object> P3 { get; }

        public ICollection<object> P4 { get; }

        public IList<object> P5 { get; }

        public int[] Indexed { get; }

        public IDictionary<string, string> Mapped { get; }

        public SupportBeanVariantOneInner Inneritem { get; }

        public int GetIndexArr(int index)
        {
            return Indexed[index];
        }

        public string GetMappedKey(string key)
        {
            return Mapped.Get(key);
        }

        public class SupportBeanVariantOneInner
        {
            public SupportBeanVariantOneInner(string val)
            {
                Val = val;
            }

            public string Val { get; }
        }
    }
} // end of namespace
