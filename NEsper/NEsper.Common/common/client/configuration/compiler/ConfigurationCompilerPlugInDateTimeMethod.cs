///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.configuration.compiler
{
    /// <summary>
    ///     Configuration information for plugging in a custom date-time-method.
    /// </summary>
    [Serializable]
    public class ConfigurationCompilerPlugInDateTimeMethod
    {
        private string _forgeClassName;
        private string _name;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public ConfigurationCompilerPlugInDateTimeMethod()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="name">of the date-time method</param>
        /// <param name="forgeClassName">the name of the date-time method forge factory class</param>
        public ConfigurationCompilerPlugInDateTimeMethod(
            string name,
            string forgeClassName)
        {
            _name = name;
            _forgeClassName = forgeClassName;
        }

        /// <summary>
        ///     Returns the date-time method name.
        /// </summary>
        /// <value>name</value>
        public string Name {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        ///     Returns the class name of the date-time method forge factory class.
        /// </summary>
        /// <value>class name</value>
        public string ForgeClassName {
            get => _forgeClassName;
            set => _forgeClassName = value;
        }

        protected bool Equals(ConfigurationCompilerPlugInDateTimeMethod other)
        {
            return _forgeClassName == other._forgeClassName && _name == other._name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return Equals((ConfigurationCompilerPlugInDateTimeMethod) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_forgeClassName, _name);
        }
    }
} // end of namespace