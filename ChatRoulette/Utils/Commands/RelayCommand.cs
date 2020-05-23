using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatRoulette.Utils.Commands
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        private readonly Func<Task> _methodToExecute;
        private readonly Func<bool> _canExecuteEvaluator;

        public RelayCommand(Func<Task> methodToExecute, Func<bool> canExecuteEvaluator)
        {
            this._methodToExecute = methodToExecute;
            this._canExecuteEvaluator = canExecuteEvaluator;
        }

        public RelayCommand(Func<Task> methodToExecute)
            : this(methodToExecute, null)
        {
        }

        public bool CanExecute(object parameter)
        {
            if (this._canExecuteEvaluator == null)
                return true;

            var result = this._canExecuteEvaluator.Invoke();
            return result;
        }

        public async void Execute(object parameter)
        {
            await this._methodToExecute();
        }
    }
}