﻿using JabbR.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace JabbR.Infrastructure
{
    public static class MentionExtractor
    {
        private const string UsernameMentionPattern = @"(?<user>(?<=@{1}(?!@))[a-zA-Z0-9-_\.]{1,50})";
        private const string CustomMentionPattern = @"(?<=\W|^)(?:{0})(?=\W|$)";
        private const string GroupFormat = @"(?<{0}>{1})";

        private static string _customCachedPattern = null;
        private static int[] _customCachedPatternMentions;

        public static IList<string> ExtractMentions(string message, IQueryable<ChatUserMention> mentions = null)
        {
            if (message == null)
                return new List<string>();


            // Find username mentions
            var matches = Regex.Matches(message, UsernameMentionPattern)
                .Cast<Match>()
                .Where(m => m.Success)
                .Select(m => m.Groups["user"].Value.Trim())
                .Where(u => !String.IsNullOrEmpty(u))
                .ToList();

            // Find custom mentions
            if (mentions == null || !mentions.Any()) return matches;
            
            var regex = new Regex(GetPattern(mentions), RegexOptions.IgnoreCase);

            foreach (var match in regex.Matches(message)
                                       .Cast<Match>()
                                       .Where(m => m.Success))
            {
                for (var i = 1; i < match.Groups.Count; i++)
                {
                    if (!match.Groups[i].Success) continue;
                    
                    matches.Add(regex.GroupNameFromNumber(i));
                }
            }

            return matches;
        }

        public static string GetPattern(IQueryable<ChatUserMention> mentions)
        {
            // Rebuild if nothing is cached
            if (_customCachedPattern == null || _customCachedPatternMentions == null)
                return UpdatePattern(mentions.ToList());

            // Check all the users are in the pattern
            var addedCount = mentions.Count(p => !_customCachedPatternMentions.Contains(p.Key));
            if (addedCount > 0)
            {
                return UpdatePattern(mentions.ToList());
            }

            var currentKeys = mentions.Select(p => p.Key).ToList();

            var removedCount = _customCachedPatternMentions.Where(p => !currentKeys.Contains(p)).Count();

            return removedCount > 0 ? UpdatePattern(mentions.ToList()) : _customCachedPattern;
        }

        public static string UpdatePattern(IList<ChatUserMention> mentions)
        {
            _customCachedPattern = string.Format(CustomMentionPattern, String.Join("|",
                mentions.GroupBy(g => g.UserKey)
                        .Select(p => string.Format(GroupFormat, p.First().User.Name,
                            String.Join("|",
                                p.Select(j => j.String)
                                    .Concat(new [] { p.First().User.Name })
                            )
                        ))
            ));

            _customCachedPatternMentions = mentions.Select(p => p.Key).ToArray();
            return _customCachedPattern;
        }
    }
}
