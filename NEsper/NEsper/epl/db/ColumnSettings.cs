///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.db
{
    /// <summary>
    /// Column-level configuration settings are held in this immutable descriptor.
    /// </summary>
	public class ColumnSettings
	{
        private readonly ConfigurationDBRef.MetadataOriginEnum metadataOriginEnum;
        private readonly ConfigurationDBRef.ColumnChangeCaseEnum columnCaseConversionEnum;
        private readonly IDictionary<Type, Type> dataTypesMapping;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="metadataOriginEnum">defines how to obtain output columnn metadata</param>
        /// <param name="columnCaseConversionEnum">defines if to change case on output columns</param>
        /// <param name="dataTypesMapping">The data types mapping.</param>
        public ColumnSettings(ConfigurationDBRef.MetadataOriginEnum metadataOriginEnum,
                              ConfigurationDBRef.ColumnChangeCaseEnum columnCaseConversionEnum,
                              IDictionary<Type, Type> dataTypesMapping)
	    {
	        this.metadataOriginEnum = metadataOriginEnum;
	        this.columnCaseConversionEnum = columnCaseConversionEnum;
            this.dataTypesMapping = dataTypesMapping;
	    }

	    /// <summary>Returns the metadata orgin.</summary>
	    /// <returns>indicator how the engine obtains output column metadata</returns>
	    public ConfigurationDBRef.MetadataOriginEnum MetadataRetrievalEnum
	    {
            get { return metadataOriginEnum; }
	    }

	    /// <summary>Returns the change case policy.</summary>
	    /// <returns>indicator how the engine should change case on output columns</returns>
	    public ConfigurationDBRef.ColumnChangeCaseEnum ColumnCaseConversionEnum
	    {
            get { return columnCaseConversionEnum; }
	    }

        /// <summary>
        /// Gets the data types mapping.
        /// </summary>
        /// <value>The data types mapping.</value>
        public IDictionary<Type, Type> DataTypesMapping
        {
            get { return dataTypesMapping; }
        }
	}
} // End of namespace
