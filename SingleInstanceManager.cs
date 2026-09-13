namespace Rounder.Windows;

public sealed class SingleInstanceManager : IDisposable
{
    private const string MutexName = @"Local\Rounder.Windows.Singleton";
    private const string ActivationEventName = @"Local\Rounder.Windows.Activate";

    private readonly Mutex mutex;
    private readonly EventWaitHandle activationEvent;
    private RegisteredWaitHandle? registeredWait;

    public SingleInstanceManager()
    {
        mutex = new Mutex(initiallyOwned: true, MutexName, out var createdNew);
        IsFirstInstance = createdNew;
        activationEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ActivationEventName);
    }

    public bool IsFirstInstance { get; }

    public void StartListening(Action activationRequested)
    {
        if (!IsFirstInstance)
        {
            return;
        }

        registeredWait = ThreadPool.RegisterWaitForSingleObject(
            activationEvent,
            (_, timedOut) =>
            {
                if (!timedOut)
                {
                    activationRequested();
                }
            },
            null,
            Timeout.Infinite,
            executeOnlyOnce: false);
    }

    public void SignalActivate()
    {
        activationEvent.Set();
    }

    public void Dispose()
    {
        registeredWait?.Unregister(null);
        activationEvent.Dispose();
        if (IsFirstInstance)
        {
            try
            {
                mutex.ReleaseMutex();
            }
            catch (ApplicationException)
            {
            }
        }

        mutex.Dispose();
    }
}
