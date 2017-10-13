///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.core.service
{
    public class ExpressionResultCacheEntry<TReference, TResult>
    {
        public TReference Reference { get; set; }
        public TResult Result { get; set; }

        public ExpressionResultCacheEntry(TReference reference, TResult result)
        {
            Reference = reference;
            Result = result;
        }
    }
}