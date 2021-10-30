using BankExplorer.Commands;
using Domain.Model;
using Persistance.Conext;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BankExplorer.ViewModels
{
    public class DepsViewModel : ViewModelBase
    {
        #region Fields
        /// <summary>
        /// Хранит флаг завершения редактирования или добавления отдела в таблицу.
        /// </summary>
        private bool endEditFlag;
        /// <summary>
        /// Хранит ссылку на выделенный отдел.
        /// </summary>
        private Department selDep;
        private RelayCommand selCommand;
        private RelayCommand removeDepCommand;
        private RelayCommand endDepEditCommand;
        #endregion
        #region Properties
        public string BankName { get; set; }
        public DataContext Context { get; set; }
        /// <summary>
        /// Устанавливает и возвращает ссылку на текущий источник данных в таблице. 
        /// </summary>
        public object DataSource { get; set; }
        public Department SelDep { get => selDep; set { selDep = value; RaisePropertyChanged(nameof(SelDep)); } }
        public ICommand SelCommand => selCommand ??= new RelayCommand(SelectDepartment);
        public ICommand RemoveDepCommand => removeDepCommand ??= new RelayCommand(RemoveDepartmnet);
        public ICommand EndDepEditCommand => endDepEditCommand ??= new RelayCommand(EndEditDepartment);
        #endregion
        private void SelectDepartment(object e)
        {
            if (endEditFlag)
            {
                Context.SaveChanges();
                endEditFlag = false;
            }
            SelDep = (e as DataGrid).SelectedItem is Department dep ? dep : null;
            if (SelDep == null)
                MainViewModel.Log($"Добавили отдел.");
        }
        private void EndEditDepartment(object e)
        {
            endEditFlag = true;
            MainViewModel.Log($"Имя отдела {selDep} отредактировано.");
        }
        private void RemoveDepartmnet(object obj)
        {
            if (selDep != null &&
                MessageBox.Show($"Удалить отдел {selDep}?", $"Удаление отдела {selDep}", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                foreach (Client client in selDep.Clients)
                {
                    foreach (Account account in client.Accounts)
                    {
                        Context.Accounts.Remove(account);
                    }
                    Context.Clients.Remove(client);
                }
                Context.Departments.Remove(selDep);
                Context.SaveChanges();
                MainViewModel.Log($"Удален отдел {selDep}");
            }
        }
    }
}
