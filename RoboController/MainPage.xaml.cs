using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using NetworkLibrary;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;

namespace RoboController
{
    public sealed partial class MainPage : Page
    {

        private int _pinEn1_2 = 21;
        private int _pin1A = 20;
        private int _pin2A = 16;

        private GpioController _controller;
        private GpioPin _motorEnable;
        private GpioPin _motorControl1A;
        private GpioPin _motorControl2A;

        private DeviceClient _deviceClient;

        public static async Task CallOnUiThreadAsync(CoreDispatcher dispatcher, DispatchedHandler handler) =>
        await dispatcher.RunAsync(CoreDispatcherPriority.Normal, handler);

        public static async Task CallOnMainViewUiThreadAsync(DispatchedHandler handler) =>
        await CallOnUiThreadAsync(CoreApplication.MainView.CoreWindow.Dispatcher, handler);

        public MainPage()
        {
            this.InitializeComponent();

            _controller = GpioController.GetDefault();
            _motorEnable = _controller.OpenPin(_pinEn1_2);
            _motorControl1A = _controller.OpenPin(_pin1A);
            _motorControl2A = _controller.OpenPin(_pin2A);
            _motorEnable.SetDriveMode(GpioPinDriveMode.Output);
            _motorControl1A.SetDriveMode(GpioPinDriveMode.Output);
            _motorControl2A.SetDriveMode(GpioPinDriveMode.Output);

            _deviceClient = DeviceClient.CreateFromConnectionString(GlobalConstant.DEVICE_CONNECTION_STRING, TransportType.Mqtt);
            _deviceClient.SetMethodHandlerAsync("Drive", Drive, null);
        }

        private async Task<MethodResponse> Drive(MethodRequest methodRequest, object userContext)
        {
            var data = methodRequest.DataAsJson;
            DriveCommandData cmdData = JsonConvert.DeserializeObject<DriveCommandData>(data);
            await CallOnMainViewUiThreadAsync(() =>
            {
                IncomingMessage.Text = "drive"+" "+"direction: "+ cmdData.direction + " "+"speed: " + cmdData.speed + " " + "distance: " + cmdData.distance;
                TurnOnIgnition();
                if (cmdData.direction == 0)
                {
                    ReverseMotor();
                } else if(cmdData.direction == 1)
                {
                    ForwardMotor();
                }
            });
            string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
            return new MethodResponse(Encoding.UTF8.GetBytes(result), 200);
        }

        private void TurnOnIgnition()
        {
            btnIgnitionOn.IsEnabled = false;
            btnIgnitionOff.IsEnabled = true;
            btnForward.IsEnabled = true;
            btnReverse.IsEnabled = true;
            _motorEnable.Write(GpioPinValue.High);
        }

        private void ForwardMotor()
        {
            btnForward.IsEnabled = false;
            btnReverse.IsEnabled = true;
            _motorControl1A.Write(GpioPinValue.High);
            _motorControl2A.Write(GpioPinValue.Low);
        }

        private void ReverseMotor()
        {
            btnReverse.IsEnabled = false;
            btnForward.IsEnabled = true;
            _motorControl1A.Write(GpioPinValue.Low);
            _motorControl2A.Write(GpioPinValue.High);
        }

        private void StopMotor()
        {
            _motorControl1A.Write(GpioPinValue.Low);
            _motorControl2A.Write(GpioPinValue.Low);
        }

        private void TurnOffIgnition()
        {
            btnIgnitionOn.IsEnabled = true;
            btnIgnitionOff.IsEnabled = false;
            btnForward.IsEnabled = false;
            btnReverse.IsEnabled = false;
            _motorEnable.Write(GpioPinValue.Low);
            _motorControl1A.Write(GpioPinValue.Low);
            _motorControl2A.Write(GpioPinValue.Low);
        }

        private void BtnIgnitionOn_Click(object sender, RoutedEventArgs e)
        {
            TurnOnIgnition();
        }

        private void BtnIgnitionOff_Click(object sender, RoutedEventArgs e)
        {
            TurnOffIgnition();
        }

        private void BtnForward_Click(object sender, RoutedEventArgs e)
        {
            ForwardMotor();
        }

        private void BtnReverse_Click(object sender, RoutedEventArgs e)
        {
            ReverseMotor();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            StopMotor();
        }

    }
}
