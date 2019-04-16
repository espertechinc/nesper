///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.dataflow.core
{
    public class DataFlowConfigurationStateServiceImpl : DataFlowConfigurationStateService
    {
        private readonly IDictionary<string, EPDataFlowSavedConfiguration> savedConfigs = new Dictionary<string, EPDataFlowSavedConfiguration>();

        public bool Exists(string savedConfigName)
        {
            return savedConfigs.ContainsKey(savedConfigName);
        }

        public void Add(EPDataFlowSavedConfiguration savedConfiguration)
        {
            savedConfigs.Put(savedConfiguration.SavedConfigurationName, savedConfiguration);
        }

        public string[] SavedConfigNames {
            get { return savedConfigs.Keys.ToArray(); }
        }

        public EPDataFlowSavedConfiguration GetSavedConfig(string savedConfigName)
        {
            return savedConfigs.Get(savedConfigName);
        }

        object DataFlowConfigurationStateService.RemovePrototype(string savedConfigName)
        {
            return RemovePrototype(savedConfigName);
        }

        public EPDataFlowSavedConfiguration RemovePrototype(string savedConfigName)
        {
            return savedConfigs.Delete(savedConfigName);
        }
    }
} // end of namespace