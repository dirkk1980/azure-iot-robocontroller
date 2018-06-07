using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using NetworkLibrary;

namespace RoboController
{
    public sealed partial class MainPage : Page
    {

        DeviceClient deviceClient;

        public static async Task CallOnUiThreadAsync(CoreDispatcher dispatcher, DispatchedHandler handler) =>
        await dispatcher.RunAsync(CoreDispatcherPriority.Normal, handler);

        public static async Task CallOnMainViewUiThreadAsync(DispatchedHandler handler) =>
        await CallOnUiThreadAsync(CoreApplication.MainView.CoreWindow.Dispatcher, handler);

        public MainPage()
        {
            this.InitializeComponent();
            deviceClient = DeviceClient.CreateFromConnectionString(GlobalConstant.DEVICE_CONNECTION_STRING, TransportType.Mqtt);
            deviceClient.SetMethodHandlerAsync("Drive", Drive, null);
        }

        private async Task<MethodResponse> Drive(MethodRequest methodRequest, object userContext)
        {
            var data = methodRequest.DataAsJson;
            DriveCommandData cmdData = JsonConvert.DeserializeObject<DriveCommandData>(data);
            await CallOnMainViewUiThreadAsync(() =>
            {
                IncomingMessage.Text = "drive"+" "+"direction: "+ cmdData.direction + " "+"speed: " + cmdData.speed + " " + "distance: " + cmdData.distance;
            });
            string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
            return new MethodResponse(Encoding.UTF8.GetBytes(result), 200);
        }

    }
}
