// Copyright (c) Jordan Molenaar (jordanm.cloud). All rights reserved.

using jordanm.cloud.mosyle.Clients.Mosyle;
using jordanm.cloud.mosyle.Clients.SnipeIt;
using jordanm.cloud.mosyle.Options;
using jordanm.cloud.mosyle.Sync;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// Program intentionally stays thin: service registration lives here, while sync behavior
// lives in DeviceSyncService so it can be tested without touching real APIs.
var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Host.CreateApplicationBuilder loads appsettings.json, appsettings.{Environment}.json,
// environment variables, and command-line args by default. Runtime credentials and
// instance URLs are configuration-driven so organization-specific values do not
// live in code.
builder.Services.Configure<MosyleOptions>(builder.Configuration.GetSection(MosyleOptions.SectionName));
builder.Services.Configure<SnipeItOptions>(builder.Configuration.GetSection(SnipeItOptions.SectionName));

builder.Services.AddSingleton<IMosyleClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<MosyleOptions>>().Value;
    ValidateMosyleOptions(options);
    return new MosyleClient(options.AccessToken, options.BaseUrl);
});

builder.Services.AddSingleton<ISnipeItClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<SnipeItOptions>>().Value;
    ValidateSnipeItOptions(options);
    return new SnipeItClient(options.BaseUrl, options.ApiToken);
});

builder.Services.AddSingleton<IDeviceSyncService, DeviceSyncService>();

using var host = builder.Build();
await host.Services.GetRequiredService<IDeviceSyncService>().RunAsync();

static void ValidateMosyleOptions(MosyleOptions options)
{
    EnsureConfigured(options.BaseUrl, $"{MosyleOptions.SectionName}:{nameof(MosyleOptions.BaseUrl)}");
    EnsureConfigured(options.AccessToken, $"{MosyleOptions.SectionName}:{nameof(MosyleOptions.AccessToken)}");
    EnsureConfigured(options.Email, $"{MosyleOptions.SectionName}:{nameof(MosyleOptions.Email)}");
    EnsureConfigured(options.Password, $"{MosyleOptions.SectionName}:{nameof(MosyleOptions.Password)}");
}

static void ValidateSnipeItOptions(SnipeItOptions options)
{
    EnsureConfigured(options.BaseUrl, $"{SnipeItOptions.SectionName}:{nameof(SnipeItOptions.BaseUrl)}");
    EnsureConfigured(options.ApiToken, $"{SnipeItOptions.SectionName}:{nameof(SnipeItOptions.ApiToken)}");
}

static void EnsureConfigured(string value, string key)
{
    if (string.IsNullOrWhiteSpace(value))
        throw new InvalidOperationException($"Missing required configuration value '{key}'.");
}
