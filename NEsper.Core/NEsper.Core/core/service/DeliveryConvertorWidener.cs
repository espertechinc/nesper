///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.util;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Implementation of a convertor for column results that renders the result as an object array itself.
    /// </summary>
    public class DeliveryConvertorWidener : DeliveryConvertor
    {
        private readonly TypeWidener[] _wideners;

        public DeliveryConvertorWidener(TypeWidener[] wideners)
        {
            _wideners = wideners;
        }

        #region DeliveryConvertor Members

        public Object[] ConvertRow(Object[] columns)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                if (_wideners[i] == null)
                {
                    continue;
                }
                columns[i] = _wideners[i].Invoke(columns[i]);
            }
            return columns;
        }

        #endregion
    }
}