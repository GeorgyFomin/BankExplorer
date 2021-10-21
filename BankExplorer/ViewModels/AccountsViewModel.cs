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
        private Account sourceTransferAccount;
        private Account targetTransferAccount;
        private Visibility rightPanelVisibility = Visibility.Hidden;
        private Visibility menuItemsVisibility = Visibility.Collapsed;
        private Visibility menuItemAccountAddVisibility = Visibility.Visible;
        private bool transferEnabled;
        private decimal transferAmount;
        private RelayCommand accountSelectionChangedCommand;
        private RelayCommand removeAccCommand;
        private RelayCommand accountEditEndingCommand;
        private RelayCommand transferCommand;
        private RelayCommand targetTransferAccountSelectionChangedCommand;
        private RelayCommand accountAddCommand;
        private RelayCommand accountAddingCommand;
        private RelayCommand showTargetTransferAccountCommand;
        private RelayCommand accountBeginningEditCommand;
        #endregion
        #region Properties
        public DataContext Context { get; set; }
        /// <summary>
        /// Устанавливает и возвращает ссылку на текущий источник данных в таблице. 
        /// </summary>
        public ObservableCollection<Account> DataSource { get; set; }
        public Client SourceTransferClient { get; set; }
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
                            if (SourceTransferAccount == null || SourceTransferAccount.ID != account.ID)
                                accounts.Add(account);
                        }
                    }
                }
                return accounts;
            }
        }
        public ObservableCollection<Account> SourceTransferAccounts { get => SourceTransferClient?.Accounts; }
        public Account SourceTransferAccount { get => sourceTransferAccount; set { sourceTransferAccount = value; RaisePropertyChanged(nameof(SourceTransferAccount)); } }
        public Account TargetTransferAccount { get => targetTransferAccount; set { targetTransferAccount = value; RaisePropertyChanged(nameof(TargetTransferAccount)); } }
        public decimal TransferAmount { get => transferAmount; set { transferAmount = value; RaisePropertyChanged(nameof(TransferAmount)); } }
        public string SourceTransferClientName => SourceTransferClient?.Name;
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
        public ICommand AccountSelectionChangedCommand => accountSelectionChangedCommand ??= new RelayCommand((e) =>
        MenuItemsVisibility = (SourceTransferAccount = (e as DataGrid).SelectedItem is Account account ? account : null) != null ? Visibility.Visible : Visibility.Collapsed);
        public ICommand RemoveAccCommand => removeAccCommand ??= new RelayCommand(RemoveAccount);
        public ICommand AccountEditEndingCommand => accountEditEndingCommand ??= new RelayCommand((e) =>
        {
            if (SourceTransferAccount.Client == null)
            {
                SourceTransferClient.Accounts.Add(SourceTransferAccount);
                RaisePropertyChanged(nameof(SourceTransferClient));
                //MessageBox.Show("Добавлен счет");
            }
            MenuItemAccountAddVisibility = Visibility.Visible;
            MainViewModel.Log($"Поля счета {SourceTransferAccount} клиента {SourceTransferClient} отредактированы.");
        });
        public ICommand AccountAddCommand => accountAddCommand ??= new RelayCommand((e) =>
        {
            MenuItemAccountAddVisibility = Visibility.Collapsed;
            (e as DataGrid).CanUserAddRows = true;
        });
        public ICommand AccountAddingCommand => accountAddingCommand ??= new RelayCommand(AccountAdding);
        public ICommand AccountBeginningEditCommand => accountBeginningEditCommand ??= new RelayCommand((e) => MenuItemAccountAddVisibility = Visibility.Collapsed);
        public ICommand ShowTargetTransferAccountCommand => showTargetTransferAccountCommand ??= new RelayCommand((e) =>
        {
            RightPanelVisibilty = Visibility.Visible;
            RaisePropertyChanged(nameof(AllAccounts));
            TransferEnabled = false;
        });
        public ICommand TransferCommand => transferCommand ??= new RelayCommand(Transfer);
        public ICommand TargetTransferAccountSelectionChangedCommand => targetTransferAccountSelectionChangedCommand ??= new RelayCommand(TargetTransferAccountSelection);
        #endregion
        public AccountsViewModel(DataContext context, Client client)
        {
            Context = context; SourceTransferClient = client;
            DataSource = new ObservableCollection<Account>(Context.Accounts.Local.ToObservableCollection().Where((e) => e.Client == SourceTransferClient));
        }
        #region Handlers
        private void RemoveAccount(object e)
        {
            if (sourceTransferAccount == null ||
                MessageBox.Show($"Удалить счет №{sourceTransferAccount.Number}?", $"Удаление счета {sourceTransferAccount}", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            Context.Accounts.Remove(SourceTransferAccount);
            // Удаляем счет из списка счетов клиента
            DataSource.Remove(SourceTransferAccount);
            //SourceTransferClient.Accounts.Remove(sourceTransferAccount);
            RaisePropertyChanged(nameof(SourceTransferAccounts));
            RaisePropertyChanged(nameof(AllAccounts));
            Context.SaveChanges();
            MainViewModel.Log($"Удален счет {sourceTransferAccount}");
        }
        private void AccountAdding(object e)
        {
            (e as DataGrid).CanUserAddRows = false;
            MainViewModel.Log($"У клиента {SourceTransferClient} открыт новый счет.");
        }
        private void TargetTransferAccountSelection(object e)
        {
            if (e == null || (TargetTransferAccount = (e as ListBox).SelectedItem as Account) == null)
                return;
            TransferEnabled = true;
            TransferAmount = 0;
        }
        private void Transfer(object e)
        {
            if (sourceTransferAccount == null || targetTransferAccount == null || MessageBox.Show(
                $"Вы действительно хотите перевести со счета №{sourceTransferAccount.Number} на счет №{targetTransferAccount.Number} сумму {TransferAmount}?",
                "Перевод между счетами", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            SourceTransferAccount.Size -= TransferAmount;
            TargetTransferAccount.Size += TransferAmount;
            MainViewModel.Log($"Со счета {sourceTransferAccount} переведено {transferAmount} на счет {targetTransferAccount}.");
            RaisePropertyChanged(nameof(SourceTransferAccounts));
            RaisePropertyChanged(nameof(AllAccounts));
            //(e as AccountsControl).accFromGrid.Items.Refresh();
            //(e as AccountsControl).accListBox.Items.Refresh();
            TransferEnabled = false;
        }
        #endregion
    }
}
