using BankExplorer.Commands;
using Domain.Model;
using Persistance.Conext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BankExplorer.ViewModels
{
    public class DepsViewModel : ViewModelBase
    {
        #region Fields
        private bool added, toAdd;
        private Department dep;
        private Visibility removeVisibilty = Visibility.Collapsed;
        private RelayCommand selCommand;
        private RelayCommand removeDepCommand;
        private RelayCommand endDepEditCommand;
        private RelayCommand addDepCommand;
        private Visibility addVisibility = Visibility.Visible;
        private RelayCommand beginEdit;
        #endregion
        #region Properties
        public string BankName { get; set; }
        public DataContext Context { get; set; }
        /// <summary>
        /// Устанавливает и возвращает ссылку на текущий источник данных в таблице. 
        /// </summary>
        public object DataSource { get; set; }
        public Department Dep { get => dep; set { dep = value; RaisePropertyChanged(nameof(Dep)); } }
        public Visibility RemoveVisibility { get => removeVisibilty; set { removeVisibilty = value; RaisePropertyChanged(nameof(RemoveVisibility)); } }
        public Visibility AddVisibility { get => addVisibility; set { addVisibility = value; RaisePropertyChanged(nameof(AddVisibility)); } }
        public ICommand SelCommand => selCommand ??= new RelayCommand((e) =>
        {
            if (added)
            {
                toAdd = added = false;
                (e as DataGrid).CanUserAddRows = false;
            }
            Dep = (e as DataGrid).SelectedItem is Department dep ? dep : null;
            RemoveVisibility = Dep != null ? Visibility.Visible : Visibility.Collapsed;
        });
        public ICommand RemoveDepCommand => removeDepCommand ??= new RelayCommand(RemoveDep);
        public ICommand EndDepEditCommand => endDepEditCommand ??= new RelayCommand((e) =>
        {
            if (toAdd)
            {
                added = true;
                MainViewModel.Log($"Добавили отдел {dep}.");
            }
            else
            {
                MainViewModel.Log($"Имя отдела {dep} отредактировано.");
            }
            AddVisibility = Visibility.Visible;
        });
        public ICommand AddDepCommand => addDepCommand ??= new RelayCommand((e) =>
        {
            toAdd = true;// Собираемся добавить новый отдел
            AddVisibility = Visibility.Collapsed;
            (e as DataGrid).CanUserAddRows = true;
        });
        public ICommand BeginEdit => beginEdit ??= new RelayCommand((e) => AddVisibility = Visibility.Collapsed);
        #endregion
        public DepsViewModel() { }
        private void RemoveDep(object obj)
        {
            if (dep != null &&
                MessageBox.Show($"Удалить отдел {dep}?", $"Удаление отдела {dep}", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                foreach (Client client in dep.Clients)
                {
                    foreach (Account account in client.Accounts)
                    {
                        Context.Accounts.Remove(account);
                    }
                    Context.Clients.Remove(client);
                }
                Context.Departments.Remove(dep);
                Context.SaveChanges();
                MainViewModel.Log($"Удален отдел {dep}");
            }
        }
    }
}
