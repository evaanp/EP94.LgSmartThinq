# EP94.LgSmartThinq


Package is depricated, please use https://github.com/evaanp/EP94.ThinqSharp instead.



 LG SmartThinq library to communicate to LG devices through the LG api Note: it is tested with apiv2, not v1. Library is in a very early stage and only ac is currently supported.
 
 Please feel free to contribute if you have improvements, bugfixes, or want to create a client for a device other than an ac.
 
 Install:
 ```
 dotnet add package EP94.LgSmartThinq --version 0.1.1
 ```
 
 Usage:
 ```c#
 SmartThinqLogger.OnNewLogMessage += (message, logLevel, args) =>
 {
     Console.WriteLine(message, args);
 };
 SmartThinqClient smartThinqClient = new SmartThinqClient();
 smartThinqClient.OnInitializationSuccessful += async () =>
 {
     List<Device> devices = await smartThinqClient.GetDevices();
     Device device = devices.First();
     var client = smartThinqClient.GetDeviceClient(device) as AcClient;

     Snapshot desired = new Snapshot()
     {
         IsOn = true,
         TargetTemperature = 25
     };
     bool success = await client.SetSnapshot(desired);
     Console.WriteLine("Success: " + success);
 };
 await smartThinqClient.Initialize("name@email.com", "password", "US", "en-US");
```
