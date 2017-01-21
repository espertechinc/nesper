///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.collection
{
    /// <summary>
	/// Utility for reading and transforming a source event iterator.
	/// Works with a <see cref="TransformEventMethod"/> as the transformation method.
	/// </summary>

    public class TransformEventUtil
    {
        /// <summary>
        /// Transforms the enumerator using the transform method supplied.
        /// </summary>
        /// <param name="sourceEnum">The source enum.</param>
        /// <param name="transformEventMethod">The transform event method.</param>
        /// <returns></returns>
        public static IEnumerator<EventBean> Transform(IEnumerator<EventBean> sourceEnum, Func<EventBean, EventBean> transformEventMethod)
        {
            while (sourceEnum.MoveNext())
            {
                yield return transformEventMethod(sourceEnum.Current);
            }
        }
    }
} // End of namespace
