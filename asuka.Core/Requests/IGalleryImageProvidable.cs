namespace asuka.Core.Requests;

public interface IGalleryImageProvidable
{
    /// <summary>
    /// Determines which provider is this belongs to.
    /// </summary>
    /// <returns></returns>
    ProviderData ProviderFor();
}
