using System.ComponentModel;
using System.Reactive.Linq;

namespace ParseidonJson.core;

public class LoginViewModel
{
    private readonly ModelStore<ViewState> _modelStore = new(new ViewState());
    public IObservable<ViewState> viewState { get; }

    public LoginViewModel()
    {
        viewState = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                handler => _modelStore.PropertyChanged += handler,
                handler => _modelStore.PropertyChanged -= handler)
            .Where(e => e.EventArgs.PropertyName == nameof(ModelStore<ViewState>.State))
            .Select(_ => _modelStore.State)
            .StartWith(_modelStore.State); 
    }

    public void MakeLoading()
    {
        _modelStore.Update(oldState => oldState with { IsLoading = true });
    }

    public void HandleAction(object action)
    {
        switch (action)
        {
            case Action.EmailChanged emailChanged:
                emailUpdate(emailChanged.Email);
                break;
            case Action.PasswordChanged passwordChanged:
                passwordUpdate(passwordChanged.Password);
                break;
            case Action.FormValidationChanged formValidationChanged:
                formValidationUpdate(formValidationChanged.IsFormValid);
                break;
        }
    }

    private void formValidationUpdate(bool isFormValid)
    {
        _modelStore.Update(oldState => oldState with { IsFormValid = isFormValid });
    }

    private void passwordUpdate(string passwordChangedPassword)
    {
        _modelStore.Update(oldState => oldState with { Password = passwordChangedPassword });
    }

    private void emailUpdate(string emailChanged)
    {
        _modelStore.Update(oldState => oldState with { Email = emailChanged });
    }

    private void MakeLoginFailed()
    {
        _modelStore.Update(oldState => oldState with
        {
            evt = ConsumableEvent<Event>.Create(new Event.LoginFailed("Login failed"))
        });
    }
    
    public record ViewState(
        bool IsLoading = false, 
        string Email = "", 
        string Password = "", 
        bool IsFormValid = false,   
        ConsumableEvent<Event> evt = null
    );

    public sealed class Action
    {
        public record EmailChanged(string Email);
        public record PasswordChanged(string Password);
        public record FormValidationChanged(bool IsFormValid);
    }

    public abstract class Event
    {
        public class Default : Event { }
        public class LoginFailed : Event 
        {
            public string Message { get; }
            public LoginFailed(string message) => Message = message;
        }
    }


}