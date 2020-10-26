# EP94.LgSmartThinq
 LG SmartThinq library to communicate to LG devices through the LG api. Library is in a very early stage and only ac is currently supported.
 
 Usage:
 ```c#
 SmartThinqClient smartThinqClient = new SmartThinqClient();
 await smartThinqClient.Initialize("name@email.com", "password", "US", "en-US");
 List<Device> devices = await smartThinqClient.GetDevices();
 AcClient acClient = smartThinqClient.GetDeviceClient(devices.First()) as AcClient;
 await acClient.TurnOnAc();
```
