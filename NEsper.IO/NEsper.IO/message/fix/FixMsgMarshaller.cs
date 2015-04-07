///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.client;

namespace com.espertech.esperio.message.fix
{
    /// <summary>Marshaller for Fix message. </summary>
    public class FixMsgMarshaller
    {
        private const string DEFAULT_FIX_VERSION = "FIX4.2";
        private static readonly String soh;
        private static String fixVersion;

        static FixMsgMarshaller()
        {
            soh = "\u0001";
        }

        /// <summary>Marshals a fix event. </summary>
        /// <param name="event">the event to marshal</param>
        /// <returns>marshalled fix message</returns>
        public static String MarshalFix(EventBean @event)
        {
            if (fixVersion == null) {
                String fixSystemProperty = Environment.GetEnvironmentVariable("FIX_VERSION");
                fixVersion = fixSystemProperty ?? DEFAULT_FIX_VERSION;
            }

            var writer = new StringWriter();

            // as a performance enhancement one could use the EventPropertyGetter here
            String delimiter = "";
            foreach (String property in @event.EventType.PropertyNames) {
                Object value = @event.Get(property);
                if (value == null) {
                    continue;
                }
                String valueText = value.ToString();
                writer.Write(delimiter);
                writer.Write(property);
                writer.Write('=');
                writer.Write(valueText);
                delimiter = soh;
            }
            writer.Write(soh);
            var fixMsgText = writer.ToString();

            // write fix body
            writer = new StringWriter();

            writer.Write("8=");
            writer.Write(fixVersion);
            writer.Write(soh);

            writer.Write("9=");
            writer.Write(Convert.ToString(fixMsgText.Length + 1));
            writer.Write(soh);

            writer.Write(fixMsgText);
            var fixMsgBody = writer.ToString();

            // write complete fix message text
            writer = new StringWriter();
            writer.Write(fixMsgBody);
            writer.Write("10=");
            int checksum = CheckSum(fixMsgBody);
            writer.Write(checksum.ToString("000"));
            writer.Write(soh);

            return writer.ToString();
        }

        /// <summary>Compute a checksum of a fix message. </summary>
        /// <param name="s">fix message</param>
        /// <returns>checksum</returns>
        protected static int CheckSum(String s)
        {
            int sum = 0;
            for (int i = 0; i < s.Length; i++) {
                sum += s[i];
            }
            return (sum + 1)%256;
        }
    }
}