using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ParseidonJson.core;

public class ModelStore<T> : INotifyPropertyChanged where T : class
{
    private T _state;
    public T State
    {
        get => _state;
        private set
        {
            _state = value;
            OnPropertyChanged(nameof(State));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public ModelStore(T initialState)
    {
        _state = initialState;
    }

    public void Update(Func<T, T> updateFunc)
    {
        State = updateFunc(_state);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}