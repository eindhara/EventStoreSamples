#r @"..\packages\EventStore.Client.3.9.2\lib\net40\EventStore.ClientAPI.dll"
open System
open EventStore.ClientAPI
open System.Text

let store = 
    (ConnectionSettings.Create()
        .UseConsoleLogger()
        .Build(),
     System.Net.IPEndPoint(System.Net.IPAddress.Parse "127.0.0.1", 1113))
    |> EventStoreConnection.Create

store.ConnectAsync().Wait()

//let g = Guid.NewGuid()

// Post to ES
let myEvent = new EventData(Guid.NewGuid(), "testEvent", false,
                            Encoding.UTF8.GetBytes("some data 2"),
                            Encoding.UTF8.GetBytes("some metadata 2"));

store.AppendToStreamAsync("test-stream",ExpectedVersion.Any, myEvent).Wait();

let streamEvents =
    store.ReadStreamEventsForwardAsync("test-stream", 0, 1, false).Result;

// Read from ES?
let re = streamEvents.Events.[0].Event

re.Data |> Encoding.UTF8.GetString
re.Metadata |> Encoding.UTF8.GetString


