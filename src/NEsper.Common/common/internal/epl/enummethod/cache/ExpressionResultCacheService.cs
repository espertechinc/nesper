///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.threading.threadlocal;

namespace com.espertech.esper.common.@internal.epl.enummethod.cache
{
    public class ExpressionResultCacheService
    {
        private readonly int _declareExprCacheSize;
        private readonly IThreadLocal<ExpressionResultCacheServiceHolder> _threadCache;

        public ExpressionResultCacheService(
            int declareExprCacheSize,
            IThreadLocalManager threadLocalManager)
        {
            _declareExprCacheSize = declareExprCacheSize;
            _threadCache = threadLocalManager.Create(
                () => new ExpressionResultCacheServiceHolder(declareExprCacheSize));
        }

        public ExpressionResultCacheForPropUnwrap AllocateUnwrapProp =>
            _threadCache.GetOrCreate().AllocateUnwrapProp;

        public ExpressionResultCacheForDeclaredExprLastValue AllocateDeclaredExprLastValue =>
            _threadCache.GetOrCreate().AllocateDeclaredExprLastValue;

        public ExpressionResultCacheForDeclaredExprLastColl AllocateDeclaredExprLastColl =>
            _threadCache.GetOrCreate().AllocateDeclaredExprLastColl;

        public ExpressionResultCacheForEnumerationMethod AllocateEnumerationMethod =>
            _threadCache.GetOrCreate().AllocateEnumerationMethod;

        public bool IsDeclaredExprCacheEnabled => _declareExprCacheSize > 0;
    }
}