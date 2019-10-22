/*
 * CREDIT: Apache-Common-Text Version 1.2
 * Apache V2 licensed per https://commons.apache.org/proper/commons-text
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util.apachecommonstext
{
    /// <summary>
    /// Class holding various entity data for HTML and XML - generally for use with
    /// the LookupTranslator.
    /// </summary>
    /// <unknown>@since 1.0</unknown>
    public class EntityArrays
    {
        /// <summary>
        /// A Map&lt;string, string&gt; to to escape
        /// <a href="https://secure.wikimedia.org/wikipedia/en/wiki/ISO/IEC_8859-1">ISO-8859-1</a>characters to their named HTML 3.x equivalents.
        /// </summary>
        public static IDictionary<string, string> ISO8859_1_ESCAPE;

        /// <summary>
        /// Reverse of {@link #HTML40_EXTENDED_ESCAPE} for unescaping purposes.
        /// </summary>
        public static IDictionary<string, string> HTML40_EXTENDED_UNESCAPE;

        /// <summary>
        /// Reverse of {@link #ISO8859_1_ESCAPE} for unescaping purposes.
        /// </summary>
        public static IDictionary<string, string> ISO8859_1_UNESCAPE;

        /// <summary>
        /// A Map&lt;string, string&gt; to escape additional
        /// HTML 4.0 character entities.
        /// </summary>
        public static IDictionary<string, string> HTML40_EXTENDED_ESCAPE;

        /// <summary>
        /// A Map&lt;string, string&gt; to escape the basic XML and HTML
        /// character entities.
        /// <para /></summary>
        public static IDictionary<string, string> BASIC_ESCAPE;

        /// <summary>
        /// Reverse of <seealso cref="BASIC_ESCAPE" /> for unescaping purposes.
        /// </summary>
        public static IDictionary<string, string> BASIC_UNESCAPE;

        /// <summary>
        /// A Map&lt;string, string&gt; to escape the apostrophe character to
        /// its XML character entity.
        /// </summary>
        public static IDictionary<string, string> APOS_ESCAPE;

        /// <summary>
        /// Reverse of <seealso cref="APOS_ESCAPE" /> for unescaping purposes.
        /// </summary>
        public static IDictionary<string, string> APOS_UNESCAPE;

        /// <summary>
        /// A Map&lt;string, string&gt; to escape the control characters.
        /// <para />Namely: {@code \b \n \t \f \r}
        /// </summary>
        public static IDictionary<string, string> JAVA_CTRL_CHARS_ESCAPE;

        /// <summary>
        /// Reverse of <seealso cref="JAVA_CTRL_CHARS_ESCAPE" /> for unescaping purposes.
        /// </summary>
        public static IDictionary<string, string> JAVA_CTRL_CHARS_UNESCAPE;

        static EntityArrays()
        {
            Initialize_ISO8859_1_ESCAPE();
            Initialize_ISO8859_1_UNESCAPE();
            Initialize_HTML40_EXTENDED_ESCAPE();
            Initialize_HTML40_EXTENDED_UNESCAPE();
            Initialize_BASIC_ESCAPE();
            Initialize_BASIC_UNESCAPE();
            Initialize_APOS_ESCAPE();
            Initialize_APOS_UNESCAPE();
            Initialize_JAVA_CTRL_CHARS_ESCAPE();
            Initialize_JAVA_CTRL_CHARS_UNESCAPE();
        }

        static void Initialize_ISO8859_1_ESCAPE()
        {
            var initialMap = new Dictionary<string, string>();
            initialMap.Put("\u00A0", "&nbsp;"); // non-breaking space
            initialMap.Put("\u00A1", "&iexcl;"); // inverted exclamation mark
            initialMap.Put("\u00A2", "&cent;"); // cent sign
            initialMap.Put("\u00A3", "&pound;"); // pound sign
            initialMap.Put("\u00A4", "&curren;"); // currency sign
            initialMap.Put("\u00A5", "&yen;"); // yen sign = yuan sign
            initialMap.Put("\u00A6", "&brvbar;"); // broken bar = broken vertical bar
            initialMap.Put("\u00A7", "&sect;"); // section sign
            initialMap.Put("\u00A8", "&uml;"); // diaeresis = spacing diaeresis
            initialMap.Put("\u00A9", "&copy;"); // © - copyright sign
            initialMap.Put("\u00AA", "&ordf;"); // feminine ordinal indicator
            initialMap.Put("\u00AB", "&laquo;"); // left-pointing double angle quotation mark = left pointing guillemet
            initialMap.Put("\u00AC", "&not;"); // not sign
            initialMap.Put("\u00AD", "&shy;"); // soft hyphen = discretionary hyphen
            initialMap.Put("\u00AE", "&reg;"); // ® - registered trademark sign
            initialMap.Put("\u00AF", "&macr;"); // macron = spacing macron = overline = APL overbar
            initialMap.Put("\u00B0", "&deg;"); // degree sign
            initialMap.Put("\u00B1", "&plusmn;"); // plus-minus sign = plus-or-minus sign
            initialMap.Put("\u00B2", "&sup2;"); // superscript two = superscript digit two = squared
            initialMap.Put("\u00B3", "&sup3;"); // superscript three = superscript digit three = cubed
            initialMap.Put("\u00B4", "&acute;"); // acute accent = spacing acute
            initialMap.Put("\u00B5", "&micro;"); // micro sign
            initialMap.Put("\u00B6", "&para;"); // pilcrow sign = paragraph sign
            initialMap.Put("\u00B7", "&middot;"); // middle dot = Georgian comma = Greek middle dot
            initialMap.Put("\u00B8", "&cedil;"); // cedilla = spacing cedilla
            initialMap.Put("\u00B9", "&sup1;"); // superscript one = superscript digit one
            initialMap.Put("\u00BA", "&ordm;"); // masculine ordinal indicator
            initialMap.Put(
                "\u00BB",
                "&raquo;"); // right-pointing double angle quotation mark = right pointing guillemet
            initialMap.Put("\u00BC", "&frac14;"); // vulgar fraction one quarter = fraction one quarter
            initialMap.Put("\u00BD", "&frac12;"); // vulgar fraction one half = fraction one half
            initialMap.Put("\u00BE", "&frac34;"); // vulgar fraction three quarters = fraction three quarters
            initialMap.Put("\u00BF", "&iquest;"); // inverted question mark = turned question mark
            initialMap.Put("\u00C0", "&Agrave;"); // À - uppercase A, grave accent
            initialMap.Put("\u00C1", "&Aacute;"); // A - uppercase A, acute accent
            initialMap.Put("\u00C2", "&Acirc;"); // A - uppercase A, circumflex accent
            initialMap.Put("\u00C3", "&Atilde;"); // Ã - uppercase A, tilde
            initialMap.Put("\u00C4", "&Auml;"); // Ä - uppercase A, umlaut
            initialMap.Put("\u00C5", "&Aring;"); // Å - uppercase A, ring
            initialMap.Put("\u00C6", "&AElig;"); // Æ - uppercase AE
            initialMap.Put("\u00C7", "&Ccedil;"); // Ç - uppercase C, cedilla
            initialMap.Put("\u00C8", "&Egrave;"); // È - uppercase E, grave accent
            initialMap.Put("\u00C9", "&Eacute;"); // É - uppercase E, acute accent
            initialMap.Put("\u00CA", "&Ecirc;"); // Ê - uppercase E, circumflex accent
            initialMap.Put("\u00CB", "&Euml;"); // Ë - uppercase E, umlaut
            initialMap.Put("\u00CC", "&Igrave;"); // Ì - uppercase I, grave accent
            initialMap.Put("\u00CD", "&Iacute;"); // I - uppercase I, acute accent
            initialMap.Put("\u00CE", "&Icirc;"); // Î - uppercase I, circumflex accent
            initialMap.Put("\u00CF", "&Iuml;"); // I - uppercase I, umlaut
            initialMap.Put("\u00D0", "&ETH;"); // E - uppercase Eth, Icelandic
            initialMap.Put("\u00D1", "&Ntilde;"); // Ñ - uppercase N, tilde
            initialMap.Put("\u00D2", "&Ograve;"); // Ò - uppercase O, grave accent
            initialMap.Put("\u00D3", "&Oacute;"); // Ó - uppercase O, acute accent
            initialMap.Put("\u00D4", "&Ocirc;"); // Ô - uppercase O, circumflex accent
            initialMap.Put("\u00D5", "&Otilde;"); // Õ - uppercase O, tilde
            initialMap.Put("\u00D6", "&Ouml;"); // Ö - uppercase O, umlaut
            initialMap.Put("\u00D7", "&times;"); // multiplication sign
            initialMap.Put("\u00D8", "&Oslash;"); // Ø - uppercase O, slash
            initialMap.Put("\u00D9", "&Ugrave;"); // Ù - uppercase U, grave accent
            initialMap.Put("\u00DA", "&Uacute;"); // Ú - uppercase U, acute accent
            initialMap.Put("\u00DB", "&Ucirc;"); // Û - uppercase U, circumflex accent
            initialMap.Put("\u00DC", "&Uuml;"); // Ü - uppercase U, umlaut
            initialMap.Put("\u00DD", "&Yacute;"); // Y - uppercase Y, acute accent
            initialMap.Put("\u00DE", "&THORN;"); // Þ - uppercase THORN, Icelandic
            initialMap.Put("\u00DF", "&szlig;"); // ß - lowercase sharps, German
            initialMap.Put("\u00E0", "&agrave;"); // à - lowercase a, grave accent
            initialMap.Put("\u00E1", "&aacute;"); // á - lowercase a, acute accent
            initialMap.Put("\u00E2", "&acirc;"); // â - lowercase a, circumflex accent
            initialMap.Put("\u00E3", "&atilde;"); // ã - lowercase a, tilde
            initialMap.Put("\u00E4", "&auml;"); // ä - lowercase a, umlaut
            initialMap.Put("\u00E5", "&aring;"); // å - lowercase a, ring
            initialMap.Put("\u00E6", "&aelig;"); // æ - lowercase ae
            initialMap.Put("\u00E7", "&ccedil;"); // ç - lowercase c, cedilla
            initialMap.Put("\u00E8", "&egrave;"); // è - lowercase e, grave accent
            initialMap.Put("\u00E9", "&eacute;"); // é - lowercase e, acute accent
            initialMap.Put("\u00EA", "&ecirc;"); // ê - lowercase e, circumflex accent
            initialMap.Put("\u00EB", "&euml;"); // ë - lowercase e, umlaut
            initialMap.Put("\u00EC", "&igrave;"); // ì - lowercase i, grave accent
            initialMap.Put("\u00ED", "&iacute;"); // í - lowercase i, acute accent
            initialMap.Put("\u00EE", "&icirc;"); // î - lowercase i, circumflex accent
            initialMap.Put("\u00EF", "&iuml;"); // ï - lowercase i, umlaut
            initialMap.Put("\u00F0", "&eth;"); // ð - lowercase eth, Icelandic
            initialMap.Put("\u00F1", "&ntilde;"); // ñ - lowercase n, tilde
            initialMap.Put("\u00F2", "&ograve;"); // ò - lowercase o, grave accent
            initialMap.Put("\u00F3", "&oacute;"); // ó - lowercase o, acute accent
            initialMap.Put("\u00F4", "&ocirc;"); // ô - lowercase o, circumflex accent
            initialMap.Put("\u00F5", "&otilde;"); // õ - lowercase o, tilde
            initialMap.Put("\u00F6", "&ouml;"); // ö - lowercase o, umlaut
            initialMap.Put("\u00F7", "&divide;"); // division sign
            initialMap.Put("\u00F8", "&oslash;"); // ø - lowercase o, slash
            initialMap.Put("\u00F9", "&ugrave;"); // ù - lowercase u, grave accent
            initialMap.Put("\u00FA", "&uacute;"); // ú - lowercase u, acute accent
            initialMap.Put("\u00FB", "&ucirc;"); // û - lowercase u, circumflex accent
            initialMap.Put("\u00FC", "&uuml;"); // ü - lowercase u, umlaut
            initialMap.Put("\u00FD", "&yacute;"); // ý - lowercase y, acute accent
            initialMap.Put("\u00FE", "&thorn;"); // þ - lowercase thorn, Icelandic
            initialMap.Put("\u00FF", "&yuml;"); // ÿ - lowercase y, umlaut
            ISO8859_1_ESCAPE = initialMap;
        }

        static void Initialize_ISO8859_1_UNESCAPE()
        {
            ISO8859_1_UNESCAPE = Invert(ISO8859_1_ESCAPE);
        }

        static void Initialize_HTML40_EXTENDED_ESCAPE()
        {
            IDictionary<string, string> initialMap = new Dictionary<string, string>();
            // <!-- Latin Extended-B -->
            initialMap.Put("\u0192", "&fnof;"); // latin small f with hook = function= florin, U+0192 ISOtech -->
            // <!-- Greek -->
            initialMap.Put("\u0391", "&Alpha;"); // greek capital letter alpha, U+0391 -->
            initialMap.Put("\u0392", "&Beta;"); // greek capital letter beta, U+0392 -->
            initialMap.Put("\u0393", "&Gamma;"); // greek capital letter gamma,U+0393 ISOgrk3 -->
            initialMap.Put("\u0394", "&Delta;"); // greek capital letter delta,U+0394 ISOgrk3 -->
            initialMap.Put("\u0395", "&Epsilon;"); // greek capital letter epsilon, U+0395 -->
            initialMap.Put("\u0396", "&Zeta;"); // greek capital letter zeta, U+0396 -->
            initialMap.Put("\u0397", "&Eta;"); // greek capital letter eta, U+0397 -->
            initialMap.Put("\u0398", "&Theta;"); // greek capital letter theta,U+0398 ISOgrk3 -->
            initialMap.Put("\u0399", "&Iota;"); // greek capital letter iota, U+0399 -->
            initialMap.Put("\u039A", "&Kappa;"); // greek capital letter kappa, U+039A -->
            initialMap.Put("\u039B", "&Lambda;"); // greek capital letter lambda,U+039B ISOgrk3 -->
            initialMap.Put("\u039C", "&Mu;"); // greek capital letter mu, U+039C -->
            initialMap.Put("\u039D", "&Nu;"); // greek capital letter nu, U+039D -->
            initialMap.Put("\u039E", "&Xi;"); // greek capital letter xi, U+039E ISOgrk3 -->
            initialMap.Put("\u039F", "&Omicron;"); // greek capital letter omicron, U+039F -->
            initialMap.Put("\u03A0", "&Pi;"); // greek capital letter pi, U+03A0 ISOgrk3 -->
            initialMap.Put("\u03A1", "&Rho;"); // greek capital letter rho, U+03A1 -->
            // <!-- there is no Sigmaf, and no U+03A2 character either -->
            initialMap.Put("\u03A3", "&Sigma;"); // greek capital letter sigma,U+03A3 ISOgrk3 -->
            initialMap.Put("\u03A4", "&Tau;"); // greek capital letter tau, U+03A4 -->
            initialMap.Put("\u03A5", "&Upsilon;"); // greek capital letter upsilon,U+03A5 ISOgrk3 -->
            initialMap.Put("\u03A6", "&Phi;"); // greek capital letter phi,U+03A6 ISOgrk3 -->
            initialMap.Put("\u03A7", "&Chi;"); // greek capital letter chi, U+03A7 -->
            initialMap.Put("\u03A8", "&Psi;"); // greek capital letter psi,U+03A8 ISOgrk3 -->
            initialMap.Put("\u03A9", "&Omega;"); // greek capital letter omega,U+03A9 ISOgrk3 -->
            initialMap.Put("\u03B1", "&alpha;"); // greek small letter alpha,U+03B1 ISOgrk3 -->
            initialMap.Put("\u03B2", "&beta;"); // greek small letter beta, U+03B2 ISOgrk3 -->
            initialMap.Put("\u03B3", "&gamma;"); // greek small letter gamma,U+03B3 ISOgrk3 -->
            initialMap.Put("\u03B4", "&delta;"); // greek small letter delta,U+03B4 ISOgrk3 -->
            initialMap.Put("\u03B5", "&epsilon;"); // greek small letter epsilon,U+03B5 ISOgrk3 -->
            initialMap.Put("\u03B6", "&zeta;"); // greek small letter zeta, U+03B6 ISOgrk3 -->
            initialMap.Put("\u03B7", "&eta;"); // greek small letter eta, U+03B7 ISOgrk3 -->
            initialMap.Put("\u03B8", "&theta;"); // greek small letter theta,U+03B8 ISOgrk3 -->
            initialMap.Put("\u03B9", "&iota;"); // greek small letter iota, U+03B9 ISOgrk3 -->
            initialMap.Put("\u03BA", "&kappa;"); // greek small letter kappa,U+03BA ISOgrk3 -->
            initialMap.Put("\u03BB", "&lambda;"); // greek small letter lambda,U+03BB ISOgrk3 -->
            initialMap.Put("\u03BC", "&mu;"); // greek small letter mu, U+03BC ISOgrk3 -->
            initialMap.Put("\u03BD", "&nu;"); // greek small letter nu, U+03BD ISOgrk3 -->
            initialMap.Put("\u03BE", "&xi;"); // greek small letter xi, U+03BE ISOgrk3 -->
            initialMap.Put("\u03BF", "&omicron;"); // greek small letter omicron, U+03BF NEW -->
            initialMap.Put("\u03C0", "&pi;"); // greek small letter pi, U+03C0 ISOgrk3 -->
            initialMap.Put("\u03C1", "&rho;"); // greek small letter rho, U+03C1 ISOgrk3 -->
            initialMap.Put("\u03C2", "&sigmaf;"); // greek small letter final sigma,U+03C2 ISOgrk3 -->
            initialMap.Put("\u03C3", "&sigma;"); // greek small letter sigma,U+03C3 ISOgrk3 -->
            initialMap.Put("\u03C4", "&tau;"); // greek small letter tau, U+03C4 ISOgrk3 -->
            initialMap.Put("\u03C5", "&upsilon;"); // greek small letter upsilon,U+03C5 ISOgrk3 -->
            initialMap.Put("\u03C6", "&phi;"); // greek small letter phi, U+03C6 ISOgrk3 -->
            initialMap.Put("\u03C7", "&chi;"); // greek small letter chi, U+03C7 ISOgrk3 -->
            initialMap.Put("\u03C8", "&psi;"); // greek small letter psi, U+03C8 ISOgrk3 -->
            initialMap.Put("\u03C9", "&omega;"); // greek small letter omega,U+03C9 ISOgrk3 -->
            initialMap.Put("\u03D1", "&thetasym;"); // greek small letter theta symbol,U+03D1 NEW -->
            initialMap.Put("\u03D2", "&upsih;"); // greek upsilon with hook symbol,U+03D2 NEW -->
            initialMap.Put("\u03D6", "&piv;"); // greek pi symbol, U+03D6 ISOgrk3 -->
            // <!-- General Punctuation -->
            initialMap.Put("\u2022", "&bull;"); // bullet = black small circle,U+2022 ISOpub -->
            // <!-- bullet is NOT the same as bullet operator, U+2219 -->
            initialMap.Put("\u2026", "&hellip;"); // horizontal ellipsis = three dot leader,U+2026 ISOpub -->
            initialMap.Put("\u2032", "&prime;"); // prime = minutes = feet, U+2032 ISOtech -->
            initialMap.Put("\u2033", "&Prime;"); // double prime = seconds = inches,U+2033 ISOtech -->
            initialMap.Put("\u203E", "&oline;"); // overline = spacing overscore,U+203E NEW -->
            initialMap.Put("\u2044", "&frasl;"); // fraction slash, U+2044 NEW -->
            // <!-- Letterlike Symbols -->
            initialMap.Put("\u2118", "&weierp;"); // script capital P = power set= Weierstrass p, U+2118 ISOamso -->
            initialMap.Put("\u2111", "&image;"); // blackletter capital I = imaginary part,U+2111 ISOamso -->
            initialMap.Put("\u211C", "&real;"); // blackletter capital R = real part symbol,U+211C ISOamso -->
            initialMap.Put("\u2122", "&trade;"); // trade mark sign, U+2122 ISOnum -->
            initialMap.Put("\u2135", "&alefsym;"); // alef symbol = first transfinite cardinal,U+2135 NEW -->
            // <!-- alef symbol is NOT the same as hebrew letter alef,U+05D0 although the
            // same glyph could be used to depict both characters -->
            // <!-- Arrows -->
            initialMap.Put("\u2190", "&larr;"); // leftwards arrow, U+2190 ISOnum -->
            initialMap.Put("\u2191", "&uarr;"); // upwards arrow, U+2191 ISOnum-->
            initialMap.Put("\u2192", "&rarr;"); // rightwards arrow, U+2192 ISOnum -->
            initialMap.Put("\u2193", "&darr;"); // downwards arrow, U+2193 ISOnum -->
            initialMap.Put("\u2194", "&harr;"); // left right arrow, U+2194 ISOamsa -->
            initialMap.Put(
                "\u21B5",
                "&crarr;"); // downwards arrow with corner leftwards= carriage return, U+21B5 NEW -->
            initialMap.Put("\u21D0", "&lArr;"); // leftwards double arrow, U+21D0 ISOtech -->
            // <!-- ISO 10646 does not say that lArr is the same as the 'is implied by'
            // arrow but also does not have any other character for that function.
            // So ? lArr canbe used for 'is implied by' as ISOtech suggests -->
            initialMap.Put("\u21D1", "&uArr;"); // upwards double arrow, U+21D1 ISOamsa -->
            initialMap.Put("\u21D2", "&rArr;"); // rightwards double arrow,U+21D2 ISOtech -->
            // <!-- ISO 10646 does not say this is the 'implies' character but does not
            // have another character with this function so ?rArr can be used for
            // 'implies' as ISOtech suggests -->
            initialMap.Put("\u21D3", "&dArr;"); // downwards double arrow, U+21D3 ISOamsa -->
            initialMap.Put("\u21D4", "&hArr;"); // left right double arrow,U+21D4 ISOamsa -->
            // <!-- Mathematical Operators -->
            initialMap.Put("\u2200", "&forall;"); // for all, U+2200 ISOtech -->
            initialMap.Put("\u2202", "&part;"); // partial differential, U+2202 ISOtech -->
            initialMap.Put("\u2203", "&exist;"); // there exists, U+2203 ISOtech -->
            initialMap.Put("\u2205", "&empty;"); // empty set = null set = diameter,U+2205 ISOamso -->
            initialMap.Put("\u2207", "&nabla;"); // nabla = backward difference,U+2207 ISOtech -->
            initialMap.Put("\u2208", "&isin;"); // element of, U+2208 ISOtech -->
            initialMap.Put("\u2209", "&notin;"); // not an element of, U+2209 ISOtech -->
            initialMap.Put("\u220B", "&ni;"); // contains as member, U+220B ISOtech -->
            // <!-- should there be a more memorable name than 'ni'? -->
            initialMap.Put("\u220F", "&prod;"); // n-ary product = product sign,U+220F ISOamsb -->
            // <!-- prod is NOT the same character as U+03A0 'greek capital letter pi'
            // though the same glyph might be used for both -->
            initialMap.Put("\u2211", "&sum;"); // n-ary summation, U+2211 ISOamsb -->
            // <!-- sum is NOT the same character as U+03A3 'greek capital letter sigma'
            // though the same glyph might be used for both -->
            initialMap.Put("\u2212", "&minus;"); // minus sign, U+2212 ISOtech -->
            initialMap.Put("\u2217", "&lowast;"); // asterisk operator, U+2217 ISOtech -->
            initialMap.Put("\u221A", "&radic;"); // square root = radical sign,U+221A ISOtech -->
            initialMap.Put("\u221D", "&prop;"); // proportional to, U+221D ISOtech -->
            initialMap.Put("\u221E", "&infin;"); // infinity, U+221E ISOtech -->
            initialMap.Put("\u2220", "&ang;"); // angle, U+2220 ISOamso -->
            initialMap.Put("\u2227", "&and;"); // logical and = wedge, U+2227 ISOtech -->
            initialMap.Put("\u2228", "&or;"); // logical or = vee, U+2228 ISOtech -->
            initialMap.Put("\u2229", "&cap;"); // intersection = cap, U+2229 ISOtech -->
            initialMap.Put("\u222A", "&cup;"); // union = cup, U+222A ISOtech -->
            initialMap.Put("\u222B", "&int;"); // integral, U+222B ISOtech -->
            initialMap.Put("\u2234", "&there4;"); // therefore, U+2234 ISOtech -->
            initialMap.Put("\u223C", "&sim;"); // tilde operator = varies with = similar to,U+223C ISOtech -->
            // <!-- tilde operator is NOT the same character as the tilde, U+007E,although
            // the same glyph might be used to represent both -->
            initialMap.Put("\u2245", "&cong;"); // approximately equal to, U+2245 ISOtech -->
            initialMap.Put("\u2248", "&asymp;"); // almost equal to = asymptotic to,U+2248 ISOamsr -->
            initialMap.Put("\u2260", "&ne;"); // not equal to, U+2260 ISOtech -->
            initialMap.Put("\u2261", "&equiv;"); // identical to, U+2261 ISOtech -->
            initialMap.Put("\u2264", "&le;"); // less-than or equal to, U+2264 ISOtech -->
            initialMap.Put("\u2265", "&ge;"); // greater-than or equal to,U+2265 ISOtech -->
            initialMap.Put("\u2282", "&sub;"); // subset of, U+2282 ISOtech -->
            initialMap.Put("\u2283", "&sup;"); // superset of, U+2283 ISOtech -->
            // <!-- note that nsup, 'not a superset of, U+2283' is not covered by the
            // Symbol font encoding and is not included. Should it be, for symmetry?
            // It is in ISOamsn -->,
            initialMap.Put("\u2284", "&nsub;"); // not a subset of, U+2284 ISOamsn -->
            initialMap.Put("\u2286", "&sube;"); // subset of or equal to, U+2286 ISOtech -->
            initialMap.Put("\u2287", "&supe;"); // superset of or equal to,U+2287 ISOtech -->
            initialMap.Put("\u2295", "&oplus;"); // circled plus = direct sum,U+2295 ISOamsb -->
            initialMap.Put("\u2297", "&otimes;"); // circled times = vector product,U+2297 ISOamsb -->
            initialMap.Put("\u22A5", "&perp;"); // up tack = orthogonal to = perpendicular,U+22A5 ISOtech -->
            initialMap.Put("\u22C5", "&sdot;"); // dot operator, U+22C5 ISOamsb -->
            // <!-- dot operator is NOT the same character as U+00B7 middle dot -->
            // <!-- Miscellaneous Technical -->
            initialMap.Put("\u2308", "&lceil;"); // left ceiling = apl upstile,U+2308 ISOamsc -->
            initialMap.Put("\u2309", "&rceil;"); // right ceiling, U+2309 ISOamsc -->
            initialMap.Put("\u230A", "&lfloor;"); // left floor = apl downstile,U+230A ISOamsc -->
            initialMap.Put("\u230B", "&rfloor;"); // right floor, U+230B ISOamsc -->
            initialMap.Put("\u2329", "&lang;"); // left-pointing angle bracket = bra,U+2329 ISOtech -->
            // <!-- lang is NOT the same character as U+003C 'less than' or U+2039 'single left-pointing angle quotation
            // mark' -->
            initialMap.Put("\u232A", "&rang;"); // right-pointing angle bracket = ket,U+232A ISOtech -->
            // <!-- rang is NOT the same character as U+003E 'greater than' or U+203A
            // 'single right-pointing angle quotation mark' -->
            // <!-- Geometric Shapes -->
            initialMap.Put("\u25CA", "&loz;"); // lozenge, U+25CA ISOpub -->
            // <!-- Miscellaneous Symbols -->
            initialMap.Put("\u2660", "&spades;"); // black spade suit, U+2660 ISOpub -->
            // <!-- black here seems to mean filled as opposed to hollow -->
            initialMap.Put("\u2663", "&clubs;"); // black club suit = shamrock,U+2663 ISOpub -->
            initialMap.Put("\u2665", "&hearts;"); // black heart suit = valentine,U+2665 ISOpub -->
            initialMap.Put("\u2666", "&diams;"); // black diamond suit, U+2666 ISOpub -->

            // <!-- Latin Extended-A -->
            initialMap.Put("\u0152", "&OElig;"); // -- latin capital ligature OE,U+0152 ISOlat2 -->
            initialMap.Put("\u0153", "&oelig;"); // -- latin small ligature oe, U+0153 ISOlat2 -->
            // <!-- ligature is a misnomer, this is a separate character in some languages -->
            initialMap.Put("\u0160", "&Scaron;"); // -- latin capital letter S with caron,U+0160 ISOlat2 -->
            initialMap.Put("\u0161", "&scaron;"); // -- latin small letter s with caron,U+0161 ISOlat2 -->
            initialMap.Put("\u0178", "&Yuml;"); // -- latin capital letter Y with diaeresis,U+0178 ISOlat2 -->
            // <!-- Spacing Modifier Letters -->
            initialMap.Put("\u02C6", "&circ;"); // -- modifier letter circumflex accent,U+02C6 ISOpub -->
            initialMap.Put("\u02DC", "&tilde;"); // small tilde, U+02DC ISOdia -->
            // <!-- General Punctuation -->
            initialMap.Put("\u2002", "&ensp;"); // en space, U+2002 ISOpub -->
            initialMap.Put("\u2003", "&emsp;"); // em space, U+2003 ISOpub -->
            initialMap.Put("\u2009", "&thinsp;"); // thin space, U+2009 ISOpub -->
            initialMap.Put("\u200C", "&zwnj;"); // zero width non-joiner,U+200C NEW RFC 2070 -->
            initialMap.Put("\u200D", "&zwj;"); // zero width joiner, U+200D NEW RFC 2070 -->
            initialMap.Put("\u200E", "&lrm;"); // left-to-right mark, U+200E NEW RFC 2070 -->
            initialMap.Put("\u200F", "&rlm;"); // right-to-left mark, U+200F NEW RFC 2070 -->
            initialMap.Put("\u2013", "&ndash;"); // en dash, U+2013 ISOpub -->
            initialMap.Put("\u2014", "&mdash;"); // em dash, U+2014 ISOpub -->
            initialMap.Put("\u2018", "&lsquo;"); // left single quotation mark,U+2018 ISOnum -->
            initialMap.Put("\u2019", "&rsquo;"); // right single quotation mark,U+2019 ISOnum -->
            initialMap.Put("\u201A", "&sbquo;"); // single low-9 quotation mark, U+201A NEW -->
            initialMap.Put("\u201C", "&ldquo;"); // left double quotation mark,U+201C ISOnum -->
            initialMap.Put("\u201D", "&rdquo;"); // right double quotation mark,U+201D ISOnum -->
            initialMap.Put("\u201E", "&bdquo;"); // double low-9 quotation mark, U+201E NEW -->
            initialMap.Put("\u2020", "&dagger;"); // dagger, U+2020 ISOpub -->
            initialMap.Put("\u2021", "&Dagger;"); // double dagger, U+2021 ISOpub -->
            initialMap.Put("\u2030", "&permil;"); // per mille sign, U+2030 ISOtech -->
            initialMap.Put("\u2039", "&lsaquo;"); // single left-pointing angle quotation mark,U+2039 ISO proposed -->
            // <!-- lsaquo is proposed but not yet ISO standardized -->
            initialMap.Put("\u203A", "&rsaquo;"); // single right-pointing angle quotation mark,U+203A ISO proposed -->
            // <!-- rsaquo is proposed but not yet ISO standardized -->
            initialMap.Put("\u20AC", "&euro;"); // -- euro sign, U+20AC NEW -->
            HTML40_EXTENDED_ESCAPE = initialMap;
        }

        static void Initialize_HTML40_EXTENDED_UNESCAPE()
        {
            HTML40_EXTENDED_UNESCAPE = Invert(HTML40_EXTENDED_ESCAPE);
        }

        static void Initialize_BASIC_ESCAPE()
        {
            IDictionary<string, string> initialMap = new Dictionary<string, string>();
            initialMap.Put("\"", "&quot;"); // " - double-quote
            initialMap.Put("&", "&amp;"); // & - ampersand
            initialMap.Put("<", "&lt;"); // < - less-than
            initialMap.Put(">", "&gt;"); // > - greater-than
            BASIC_ESCAPE = initialMap;
        }

        static void Initialize_BASIC_UNESCAPE()
        {
            BASIC_UNESCAPE = Invert(BASIC_ESCAPE);
        }

        static void Initialize_APOS_ESCAPE()
        {
            IDictionary<string, string> initialMap = new Dictionary<string, string>();
            initialMap.Put("'", "&apos;"); // XML apostrophe
            APOS_ESCAPE = initialMap;
        }

        static void Initialize_APOS_UNESCAPE()
        {
            APOS_UNESCAPE = Invert(APOS_ESCAPE);
        }

        static void Initialize_JAVA_CTRL_CHARS_ESCAPE()
        {
            IDictionary<string, string> initialMap = new Dictionary<string, string>();
            initialMap.Put("\b", "\\b");
            initialMap.Put("\n", "\\n");
            initialMap.Put("\t", "\\t");
            initialMap.Put("\f", "\\f");
            initialMap.Put("\r", "\\r");
            JAVA_CTRL_CHARS_ESCAPE = initialMap;
        }

        static void Initialize_JAVA_CTRL_CHARS_UNESCAPE()
        {
            JAVA_CTRL_CHARS_UNESCAPE = Invert(JAVA_CTRL_CHARS_ESCAPE);
        }

        /// <summary>
        /// Used to invert an escape Map into an unescape Map.
        /// </summary>
        /// <param name="map">Map&amp;lt;String, String&amp;gt; to be inverted</param>
        /// <returns>Map&amp;lt;String, String&amp;gt; inverted array</returns>
        public static IDictionary<string, string> Invert(IDictionary<string, string> map)
        {
            var newMap = new Dictionary<string, string>();
            foreach (var pair in map) {
                newMap.Put(pair.Value, pair.Key);
            }

            return newMap;
        }
    }
} // end of namespace