using BankExplorer.Commands;
using BankExplorer.View;
using Domain.Model;
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
    public class AccountsViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<Account> dataSource;
        /// <summary>
        /// Блокирует обработчик редактирования строки.
        /// </summary>
        private bool blockAccountEditEndingHandler;
        private Client client;
        private Account account;
        private Account targetAccount;
        private Visibility rightPanelVisibility = Visibility.Hidden;
        private Visibility menuItemsVisibility = Visibility.Collapsed;
        private Visibility menuItemAccountAddVisibility = Visibility.Visible;
        private bool transferEnabled;
        private decimal transferAmount;
        private RelayCommand accountSelectionChangedCommand;
        private RelayCommand removeAccCommand;
        private RelayCommand accountAddCommand;
        private RelayCommand accountAddingCommand;
        private RelayCommand accountEditBeginCommand;
        private RelayCommand accountEditEndingCommand;
        private RelayCommand transferCommand;
        private RelayCommand targetTransferAccountSelectionChangedCommand;
        private RelayCommand showTargetTransferAccountCommand;
        #endregion
        #region Properties
        private DataContext Context { get; set; }
        /// <summary>
        /// Устанавливает и возвращает ссылку на текущий источник данных в таблице. 
        /// </summary>
        public ObservableCollection<Account> DataSource
        {
            get => dataSource; set { dataSource = value; RaisePropertyChanged(nameof(DataSource)); }
        }
        public Client Client { get => client; set { client = value; RaisePropertyChanged(nameof(Domain.Model.Client)); } }
        public ObservableCollection<Account> AllAccounts
        {
            get
            {
                ObservableCollection<Account> accounts = new();
                foreach (Department dep in Context.Departments)
                {
                    foreach (Client client in dep.Clients)
                    {
                        foreach (Account account in client.Accounts)
                        {
                            if (Account == null || Account.ID != account.ID)
                                accounts.Add(account);
                        }
                    }
                }
                return accounts;
            }
        }
        public ObservableCollection<Account> ClientAccounts { get => Client?.Accounts; }
        public Account Account { get => account; set { account = value; RaisePropertyChanged(nameof(Account)); } }
        public Account TargetAccount { get => targetAccount; set { targetAccount = value; RaisePropertyChanged(nameof(TargetAccount)); } }
        public decimal TransferAmount { get => transferAmount; set { transferAmount = value; RaisePropertyChanged(nameof(TransferAmount)); } }
        public string ClientName => Client?.Name;
        public bool TransferEnabled { get => transferEnabled; set { transferEnabled = value; RaisePropertyChanged(nameof(TransferEnabled)); } }
        public Visibility RightPanelVisibilty { get => rightPanelVisibility; set { rightPanelVisibility = value; RaisePropertyChanged(nameof(RightPanelVisibilty)); } }
        public Visibility MenuItemsVisibility { get => menuItemsVisibility; set { menuItemsVisibility = value; RaisePropertyChanged(nameof(MenuItemsVisibility)); } }
        public Visibility MenuItemAccountAddVisibility
        {
            get => menuItemAccountAddVisibility; set
            {
                menuItemAccountAddVisibility = value;
                RaisePropertyChanged(nameof(MenuItemAccountAddVisibility));
            }
        }
        public ICommand AccountSelectionChangedCommand => accountSelectionChangedCommand ??= new RelayCommand((e) => MenuItemsVisibility =
        (Account = (e as DataGrid).SelectedItem is Account account ? account : null) != null ? Visibility.Visible : Visibility.Collapsed);
        public ICommand RemoveAccCommand => removeAccCommand ??= new RelayCommand(RemoveAccount);
        public ICommand AccountEditEndingCommand => accountEditEndingCommand ??= new RelayCommand(AccountEditEnd);
        public ICommand AccountAddCommand => accountAddCommand ??= new RelayCommand(AccountAdd);
        public ICommand AccountAddingCommand => accountAddingCommand ??= new RelayCommand(AccountAdding);
        public ICommand AccountEditBeginCommand => accountEditBeginCommand ??= new RelayCommand((e) => MenuItemAccountAddVisibility = Visibility.Collapsed);
        public ICommand ShowTargetTransferAccountCommand => showTargetTransferAccountCommand ??= new RelayCommand(ShowTargetTransfer);
        public ICommand TransferCommand => transferCommand ??= new RelayCommand(Transfer);
        public ICommand TargetTransferAccountSelectionChangedCommand => targetTransferAccountSelectionChangedCommand ??= new RelayCommand(TargetTransferAccountSelection);
        #endregion
        public AccountsViewModel(DataContext context, Client client)
        {
            Context = context; Client = client;
            DataSource = Client.Accounts;
        }
        #region Handlers
        private void AccountEditEnd(object e)
        {
            if (blockAccountEditEndingHandler) return;
            DataGrid grid = e as DataGrid;
            bool AccountAdded;
            if (Account.Client == null)
            {
                Account.Client = client;
                //MessageBox.Show("Добавлен счет");
                MainViewModel.Log($"Клиенту{Client} добавлен счет {Account}.");
                AccountAdded = true;
            }
            else
            {
                AccountAdded = false;
                MainViewModel.Log($"Счет {Account} отредактирован.");
                //MessageBox.Show("Счет отредактирован");
            }

            MenuItemAccountAddVisibility = Visibility.Visible;
            blockAccountEditEndingHandler = true;
            grid.CommitEdit();
            if (AccountAdded)
                Context.Accounts.Add(Account);
            Context.SaveChanges();
            blockAccountEditEndingHandler = false;

        }
        private void AccountAdd(object e)
        {
            MenuItemAccountAddVisibility = Visibility.Collapsed;
            (e as DataGrid).CanUserAddRows = true;
        }
        private void AccountAdding(object e)
        {
            (e as DataGrid).CanUserAddRows = false;
            MainViewModel.Log($"У клиента {Client} открыт новый счет.");
        }
        private void RemoveAccount(object e)
        {
            if (account == null || MessageBox.Show($"Удалить счет №{account.Number}?", $"Удаление счета {account}", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            Context.Accounts.Remove(Account);
            // Удаляем счет из списка счетов клиента
            DataSource.Remove(Account);
            RaisePropertyChanged(nameof(ClientAccounts));
            RaisePropertyChanged(nameof(AllAccounts));
            Context.SaveChanges();
            MainViewModel.Log($"Удален счет {account}");
        }
        private void ShowTargetTransfer(object e)
        {
            RightPanelVisibilty = Visibility.Visible;
            RaisePropertyChanged(nameof(AllAccounts));
            TransferEnabled = false;
            TransferAmount = 0;
        }
        private void TargetTransferAccountSelection(object e)
        {
            if (e == null || (TargetAccount = (e as ListBox).SelectedItem as Account) == null)
                return;
            TransferEnabled = true;
            TransferAmount = 0;
        }
        private void Transfer(object e)
        {
            if (account == null || targetAccount == null || MessageBox.Show(
                $"Вы действительно хотите перевести со счета №{account.Number} на счет №{targetAccount.Number} сумму {TransferAmount}?",
                "Перевод между счетами", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            AccountsControl accControl = e as AccountsControl;
            Account.Size -= TransferAmount;
            TargetAccount.Size += TransferAmount;
            MainViewModel.Log($"Со счета {account} переведено {transferAmount} на счет {targetAccount}.");
            accControl.accountTargetBox.Items.Refresh();
            //RaisePropertyChanged(nameof(ClientAccounts));
            RaisePropertyChanged(nameof(AllAccounts));
            TransferEnabled = false;
            TransferAmount = 0;
            accControl.accountGridView.Items.Refresh();
            accControl.accountGridView.CommitEdit();
            Context.SaveChanges();
        }
        #endregion
    }
}
