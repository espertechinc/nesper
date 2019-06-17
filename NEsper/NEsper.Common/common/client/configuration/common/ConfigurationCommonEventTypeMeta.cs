///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client.hook.type;
using com.espertech.esper.common.client.util;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Event representation metadata.
    /// </summary>
    [Serializable]
    public class ConfigurationCommonEventTypeMeta
    {
        private AvroSettingsConfig _avroSettings;
        private PropertyResolutionStyle _classPropertyResolutionStyle;
        private AccessorStyle _defaultAccessorStyle;
        private EventUnderlyingType _defaultEventRepresentation;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public ConfigurationCommonEventTypeMeta()
        {
            _classPropertyResolutionStyle = PropertyResolutionStyle.DEFAULT;
            _defaultAccessorStyle = AccessorStyle.NATIVE;
            _defaultEventRepresentation = EventUnderlyingType.GetDefault();
            _avroSettings = new AvroSettingsConfig();
        }

        /// <summary>
        ///     Returns the default accessor style, native unless changed.
        /// </summary>
        /// <value>style enum</value>
        public AccessorStyle DefaultAccessorStyle {
            get => _defaultAccessorStyle;
            set => _defaultAccessorStyle = value;
        }

        /// <summary>
        ///     Sets the property resolution style to use for resolving property names
        ///     of types.
        /// </summary>
        /// <value>style of property resolution</value>
        public PropertyResolutionStyle ClassPropertyResolutionStyle {
            set => _classPropertyResolutionStyle = value;
            get => _classPropertyResolutionStyle;
        }

        /// <summary>
        ///     Returns the default event representation.
        /// </summary>
        /// <value>setting</value>
        public EventUnderlyingType DefaultEventRepresentation {
            get => _defaultEventRepresentation;
            set => _defaultEventRepresentation = value;
        }

        /// <summary>
        ///     Returns the Avro settings.
        /// </summary>
        /// <value>avro settings</value>
        public AvroSettingsConfig AvroSettings {
            get => _avroSettings;
            set => _avroSettings = value;
        }

        /// <summary>
        ///     Avro settings.
        /// </summary>
        [Serializable]
        public class AvroSettingsConfig
        {
            private bool _enableAvro = true;
            private bool _enableNativeString = true;
            private bool _enableSchemaDefaultNonNull = true;
            private string _objectValueTypeWidenerFactoryClass;
            private string _typeRepresentationMapperClass;

            /// <summary>
            ///     Returns the indicator whether Avro support is enabled when available (true by default).
            /// </summary>
            /// <value>indicator</value>
            public bool IsEnableAvro {
                get => _enableAvro;
                set => _enableAvro = value;
            }

            /// <summary>
            ///     Returns indicator whether for String-type values to use the "avro.java.string=String" (true by default)
            /// </summary>
            /// <value>indicator</value>
            public bool IsEnableNativeString {
                get => _enableNativeString;
                set => _enableNativeString = value;
            }

            /// <summary>
            ///     Returns indicator whether generated schemas should assume non-null values (true by default)
            /// </summary>
            /// <value>indicator</value>
            public bool IsEnableSchemaDefaultNonNull {
                get => _enableSchemaDefaultNonNull;
                set => _enableSchemaDefaultNonNull = value;
            }

            /// <summary>
            ///     Returns class name of mapping provider that maps types to an Avro schema; a mapper should implement
            ///     <seealso cref="TypeRepresentationMapper" />(null by default, using default mapping)
            /// </summary>
            /// <value>class name</value>
            public string TypeRepresentationMapperClass {
                get => _typeRepresentationMapperClass;
                set => _typeRepresentationMapperClass = value;
            }

            /// <summary>
            ///     Returns the class name of widening provider that widens, coerces or transforms object values to an
            ///     Avro field value or record; a widener should implement <seealso cref="ObjectValueTypeWidenerFactory" />
            ///     (null by default, using default widening)
            /// </summary>
            /// <value>class name</value>
            public string ObjectValueTypeWidenerFactoryClass {
                get => _objectValueTypeWidenerFactoryClass;
                set => _objectValueTypeWidenerFactoryClass = value;
            }
        }
    }
} // end of namespace