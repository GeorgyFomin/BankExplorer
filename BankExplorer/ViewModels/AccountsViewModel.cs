using BankExplorer.Commands;
using BankExplorer.View;
using Domain.Model;
using Persistance.Conext;
using System.Collections.ObjectModel;
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
        private bool cellEdited;
        private Client client;
        private Account account;
        private Account targetAccount;
        private Visibility rightPanelVisibility = Visibility.Hidden;
        private Visibility menuVisibility = Visibility.Collapsed;
        private Visibility menuAddVisibility = Visibility.Visible;
        private bool transferEnabled;
        private decimal transferAmount;
        private RelayCommand accountSelectedCommand;
        private RelayCommand removeAccCommand;
        private RelayCommand accountAddCommand;
        private RelayCommand accountAddingCommand;
        private RelayCommand accountEditBeginCommand;
        private RelayCommand accountCellEditEndingCommand;
        private RelayCommand accountRowEditEndingCommand;
        private RelayCommand accountCurrCellChangedCommand;
        private RelayCommand transferCommand;
        private RelayCommand targetAccountSelectedCommand;
        private RelayCommand showTargetAccountCommand;
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
        public Visibility MenuVisibility { get => menuVisibility; set { menuVisibility = value; RaisePropertyChanged(nameof(MenuVisibility)); } }
        public Visibility MenuAddVisibility { get => menuAddVisibility; set { menuAddVisibility = value; RaisePropertyChanged(nameof(MenuAddVisibility)); } }
        public ICommand AccountSelectedCommand => accountSelectedCommand ??= new RelayCommand((e) => MenuVisibility =
        (Account = (e as DataGrid).SelectedItem is Account account ? account : null) != null ? Visibility.Visible : Visibility.Collapsed);
        public ICommand AccountRemoveCommand => removeAccCommand ??= new RelayCommand(RemoveAccount);
        public ICommand AccountAddCommand => accountAddCommand ??= new RelayCommand(AccountAdd);
        public ICommand AccountAddingCommand => accountAddingCommand ??= new RelayCommand((e) => (e as DataGrid).CanUserAddRows = false);
        public ICommand AccountEditBeginCommand => accountEditBeginCommand ??= new RelayCommand((e) => MenuAddVisibility = Visibility.Collapsed);
        public ICommand AccountRowEditEndingCommand => accountRowEditEndingCommand ??= new RelayCommand(AccountRowEditEnd);
        public ICommand AccountCellEditEndingCommand => accountCellEditEndingCommand ??= new RelayCommand((e) => cellEdited = true);
        public ICommand AccountCurrCellChangedCommand => accountCurrCellChangedCommand ??= new RelayCommand(AccountCurrCellChanged);
        public ICommand ShowTargetAccountCommand => showTargetAccountCommand ??= new RelayCommand(ShowTargetTransfer);
        public ICommand TransferCommand => transferCommand ??= new RelayCommand(Transfer);
        public ICommand TargetAccountSelectedCommand => targetAccountSelectedCommand ??= new RelayCommand(TargetAccountSelection);
        #endregion
        public AccountsViewModel(DataContext context, Client client)
        {
            Context = context; Client = client;
            DataSource = Client.Accounts;
        }
        #region Handlers
        private void AccountRowEditEnd(object e)
        {
            if (blockAccountEditEndingHandler) return;
            //MessageBox.Show("Row Edited");
            cellEdited = false;
            bool AccountAdded;
            if (AccountAdded = Account.Client == null)
            {
                Account.Client = client;
            }
            MenuAddVisibility = Visibility.Visible;
            blockAccountEditEndingHandler = true;
            (e as DataGrid).CommitEdit();
            if (AccountAdded)
            {
                Context.Accounts.Add(Account);
                //MessageBox.Show($"У клиента {Client} открыт счет {Account.Number}.");
                MainViewModel.Log($"У клиента {Client} открыт счет {Account.Number}.");
            }
            else
            {
                MainViewModel.Log($"Счет {Account} отредактирован.");
                //MessageBox.Show($"Счет {Account} отредактирован.");
            }
            Context.SaveChanges();
            blockAccountEditEndingHandler = false;
        }
        private void AccountCurrCellChanged(object e)
        {
            if (!cellEdited)
                return;
            cellEdited = false;
            (e as DataGrid).CommitEdit();
            Context.SaveChanges();
            MainViewModel.Log($"Счет {Account} отредактирован.");
            //MessageBox.Show($"Счет {Account} отредактирован.");
            //MessageBox.Show("Cell Changed");
        }
        private void AccountAdd(object e)
        {
            MenuAddVisibility = Visibility.Collapsed;
            (e as DataGrid).CanUserAddRows = true;
        }
        //private void AccountAdding(object e)
        //{
        //    (e as DataGrid).CanUserAddRows = false;
        //}
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
        private void TargetAccountSelection(object e)
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
            accControl.accountTargetBox.UnselectAll();
            accControl.accountTargetBox.Items.Refresh();
            RaisePropertyChanged(nameof(AllAccounts));
            TransferEnabled = false;
            TransferAmount = 0;
            accControl.accountGridView.Items.Refresh();
            accControl.accountGridView.CommitEdit();
            Context.SaveChanges();
            RightPanelVisibilty = Visibility.Hidden;
        }
        #endregion
    }
}
