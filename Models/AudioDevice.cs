namespace MP3PlayerV2.Models
{
    public class AudioDevice
    {
        public string DeviceID { get; set; } = string.Empty;
        public string FriendlyName { get; set; } = string.Empty;
        public bool? IsDefault { get; set; }
        public bool UserSelected { get; set; } = false;

        public override string ToString()
        {
            return FriendlyName;
        }

    }
}
