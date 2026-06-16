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
    [Command(
        name: "queue",
        author: "Bazthal",
        version: "1.0.0",
        description: "Queues a playlist track by name. Queued tracks play in the order they were added after the current playback finishes.",
        category: "Playlist",
        example: "{ \"Command\": \"queue\" , \"Value\": \"Bohemian Rhapsody\" }"
        )]
    public class QueueCommand : ICommandHandler
    {
        /// <summary>
        /// Executes the specified player command within the given command context.
        /// </summary>
        /// <remarks>This method processes various player commands, such as retrieving the current queue,
        /// clearing the queue, or queuing a track by name. The behavior of the method depends on the value of the
        /// <paramref name="cmd"/> parameter: <list type="bullet"> <item> <description>If the command value is empty or
        /// whitespace, the method attempts to retrieve and respond with the current queue.</description> </item> <item>
        /// <description>If the command value is "count", the method responds with the number of tracks in the
        /// queue.</description> </item> <item> <description>If the command value is "clear", the method clears the
        /// queue and responds accordingly.</description> </item> <item> <description>For any other value, the method
        /// attempts to queue a track by name, provided the playlist is not empty.</description> </item> </list> If the
        /// playlist is empty and a track name is specified, the method responds with an error message and returns <see
        /// langword="false"/>.</remarks>
        /// <param name="cmd">The command to execute, containing the action and any associated value.</param>
        /// <param name="ctx">The context in which the command is executed, providing access to the playlist, queue, and response
        /// mechanisms.</param>
        /// <returns><see langword="true"/> if the command was successfully executed; otherwise, <see langword="false"/>.</returns>
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (string.IsNullOrWhiteSpace(cmd.Value))
            {
                var allTracks = ctx.GetPlaylistTracks();
                var returnList = new List<Track>();
                var queuedTrack = ctx.GetQueuedTracks();

                if (queuedTrack.Count <= 0)
                {
                    ctx.Respond("QueueCommand", true, "Queue is empty", returnList);
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
                ctx.Respond(cmd.Command, true, $"{returnList.Count} track(s) in the Queue", returnList);
                return true;
            }

            string val = cmd.Value.ToLowerInvariant();

            switch (val)
            {
                case "count":
                    ctx.Respond(cmd.Command, true, $"{ctx.GetQueuedTracks().Count} track(s) in the queue", null);
                    return true;
                case "clear":
                    ctx.Respond(cmd.Command, true, "Queue has been cleared", null);
                    ctx.ClearQueue();
                    return true;
                default:
                    if (ctx.IsPlaylistEmpty())
                    {
                        ctx.Respond(cmd.Command, false, "Playlist is empty", null);
                        return false;
                    }
                    var allTracksForQueue = ctx.GetPlaylistTracks();
                    var bestMatchQueue = ctx.GetApplicationStateService().TrackSearch.FindBestMatch(allTracksForQueue, cmd.Value);
                    if (bestMatchQueue != null)
                    {
                        ctx.Invoke(() =>
                        {
                            ctx.QueueTrackByName(cmd.Value);
                        });
                        ctx.Respond(cmd.Command, true, $"Added {bestMatchQueue} to the Queue", null);
                    }
                    else
                    {
                        ctx.Respond(cmd.Command, false, "No matching item found in the playlist", null);
                    }
                    return true;
            }
        }
    }
}