///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.mgr
{
    public class ContextControllerSelectorUtil
    {
        public static InvalidContextPartitionSelector GetInvalidSelector(
            Type[] choice,
            ContextPartitionSelector selector)
        {
            return GetInvalidSelector(choice, selector, false);
        }

        public static InvalidContextPartitionSelector GetInvalidSelector(
            Type[] choice,
            ContextPartitionSelector selector,
            bool isNested)
        {
            var expected = new LinkedHashSet<string>();
            expected.Add(typeof(ContextPartitionSelectorAll).Name);
            if (!isNested) {
                expected.Add(typeof(ContextPartitionSelectorFiltered).Name);
            }

            expected.Add(typeof(ContextPartitionSelectorById).Name);
            for (var i = 0; i < choice.Length; i++) {
                expected.Add(choice[i].GetSimpleName());
            }

            var expectedList = CollectionUtil.ToString(expected);
            var receivedClass = selector.GetType().FullName;
            var message = "Invalid context partition selector, expected an implementation class of any of [" +
                          expectedList +
                          "] interfaces but received " +
                          receivedClass;
            return new InvalidContextPartitionSelector(message);
        }
    }
} // end of namespace