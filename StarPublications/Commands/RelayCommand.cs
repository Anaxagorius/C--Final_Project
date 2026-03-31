using System;
using System.Windows.Input;

namespace StarPublications.Commands
{
    /// <summary>
    /// A reusable <see cref="ICommand"/> implementation that delegates execution
    /// and can-execute logic to caller-provided delegates.
    /// Implements the Command pattern for use in MVVM ViewModels.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        /// <summary>
        /// Event raised when the ability to execute the command changes.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RelayCommand"/>.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <param name="canExecute">Optional predicate that determines if the command can execute.</param>
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Determines whether the command can execute.
        /// </summary>
        public bool CanExecute(object? parameter) =>
            _canExecute == null || _canExecute(parameter);

        /// <summary>
        /// Executes the command.
        /// </summary>
        public void Execute(object? parameter) => _execute(parameter);

        /// <summary>
        /// Raises <see cref="CanExecuteChanged"/> to re-evaluate command availability.
        /// </summary>
        public void RaiseCanExecuteChanged() =>
            CommandManager.InvalidateRequerySuggested();
    }
}
