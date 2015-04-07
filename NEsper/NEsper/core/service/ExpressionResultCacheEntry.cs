///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.core.service
{
    public class ExpressionResultCacheEntry<TReference, TResult>
    {
        public ExpressionResultCacheEntry(TReference reference, TResult result)
        {
            Reference = reference;
            Result = result;
        }

        public TReference Reference { get; private set; }

        public TResult Result { get; private set; }
    }
}