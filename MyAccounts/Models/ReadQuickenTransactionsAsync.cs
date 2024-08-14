using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MyAccounts.Models;

namespace MyAccounts.Models
{
    using System.Text.RegularExpressions;

    public class QuickenTransactionImporter
    {
        public static async Task<List<TransactionModel>> ReadQuickenTransactionsAsync(string csvFilePath)
        {
            var quickenLines = await File.ReadAllLinesAsync(csvFilePath);

            var csvLines = quickenLines
                .Where(x => x.StartsWith(",,") || x.StartsWith(",\"") || x.StartsWith(",S"))
                .ToList();

            var csvString = string.Join(Environment.NewLine, csvLines).Replace("Payee/Security", "Payee"); ;
            using StringReader reader = new(csvString);
            var transactions = new List<TransactionModel>();

            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            }))
            {
                //csv.Context.RegisterClassMap<RecordMap>();
                transactions = csv.GetRecords<TransactionModel>().ToList();
            }

            return transactions;
        }
    }

}
