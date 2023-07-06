using System;

namespace asuka.Application.Output.Progress;

public interface IProgressProvider
{
    /// <summary>
    /// Increments progress by 1
    /// </summary>
    void Tick();

    /// <summary>
    /// Increments progress and changes the maximum progress to a new one.
    /// </summary>
    /// <param name="newMaxTicks"></param>
    void Tick(int newMaxTicks);

    /// <summary>
    /// Increments progress and changes the text on the progress.
    /// </summary>
    /// <param name="message"></param>
    void Tick(string message);

    /// <summary>
    /// Increments progress and changes the maximum progress and the text.
    /// </summary>
    /// <param name="newMaxTicks"></param>
    /// <param name="message"></param>
    void Tick(int newMaxTicks, string message);
    
    /// <summary>
    /// Marks the progress bar as closed.
    /// </summary>
    void Close();
    
    /// <summary>
    /// Returns the status whether if the progress is closed or not.
    /// </summary>
    /// <returns></returns>
    bool IsClosed();
    
    /// <summary>
    /// Spawns child progressbar.
    /// </summary>
    /// <param name="maxTicks"></param>
    /// <param name="message"></param>
    /// <returns>Child progressbar</returns>
    IProgressProvider Spawn(int maxTicks, string message);
}
