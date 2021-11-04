using BankExplorer.Commands;
using Domain.Model;
using Persistance.Conext;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BankExplorer.ViewModels
{
    public class ClientsViewModel : ViewModelBase
    {
        #region Fields
        private bool cellEdited;
        private Client client;
        private RelayCommand clientSelectedCommand;
        private RelayCommand clientRemoveCommand;
        private RelayCommand clientCellEditEndCommand;
        private RelayCommand clientAddCommand;
        private RelayCommand clientEditBeginCommand;
        private RelayCommand clientRowEditEndCommand;
        private RelayCommand clientAddNewCommand;
        private RelayCommand clientCurrCellChangedCommand;
        private bool blockAccountEditEndingHandler;
        private Department department;
        private ObservableCollection<Client> dataSource;
        private Visibility menuAddVisibility = Visibility.Visible;
        #endregion
        #region Properties
        public DataContext Context { get; set; }
        /// <summary>
        /// Устанавливает и возвращает ссылку на текущий источник данных в таблице. 
        /// </summary>
        public ObservableCollection<Client> DataSource { get => dataSource; set { dataSource = value; RaisePropertyChanged(nameof(DataSource)); } }
        public Department Department { get => department; set { department = value; RaisePropertyChanged(nameof(Department)); } }
        public Visibility MenuAddVisibility { get => menuAddVisibility; set { menuAddVisibility = value; RaisePropertyChanged(nameof(MenuAddVisibility)); } }
        public string DepName => Department?.Name;
        public Client Client { get => client; set { client = value; RaisePropertyChanged(nameof(Client)); } }
        public ICommand ClientSelectedCommand => clientSelectedCommand ??= new RelayCommand((e) => Client = (e as DataGrid).SelectedItem is Client client ? client : null);
        public ICommand ClientRemoveCommand => clientRemoveCommand ??= new RelayCommand(RemoveClient);
        public ICommand ClientCellEditEndCommand => clientCellEditEndCommand ??= new RelayCommand((e) => cellEdited = true);
        public ICommand ClientCurrCellChangedCommand => clientCurrCellChangedCommand ??= new RelayCommand(ClientCurrCellChanged);
        public ICommand ClientAddCommand => clientAddCommand ??= new RelayCommand(ClientAdd);
        public ICommand ClientEditBeginCommand => clientEditBeginCommand ??= new RelayCommand((e) => MenuAddVisibility = Visibility.Collapsed);
        public ICommand ClientAddNewCommand => clientAddNewCommand ??= new RelayCommand(ClientAddNew);
        public ICommand ClientRowEditEndCommand => clientRowEditEndCommand ??= new RelayCommand(ClientRowEditEnd);
        #endregion
        public ClientsViewModel(DataContext context, Department department)
        {
            Context = context; Department = department;
            DataSource = Department.Clients;
        }
        private void RemoveClient(object e)
        {
            if (client != null &&
                MessageBox.Show($"Удалить клиента {client}?", $"Удаление клиента {client}", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // Удаляем все счета клиента.
                foreach (Account account in client.Accounts)
                    Context.Accounts.Remove(account);
                // Удаляем самого клиента.
                Context.Clients.Remove(client);
                // Удаляем клиента из списка клиентов отдела.
                DataSource.Remove(client);
                //Department.Clients.Remove(client);
                Context.SaveChanges();
                MainViewModel.Log($"Удален клиент {client}");
            }
        }
        private void ClientAdd(object e)
        {
            MenuAddVisibility = Visibility.Collapsed;
            (e as DataGrid).CanUserAddRows = true;
        }
        private void ClientAddNew(object e)
        {
            (e as DataGrid).CanUserAddRows = false;
            //MessageBox.Show("New Client");
        }
        private void ClientRowEditEnd(object e)
        {
            if (blockAccountEditEndingHandler) return;
            DataGrid grid = e as DataGrid;
            //MessageBox.Show("Row Edited");
            cellEdited = false;
            bool clientAdded;
            if (clientAdded = Client.Department == null)
            {
                Client.Department = Department;
            }
            MenuAddVisibility = Visibility.Visible;
            blockAccountEditEndingHandler = true;
            grid.CommitEdit();
            if (clientAdded)
            {
                Context.Clients.Add(client);
                MainViewModel.Log($"В отдел {department} добавили клиента {client}.");
                //MessageBox.Show($"В отдел {department} добавили клиента {client}.");
            }
            else
            {
                MainViewModel.Log($"В отделе {department} отредактирован клиент {client}.");
                //MessageBox.Show($"В отделе {department} отредактирован клиент {client}.");
            }
            Context.SaveChanges();
            blockAccountEditEndingHandler = false;
        }
        private void ClientCurrCellChanged(object e)
        {
            if (!cellEdited)
                return;
            cellEdited = false;
            (e as DataGrid).CommitEdit();
            Context.SaveChanges();
            MainViewModel.Log($"Клиент {client} отредактирован.");
            //MessageBox.Show("Cell Changed");
        }
    }
}
