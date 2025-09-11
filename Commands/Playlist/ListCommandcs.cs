using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playlist
{
    /// <summary>
    /// Handles the "list" command, which filters and displays tracks from the playlist based on the specified criteria.
    /// </summary>
    /// <remarks>This command supports filtering tracks by predefined modes such as "unplayed", "liked", or
    /// "disliked",  or by a custom search value. If the playlist is empty or the command value is invalid, the
    /// operation is canceled.</remarks>
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

            var mode = cmd.Value.ToLowerInvariant();

            switch (mode)
            {
                case "unplayed":
                    matches = allTracks.Where(t => (t.PlayCount ?? 0) == 0).ToList();
                    break;
                case "liked":
                    matches = allTracks.Where(t => t.Liked == true).ToList();
                    break;
                case "disliked":
                    matches = allTracks.Where(t => t.Disliked == true).ToList();
                    break;
                default:
                    var regex = ctx.BuildSearchRegex(cmd.Value);

                    foreach (var track in allTracks)
                    {
                        string trackName = ctx.NormalizeText(track.ToString());
                        if (regex.IsMatch(trackName))
                            matches.Add(track);
                    }
                    break;
            }

            ctx.Respond(true, $"{matches.Count} Match(es) found for filter {cmd.Value}", matches);
            return true;
        }
    }
}

