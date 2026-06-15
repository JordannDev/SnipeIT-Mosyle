// Copyright (c) Jordan Molenaar (jordanm.cloud). All rights reserved.

namespace jordanm.cloud.mosyle.Sync;

/// <summary>
/// Coordinates the Mosyle-to-Snipe-IT device synchronization workflow.
/// </summary>
public interface IDeviceSyncService
{
    /// <summary>
    /// Runs one synchronization pass.
    /// </summary>
    Task RunAsync(CancellationToken cancellationToken = default);
}
