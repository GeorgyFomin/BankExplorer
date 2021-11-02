using System;

namespace Domain.Model
{
    /// <summary>
    /// Имеет идентификатор.
    /// </summary>
    public class GUIDed
    {
        /// <summary>
        /// Устанавливает и возвращает уникальный идентификатор.
        /// </summary>
        public Guid ID { get; private set; } = Guid.NewGuid();
    }
}
