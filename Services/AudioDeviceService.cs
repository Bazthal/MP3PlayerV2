using CSCore.CoreAudioAPI;
using MP3PlayerV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MP3PlayerV2.Services
{
    /// <summary>
    /// Manages audio output devices and handles device change notifications.
    /// </summary>
    public class AudioDeviceService : IDisposable
    {
        #region Fields

        private readonly List<AudioDevice> _devices = [];
        private string? _lastSelectedDeviceId;
        private bool _isRefreshing;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the list of available audio devices changes.
        /// </summary>
        public event EventHandler<DevicesChangedEventArgs>? DevicesChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a read-only list of available audio devices.
        /// </summary>
        public IReadOnlyList<AudioDevice> Devices => _devices.AsReadOnly();

        /// <summary>
        /// Gets or sets the ID of the last selected device.
        /// </summary>
        public string? LastSelectedDeviceId
        {
            get => _lastSelectedDeviceId;
            set => _lastSelectedDeviceId = value;
        }

        /// <summary>
        /// Gets whether the service is currently refreshing the device list.
        /// </summary>
        public bool IsRefreshing => _isRefreshing;

        #endregion

        #region Public Methods

        /// <summary>
        /// Enumerates and populates the list of available audio output devices.
        /// </summary>
        /// <returns>The number of devices found.</returns>
        public int EnumerateDevices()
        {
            _devices.Clear();

            try
            {
                using var enumerator = new MMDeviceEnumerator();
                using var devices = enumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);

                var defaultDeviceId = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia).DeviceID;

                foreach (var device in devices)
                {
                    var audioDevice = new AudioDevice
                    {
                        DeviceID = device.DeviceID,
                        FriendlyName = ExtractFriendlyName(device.FriendlyName),
                        IsDefault = device.DeviceID == defaultDeviceId
                    };

                    _devices.Add(audioDevice);
                }

                BazthalLib.DebugUtils.Log("AudioDeviceService", "EnumerateDevices", 
                    $"Found {_devices.Count} audio device(s)", 
                    logLevel: BazthalLib.DebugUtils.LogLevel.Info);
            }
            catch (Exception ex)
            {
                BazthalLib.DebugUtils.Log("AudioDeviceService", "EnumerateDevices", 
                    $"Error enumerating devices: {ex.Message}", 
                    logLevel: BazthalLib.DebugUtils.LogLevel.Error);
            }

            return _devices.Count;
        }

        /// <summary>
        /// Refreshes the device list and attempts to restore the previously selected device.
        /// </summary>
        /// <returns>The index of the restored device, or -1 if not found.</returns>
        public int RefreshDevices()
        {
            _isRefreshing = true;

            var oldDeviceId = _lastSelectedDeviceId;
            EnumerateDevices();

            int newIndex = -1;

            // Try to find the previously selected device or a user-selected device
            if (oldDeviceId != null)
            {
                for (int i = 0; i < _devices.Count; i++)
                {
                    if (_devices[i].DeviceID == oldDeviceId || _devices[i].UserSelected)
                    {
                        newIndex = i;
                        break;
                    }
                }
            }

            // Default to first device if previous selection not found
            if (newIndex == -1 && _devices.Count > 0)
                newIndex = 0;

            _isRefreshing = false;

            // Notify subscribers
            DevicesChanged?.Invoke(this, new DevicesChangedEventArgs(_devices, newIndex));

            BazthalLib.DebugUtils.Log("AudioDeviceService", "RefreshDevices", 
                $"Refreshed devices. Selected index: {newIndex}", 
                logLevel: BazthalLib.DebugUtils.LogLevel.Info);

            return newIndex;
        }

        /// <summary>
        /// Gets the device at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the device.</param>
        /// <returns>The audio device, or null if the index is out of range.</returns>
        public AudioDevice? GetDevice(int index)
        {
            if (index >= 0 && index < _devices.Count)
                return _devices[index];
            return null;
        }

        /// <summary>
        /// Finds the index of a device by its ID.
        /// </summary>
        /// <param name="deviceId">The device ID to search for.</param>
        /// <returns>The index of the device, or -1 if not found.</returns>
        public int FindDeviceIndex(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                return -1;

            for (int i = 0; i < _devices.Count; i++)
            {
                if (_devices[i].DeviceID == deviceId)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Finds the index of a device by its friendly name.
        /// </summary>
        /// <param name="friendlyName">The friendly name to search for.</param>
        /// <returns>The index of the device, or -1 if not found.</returns>
        public int FindDeviceByName(string friendlyName)
        {
            if (string.IsNullOrEmpty(friendlyName))
                return -1;

            for (int i = 0; i < _devices.Count; i++)
            {
                if (_devices[i].FriendlyName == friendlyName)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Marks a device as user-selected.
        /// </summary>
        /// <param name="index">The index of the device to mark.</param>
        public void MarkDeviceAsUserSelected(int index)
        {
            if (index >= 0 && index < _devices.Count)
            {
                // Clear all user selections
                foreach (var device in _devices)
                {
                    device.UserSelected = false;
                }

                // Mark the selected one
                _devices[index].UserSelected = true;
                _lastSelectedDeviceId = _devices[index].DeviceID;
            }
        }

        /// <summary>
        /// Gets the default audio device.
        /// </summary>
        /// <returns>The default device, or null if none found.</returns>
        public AudioDevice? GetDefaultDevice()
        {
            return _devices.FirstOrDefault(d => d.IsDefault.HasValue && d.IsDefault.Value);
        }

        /// <summary>
        /// Clears the device list.
        /// </summary>
        public void Clear()
        {
            _devices.Clear();
        }

        /// <summary>
        /// Releases all resources used by the service.
        /// </summary>
        public void Dispose()
        {
            _devices.Clear();
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Extracts a friendly device name by removing parenthetical content.
        /// </summary>
        /// <param name="fullName">The full device name.</param>
        /// <returns>The friendly name without parenthetical content.</returns>
        private static string ExtractFriendlyName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return fullName;

            if (fullName.Contains('('))
            {
                var parts = fullName.Split('(');
                return parts[0].Trim();
            }

            return fullName;
        }

        #endregion
    }

    #region Event Args

    /// <summary>
    /// Provides data for device changed events.
    /// </summary>
    public class DevicesChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the updated list of devices.
        /// </summary>
        public IReadOnlyList<AudioDevice> Devices { get; }

        /// <summary>
        /// Gets the suggested index to select (based on previous selection or default).
        /// </summary>
        public int SuggestedIndex { get; }

        public DevicesChangedEventArgs(IReadOnlyList<AudioDevice> devices, int suggestedIndex)
        {
            Devices = devices;
            SuggestedIndex = suggestedIndex;
        }
    }

    #endregion
}
