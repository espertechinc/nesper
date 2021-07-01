///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration.common;

namespace com.espertech.esper.common.@internal.epl.historical.database.core
{
    /// <summary>
    ///     Column-level configuration settings are held in this immutable descriptor.
    /// </summary>
    public class ColumnSettings
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="metadataOriginEnum">defines how to obtain output columnn metadata</param>
        /// <param name="columnCaseConversionEnum">defines if to change case on output columns</param>
        /// <param name="dataTypesMapping">The data types mapping.</param>
        public ColumnSettings(
            MetadataOriginEnum metadataOriginEnum,
            ColumnChangeCaseEnum columnCaseConversionEnum,
            IDictionary<Type, Type> dataTypesMapping)
        {
            MetadataRetrievalEnum = metadataOriginEnum;
            ColumnCaseConversionEnum = columnCaseConversionEnum;
            DataTypesMapping = dataTypesMapping;
        }

        /// <summary>Returns the metadata orgin.</summary>
        /// <returns>indicator how the engine obtains output column metadata</returns>
        public MetadataOriginEnum MetadataRetrievalEnum { get; }

        /// <summary>Returns the change case policy.</summary>
        /// <returns>indicator how the engine should change case on output columns</returns>
        public ColumnChangeCaseEnum ColumnCaseConversionEnum { get; }

        /// <summary>
        ///     Gets the data types mapping.
        /// </summary>
        /// <value>The data types mapping.</value>
        public IDictionary<Type, Type> DataTypesMapping { get; }
    }
} // End of namespace