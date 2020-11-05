///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.mgr;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    public class FAFQueryMethodEnv
    {
        private readonly ContextManagementService contextManagementService;

        public FAFQueryMethodEnv(ContextManagementService contextManagementService)
        {
            this.contextManagementService = contextManagementService;
        }

        public ContextManagementService ContextManagementService {
            get => contextManagementService;
        }
    }
} // end of namespace