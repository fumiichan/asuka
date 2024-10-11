using System;

namespace asuka.Application.Services.Downloader;

internal sealed class DownloaderConfiguration
{
    public required string? OutputPath { get; set; }
    
    public bool Pack { get; set; }
    public bool SaveMetadata { get; set; }
}
