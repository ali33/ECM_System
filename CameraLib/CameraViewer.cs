using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing.Imaging;
using DirectShowLib;
using System.IO;
using System.Threading;

namespace Ecm.CameraLib
{
    public partial class CameraViewer : Form, ISampleGrabberCB
    {
        #region Private variables

        private int _timerCounter;

        private string _tempOutputFile = string.Empty;

        /// <summary> flag to detect first Form appearance </summary>
        private bool _firstActive;

        /// <summary> base filter of the actually used video devices. </summary>
        private IBaseFilter _capFilter;

        /// <summary> base filter of the actually used audio devices. </summary>
        private IBaseFilter _audFilter;

        /// <summary> graph builder interface. </summary>
        private IGraphBuilder _graphBuilder;

        /// <summary> capture graph builder interface. </summary>
        private ICaptureGraphBuilder2 _capGraph;
        private ISampleGrabber _sampGrabber;

        /// <summary> control interface. </summary>
        private IMediaControl _mediaCtrl;

        /// <summary> event interface. </summary>
        private IMediaEventEx _mediaEvt;

        /// <summary> video window interface. </summary>
        private IVideoWindow _videoWin;

        /// <summary> grabber filter interface. </summary>
        private IBaseFilter _baseGrabFlt;

        /// <summary> structure describing the bitmap to grab. </summary>
        private VideoInfoHeader _videoInfoHeader;
        private bool _captured = true;

        /// <summary> buffer for bitmap data. </summary>
        private byte[] _savedArray;

        /// <summary> list of installed video devices. </summary>
        private readonly DsDevice[] _capDevices;
        private readonly DsDevice[] _audDevices;

        private const int WmGraphnotify = 0x00008001;	// message from graph

        /// <summary> event when callback has finished (ISampleGrabberCB.BufferCB). </summary>
        private delegate void CaptureDone();

        #endregion

        #region Public methods

        public CameraViewer(string workingFolder, string cameraDeviceName, string audioDeviceName)
        {
            InitializeComponent();
            _capDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            CameraDevice = _capDevices.FirstOrDefault(p => p.Name == cameraDeviceName) ?? _capDevices[0];

            _audDevices = DsDevice.GetDevicesOfCat(FilterCategory.AudioInputDevice);
            AudioDevice = _audDevices.FirstOrDefault(p => p.Name == audioDeviceName) ?? _audDevices[0];

            SetDefaultTextForActionButton();
            WorkingFolder = workingFolder;
        }

        public DsDevice CameraDevice { get; private set; }

        public DsDevice AudioDevice { get; private set; }

        public string WorkingFolder { get; private set; }

        public event CaptureOutputHandler CaptureOutput;

        #endregion

        #region Helper methods

        /// <summary> Clean up any resources being used. </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CloseInterfaces(true);
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        /// <summary> override window fn to handle graph events. </summary>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmGraphnotify)
            {
                if (_mediaEvt != null)
                {
                    OnGraphNotify();
                }

                return;
            }

            base.WndProc(ref m);
        }

        private void ActionButtonClick(object sender, EventArgs e)
        {
            if (videoOption.Checked)
            {
                if (actionButton.Text == "Record")
                {
                    GenerateOutputFile();
                    CloseInterfaces(false);
                    if (StartVideo(ViewerMode.Video))
                    {
                        StartTimer();
                        actionButton.Text = "Stop";
                    }
                }
                else
                {
                    SimulateActionAffected();
                    actionButton.Text = "Record";
                    timer.Stop();
                    _mediaCtrl.Stop();
                    RaiseCaptureOutputEvent();
                }
            }
            else
            {
                GenerateOutputFile();
                if (_savedArray == null)
                {
                    int size = _videoInfoHeader.BmiHeader.ImageSize;
                    if ((size < 1000) || (size > 16000000))
                    {
                        return;
                    }

                    _savedArray = new byte[size + 64000];
                    _captured = false;
                    SimulateActionAffected();
                    if (_sampGrabber != null)
                    {
                        _sampGrabber.SetCallback(this, 1);
                    }
                }
            }
        }

