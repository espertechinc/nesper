///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.core.service
{
    public class ExpressionResultCacheServiceHolder
    {
        private readonly int _declareExprCacheSize;

        private ExpressionResultCacheForPropUnwrap _propUnwrap;
        private ExpressionResultCacheForDeclaredExprLastValue _declaredExprLastValue;
        private ExpressionResultCacheForDeclaredExprLastColl _declaredExprLastColl;
        private ExpressionResultCacheForEnumerationMethod _enumerationMethod;

        public ExpressionResultCacheServiceHolder(int declareExprCacheSize)
        {
            this._declareExprCacheSize = declareExprCacheSize;
        }

        public ExpressionResultCacheForPropUnwrap GetAllocateUnwrapProp()
        {
            return _propUnwrap ?? (_propUnwrap = new ExpressionResultCacheForPropUnwrapImpl());
        }

        public ExpressionResultCacheForDeclaredExprLastValue GetAllocateDeclaredExprLastValue()
        {
            if (_declaredExprLastValue == null)
            {
                if (_declareExprCacheSize < 1)
                {
                    _declaredExprLastValue = new ExpressionResultCacheForDeclaredExprLastValueNone();
                }
                else if (_declareExprCacheSize < 2)
                {
                    _declaredExprLastValue = new ExpressionResultCacheForDeclaredExprLastValueSingle();
                }
                else
                {
                    _declaredExprLastValue = new ExpressionResultCacheForDeclaredExprLastValueMulti(_declareExprCacheSize);
                }
            }
            return _declaredExprLastValue;
        }

        public ExpressionResultCacheForDeclaredExprLastColl GetAllocateDeclaredExprLastColl()
        {
            return _declaredExprLastColl ?? (_declaredExprLastColl = new ExpressionResultCacheForDeclaredExprLastCollImpl());
        }

        public ExpressionResultCacheForEnumerationMethod GetAllocateEnumerationMethod()
        {
            return _enumerationMethod ?? (_enumerationMethod = new ExpressionResultCacheForEnumerationMethodImpl());
        }
    }
} // end of namespace
