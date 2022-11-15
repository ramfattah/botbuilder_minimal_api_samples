using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using TeamsAuth.Bots;
using TeamsAuth.Dialogs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
});

// Create the Bot Framework Authentication to be used with the Bot Adapter.
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Create the Bot Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();


// Create the storage we'll be using for User and Conversation state, as well as Single Sign On.
// (Memory is great for testing purposes.)
builder.Services.AddSingleton<IStorage, MemoryStorage>();

// For SSO, use CosmosDbPartitionedStorage

/* COSMOSDB STORAGE - Uncomment the code in this section to use CosmosDB storage */

// var cosmosDbStorageOptions = new CosmosDbPartitionedStorageOptions()
// {
//     CosmosDbEndpoint = "<endpoint-for-your-cosmosdb-instance>",
//     AuthKey = "<your-cosmosdb-auth-key>",
//     DatabaseId = "<your-database-id>",
//     ContainerId = "<cosmosdb-container-id>"
// };
// var storage = new CosmosDbPartitionedStorage(cosmosDbStorageOptions);

/* END COSMOSDB STORAGE */

// Create the User state. (Used in this bot's Dialog implementation.)
builder.Services.AddSingleton<UserState>();

// Create the Conversation state. (Used by the Dialog system itself.)
builder.Services.AddSingleton<ConversationState>();

// The Dialog that will be run by the bot.
builder.Services.AddSingleton<MainDialog>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot, TeamsBot<MainDialog>>();

var app = builder.Build();

app.MapPost("/api/messages",
    async (IBotFrameworkHttpAdapter adapter, IBot bot, HttpRequest request, HttpResponse response) =>
    {
        await adapter.ProcessAsync(request, response, bot);
    });

app.Run("http://localhost:3978");
