///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


/*
 * Created on Apr 23, 2006
 *
 */

using System.Collections.Generic;
using System.IO;
using com.espertech.esper.compat.logging;

namespace NEsper.Examples.Transaction.sim
{
    /** Subclass to output events in your preferred format.
     * 
     * @author Hans Gilde, Thomas Bernhardt
     *
     */
    public class PrinterOutputStream : OutputStream
    {
        private TextWriter os;

        public PrinterOutputStream(Stream os)
        {
            this.os = new StreamWriter(os);
        }

        public PrinterOutputStream(TextWriter os)
        {
            this.os = os;
        }

        public void Output(IList<TxnEventBase> bucket)
        {
            Log.Info(".output Start of bucket, " + bucket.Count + " items");
            foreach (TxnEventBase eventBean in bucket)
            {
                os.WriteLine(eventBean.ToString());
            }
            Log.Info(".output End of bucket");
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
