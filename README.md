# SnipeIT - Mosyle Sync

MosyleSync is a .NET console application that synchronizes Mosyle device data into Snipe-IT hardware assets. The main purpose is to pull iPads into Snipe with Phone Numbers from SIMs attached.

## Disclaimer

The core application was originally written by Jordan Molenaar. Codex was used to clean up, reorganize, document, and prepare the codebase for other parties to use and consume.

The tests in this repository were written by Codex. I will not guarantee any degree of accuracy, completeness, or coverage from those tests. Review the code and test behavior yourself before relying on this project in production or in another environment.

The app currently performs one sync pass:

- logs in to Mosyle.
- pulls all Mosyle device pages.
- looks up each device in Snipe-IT by serial number.
- creates missing Snipe-IT assets.
- updates tracked Snipe-IT fields when Mosyle has newer values.
- walks all Snipe-IT model pages before creating or updating assets.
- waits briefly between paged requests and retries rate-limited API calls.

## Project Layout

- `SnipeMosyle/Clients/Mosyle`: Mosyle API client, interface, and DTOs.
- `SnipeMosyle/Clients/SnipeIt`: Snipe-IT API client, interface, requests, responses, and DTOs.
- `SnipeMosyle/Sync`: sync orchestration and inventory decision logic.
- `SnipeMosyle/Options`: configuration objects bound from `appsettings.json`.
- `SnipeMosyle.Tests`: focused tests for sync decisions.

## Configuration

Create `SnipeMosyle/appsettings.json` from `SnipeMosyle/appsettings.example.json` and fill in your environment values.

```json
{
  "Mosyle": {
    "BaseUrl": "https://businessapi.mosyle.com/v1",
    "AccessToken": "your-mosyle-access-token",
    "Email": "your-mosyle-login@example.com",
    "Password": "your-mosyle-password",
    "PageDelayMilliseconds": 250,
    "RateLimitMaxRetries": 3
  },
  "SnipeIt": {
    "BaseUrl": "https://your-snipeit-instance.example.com",
    "ApiToken": "your-snipeit-api-token",
    "DefaultStatusId": 2,
    "DefaultLocationId": 13,
    "DefaultManufacturerId": 1,
    "DefaultCategoryId": 6,
    "DefaultFieldsetId": 5,
    "DefaultModelEol": 0,
    "PageDelayMilliseconds": 250,
    "RateLimitMaxRetries": 3
  }
}
```

`SnipeMosyle/appsettings.json` is ignored by Git. Keep real credentials out of commits.

The numeric Snipe-IT IDs preserve the current sync behavior:

- `DefaultStatusId`: status for newly-created assets.
- `DefaultLocationId`: location for newly-created assets.
- `DefaultManufacturerId`: manufacturer used when creating missing models.
- `DefaultCategoryId`: category used when creating missing models.
- `DefaultFieldsetId`: fieldset used when creating missing models.
- `DefaultModelEol`: EOL value sent when creating missing models.
- `PageDelayMilliseconds`: delay between successful paged requests. Defaults to `250`.
- `RateLimitMaxRetries`: retries attempted after HTTP 429 responses. Defaults to `3`; `Retry-After` is honored when present.

## Running

From the repository root:

```bash
dotnet run --project SnipeMosyle/SnipeMosyle.csproj
```

The app validates required configuration at startup. Missing values fail fast with a clear configuration key in the error message.

## Logging

Logging follows a simple pattern:

- `Information`: normal progress and successful work, such as sync start, devices pulled, asset creation, asset updates, and final summary counts.
- `Warning`: skipped or ambiguous inventory states that need review, such as missing serial numbers, archived-only matches, or duplicate active Snipe-IT assets.
- `Error`: failed work that prevents a device from being synced, such as failing to create or resolve a required model or asset.

Each run ends with a summary like:

```text
Completed sync. Processed=10, Created=2, Updated=3, Unchanged=4, Skipped=1, Errors=0.
```

## Testing and Build

Run the test suite:

```bash
dotnet test SnipeMosyle.sln
```

Build without restoring:

```bash
dotnet build SnipeMosyle.sln --no-restore
```

## Docker

Build the container image:

```bash
docker build -f SnipeMosyle/Dockerfile .
```

Or use Compose:

```bash
docker compose build
```
