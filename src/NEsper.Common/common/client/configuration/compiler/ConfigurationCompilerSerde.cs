///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.client.configuration.compiler
{
	/// <summary>
	///     Serialization and Deserialization options.
	/// </summary>
	[Serializable]
    public class ConfigurationCompilerSerde
    {
        private bool _enableExtendedBuiltin = true;
        private bool _enableExternalizable;
        private bool _enableSerializable;
        private bool _enableSerializationFallback;
        private IList<string> _serdeProviderFactories = new List<string>(2);

        /// <summary>
        ///     Returns indicator whether the runtime provides the serde for extended builtin classes (see doc).
        /// </summary>
        /// <value>indicator</value>
        public bool IsEnableExtendedBuiltin {
            get => _enableExtendedBuiltin;
            set => _enableExtendedBuiltin = value;
        }

        /// <summary>
        ///     Returns indicator whether the runtime considers the Serializable attribute for serializing types
        /// </summary>
        /// <value>indicator</value>
        public bool IsEnableSerializable {
            get => _enableSerializable;
            set => _enableSerializable = value;
        }

        /// <summary>
        /// This exists for API compatibility and has no meaning in .NET.  To be removed.
        /// </summary>
        /// <value>indicator</value>
        public bool IsEnableExternalizable {
            get => _enableExternalizable;
            set => _enableExternalizable = value;
        }

        /// <summary>
        ///     Returns currently-registered serde provider factories.
        ///     Each entry is the fully-qualified class name of the serde provider factory.
        /// </summary>
        /// <value>serde provider factory class names</value>
        public IList<string> SerdeProviderFactories {
            get => _serdeProviderFactories;
            set => _serdeProviderFactories = value;
        }

        /// <summary>
        ///     Returns indicator whether the runtime, for types for which no other serde is available,
        ///     falls back to using JVM serialization. Fallback does not check whether the type actually implements Serializable.
        /// </summary>
        /// <value>indicator</value>
        public bool IsEnableSerializationFallback {
            get => _enableSerializationFallback;
            set => _enableSerializationFallback = value;
        }

        /// <summary>
        ///     Add a serde provider factory. Provide the fully-qualified class name of the serde provider factory.
        /// </summary>
        /// <param name="className">serde provider factory class name</param>
        public void AddSerdeProviderFactory(string className)
        {
            _serdeProviderFactories.Add(className);
        }
    }
} // end of namespace