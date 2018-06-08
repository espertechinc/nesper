///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.context;
using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

namespace com.espertech.esper.core.context.util
{
    public class ContextControllerSelectorUtil
    {
        public static InvalidContextPartitionSelector GetInvalidSelector(Type[] choice, ContextPartitionSelector selector)
        {
            return GetInvalidSelector(choice, selector, false);
        }

        public static InvalidContextPartitionSelector GetInvalidSelector(Type[] choice, ContextPartitionSelector selector, bool isNested)
        {
            var expected = new LinkedHashSet<String>();
            expected.Add(typeof(ContextPartitionSelectorAll).Name);
            if (!isNested)
            {
                expected.Add(typeof(ContextPartitionSelectorFiltered).Name);
            }
            expected.Add(typeof(ContextPartitionSelectorById).Name);
            for (int i = 0; i < choice.Length; i++)
            {
                expected.Add(choice[i].Name);
            }
            String expectedList = CollectionUtil.ToString(expected);
            String receivedClass = selector.GetType().GetCleanName();
            String message = "Invalid context partition selector, expected an implementation class of any of [" + expectedList + "] interfaces but received " + receivedClass;
            return new InvalidContextPartitionSelector(message);
        }
    }
}
