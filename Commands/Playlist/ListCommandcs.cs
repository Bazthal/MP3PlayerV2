using System.Text.Json;
using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playlist
{
    /// <summary>
    /// Handles the "list" command, which retrieves and processes tracks from a playlist based on specified criteria.
    /// </summary>
    /// <remarks>This command checks if the playlist is empty or if the command value is not set, responding
    /// with an appropriate message in such cases. If the command value is "unplayed", it lists all tracks that have not
    /// been played. Otherwise, it searches for tracks matching the provided query. The results are serialized to JSON
    /// and set as the command response.</remarks>
    [Command("list")]
    public class ListCommand : ICommandHandler
    {
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (ctx.IsPlaylistEmpty())
            {
                ctx.Respond(false, "Playlist is empty", null);
                return false;
            }

            if (string.IsNullOrWhiteSpace(cmd.Value))
            {
                ctx.Respond(false, "Value not set; cancelling listing", null);
                return false;
            }

            var allTracks = ctx.GetPlaylistTracks();
            var matches = new List<Track>();

            if (cmd.Value.Equals("unplayed", StringComparison.InvariantCultureIgnoreCase))
            {
                matches = allTracks.Where(t => (t.PlayCount ?? 0) == 0).ToList();
            }
            else
            {
                var query = ctx.NormalizeText(cmd.Value.ToLowerInvariant());

                foreach (var track in allTracks)
                {
                    var normalized = ctx.NormalizeText(track.ToString().ToLowerInvariant());
                    if (normalized.Contains(query))
                        matches.Add(track);
                }
            }

            string resultJson = JsonSerializer.Serialize(matches, ctx.JsonOptions);
            ctx.SetCommandResponseJson(resultJson);
            return true;
        }
    }

}
