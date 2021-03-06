﻿using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace IISHelpers
{
    internal static class TemplatingHelper
    {
        internal static string PLACEHOLDER_PREFIX = "{{";   //Placeholders' prefix
        internal static string PLACEHOLDER_SUFIX = "}}";    //Placeholders' suffix
        internal static string PLACEHOLDER_NAME_REGEX = @"[0-9A-Z\/\.\-_]+?";  //Placeholders' name pattern (includes "/" for paths, "." for file names


        //Extension Method for strings that does a Case-Insensitive Replace()
        //Takes into account replacement strings with $x that would be mistaken for RegExp substitutions
        internal static string ReplacePlaceHolder(string originalContent, string placeholderName, string newValue)
        {
            if (string.IsNullOrEmpty(originalContent) || string.IsNullOrEmpty(placeholderName))
                return originalContent;

            if (string.IsNullOrEmpty(newValue))
                newValue = string.Empty;

            return Regex.Replace(originalContent,
                GetPlaceholderRegexString(placeholderName),
                Regex.Replace(newValue, "\\$[0-9]+", @"$$$0"),
                RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Builds the appropriate regular expression to search for an specific placeholder name using the correct prefix and suffix
        /// </summary>
        /// <param name="placeholderName">The name of the placeholder</param>
        /// <returns></returns>
        private static string GetPlaceholderRegexString(string placeholderName, string phPrefix = "")
        {
            if (placeholderName != PLACEHOLDER_NAME_REGEX)
                placeholderName = Regex.Escape(placeholderName);

            return Regex.Escape(PLACEHOLDER_PREFIX) + @"\s*?" + Regex.Escape(phPrefix) + placeholderName + @"\s*?" + Regex.Escape(PLACEHOLDER_SUFIX);
        }

        /// <summary>
        /// Gets all the placeholders in the content, as regular expression matches
        /// </summary>
        /// <param name="content"></param>
        /// <param name="name">Optional. The name of a specific palceholder to search. If not provided returns all the placeholders present in the content</param>
        /// <returns>A collection of RegExp matches with the placeholders in a content</returns>
        internal static MatchCollection GetAllPlaceHolderMatches(string content, string name = "", string phPrefix = "")
        {
            string phName = string.IsNullOrEmpty(name) ? PLACEHOLDER_NAME_REGEX : Regex.Escape(name);
            Regex rePlaceholders = new Regex(GetPlaceholderRegexString(phName, phPrefix), RegexOptions.IgnoreCase);
            return rePlaceholders.Matches(content);
        }

        /// <summary>
        /// Gets all the placeholders in the content, as a string array
        /// </summary>
        /// <param name="content"></param>
        /// <returns>An array of strings with the name, without duplicates</returns>
        internal static string[] GetAllPlaceHolderNames(string content, string name = "", string phPrefix = "")
        {
            MatchCollection matches = GetAllPlaceHolderMatches(content, name, phPrefix);

            string[] names = new string[matches.Count];

            for (int i =0;  i<matches.Count; i++)
            {
                Match field = matches[i];
                names[i] = GetFieldName(field.Value);
            }
            return names.Distinct<string>().ToArray();
        }

        /// <summary>
        /// Checks if the specified placeholder is present in the content
        /// </summary>
        /// <param name="content">The content to test</param>
        /// <param name="placeholderName">The name of the placeholder (without wrappers)</param>
        /// <returns></returns>
        internal static bool IsPlaceHolderPresent(string content, string placeholderName)
        {
            Regex rePlaceHolder = new Regex(GetPlaceholderRegexString(placeholderName), RegexOptions.IgnoreCase);
            return rePlaceHolder.IsMatch(content);
        }

        /// <summary>
        /// Given a placeholder found in a content it extracts it's name without prefix and suffix and in lowercase.
        /// It takes for granted that it's a correct placeholder and won't check it (it's only used internally after a match)
        /// </summary>
        /// <param name="placeholder">The placeholder that was found</param>
        /// <returns>The name of the placeholder in lowercase</returns>
        internal static string GetFieldName(string placeholder)
        {
            return placeholder.Substring(PLACEHOLDER_PREFIX.Length, placeholder.Length - (PLACEHOLDER_PREFIX.Length + PLACEHOLDER_SUFIX.Length)).Trim().ToLower();
        }

        /// <summary>
        /// Given a placeholder name, returns the full placeholder, using the defined prefix and suffix
        /// </summary>
        /// <param name="name">The placeholder name</param>
        /// <returns></returns>
        internal static string GetPlaceholderName(string name)
        {
            return PLACEHOLDER_PREFIX + name + PLACEHOLDER_SUFIX;
        }
    }
}
