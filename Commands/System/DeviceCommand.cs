using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.System
{
    /// <summary>
    /// Handles the execution of the "Device" command, which manages audio device selection.
    /// </summary>
    /// <remarks>This command allows users to query the current audio device or change the audio device to one
    /// that matches the specified criteria. If no criteria are provided, it returns the current audio device. If the
    /// specified device is not found, it notifies the user.</remarks>
    [Command("device")]
    public class DeviceCommand : ICommandHandler
    {
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (ctx.GetAudioDeviceCount() <= 1)
            {
                ctx.Respond(false, "Device list is empty or only has the one output device", null);
                return false;
            }

            if (string.IsNullOrWhiteSpace(cmd.Value))
            {
                string curdevice = ctx.GetSelectedAudioDevice();
                ctx.Respond(false, $"Current Audio Device is set to '{curdevice}'", null);
                return false;
            }

            for (int i = 0; i < ctx.GetAudioDeviceCount(); i++)
                {
                    string name = ctx.GetAudioDeviceNameAt(i);
                    if (name.Contains(cmd.Value, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ctx.SetAudioDeviceIndex(i);
                        string selected = ctx.GetSelectedAudioDevice();
                        ctx.Respond(true, $"Audio Device has been changed to '{selected}'", null);
                        return true;
                    }
                }

                ctx.Respond(false, "No matching audio device found", null);
                return false;
            


        }
    }
}
