using System;
using System.Collections.ObjectModel;
using System.IO;

namespace Domain.Model
{
    public class Client : Named
    {
        /// <summary>
        /// Хранит список вкладов.
        /// </summary>
        private ObservableCollection<Account> accounts = new();
        public virtual Department Dep { get; set; }
        /// <summary>
        /// Устанавливает и возвращает ссылки на счета клиента.
        /// </summary>
        public virtual ObservableCollection<Account> Accounts
        {
            get => accounts; set
            {
                accounts = value ?? new ObservableCollection<Account>();
                foreach (Account account in accounts)
                {
                    account.Client = this;
                }
            }
        }

        /// <summary>
        /// Печатает сведения о клиенте.
        /// </summary>
        /// <param name="tw">Райтер.</param>
        public void Print(TextWriter tw)
        {
            // Печатаем информацию о клиенте.
            tw.WriteLine("Клиент\n" + Info() + "\nСчета");
            tw.WriteLine(Account.header);
            // Печатаем сведения о счетах.
            foreach (Account account in Accounts)
            {
                account.PrintFields(tw);
            }
        }
        public override string ToString()
        {
            return (Dep != null ? Dep.ToString() : string.Empty) + ";Client " + base.ToString();
        }
    }
}
