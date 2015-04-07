///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.dataflow;

namespace com.espertech.esper.dataflow.core
{
    public interface DataFlowConfigurationStateService
    {
        bool Exists(String savedConfigName);
        void Add(EPDataFlowSavedConfiguration epDataFlowSavedConfiguration);
        string[] SavedConfigNames { get; }
        EPDataFlowSavedConfiguration GetSavedConfig(String savedConfigName);
        Object RemovePrototype(String savedConfigName);
    }
}