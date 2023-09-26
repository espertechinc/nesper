///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.fabric;

namespace com.espertech.esper.common.@internal.context.aifactory.core
{
    public class StmtForgeMethodRegisterResult
    {
        private readonly string objectName;
        private readonly FabricCharge fabricCharge;

        public StmtForgeMethodRegisterResult(
            string objectName,
            FabricCharge fabricCharge)
        {
            this.objectName = objectName;
            this.fabricCharge = fabricCharge;
        }

        public string ObjectName => objectName;

        public FabricCharge FabricCharge => fabricCharge;
    }
} // end of namespace