        private void SimulateActionAffected()
        {
            Cursor = Cursors.WaitCursor;
            Thread.Sleep(1000);
            Cursor = null;
        }

        private void CloseButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        private void PhotoOptionCheckedChanged(object sender, EventArgs e)
        {
            videoOption.Checked = !photoOption.Checked;
            CloseInterfaces(false);

            if (photoOption.Checked)
            {
                StartVideo(ViewerMode.TakePhoto);
            }

            SetDefaultTextForActionButton();
            CleanGarbageOutput();
        }

        private void VideoOptionCheckedChanged(object sender, EventArgs e)
        {
            photoOption.Checked = !videoOption.Checked;
            CloseInterfaces(false);

            if (videoOption.Checked)
            {
                StartVideo(ViewerMode.Preview);
            }

            SetDefaultTextForActionButton();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            _timerCounter++;
            ConvertTickToText();
        }

        private void CameraViewerActivated(object sender, EventArgs e)
        {
            if (_firstActive)
            {
                return;
            }

            _firstActive = true;
            if (!StartVideo(ViewerMode.TakePhoto))
            {
                Close();
            }
        }

        private void CameraViewerFormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            CloseInterfaces(true);
        }

        private void VideoPanelResize(object sender, EventArgs e)
        {
            ResizeVideoWindow();
        }

        private void SetDefaultTextForActionButton()
        {
            if (videoOption.Checked)
            {
                actionButton.Text = "Record";
                _timerCounter = 0;
                timerLabel.Visible = true;
                timer.Stop();
                ConvertTickToText();
            }
            else
            {
                actionButton.Text = "Take picture";
                timerLabel.Visible = false;
            }
        }

        private void StartTimer()
        {
            _timerCounter = 0;
            ConvertTickToText();
            timer.Start();
        }

        private void ConvertTickToText()
        {
            int seconds = _timerCounter % 60;
            int minutes = _timerCounter / 60;
            int hours = minutes / 60;
            minutes = minutes % 60;
            timerLabel.Text = hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
        }

        private bool StartVideo(ViewerMode mode)
        {
            try
            {
                if (!CreateCaptureDevice(CameraDevice.Mon))
                {
                    return false;
                }

                if (mode == ViewerMode.Video)
                {
                    CreateAudioDevice(AudioDevice.Mon);
                }

                if (!GetInterfaces())
                {
                    return false;
                }

                switch (mode)
                {
                    case ViewerMode.Preview:
                        if (!SetupPreviewGraph())
                        {
                            return false;
                        }

                        break;
                    case ViewerMode.Video:
                        if (!SetupVideoGraph())
                        {
                            return false;
                        }

                        break;
                    case ViewerMode.TakePhoto:
                        if (!SetupPhotoGraph())
                        {
                            return false;
                        }

                        break;
                }

                if (!SetupVideoWindow())
                {
                    return false;
                }

                int hr = _mediaCtrl.Run();
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                return true;
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, "Could not start video stream\r\n" + ee.Message, "DirectShow.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }
        }

        private bool CreateCaptureDevice(IMoniker mon)
        {
            object capObj = null;
            try
            {
                Guid gbf = typeof(IBaseFilter).GUID;
                mon.BindToObject(null, null, ref gbf, out capObj);
                _capFilter = (IBaseFilter)capObj;
                capObj = null;
                return true;
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, "Could not create capture device\r\n" + ee.Message, "ILINX", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }
            finally
            {
                if (capObj != null)
                {
                    Marshal.ReleaseComObject(capObj);
                }
            }

        }

        private void CreateAudioDevice(IMoniker mon)
        {
            object audObj = null;
            try
            {
                Guid gbf = typeof(IBaseFilter).GUID;
                mon.BindToObject(null, null, ref gbf, out audObj);
                _audFilter = (IBaseFilter)audObj;
                audObj = null;
                return;
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, "Could not create capture device\r\n" + ee.Message, "ILINX", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            finally
            {
                if (audObj != null)
                {
                    Marshal.ReleaseComObject(audObj);
                }
            }
        }

        /// <summary> create the used COM components and get the interfaces. </summary>
        private bool GetInterfaces()
        {
            try
            {
                _graphBuilder = (IGraphBuilder)new FilterGraph();
                _capGraph = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
                _sampGrabber = (ISampleGrabber)new SampleGrabber();
                _mediaCtrl = (IMediaControl)_graphBuilder;
                _videoWin = (IVideoWindow)_graphBuilder;
                _mediaEvt = (IMediaEventEx)_graphBuilder;
                _baseGrabFlt = (IBaseFilter)_sampGrabber;

                return true;
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, "Could not get interfaces\r\n" + ee.Message, "DirectShow.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }
        }

        /// <summary> build the capture graph for preview. </summary>
        private bool SetupPreviewGraph()
        {
            try
            {
                int hr = _capGraph.SetFiltergraph(_graphBuilder);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                hr = _graphBuilder.AddFilter(_capFilter, "Ds.NET Video Capture Device");
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                Guid cat = PinCategory.Preview;
                Guid med = MediaType.Video;
                hr = _capGraph.RenderStream(cat, med, _capFilter, null, null); // baseGrabFlt 
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                return true;
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, "Could not setup graph\r\n" + ee.Message, "DirectShow.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }
        }

        /// <summary> build the capture graph for grabber. </summary>
        private bool SetupPhotoGraph()
        {
            try
            {
                int hr = _capGraph.SetFiltergraph(_graphBuilder);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                hr = _graphBuilder.AddFilter(_capFilter, "Ds.NET Video Capture Device");
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                var media = new AMMediaType
                                {
                                    majorType = MediaType.Video,
                                    subType = MediaSubType.RGB24,
                                    formatType = FormatType.VideoInfo
                                };
                hr = _sampGrabber.SetMediaType(media);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                hr = _graphBuilder.AddFilter(_baseGrabFlt, "Ds.NET Grabber");
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                Guid cat = PinCategory.Preview;
                Guid med = MediaType.Video;
                hr = _capGraph.RenderStream(cat, med, _capFilter, null, null); // baseGrabFlt 
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                cat = PinCategory.Capture;
                med = MediaType.Video;
                hr = _capGraph.RenderStream(cat, med, _capFilter, null, _baseGrabFlt); // baseGrabFlt 
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                media = new AMMediaType();
                hr = _sampGrabber.GetConnectedMediaType(media);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                if ((media.formatType != FormatType.VideoInfo) || (media.formatPtr == IntPtr.Zero))
                {
                    throw new NotSupportedException("Unknown Grabber Media Format");
                }

                _videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(media.formatPtr, typeof(VideoInfoHeader));
                Marshal.FreeCoTaskMem(media.formatPtr);
                media.formatPtr = IntPtr.Zero;
                hr = _sampGrabber.SetBufferSamples(false);
                if (hr == 0)
                {
                    _sampGrabber.SetOneShot(false);
                    hr = _sampGrabber.SetCallback(null, 0);
                }

                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                return true;
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, "Could not setup graph\r\n" + ee.Message, "DirectShow.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }
        }

        /// <summary> build the capture graph. </summary>
        private bool SetupVideoGraph()
        {
            IFileSinkFilter sink = null;
            IBaseFilter wmasfWriter = null;
            try
            {
                int hr = _capGraph.SetFiltergraph(_graphBuilder);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                hr = _graphBuilder.AddFilter(_capFilter, "Ds.NET Video Capture Device");
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                hr = _graphBuilder.AddFilter(_audFilter, "Ds.NET Audio Capture Device");
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                wmasfWriter = (IBaseFilter)new WMAsfWriter();
                hr = _graphBuilder.AddFilter(wmasfWriter, "WM ASF Writer");
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                sink = wmasfWriter as IFileSinkFilter;
                if (sink != null)
                {
                    hr = sink.SetFileName(_tempOutputFile, null);
                    if (hr < 0)
                    {
                        Marshal.ThrowExceptionForHR(hr);
                    }
                }

                Guid cat = PinCategory.Preview;
                Guid med = MediaType.Video;
                hr = _capGraph.RenderStream(cat, med, _capFilter, null, null); // preview window
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                cat = PinCategory.Capture;
                hr = _capGraph.RenderStream(cat, med, _capFilter, null, wmasfWriter); // preview window
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                hr = _graphBuilder.ConnectDirect(GetPin(_audFilter, "Capture"), GetPin(wmasfWriter, "Audio Input 01"), null);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                return true;
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, "Could not setup graph\r\n" + ee.Message, "DirectShow.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }
            finally
            {
                if (sink != null)
                {
                    Marshal.ReleaseComObject(sink);
                }

                if (wmasfWriter != null)
                {
                    Marshal.ReleaseComObject(wmasfWriter);
                }
            }
        }

        /// <summary> make the video preview window to show in videoPanel. </summary>
        private bool SetupVideoWindow()
        {
            try
            {
                // Set the video window to be a child of the main window
                int hr = _videoWin.put_Owner(videoPanel.Handle);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                // Set video window style
                hr = _videoWin.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipChildren);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                // Use helper function to position video window in client rect of owner window
                ResizeVideoWindow();

                // Make the video window visible, now that it is properly positioned
                hr = _videoWin.put_Visible(OABool.True);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                hr = _mediaEvt.SetNotifyWindow(Handle, WmGraphnotify, IntPtr.Zero);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                return true;
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, "Could not setup video window\r\n" + ee.Message, "DirectShow.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }
        }

        private void ResizeVideoWindow()
        {
            if (_videoWin != null)
            {
                Rectangle rc = videoPanel.ClientRectangle;
                _videoWin.SetWindowPosition(0, 0, rc.Right, rc.Bottom);
            }
        }

        private void GenerateOutputFile()
        {
            CleanGarbageOutput();
            _tempOutputFile = Path.Combine(WorkingFolder, Guid.NewGuid().ToString() + ".jpeg");
            if (videoOption.Checked)
            {
                _tempOutputFile = Path.Combine(WorkingFolder, Guid.NewGuid().ToString() + ".wmv");
            }
        }

        private void CleanGarbageOutput()
        {
            if (!string.IsNullOrEmpty(_tempOutputFile) && File.Exists(_tempOutputFile))
            {
                try
                {
                    File.Delete(_tempOutputFile);
                }
                catch { }
            }
        }

        /// <summary> do cleanup and release DirectShow. </summary>
        private void CloseInterfaces(bool disposeDevices)
        {
            try
            {
                if (_mediaCtrl != null)
                {
                    _mediaCtrl.Stop();
                    _mediaCtrl = null;
                }

                if (_mediaEvt != null)
                {
                    _mediaEvt.SetNotifyWindow(IntPtr.Zero, WmGraphnotify, IntPtr.Zero);
                    _mediaEvt = null;
                }

                if (_videoWin != null)
                {
                    _videoWin.put_Visible(OABool.False);
                    _videoWin.put_Owner(IntPtr.Zero);
                    _videoWin = null;
                }

                _baseGrabFlt = null;
                if (_sampGrabber != null)
                {
                    Marshal.ReleaseComObject(_sampGrabber);
                    _sampGrabber = null;
                }

                if (_capGraph != null)
                {
                    Marshal.ReleaseComObject(_capGraph);
                    _capGraph = null;
                }

                if (_graphBuilder != null)
                {
                    Marshal.ReleaseComObject(_graphBuilder);
                    _graphBuilder = null;
                }

                if (_capFilter != null)
                {
                    Marshal.ReleaseComObject(_capFilter);
                    _capFilter = null;
                }

                if (_audFilter != null)
                {
                    Marshal.ReleaseComObject(_audFilter);
                    _audFilter = null;
                }

                CleanGarbageOutput();
                if (disposeDevices)
                {
                    foreach (DsDevice device in _capDevices)
                    {
                        device.Dispose();
                    }
                }
            }
            catch
            { }
        }

        /// <summary> graph event (WM_GRAPHNOTIFY) handler. </summary>
        private void OnGraphNotify()
        {
            int hr;
            do
            {
                IntPtr p1;
                IntPtr p2;
                EventCode code;
                hr = _mediaEvt.GetEvent(out code, out p1, out p2, 0);
                if (hr < 0)
                {
                    break;
                }

                hr = _mediaEvt.FreeEventParams(code, p1, p2);
            }
            while (hr == 0);
        }

        private IPin GetPin(IBaseFilter filter, string pinName)
        {
            IEnumPins epins;
            int hr = filter.EnumPins(out epins);
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            IntPtr fetched = Marshal.AllocCoTaskMem(4);
            var pins = new IPin[1];
            while (epins.Next(1, pins, fetched) == 0)
            {
                PinInfo pinfo;
                pins[0].QueryPinInfo(out pinfo);
                bool found = (pinfo.name == pinName);
                DsUtils.FreePinInfo(pinfo);
                if (found)
                {
                    return pins[0];
                }
            }

            throw new Exception("Pin not found");
        }

        public int BufferCB(double sampleTime, IntPtr buffer, int bufferLen)
        {
            if (_captured || (_savedArray == null))
            {
                Trace.WriteLine("!!CB: ISampleGrabberCB.BufferCB");
                return 0;
            }

            _captured = true;
            Trace.WriteLine("!!CB: ISampleGrabberCB.BufferCB  !GRAB! size = " + bufferLen.ToString());
            if ((buffer != IntPtr.Zero) && (bufferLen > 1000) && (bufferLen <= _savedArray.Length))
            {
                Marshal.Copy(buffer, _savedArray, 0, bufferLen);
            }
            else
            {
                Trace.WriteLine("    !!!GRAB! failed ");
            }

            BeginInvoke(new CaptureDone(OnCaptureDone));
            return 0;
        }

        public int SampleCB(double sampleTime, IMediaSample pSample)
        {
            Trace.WriteLine("!!CB: ISampleGrabberCB.SampleCB");
            return 0;
        }

        /// <summary> capture event, triggered by buffer callback. </summary>
        private void OnCaptureDone()
        {
            Trace.WriteLine("!!DLG: OnCaptureDone");
            try
            {
                if (_sampGrabber == null)
                {
                    return;
                }

                _sampGrabber.SetCallback(null, 0);
                int w = _videoInfoHeader.BmiHeader.Width;
                int h = _videoInfoHeader.BmiHeader.Height;
                if (((w & 0x03) != 0) || (w < 32) || (w > 4096) || (h < 32) || (h > 4096))
                {
                    return;
                }

                int stride = w * 3;
                GCHandle handle = GCHandle.Alloc(_savedArray, GCHandleType.Pinned);
                var scan0 = (int)handle.AddrOfPinnedObject();
                scan0 += (h - 1) * stride;
                var b = new Bitmap(w, h, -stride, PixelFormat.Format24bppRgb, (IntPtr)scan0);
                handle.Free();
                _savedArray = null;

                b.Save(_tempOutputFile, ImageFormat.Jpeg);
                b.Dispose();
                RaiseCaptureOutputEvent();
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, "Could not grab picture\r\n" + ee.Message, "DirectShow.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void RaiseCaptureOutputEvent()
        {
            if (CaptureOutput != null && File.Exists(_tempOutputFile))
            {
                CaptureOutput(this, new CaptureOutputEventArgs { FilePath = _tempOutputFile, UnRejectPage = UnRejectPage });
            }

            _tempOutputFile = string.Empty;
        }

        #endregion

        public Action UnRejectPage { get; set; }
    }

    public delegate void CaptureOutputHandler(object obj, CaptureOutputEventArgs e);

    public class CaptureOutputEventArgs : EventArgs
    {
        public string FilePath { get; set; }
        public Action UnRejectPage { get; set; }
    }
}
