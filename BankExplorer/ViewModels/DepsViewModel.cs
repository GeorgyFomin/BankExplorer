using BankExplorer.Commands;
using Domain.Model;
using Persistance.Conext;
using System;
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
        private bool blockAccountEditEndingHandler;
        private bool cellEdited;
        /// <summary>
        /// Хранит ссылку на выделенный отдел.
        /// </summary>
        private Department department;
        private Visibility menuAddVisibility = Visibility.Visible;
        private RelayCommand depSelectionCommand;
        private RelayCommand depRemoveCommand;
        private RelayCommand depAddCommand;
        private RelayCommand depBeginEditCommand;
        private RelayCommand depCellEditEndCommand;
        private RelayCommand depRowEditEndCommand;
        private RelayCommand depCurrCellChangedCommand;
        private RelayCommand depAddingNewCommand;
        #endregion
        #region Properties
        public string BankName { get; set; }
        public DataContext Context { get; set; }
        /// <summary>
        /// Устанавливает и возвращает ссылку на текущий источник данных в таблице. 
        /// </summary>
        public object DataSource { get; set; }
        public Department Department { get => department; set { department = value; RaisePropertyChanged(nameof(Department)); } }
        public Visibility MenuAddVisibility { get => menuAddVisibility; set { menuAddVisibility = value; RaisePropertyChanged(nameof(MenuAddVisibility)); } }
        public ICommand DepSelectionCommand => depSelectionCommand ??= new RelayCommand(SelectDepartment);
        public ICommand DepRemoveCommand => depRemoveCommand ??= new RelayCommand(RemoveDepartmnet);
        public ICommand DepBeginEditCommand => depBeginEditCommand ??= new RelayCommand((e) => MenuAddVisibility = Visibility.Collapsed);
        public ICommand DepCellEditEndCommand => depCellEditEndCommand ??= new RelayCommand(CellEditEndDepartment);
        public ICommand DepAddCommand => depAddCommand ??= new RelayCommand(AddDep);
        public ICommand DepRowEditEndCommand => depRowEditEndCommand ??= new RelayCommand(DepRowEditEnd);
        public ICommand DepCurrCellChangedCommand => depCurrCellChangedCommand ??= new RelayCommand(DepCurrCellChanged);
        public ICommand DepAddingNewCommand => depAddingNewCommand ??= new RelayCommand(AddingNewDepartment);
        #endregion
        private void SelectDepartment(object e)
        {
            //if (endEditFlag)
            //{
            //    Context.SaveChanges();
            //    endEditFlag = false;
            //}
            Department = (e as DataGrid).SelectedItem is Department dep ? dep : null;
            //if (Department == null)
            //    MainViewModel.Log($"Добавили отдел.");
        }
        private void CellEditEndDepartment(object e)
        {
            //endEditFlag = true;
            cellEdited = true;
        }
        private void RemoveDepartmnet(object obj)
        {
            if (department == null || MessageBox.Show($"Удалить отдел {department}?", $"Удаление отдела {department}", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            foreach (Client client in department.Clients)
            {
                foreach (Account account in client.Accounts)
                {
                    Context.Accounts.Remove(account);
                }
                Context.Clients.Remove(client);
            }
            Context.Departments.Remove(department);
            Context.SaveChanges();
            MainViewModel.Log($"Удален отдел {department}");
        }
        private void DepCurrCellChanged(object e)
        {
            if (!cellEdited)
                return;
            cellEdited = false;
            (e as DataGrid).CommitEdit();
            Context.SaveChanges();
            MainViewModel.Log($"Отдел {department} отредактирован.");
            //MessageBox.Show("Cell Changed");
        }
        private void DepRowEditEnd(object e)
        {
            if (blockAccountEditEndingHandler) return;
            DataGrid grid = e as DataGrid;
            //MessageBox.Show("Row Edited");
            cellEdited = false;
            MenuAddVisibility = Visibility.Visible;
            blockAccountEditEndingHandler = true;
            grid.CommitEdit();
            Context.SaveChanges();
            MainViewModel.Log($"Отдел {Department} отредактирован.");
            //MessageBox.Show($"Отдел {Department} отредактирован.");
            blockAccountEditEndingHandler = false;
        }
        private void AddingNewDepartment(object e)
        {
            (e as DataGrid).CanUserAddRows = false;
            //MessageBox.Show("Добавлен отдел.");
            MainViewModel.Log("Добавлен отдел.");
        }
        private void AddDep(object e)
        {
            MenuAddVisibility = Visibility.Collapsed;
            (e as DataGrid).CanUserAddRows = true;
        }

    }
}
