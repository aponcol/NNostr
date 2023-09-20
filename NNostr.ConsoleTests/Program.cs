// See https://aka.ms/new-console-template for more information
using NBitcoin.Secp256k1;
using NNostr.Client;

// Load a private key from a hex string
var testPrivKey = "0c0cf4dba557094eea6341e8f0b0b7eaf71d90a465104359bf930705676d4d3f";
var testPublicKey = "381af99c0282f71df21fa57d38bf7d2cb8f8609fc3ffe3117ec5236d8a99fea2";
//var relayForTest = new Uri("wss://localhost:5001");
var relayForTest = new Uri("wss://pretty-jaybird-causal.ngrok-free.app");

// TODO:  Fix Readme if this works
var client = new NostrClient(relayForTest);
await client.Connect();

var thisSubsId = Guid.NewGuid().ToString();
Console.WriteLine($"Starting NNostr Client. Subscription: {thisSubsId}");
Console.WriteLine("Press 'ESC' to exit the process...");

SubscribeToNotices(client);
await CreateSubscription(testPublicKey, client, thisSubsId);
SubscribeToEvents(client, thisSubsId);

var message = "Starting... ";
await SendMessageEvent(message, client, testPrivKey);

// Keep the program running and listening to events
// here it ask to press "ESC" to exit
while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
{
    await Task.Delay(5000); // Wait for 1 second before checking for events again
}

await client.CloseSubscription(thisSubsId);
Console.WriteLine($"Closed Subscription: {thisSubsId}");


static async Task SendMessageEvent(string message, NostrClient client, string testPrivKey)
{
    var key = Context.Instance.CreateECPrivKey(Convert.FromHexString(testPrivKey));

    // create a new event
    var newEvent = new NostrEvent()
    {
        Kind = 1,
        Content = $"{message} {DateTime.Now:f}"
    };
    // sign the event
    await newEvent.ComputeIdAndSignAsync(key);
    // send the event
    await client.SendEventsAndWaitUntilReceived(new[] { newEvent }, CancellationToken.None);
}

static void SubscribeToEvents(NostrClient? client, string thisSubsId)
{
    void OnClientOnEventsReceived(object sender, (string subscriptionId, NostrEvent[] events) args)
    {
        if (args.subscriptionId == thisSubsId)
        {
            foreach (var nostrEvent in args.events)
            {
                Console.WriteLine($"Event: {nostrEvent.Content}");
            }
        }
    }
    client.EventsReceived += OnClientOnEventsReceived;
}

static void SubscribeToNotices(NostrClient? client)
{
    void OnClientOnNoticeReceived(object sender, string notice)
    {
        Console.WriteLine($"Notice: {notice}");
    }
    client.NoticeReceived += OnClientOnNoticeReceived;
}

static async Task CreateSubscription(string testPublicKey, NostrClient client, string thisSubsId)
{
    await client.CreateSubscription(thisSubsId, new[]
        {
        new NostrSubscriptionFilter()
        {
            Kinds = new []{1},
            Authors = new []{ testPublicKey },
            Since = new DateTimeOffset(DateTime.Now.AddHours(-1))
        }
});
}