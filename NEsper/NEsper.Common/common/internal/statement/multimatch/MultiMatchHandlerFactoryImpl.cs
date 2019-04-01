///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.statement.multimatch
{
    public class MultiMatchHandlerFactoryImpl : MultiMatchHandlerFactory
    {
        private readonly bool isSubselectPreeval;

        public MultiMatchHandlerFactoryImpl(bool isSubselectPreeval)
        {
            this.isSubselectPreeval = isSubselectPreeval;
        }

        public MultiMatchHandler Make(bool hasSubselect, bool needDedup)
        {
            if (!hasSubselect) {
                if (!needDedup) {
                    return MultiMatchHandlerNoSubqueryNoDedup.INSTANCE;
                }

                return MultiMatchHandlerNoSubqueryWDedup.INSTANCE;
            }

            if (!needDedup) {
                if (isSubselectPreeval) {
                    return MultiMatchHandlerSubqueryPreevalNoDedup.INSTANCE;
                }

                return MultiMatchHandlerSubqueryPostevalNoDedup.INSTANCE;
            }

            return new MultiMatchHandlerSubqueryWDedup(isSubselectPreeval);
        }
    }
} // end of namespace