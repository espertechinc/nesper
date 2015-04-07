///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.dataflow
{
    /// <summary>A data flow configuration is just a configuration name, a data flow name and an instantiation options object. </summary>
    [Serializable]
    public class EPDataFlowSavedConfiguration
    {
        /// <summary>Ctor. </summary>
        /// <param name="savedConfigurationName">name of saved configuration</param>
        /// <param name="dataflowName">data flow name</param>
        /// <param name="options">options object</param>
        public EPDataFlowSavedConfiguration(String savedConfigurationName,
                                            String dataflowName,
                                            EPDataFlowInstantiationOptions options)
        {
            SavedConfigurationName = savedConfigurationName;
            DataflowName = dataflowName;
            Options = options;
        }

        /// <summary>Configuation name. </summary>
        /// <value>name</value>
        public string SavedConfigurationName { get; private set; }

        /// <summary>Data flow name. </summary>
        /// <value>data flow name</value>
        public string DataflowName { get; private set; }

        /// <summary>Data flow instantiation options. </summary>
        /// <value>options</value>
        public EPDataFlowInstantiationOptions Options { get; private set; }
    }
}