using MP3PlayerV2.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MP3PlayerV2.Services
{
    /// <summary>
    /// Provides search and filtering operations for tracks.
    /// </summary>
    public class TrackSearchService
    {
        #region Search Operations

        /// <summary>
        /// Finds tracks matching the specified search term.
        /// </summary>
        /// <param name="tracks">The collection of tracks to search.</param>
        /// <param name="searchTerm">The search term to match against track names.</param>
        /// <returns>A list of tracks matching the search term.</returns>
        public List<Track> FindTracksByName(IEnumerable<Track> tracks, string searchTerm)
        {
            if (tracks == null || !tracks.Any() || string.IsNullOrWhiteSpace(searchTerm))
                return new List<Track>();

            var regex = BuildSearchRegex(searchTerm);
            var matches = new List<Track>();

            foreach (var track in tracks)
            {
                if (track == null) continue;

                string trackName = NormalizeText(track.ToString());
                if (regex.IsMatch(trackName))
                {
                    matches.Add(track);
                }
            }

            return matches;
        }

        /// <summary>
        /// Finds the best matching track for the specified search term.
        /// </summary>
        /// <param name="tracks">The collection of tracks to search.</param>
        /// <param name="searchTerm">The search term to match against track names.</param>
        /// <returns>The best matching track, or null if no match is found.</returns>
        public Track? FindBestMatch(IEnumerable<Track> tracks, string searchTerm)
        {
            if (tracks == null || !tracks.Any() || string.IsNullOrWhiteSpace(searchTerm))
                return null;

            var regex = BuildSearchRegex(searchTerm);
            Track? bestMatch = null;
            int bestScore = int.MinValue;

            foreach (var track in tracks)
            {
                if (track == null) continue;

                string trackName = NormalizeText(track.ToString());
                if (regex.IsMatch(trackName))
                {
                    int score = CalculateMatchScore(trackName, searchTerm);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatch = track;
                    }
                }
            }

            return bestMatch;
        }

        /// <summary>
        /// Finds tracks by their play data status.
        /// </summary>
        /// <param name="tracks">The collection of tracks to filter.</param>
        /// <param name="filterType">The type of filter (unplayed, liked, disliked, neutral).</param>
        /// <returns>A list of tracks matching the filter criteria.</returns>
        public List<Track> FindTracksByPlayData(IEnumerable<Track> tracks, string filterType)
        {
            if (tracks == null || !tracks.Any())
                return new List<Track>();

            var trackList = tracks.ToList();

            return filterType?.ToLowerInvariant() switch
            {
                "unplayed" => trackList.Where(t => (t.PlayCount ?? 0) == 0).ToList(),
                "liked" => trackList.Where(t => t.Liked == true).ToList(),
                "disliked" => trackList.Where(t => t.Disliked == true).ToList(),
                "neutral" => trackList.Where(t => t.Liked == false && t.Disliked == false).ToList(),
                _ => new List<Track>()
            };
        }

        #endregion

        #region Score Calculation

        /// <summary>
        /// Calculates a match score between a track name and a search term based on their similarity.
        /// </summary>
        /// <param name="trackName">The name of the track to evaluate.</param>
        /// <param name="searchTerm">The search term to compare against the track name.</param>
        /// <returns>An integer representing the match score. Higher scores indicate closer matches.</returns>
        public int CalculateMatchScore(string trackName, string searchTerm)
        {
            int score = 0;
            string normalizedTrack = NormalizeText(trackName);
            string normalizedSearch = NormalizeText(searchTerm);

            score += 10;

            if (normalizedTrack.StartsWith(normalizedSearch, StringComparison.OrdinalIgnoreCase))
                score += 5;

            var words = normalizedSearch.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int lastIndex = -1;
            bool allWordsInOrder = true;

            foreach (var word in words)
            {
                int idx = normalizedTrack.IndexOf(word, lastIndex + 1, StringComparison.OrdinalIgnoreCase);
                if (idx == -1)
                {
                    allWordsInOrder = false;
                    break;
                }
                lastIndex = idx;
            }

            if (allWordsInOrder)
                score += 3;

            return score;
        }

        #endregion

        #region Text Normalization

        /// <summary>
        /// Normalizes text by removing diacritics and standardizing quotation marks.
        /// </summary>
        /// <param name="text">The input text to be normalized.</param>
        /// <returns>A normalized version of the input text.</returns>
        public string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Remove diacritics
            string normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString()
                     .Normalize(NormalizationForm.FormC)
                     .Replace("\u2018", "'")
                     .Replace("\u2019", "'")
                     .Replace("\u00B4", "'")
                     .Replace("`", "'")
                     .Replace("\u201C", "\"")
                     .Replace("\u201D", "\"")
                     .Replace("\u2033", "\"")
                     .Trim()
                     .ToLowerInvariant();
        }

        /// <summary>
        /// Builds a regular expression to match the specified search term.
        /// </summary>
        /// <param name="searchTerm">The search term to convert into a regular expression.</param>
        /// <returns>A Regex instance that matches the specified search term.</returns>
        public Regex BuildSearchRegex(string searchTerm)
        {
            string normalizedSearch = NormalizeText(searchTerm);

            normalizedSearch = Regex.Replace(normalizedSearch, @"([*])\1+", "$1");

            string pattern;

            if (normalizedSearch.Contains("*"))
            {
                pattern = Regex.Escape(normalizedSearch).Replace("\\*", ".*");
            }
            else
            {
                pattern = string.Join(".*", normalizedSearch
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(Regex.Escape));

                pattern = ".*" + pattern + ".*";
            }

            return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        #endregion
    }
}
