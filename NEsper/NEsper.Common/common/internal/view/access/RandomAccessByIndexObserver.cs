///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.view.access
{
    /// <summary>For indicating that the collection has been updated. </summary>
    public interface RandomAccessByIndexObserver
    {
        /// <summary>Callback to indicate an Update </summary>
        /// <param name="randomAccessByIndex">is the collection</param>
        void Updated(RandomAccessByIndex randomAccessByIndex);
    }

    public class ProxyRandomAccessByIndexObserver : RandomAccessByIndexObserver
    {
        public Action<RandomAccessByIndex> UpdatedFunc { get; set; }

        public void Updated(RandomAccessByIndex randomAccessByIndex)
        {
            UpdatedFunc.Invoke(randomAccessByIndex);
        }
    }
}