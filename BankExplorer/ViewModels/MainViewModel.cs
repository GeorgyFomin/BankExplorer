using BankExplorer.Commands;
using BankExplorer.Dialogs;
using Domain.Model;
using FontAwesome.Sharp;
using Persistance.Conext;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BankExplorer.ViewModels
{
    public class ObsCollection<T> : ObservableCollection<T>
    {
        private bool suppressNotification;
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!suppressNotification)
                base.OnCollectionChanged(e);
        }
        public void AddRange(IEnumerable<T> list)
        {
            if (list == null)
                throw new ArgumentNullException();
            suppressNotification = true;
            foreach (T item in list)
                Add(item);
            suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Хранит число фрагментов, на каждом из которых обновляется список при загрузке записей из лог-файла.
        /// Каждый фрагмент содержит порядка 10 000 записей.
        /// </summary>
        private const int logRecordBuffers = 1000;
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
        private RelayCommand retrieveCommand;
        private RelayCommand retrieveLogdialogCommand;
        private ObservableCollection<Department> deps;
        private string bankName;
        private string resultText = string.Empty;
        private bool rtrEnabled = true;
        private List<string> logRecords = new();
        private ScrollBarVisibility scrollVis = ScrollBarVisibility.Hidden;
        private ObsCollection<string> logItems = new();
        private double barValue;
        private string listCount;
        private RelayCommand visualLogicTreesDialogCommand;
        private MainWindow mainWindow;
        private RelayCommand showTreeViewCommand;
        #endregion
        #region Properties
        public MainWindow MainWindow { get => mainWindow; set { mainWindow = value; RaisePropertyChanged(nameof(MainWindow)); } }
        public ICommand VisualLogicTreesDialogCommand => visualLogicTreesDialogCommand ??=
            new RelayCommand((e) =>
            {
                MainWindow = e as MainWindow;
                new VisualLogicTreesDialog { DataContext = this }.ShowDialog();
            });
        public ICommand ShowTreeViewCommand => showTreeViewCommand ??= new RelayCommand(ShowTreeView);
        public ViewModelBase ViewModel { get => viewModel; set { viewModel = value; RaisePropertyChanged(nameof(ViewModel)); } }
        public ObservableCollection<Department> Departments { get => deps; set { deps = value; RaisePropertyChanged(nameof(Departments)); } }
        public string BankName { get => bankName; set { bankName = value; RaisePropertyChanged(nameof(BankName)); } }
        public string ResultText { get => resultText; set { resultText = value; RaisePropertyChanged(nameof(ResultText)); } }
        public ObsCollection<string> LogItems { get => logItems; set { logItems = value; RaisePropertyChanged(nameof(LogItems)); } }
        public List<string> LogRecords { get => logRecords; set { logRecords = value; RaisePropertyChanged(nameof(LogRecords)); } }
        public double BarValue { get => barValue; set { barValue = value; RaisePropertyChanged(nameof(BarValue)); } }
        public string ListCount { get => listCount; set { listCount = value; RaisePropertyChanged(nameof(ListCount)); } }
        public bool RtrEnabled { get => rtrEnabled; set { rtrEnabled = value; RaisePropertyChanged(nameof(RtrEnabled)); } }
        public ScrollBarVisibility ScrollVis { get => scrollVis; set { scrollVis = value; RaisePropertyChanged(nameof(ScrollVis)); } }
        public ICommand RetrieveCommand => (retrieveCommand ??= new RelayCommand(
        //async (e) => await DoRetrieve()));// Это Ваш код
        (e) => Task.Factory.StartNew(() => DoRetrieve())));// Это моя версия
        //(e) => DoRetrieve()));// Это обычный вызов без использования потоков.
        //Тот же эффект, что async-await, только не работает ProgressBar в ходе заполнения коллекции.
        public ICommand RetrieveLogDialogCommand => (retrieveLogdialogCommand ??= new RelayCommand((e) => new RetrieveLogDialog { DataContext = this }.ShowDialog()));
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
            context.Dispose();
            (e as MainWindow).Close();
        });
        public ICommand ResetBankCommand => resetBankCommand ??= new RelayCommand((e) => ResetBank());
        public ICommand DepsCommand => depsCommand ??= new RelayCommand((e) => ViewModel = new DepsViewModel()
        {
            Context = context,
            DataSource = Departments,
            BankName = BankName
        });
        public ICommand DepClientsCommand => depClientsCommand ??= new RelayCommand((e) => ViewModel = new ClientsViewModel(context, e as Department));
        public ICommand ClientAccountsCommand => clientAccountsCommand ??= new RelayCommand((e) => ViewModel = new AccountsViewModel(context, e as Client));
        #endregion
        private void ClearTables()
        {
            foreach (Account account in context.Accounts)
            {
                context.Accounts.Remove(account);
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
        private void ResetBank()
        {
            ClearTables();
            FillTables();
            BankName = RandomBank.GetRandomString(4, RandomBank.random);
            ViewModel = new BankNameViewModel() { BankName = BankName };
            Log($"Создан банк {BankName}.");
            Departments = context.Departments.Local.ToObservableCollection();
        }
        public MainViewModel()
        {
            ResetBank();
        }
        /// <summary>
        /// Реализует загрузку записей из лог-файла в отдельном потоке.
        /// </summary>
        private void DoRetrieve(bool isList = true)
        {
            RtrEnabled = false;
            BarValue = 0;
            ListCount = (isList ? logRecords.Count : logItems.Count).ToString("n0");
            string s = null;
            List<string> list = new();
            Stopwatch stopWatch = new();
            stopWatch.Start();
            for (int i = 0; i < logRecordBuffers; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    using TextReader tr = File.OpenText("log.txt");
                    while ((s = tr.ReadLine()) != null)
                    {
                        list.Add(s);
                    }
                }
                BarValue = 100.0 * i / logRecordBuffers;
                if (isList)
                {
                    LogRecords.AddRange(list);
                    ListCount = logRecords.Count.ToString("n0") + ";" + stopWatch.ElapsedMilliseconds.ToString("n0");
                    list.Clear();
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LogItems.AddRange(list);
                        ListCount = logItems.Count.ToString("n0") + ";" + stopWatch.ElapsedMilliseconds.ToString("n0");
                        list.Clear();
                    });
                }
            }
            ResultText = "That's All";
            ScrollVis = ScrollBarVisibility.Visible;
        }
        //private async Task DoRetrieve(bool isList = true)
        //{
        //    RtrEnabled = false;
        //    BarValue = 0;
        //    ListCount = (isList ? logRecords.Count : logItems.Count).ToString("n0");
        //    string s = null;
        //    List<string> list = new List<string>();
        //    Stopwatch stopWatch = new Stopwatch();
        //    stopWatch.Start();
        //    await Task.Run(() =>
        //    {
        //        for (int i = 0; i < logRecordBuffers; i++)
        //        {
        //            for (int j = 0; j < 10; j++)
        //            {
        //                using (TextReader tr = File.OpenText("log.txt"))
        //                    while ((s = tr.ReadLine()) != null)
        //                        list.Add(s);
        //            }
        //            Application.Current.Dispatcher.Invoke(() =>
        //            {
        //                BarValue = 100.0 * i / logRecordBuffers;
        //                if (isList)
        //                {
        //                    LogRecords.AddRange(list);
        //                    ListCount = logRecords.Count.ToString("n0") + ";" + stopWatch.ElapsedMilliseconds.ToString("n0");
        //                }
        //                else
        //                {
        //                    LogItems.AddRange(list);
        //                    ListCount = logItems.Count.ToString("n0") + ";" + stopWatch.ElapsedMilliseconds.ToString("n0");
        //                }
        //                list.Clear();
        //            });
        //        }
        //    });
        //    ResultText = "That's All";
        //    ScrollVis = ScrollBarVisibility.Visible;
        //}
        private void ShowTreeView(object e)
        {
            VisualLogicTreesDialog window = e as VisualLogicTreesDialog;
            window.visualTreeView.Items.Clear();
            window.logicTreeView.Items.Clear();
            ProcessVisElement(window, mainWindow, null);
            ProcessLogicElement(window, mainWindow, null);
        }
        public void ProcessVisElement(VisualLogicTreesDialog window, DependencyObject element, TreeViewItem previousItem)
        {
            TreeViewItem item = new() { ItemContainerStyle = (Style)window.Resources["treeViewItemStyle"] };
            if (element == null)
                return;
            item.Header = element.GetType().Name;
            item.IsExpanded = true;
            if (previousItem == null)
                window.visualTreeView.Items.Add(item);
            else
                previousItem.Items.Add(item);
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
                ProcessVisElement(window, VisualTreeHelper.GetChild(element, i), item);
        }
        public void ProcessLogicElement(VisualLogicTreesDialog window, DependencyObject element, TreeViewItem previousItem)
        {
            TreeViewItem item = new() { ItemContainerStyle = (Style)window.Resources["treeViewItemStyle"] };
            if (element == null)
                return;
            item.Header = element.GetType().Name;
            item.IsExpanded = true;
            if (previousItem == null)
                window.logicTreeView.Items.Add(item);
            else
                previousItem.Items.Add(item);
            foreach (var child in LogicalTreeHelper.GetChildren(element))
            {
                ProcessLogicElement(window, child as DependencyObject, item);
            }
        }
    }
}
