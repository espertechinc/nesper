///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat;

namespace com.espertech.esperio.message.fix
{
    /// <summary>
    /// Parses and performs very basic validation of a fix message.
    /// <para/>
    /// Validations are:
    /// <ul>
    /// <li>The message must be parsable as a fix message.</li>
    /// <li>The tags 8, 9, 35, 10 must exist</li>
    /// </ul>
    /// </summary>
    public class FixMsgParser
    {
        private const string soh = "\u0001";
    
        /// <summary>Parses a fix message. </summary>
        /// <param name="fixMsg">message to parse</param>
        /// <returns>map of tags</returns>
        /// <throws>FixMsgParserException if the parse failed</throws>
        public static IDictionary<String, String> Parse(String fixMsg)
        {
    		IDictionary<String, String> parsedMessage = InternalParse(fixMsg);
            Validate(parsedMessage, fixMsg);
            return parsedMessage;
    	}
    
        private static IDictionary<String, String> InternalParse(String fixMsg)
        {
            if (fixMsg == null)
            {
                throw new FixMsgUnrecognizableException("Unrecognizable fix message, message is a null string");
            }
    
            if (fixMsg.Length == 0)
            {
                throw new FixMsgUnrecognizableException("Unrecognizable fix message, message is a empty string");
            }
    
            IDictionary<String, String> parsedMessage = new HashMap<String, String>();
            StringTokenizer tok = new StringTokenizer(fixMsg, soh);
            if (tok.CountTokens() < 4)
            {
                throw new FixMsgUnrecognizableException("Unrecognizable fix message, number of tokens is less then 4", fixMsg);
            }
    
            while(tok.HasMoreTokens())
            {
                String filed = tok.NextToken();
                StringTokenizer innerTokens = new StringTokenizer(filed, "=");
                if (innerTokens.CountTokens() != 2)
                {
                     continue;
                }
                String tag = innerTokens.NextToken();
                String value = innerTokens.NextToken();
                parsedMessage.Put(tag, value);
            }
    
            return parsedMessage;
        }
    
        /// <summary>Validate the Fix message. </summary>
        /// <param name="fix">tags to validate</param>
        /// <param name="fixMsg">the message text in native form</param>
        /// <throws>FixMsgInvalidException if validation fails</throws>
        public static void Validate(IDictionary<String, String> fix, String fixMsg)
        {
            foreach (String required in new String[] {"8", "9", "35", "10"})
            {
                if (!fix.ContainsKey(required) || fix.Get(required).Equals(""))
                {
                    throw new FixMsgInvalidException("Failed to find tag " + required + " in fix message");
                }
            }
    
            String checksum = fix.Get("10");
            if (checksum == null)
            {
                throw new FixMsgInvalidException("Checksum validation failed, could not find tag '8'");
            }
    
            int deliveredChecksum;
            if (!Int32.TryParse(checksum, out deliveredChecksum)) {
                throw new FixMsgInvalidException("Invalid non-numeric checksum");
            }
    
            String checksumText = fixMsg.Substring(fixMsg.IndexOf("8="));
            int offset = checksumText.LastIndexOf("\u000110=");
            checksumText = checksumText.Substring(0, offset) + soh;
            int computedChecksum = FixMsgMarshaller.CheckSum(checksumText);
    
            if (computedChecksum != deliveredChecksum)
            {
                throw new FixMsgInvalidException("Invalid checksum, failed to validate checksum, found " + deliveredChecksum + " but should be " + computedChecksum);
            }
        }
    }
}
