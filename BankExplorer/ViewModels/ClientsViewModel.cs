using BankExplorer.Commands;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistance.Conext;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BankExplorer.ViewModels
{
    public class ClientsViewModel : ViewModelBase
    {
        #region Fields
        private bool added, toAdd;
        private Client client;
        private Visibility removeVisibility = Visibility.Collapsed;
        private Visibility addVisibility = Visibility.Visible;
        private RelayCommand selCommand;
        private RelayCommand removeClientCommand;
        private RelayCommand endClientEditCommand;
        private RelayCommand addClientCommand;
        //private RelayCommand clientAddedCommand;
        private RelayCommand beginningEdit;
        #endregion
        #region Properties
        public DataContext Context { get; set; }
        /// <summary>
        /// Устанавливает и возвращает ссылку на текущий источник данных в таблице. 
        /// </summary>
        public ObservableCollection<Client> DataSource { get; set; }
        public Department Department { get; set; }
        public string DepName => Department?.Name;
        public Visibility RemoveVisibility { get => removeVisibility; set { removeVisibility = value; RaisePropertyChanged(nameof(RemoveVisibility)); } }
        //public ObservableCollection<Client> Clients { get => Department?.Clients; }
        public Visibility AddVisibility { get => addVisibility; set { addVisibility = value; RaisePropertyChanged(nameof(AddVisibility)); } }
        public Client Client { get => client; set { client = value; RaisePropertyChanged(nameof(Client)); } }
        public ICommand SelCommand => selCommand ??= new RelayCommand((e) =>
        {
            if (added)
            {
                toAdd = added = false;
                (e as DataGrid).CanUserAddRows = false;
            }
            Client = (e as DataGrid).SelectedItem is Client client ? client : null;
            RemoveVisibility = Client != null ? Visibility.Visible : Visibility.Collapsed;
        });
        public ICommand RemoveClientCommand => removeClientCommand ??= new RelayCommand(RemoveClient);
        public ICommand EndClientEditCommand => endClientEditCommand ??= new RelayCommand((e) =>
        {
            if (toAdd)
            {
                added = true;
                MainViewModel.Log($"Добавили клиента {client}.");
                Department.Clients.Add(client);
                //Clients.Add(client);
                //Department.Clients.Add(client);
                //RaisePropertyChanged(nameof(DataSource));
                //                DataSource.Add(client);
                //MessageBox.Show($"Добавляем {client}");
                //Context.SaveChanges();
            }
            else
            {
                MainViewModel.Log($"Имя клиента {client} отредактировано.");
                //MessageBox.Show($"Редактируем {client}");
                //RaisePropertyChanged(nameof(Client));
            }
            AddVisibility = Visibility.Visible;

            //Context.SaveChanges();
        });
        public ICommand AddClientCommand => addClientCommand ??= new RelayCommand((e) =>
        {
            toAdd = true;// Собираемся добавить нового клиента
            AddVisibility = Visibility.Collapsed;
            (e as DataGrid).CanUserAddRows = true;
        });
        //public ICommand ClientAddedCommand => clientAddedCommand ??= new RelayCommand(ClientAdded);
        public ICommand BeginningEdit => beginningEdit ??= new RelayCommand((e) => AddVisibility = Visibility.Collapsed);
        #endregion
        //public ClientsViewModel()
        //{
        //}
        public ClientsViewModel(DataContext context, Department department)
        {
            Context = context; Department = department;
            //Context.Clients.Load();
            DataSource = new ObservableCollection<Client>(Context.Clients.Local.ToObservableCollection().Where((e) => e.Dep == Department));
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
        //private void ClientAdded(object e)
        //{
        //    (e as DataGrid).CanUserAddRows = false;
        //    MainViewModel.Log($"В отдел добавлен новый клиент.");
        //}
    }
}
