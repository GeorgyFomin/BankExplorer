using BankExplorer.Commands;
using Domain.Model;
using FontAwesome.Sharp;
using Microsoft.EntityFrameworkCore;
using Persistance.Conext;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BankExplorer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public static void Log(string report)
        {
            using TextWriter tw = File.AppendText("log.txt");
            tw.WriteLine($"{DateTime.Now}:{report}");
        }
        #region Fields
        private readonly DataContext context = new();
        private ViewModelBase viewModel;
        private RelayCommand dragCommand;
        private RelayCommand minimizeCommand;
        private RelayCommand maximizeCommand;
        private RelayCommand closeCommand;
        private RelayCommand depsCommand;
        private RelayCommand resetBankCommand;
        private RelayCommand depClientsCommand;
        private RelayCommand clientAccountsCommand;
        #endregion
        #region Properties
        public ViewModelBase ViewModel { get => viewModel; set { viewModel = value; RaisePropertyChanged(nameof(ViewModel)); } }
        public ICommand DragCommand => dragCommand ??= new RelayCommand((e) => (e as MainWindow).DragMove());
        public ICommand MinimizeCommand => minimizeCommand ??= new RelayCommand((e) => (e as MainWindow).WindowState = WindowState.Minimized);
        public ICommand MaximizeCommand => maximizeCommand ??= new RelayCommand((e) =>
        {
            MainWindow window = e as MainWindow;
            window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            window.MaxIconBlock.Icon = window.WindowState == WindowState.Maximized ? IconChar.WindowRestore : IconChar.WindowMaximize;
        });
        public ICommand CloseCommand => closeCommand ??= new RelayCommand((e) =>
        {
            context.SaveChanges();
            (e as MainWindow).Close();
        });
        public ICommand ResetBankCommand => resetBankCommand ??= new RelayCommand((e) => ResetBank());
        public ICommand DepsCommand => depsCommand ??= new RelayCommand((e) => ViewModel = new DepsViewModel()
        {
            Context = context,
            DataSource = Deps,
            BankName = BankName
        });
        public ICommand DepClientsCommand => depClientsCommand ??= new RelayCommand((e) => ViewModel = new ClientsViewModel(context, e as Department));
        public ICommand ClientAccountsCommand => clientAccountsCommand ??= new RelayCommand((e) => ViewModel = new AccountsViewModel(context, e as Client));
        #endregion
        private void ClearTables()
        {
            foreach (Account loan in context.Accounts)
            {
                context.Accounts.Remove(loan);
            }
            foreach (Client client in context.Clients)
            {
                context.Clients.Remove(client);
            }
            foreach (Department department in context.Departments)
            {
                context.Departments.Remove(department);
            }
            context.SaveChanges();
        }
        private void FillTables()
        {
            context.Departments.AddRange(RandomBank.Deps);
            foreach (Department dep in context.Departments)
            {
                context.Clients.AddRange(dep.Clients);
                foreach (Client client in context.Clients)
                {
                    context.Accounts.AddRange(client.Accounts);
                }
            }
            context.SaveChanges();
        }
        private ObservableCollection<Department> deps;
        public ObservableCollection<Department> Deps { get => deps; set { deps = value; RaisePropertyChanged(nameof(Deps)); } }
        private string bankName;
        public string BankName { get => bankName; set { bankName = value; RaisePropertyChanged(nameof(BankName)); } }
        private void ResetBank()
        {
            ClearTables();
            FillTables();
            BankName = RandomBank.GetRandomString(4, RandomBank.random);
            ViewModel = new BankNameViewModel() { BankName = BankName };
            Log($"Создан банк {BankName}.");
            Deps = context.Departments.Local.ToObservableCollection();
        }
        public MainViewModel()
        {
            ResetBank();
        }
    }
}
