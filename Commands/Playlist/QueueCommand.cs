using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playlist
{
    /// <summary>
    /// Handles the "Queue" command, providing functionality to manage and query the track queue.
    /// </summary>
    /// <remarks>This command supports several operations based on the input value: - If no value is provided,
    /// it returns the list of currently queued tracks in JSON format. - If the value is "count", it returns the number
    /// of tracks in the queue. - If the value is "clear", it clears the queue. - Otherwise, it attempts to queue a
    /// track by name, provided the playlist is not empty.</remarks>
    [Command("Queue")]
    public class QueueCommand : ICommandHandler
    {
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (string.IsNullOrWhiteSpace(cmd.Value))
            {
                var allTracks = ctx.GetPlaylistTracks();
                var returnList = new List<Track>();
                var queuedTrack = ctx.GetQueuedTracks;

                if (queuedTrack.Count <= 0)
                {
                    ctx.Respond(true, "Queue is empty", returnList);
                    return true;
                }

                while (queuedTrack.Count > 0)
                {
                    var nextKey = queuedTrack.Peek();
                    bool matched = false;

                    foreach (var track in allTracks)
                    {
                        if (track.ToString() == nextKey)
                        {
                            returnList.Add(track);
                            queuedTrack.Dequeue();
                            matched = true;
                            break;
                        }
                    }

                    if (!matched)
                    {
                        break;
                    }
                }
                ctx.Respond(true, $"{returnList.Count} track(s) in the Queue", returnList);
                return true;
            }

            string val = cmd.Value.ToLowerInvariant();

            switch (val)
            {
                case "count":
                    ctx.Respond(true, $"{ctx.GetQueuedTracks.Count} track(s) in the queue", null);
                    return true;
                case "clear":
                    ctx.Respond(true, "Queue has been cleared", null);
                    ctx.ClearQueue();
                    return true;
                default:
                    if (ctx.IsPlaylistEmpty())
                    {
                        ctx.Respond(false, "Playlist is empty", null);
                        return false;
                    }
                    ctx.Invoke(() =>
                    {
                        ctx.QueueTrackByName(cmd.Value);
                    });
                    return true;
            }
        }
    }
}