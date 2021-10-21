using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public static class RandomBank
    {
        /// <summary>
        /// Хранит максимально возможную сумму вклада.
        /// </summary>
        const double MaxSize = 1_000_000_000;
        /// <summary>
        /// Хранит максимально возможную доходность вклада в процентах.
        /// </summary>
        const double MaxRate = 10;
        /// <summary>
        /// Хранит ссылку на генератор случайных чисел.
        /// </summary>
        public static readonly Random random = new();
        /// <summary>
        /// Возвращает случайный банк.
        /// </summary>
        /// <returns></returns>
        //public static Bank GetBank() => new Bank() { Name = GetRandomString(4, random), Deps = GetRandomDeps(random.Next(1, 5), random) };
        public static ObservableCollection<Department> Deps => GetRandomDeps(random.Next(1, 5), random);
        /// <summary>
        /// Возвращает список случайных отделов.
        /// </summary>
        /// <param name="v">Число отделов.</param>
        /// <param name="random">Генератор случайных чисел.</param>
        /// <returns></returns>
        private static ObservableCollection<Department> GetRandomDeps(int v, Random random) =>
                   new ObservableCollection<Department>(Enumerable.Range(0, v).
                       Select(index => new Department() { Name = GetRandomString(random.Next(1, 6), random), Clients = GetRandomClients(random.Next(1, 20), random) }));
        /// <summary>
        /// Возвращает список случайных клиентов.
        /// </summary>
        /// <param name="v">Число клиентов.</param>
        /// <param name="random">Генератор случайных чисел.</param>
        /// <returns></returns>
        private static ObservableCollection<Client> GetRandomClients(int v, Random random) =>
           new ObservableCollection<Client>(Enumerable.Range(0, v).
               Select(index => new Client() { Name = GetRandomString(random.Next(3, 6), random), Accounts = GetRandomAccounts(random.Next(1, 5), random) }));
        /// <summary>
        /// Возвращает список случайных счетов.
        /// </summary>
        /// <param name="v">Число счетов.</param>
        /// <param name="random">Генератор случайных чисел.</param>
        /// <returns></returns>
        private static ObservableCollection<Account> GetRandomAccounts(int v, Random random) =>
            new ObservableCollection<Account>(Enumerable.Range(0, v).
                Select(index => new Account()
                {
                    // Случайна доходность.
                    Rate = MaxRate * (float)random.NextDouble(),
                    // Случайная капитализация.
                    Cap = random.NextDouble() < .5,
                    // Случайный размер вклада (положительный) или кредита (отрицательный).
                    Size = (decimal)(MaxSize * (2 * random.NextDouble() - 1))
                }));
        /// <summary>
        /// Генерирует случайную строку из латинских букв нижнего регистра..
        /// </summary>
        /// <param name="length">Длина строки.</param>
        /// <param name="random">Генератор случайных чисел.</param>
        /// <returns></returns>
        public static string GetRandomString(int length, Random random)
            => new string(Enumerable.Range(0, length).Select(x => (char)random.Next('a', 'z' + 1)).ToArray());
    }
}
