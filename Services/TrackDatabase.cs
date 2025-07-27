using System;
using BazthalLib;
using LiteDB;
using MP3PlayerV2.Models;

namespace MP3PlayerV2.Services
{

    public static class TrackDatabase
    {
        /// <summary>
        /// Represents the file path to the database used for storing play data.
        /// </summary>
        private const string DatabasePath = "playdata.db";

        /// <summary>
        /// Saves or updates the specified track in the database.
        /// </summary>
        /// <remarks>This method uses a LiteDB database to store track information. If a track with the
        /// same identifier already exists, it will be updated; otherwise, a new entry will be created.</remarks>
        /// <param name="track">The track to be saved or updated. Cannot be null.</param>
        public static void SaveStats(Track track)
        {
            using var db = new LiteDatabase(DatabasePath);
            var col = db.GetCollection<Track>("tracks");
            col.Upsert(track);
        }

        /// <summary>
        /// Loads the statistics for the specified track from the database.
        /// </summary>
        /// <remarks>If a record with the same hash exists in the database, the track's properties such as
        /// <c>Artist</c>, <c>Title</c>, <c>PlayCount</c>, <c>TimesSkipped</c>, <c>UserRating</c>, and <c>LastPlayed</c>
        /// are updated with the values from the database.</remarks>
        /// <param name="track">The track for which to load statistics. The track's <c>Hash</c> property is used to identify the record in
        /// the database.</param>
        public static void LoadStats(Track track)
        {
            {
                try
                {
                    using var db = new LiteDatabase(DatabasePath);
                    var col = db.GetCollection<Track>("tracks");

                    var saved = col.FindById(track.Hash);

                    if (saved != null)
                    {
                        track.Artist = saved.Artist;
                        track.Title = saved.Title;
                        track.PlayCount = saved.PlayCount;
                        track.TimesSkipped = saved.TimesSkipped;
                        track.LastPlayed = saved.LastPlayed;
                        track.Album = saved.Album;
                    }
                }
                catch (Exception ex)
                {
                    DebugUtils.Log("Error", "Load Stats", ex.Message);
                }

            }
        }

        /// <summary>
        /// Computes the SHA-1 hash of the specified file.
        /// </summary>
        /// <remarks>This method reads the entire file to compute its hash, which may impact performance
        /// for large files.</remarks>
        /// <param name="filePath">The path to the file for which the hash is to be computed. Must not be null or empty.</param>
        /// <returns>A string representing the SHA-1 hash of the file in lowercase hexadecimal format.</returns>
        public static string ComputeFileHash(string filePath)
        {
            using var sha1 = System.Security.Cryptography.SHA1.Create();
            using var stream = File.OpenRead(filePath);
            var hashBytes = sha1.ComputeHash(stream);
            return Convert.ToHexStringLower(hashBytes);
        }
    }
}
