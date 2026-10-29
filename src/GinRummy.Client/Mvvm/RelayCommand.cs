using System;
using System.Windows.Input;

namespace GinRummy.Client.Mvvm
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            _execute = parameter => execute();
            _canExecute = canExecute;
        }

        // The rows of a list share one command, so the row the player acted on arrives as the parameter.
        public RelayCommand(Action<object> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            bool canExecute = true;
            if (_canExecute != null)
            {
                canExecute = _canExecute();
            }

            return canExecute;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
