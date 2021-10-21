using System.Collections.ObjectModel;
using System.IO;

namespace Domain.Model
{
    public class Department : Named
    {
        /// <summary>
        /// Хранит список клиентов.
        /// </summary>
        private ObservableCollection<Client> clients = new();
        /// <summary>
        /// Устанавливает и возвращает ссылки на клиентов.
        /// </summary>
        public ObservableCollection<Client> Clients
        {
            get => clients; set
            {
                clients = value ?? new ObservableCollection<Client>();
                foreach (Client client in clients)
                {
                    client.Dep = this;
                }
            }
        }
        /// <summary>
        /// Печатает сведения об отделе.
        /// </summary>
        /// <param name="tw">Райтер.</param>
        public void Print(TextWriter tw)
        {
            // Печатаем информацию об отделе.
            tw.WriteLine("Отдел " + this);
            // Печатаем сведения о клиентах.
            foreach (Client client in Clients)
            {
                client.Print(tw);
            }
        }
        public override string ToString()
        {
            return "Department " + base.ToString();
        }
    }
}
