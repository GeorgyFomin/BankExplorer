using System;
using System.Windows.Input;

namespace BankExplorer.Commands
{
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// Хранит ссылку на делегат, содержащий код действия, определенного для данного объекта-команды.
        /// </summary>
        private readonly Action<object> action;

        /// <summary>
        /// Хранит ссылку на метод, определяющий может ли команда быть выполненной.
        /// </summary>
        private readonly Func<object, bool> canExecute;
        /// <summary>
        /// Инициализирует объект-команду, запоминая ссылку на делегат, содержащий код действия.
        /// </summary>
        /// <param name="action">Делегат действия, которое предполагает выполнение команды.</param>
        /// <param name="canExecute">Метод, выполняющийся для проверки условия выполнения команды.</param>
        public RelayCommand(Action<object> action, Func<object, bool> canExecute = null)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Добавляет/удаляет делегат, выполняющийся при изменении условия выполнения команды.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        /// <summary>
        /// Определяет допустимость выполнения команды.
        /// </summary>
        /// <param name="parameter">Параметр метода, определяющего условие выполнения команды.</param>
        /// <returns>true, если выполнение команды разрешено; false - в противном случае.</returns>
        public bool CanExecute(object parameter) => canExecute == null || canExecute(parameter);
        /// <summary>
        /// Выполняет команду, вызывая делегата объекта.
        /// </summary>
        /// <param name="parameter">Параметр делегата.</param>
        public void Execute(object parameter) => action?.Invoke(parameter);
    }
}
