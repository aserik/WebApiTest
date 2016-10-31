using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Enter account Id to get invoice for:");
                var accountId = Console.ReadLine();

                var provider = new MsSqlDataProvider();
                var invoice = new Invoice(provider);
                var invoiceTotalAmount = invoice.CalculateTotalAmount(Convert.ToInt32(accountId));
                Console.WriteLine("Invoice amount: {0}", invoiceTotalAmount);
            }
        }
    }

    public class Invoice
    {
        private IDataProvider _provider;
        public Invoice(IDataProvider provider)
        {
            _provider = provider;
        }

        public decimal CalculateTotalAmount(int accountId)
        {
            var charges = _provider.GetCharges(accountId);
            decimal totalAmount = 0;
            foreach(var charge in charges)
            {
                totalAmount += charge;
                
            }

            return totalAmount;
        }
    }

    public interface IDataProvider
    {
        IEnumerable<decimal> GetCharges(int accountId);
    }

    public class MsSqlDataProvider : IDataProvider
    {        
        public IEnumerable<decimal> GetCharges(int accountId)
        {
            var r = new Random();
            return new List<decimal> { r.Next(0, 1000), r.Next(0, 1000), r.Next(0, 1000) };
        }
    }
}
