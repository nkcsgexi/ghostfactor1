using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using warnings.algorithms;

namespace warnings.util
{
    public class StringUtil
    {
        /* Concatenate all the strings in an array into one single string. */
        public static String concatenateAll(String[] texts)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string text in texts)
            {
                sb.Append(text);
            }
            return sb.ToString();
        }

        /* Same as above, different input. */
        public static String concatenateAll(IEnumerable<String> texts)
        {
            StringBuilder sb = new StringBuilder();
             foreach(String s in texts)
                sb.Append(s);
            return sb.ToString();
        }

        /* Replace the specified start(length) in a text with the replacer. */
        public static String replaceWith(String text, String replacer, int start, int length)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(text.Substring(0, start));
            sb.Append(replacer);
            sb.Append(text.Substring(start + length));
            return sb.ToString();
        }

        /* Get the distance between two strings. */
        public static int getStringDistance(String a, String b)
        {
            return new Levenshtein().LD(a, b);
        }

        /* Convert an array of bytes to a string. */
        public static String bytes2String(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

    }
}
