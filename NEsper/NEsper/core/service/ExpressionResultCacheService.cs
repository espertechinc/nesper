///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.core.service
{
    public class ExpressionResultCacheService
    {
        private readonly int _declareExprCacheSize;
        private readonly IThreadLocal<ExpressionResultCacheServiceHolder> _threadCache;

        public ExpressionResultCacheService(int declareExprCacheSize, IThreadLocalManager threadLocalManager)
        {
            _declareExprCacheSize = declareExprCacheSize;
            _threadCache = threadLocalManager.Create<ExpressionResultCacheServiceHolder>(
                () => new ExpressionResultCacheServiceHolder(declareExprCacheSize));
        }

        public ExpressionResultCacheForPropUnwrap AllocateUnwrapProp
        {
            get { return _threadCache.GetOrCreate().GetAllocateUnwrapProp(); }
        }

        public ExpressionResultCacheForDeclaredExprLastValue AllocateDeclaredExprLastValue
        {
            get { return _threadCache.GetOrCreate().GetAllocateDeclaredExprLastValue(); }
        }

        public ExpressionResultCacheForDeclaredExprLastColl AllocateDeclaredExprLastColl
        {
            get { return _threadCache.GetOrCreate().GetAllocateDeclaredExprLastColl(); }
        }

        public ExpressionResultCacheForEnumerationMethod AllocateEnumerationMethod
        {
            get { return _threadCache.GetOrCreate().GetAllocateEnumerationMethod(); }
        }

        public bool IsDeclaredExprCacheEnabled
        {
            get { return _declareExprCacheSize > 0; }
        }
    }
}