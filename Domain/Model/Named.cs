using CSharpFunctionalExtensions;
using System.IO;

namespace Domain.Model
{
    /// <summary>
    /// Имеет имя и идентификатор.
    /// </summary>
    public class Named : Entity<int> //GUIDed
    {
        /// <summary>
        /// Хранит заголовок для текстового представления.
        /// </summary>
        public static readonly string header = string.Format("Name");
        /// <summary>
        /// Хранит текущее значение имени.
        /// </summary>
        private string name = "Noname";
        /// <summary>
        /// Устанавливает и возвращает имя.
        /// </summary>
        public string Name { get => name; set => name = !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(value.Trim()) ? value : "Noname"; }
        /// <summary>
        /// Перекрывает унаследованный метод текстового представления объекта.
        /// </summary>
        /// <returns>Имя отдела.</returns>
        public override string ToString() => Name;
        /// <summary>
        /// Готовит текстовое представление информации об отделе.
        /// </summary>
        /// <returns>Возвращает текстовое представление информации.</returns>
        public string Info() => string.Format(header + "\n" + Name);
        #region Printing
        /// <summary>
        /// Печатает заголовок полей объекта.
        /// </summary>
        /// <param name="tw">Райтер.</param>
        public static void PrintHeader(TextWriter tw) => tw.WriteLine(header);
        /// <summary>
        /// Печатает поля объекта.
        /// </summary>
        /// <param name="tw">Райтер.</param>
        public void PrintFields(TextWriter tw) => tw.WriteLine(Name);
        #endregion

    }
}
