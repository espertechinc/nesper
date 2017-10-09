///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.supportregression.bean
{
    [Serializable]
    public class SupportBeanVariantOne
    {
        public SupportBeanVariantOne()
        {
            Indexed = new[] {1, 2, 3};
            Mapped = new Dictionary<String, String>();
            Mapped["a"] = "val1";
            Inneritem = new SupportBeanVariantOneInner("i1");
        }

        public ISupportB P0
        {
            get { return new ISupportABCImpl("a", "b", "baseAB", "c"); }
        }

        public ISupportAImplSuperG P1
        {
            get { return new ISupportAImplSuperGImpl("g", "a", "baseAB"); }
        }

        public List<object> P2 { get; protected set; }

        public IList<object> P3 { get; protected set; }

        public ICollection<object> P4 { get; protected set; }

        public IList<object> P5 { get; protected set; }

        public int[] Indexed { get; protected set; }

        public int GetIndexArr(int index)
        {
            return Indexed[index];
        }

        public IDictionary<string, string> Mapped { get; private set; }

        public String GetMappedKey(String key)
        {
            return Mapped.Get(key);
        }

        public SupportBeanVariantOneInner Inneritem { get; private set; }

        public class SupportBeanVariantOneInner
        {
            public SupportBeanVariantOneInner(String val)
            {
                Val = val;
            }

            public string Val { get; private set; }
        }
    }
}
