using System.Collections.Generic;
using DirectShowLib;
using System;

namespace Ecm.CameraLib
{
    public class CameraWrapper
    {
        internal static string UsingCameraDeviceName { get; set; }

        internal static string UsingAudioDeviceName { get; set; }

        public static void Initialize()
        {
            DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            if (devices != null && devices.Length > 0)
            {
                UsingCameraDeviceName = devices[0].Name;
                DisposeDevices(devices);
            }

            devices = DsDevice.GetDevicesOfCat(FilterCategory.AudioInputDevice);
            if (devices != null && devices.Length > 0)
            {
                UsingAudioDeviceName = devices[0].Name;
                DisposeDevices(devices);
            }
        }

        public static void SelectCameraDevice()
        {
            DetectIfError();
            DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            var deviceSelector = new DeviceSelector(UsingCameraDeviceName, devices) { Text = "Choose default camera" };
            deviceSelector.ShowDialog();
            UsingCameraDeviceName = deviceSelector.SelectedDeviceName;
        }

        public static void SelectMicDevice()
        {
            DetectIfError();
            DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.AudioInputDevice);
            var deviceSelector = new DeviceSelector(UsingAudioDeviceName, devices) { Text = "Choose default microphone" };
            deviceSelector.ShowDialog();
            UsingAudioDeviceName = deviceSelector.SelectedDeviceName;
        }

        public static bool HasVideoInputDevice
        {
            get
            {
                DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
                if (devices != null && devices.Length > 0)
                {
                    DisposeDevices(devices);
                    return true;
                }

                return false;
            }
        }

        public CameraWrapper(string workingFolder)
        {
            _workingFolder = workingFolder;
        }

        public void ShowCamera(Action unRejectPage = null)
        {
            DetectIfError();
            var viewer = new CameraViewer(_workingFolder, UsingCameraDeviceName, UsingAudioDeviceName);
            viewer.CaptureOutput += ViewerCaptureOutput;
            viewer.ShowDialog();
        }

        public event CaptureOutputHandler CaptureOutput;

        private static void DetectIfError()
        {
            if (!HasVideoInputDevice)
            {
                throw new CameraException("No video input device found.");
            }
        }

        private void ViewerCaptureOutput(object obj, CaptureOutputEventArgs e)
        {
            if (CaptureOutput != null)
            {
                CaptureOutput(this, e);
            }
        }

        private static void DisposeDevices(IEnumerable<DsDevice> devices)
        {
            foreach (var device in devices)
            {
                device.Dispose();
            }
        }

        private readonly string _workingFolder = string.Empty;
    }
}
