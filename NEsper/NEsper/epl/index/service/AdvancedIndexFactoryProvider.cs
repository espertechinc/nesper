///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.index.service
{
    public interface AdvancedIndexFactoryProvider
    {
        EventAdvancedIndexProvisionDesc ValidateEventIndex(
            string indexName, string indexTypeName, ExprNode[] columns, ExprNode[] parameters);
        AdvancedIndexConfigContextPartition ValidateConfigureFilterIndex(
            string indexName, string indexTypeName, ExprNode[] parameters, ExprValidationContext validationContext);
    }
} // end of namespace
