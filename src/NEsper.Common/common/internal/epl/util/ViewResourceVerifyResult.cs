///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.view.access;

namespace com.espertech.esper.common.@internal.epl.util
{
    public class ViewResourceVerifyResult
    {
        private readonly ViewResourceDelegateDesc[] descriptors;
        private readonly FabricCharge fabricCharge;

        public ViewResourceVerifyResult(
            ViewResourceDelegateDesc[] descriptors,
            FabricCharge fabricCharge)
        {
            this.descriptors = descriptors;
            this.fabricCharge = fabricCharge;
        }

        public ViewResourceDelegateDesc[] Descriptors => descriptors;

        public FabricCharge FabricCharge => fabricCharge;
    }
} // end of namespace