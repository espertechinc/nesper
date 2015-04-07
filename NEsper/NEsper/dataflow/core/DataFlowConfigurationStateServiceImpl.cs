///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.dataflow.core
{
    public class DataFlowConfigurationStateServiceImpl : DataFlowConfigurationStateService
    {
        private readonly IDictionary<String, EPDataFlowSavedConfiguration> _savedConfigs =
            new Dictionary<String, EPDataFlowSavedConfiguration>();

        public bool Exists(String savedConfigName)
        {
            return _savedConfigs.ContainsKey(savedConfigName);
        }

        public void Add(EPDataFlowSavedConfiguration savedConfiguration)
        {
            _savedConfigs.Put(savedConfiguration.SavedConfigurationName, savedConfiguration);
        }

        public string[] SavedConfigNames
        {
            get
            {
                ICollection<String> names = _savedConfigs.Keys;
                return names.ToArray();
            }
        }

        public EPDataFlowSavedConfiguration GetSavedConfig(String savedConfigName)
        {
            return _savedConfigs.Get(savedConfigName);
        }

        public object RemovePrototype(String savedConfigName)
        {
            return _savedConfigs.Pluck(savedConfigName);
        }
    }
}