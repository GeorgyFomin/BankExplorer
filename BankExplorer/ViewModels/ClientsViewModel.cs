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
        private Client selClient;
        private RelayCommand selCommand;
        private RelayCommand removeClientCommand;
        private RelayCommand endClientEditCommand;
        private Department department;
        private ObservableCollection<Client> dataSource;
        private bool endEditFlag;
        #endregion
        #region Properties
        public DataContext Context { get; set; }
        /// <summary>
        /// Устанавливает и возвращает ссылку на текущий источник данных в таблице. 
        /// </summary>
        public ObservableCollection<Client> DataSource { get => dataSource; set { dataSource = value; RaisePropertyChanged(nameof(DataSource)); } }
        public Department Department { get => department; set { department = value; RaisePropertyChanged(nameof(Department)); } }
        public string DepName => Department?.Name;
        public Client SelClient { get => selClient; set { selClient = value; RaisePropertyChanged(nameof(SelClient)); } }
        public ICommand SelCommand => selCommand ??= new RelayCommand(SelectClient);
        public ICommand RemoveClientCommand => removeClientCommand ??= new RelayCommand(RemoveClient);
        public ICommand EndClientEditCommand => endClientEditCommand ??= new RelayCommand(EndEditClient);

        #endregion
        public ClientsViewModel(DataContext context, Department department)
        {
            Context = context; Department = department;
            DataSource = Department.Clients;
        }
        private void SelectClient(object e)
        {
            if (endEditFlag)
            {
                Context.SaveChanges();
                endEditFlag = false;
            }
            SelClient = (e as DataGrid).SelectedItem is Client client ? client : null;
        }
        private void EndEditClient(object e)
        {
            endEditFlag = true;
            if (SelClient.Dep == null)
            {
                SelClient.Dep = Department;
                MainViewModel.Log($"В отдел {department} добавили клиента.");
                //MessageBox.Show($"В отдел {department} добавили клиента.");
            }
            else
            {
                MainViewModel.Log($"Имя клиента {selClient} отредактировано.");
                //MessageBox.Show($"В отделе {department} отредактировали клиента {selClient}.");
            }
        }
        private void RemoveClient(object e)
        {
            if (selClient != null &&
                MessageBox.Show($"Удалить клиента {selClient}?", $"Удаление клиента {selClient}", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // Удаляем все счета клиента.
                foreach (Account account in selClient.Accounts)
                    Context.Accounts.Remove(account);
                // Удаляем самого клиента.
                Context.Clients.Remove(selClient);
                // Удаляем клиента из списка клиентов отдела.
                DataSource.Remove(selClient);
                //Department.Clients.Remove(client);
                Context.SaveChanges();
                MainViewModel.Log($"Удален клиент {selClient}");
            }
        }
    }
}
