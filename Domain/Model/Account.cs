using CSharpFunctionalExtensions;
using System;
using System.IO;

namespace Domain.Model
{
    public class Account : Entity<int> //GUIDed
    {
        /// <summary>
        /// Хранит заголовок для текстового представления счета.
        /// </summary>
        public static readonly string header = string.Format("№\t\tSize\t\t\tRate\tCap");
        #region Properties
        /// <summary>
        /// Возвращает номер счета.
        /// </summary>
        public int Number { get; private set; }
        /// <summary>
        /// Устанавливает и возвращает размеры счета.
        /// </summary>
        public decimal Size { get; set; }
        /// <summary>
        /// Устанавливает и возвращает доходность в процентах.
        /// </summary>
        public double Rate { get; set; }
        /// <summary>
        /// Устанавливает и возвращает флаг капитализации вклада.
        /// </summary>
        public bool Cap { get; set; }
        public virtual Client Client { get; set; }
        #endregion
        public Account() => Number = GetHashCode();
        public string AccFields() => $"{Number,-16}{Size,-16:n}\t{Rate:g3}\t{Cap}";
        public string Info() => string.Format(header + "\n" + AccFields());
        #region Printing
        /// <summary>
        /// Печатает заголовок полей счета.
        /// </summary>
        /// <param name="tw">Райтер.</param>
        public static void PrintHeader(TextWriter tw) => tw.WriteLine(header);
        /// <summary>
        /// Печатает поля счета.
        /// </summary>
        /// <param name="tw">Райтер.</param>
        public void PrintFields(TextWriter tw) => tw.WriteLine(AccFields());
        /// <summary>
        /// Печатает информацию о счете.
        /// </summary>
        /// <param name="tw"></param>
        public void Print(TextWriter tw) => tw.WriteLine(this);
        #endregion
        public override string ToString() =>
            (Client != null ? Client.ToString() : string.Empty) + ";" + (Size < 0 ? "Loan " : "Deposit ") + $"№{Number};Size {Size:C2};Rate {Rate:g3};Cap {Cap}";
    }
}
