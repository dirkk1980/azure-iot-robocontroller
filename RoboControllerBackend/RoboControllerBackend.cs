using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using NetworkLibrary;
using Newtonsoft.Json;

namespace RoboControllerBackend
{
    class RoboControllerBackend
    {
        private static ServiceClient s_serviceClient;  

        private static async Task InvokeMethod(DriveCommandData data)
        {
            string payload = JsonConvert.SerializeObject(data);
            var methodInvocation = new CloudToDeviceMethod("Drive") { ResponseTimeout = TimeSpan.FromSeconds(30) };

            methodInvocation.SetPayloadJson(payload);

            var response = await s_serviceClient.InvokeDeviceMethodAsync("PI3", methodInvocation);

            Console.WriteLine("Response status: {0}, payload:", response.Status);
            Console.WriteLine(response.GetPayloadAsJson());
        }

        private static void Main(string[] args)
        {
            DriveCommandData data = new DriveCommandData();
            Console.WriteLine("Please enter a direction (forward = 1, backward = 2)");
            data.direction = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Please enter speed (1-100)");
            data.speed = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Please enter travelDistance");
            data.distance = Convert.ToInt32(Console.ReadLine());
            s_serviceClient = ServiceClient.CreateFromConnectionString(GlobalConstant.SERVICE_CONNECTION_STRING);
            InvokeMethod(data).Wait();
        }
    }
}
