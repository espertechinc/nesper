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
    public class SupportBeanVariantTwo
    {
        private readonly SupportBeanVariantOne.SupportBeanVariantOneInner inneritem;
    
        public SupportBeanVariantTwo()
        {
            Indexed = new int[] {10, 20, 30};
            Mapped = new Dictionary<String, String>();
            Mapped["a"] = "val2";
            inneritem = new SupportBeanVariantOne.SupportBeanVariantOneInner("i2");
        }

        public ISupportBaseAB P0 { get; private set; }

        public ISupportAImplSuperGImplPlus P1 { get; private set; }

        public LinkedList<object> P2 { get; private set; }

        public IList<object> P3 { get; private set; }

        public IList<object> P4 { get; private set; }

        public ICollection<object> P5 { get; private set; }

        public int[] Indexed { get; private set; }

        public int GetIndexArr(int index)
        {
            return Indexed[index];
        }

        public IDictionary<string, string> Mapped { get; private set; }

        public String GetMappedKey(String key)
        {
            return Mapped.Get(key);
        }

        public SupportBeanVariantOne.SupportBeanVariantOneInner Inneritem
        {
            get { return inneritem; }
        }
    }
}
