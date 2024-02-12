namespace ParseidonJson.core;

public class ConsumableEvent<T>
{
    private bool _consumed = false;
    private T _value;

    public T Value
    {
        get
        {
            if (_consumed)
            {
                return default; // Returns null for reference types or the default value for value types
            }
            _consumed = true;
            return _value;
        }
        set
        {
            _value = value;
            _consumed = false;
        }
    }

    public ConsumableEvent(T value)
    {
        _value = value;
    }

    public static ConsumableEvent<T> Create(T value) => new(value);

    // Method to handle the event, executing the action if the event has not been consumed
    public void HandleEvent(Action<T> action)
    {
        if (!_consumed && _value != null)
        {
            action(_value);
            _consumed = true;
        }
    }
}
