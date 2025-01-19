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
    using CsvHelper.Configuration.Attributes;
    using System.Text.RegularExpressions;

    public class BankTransactionImporter
    {
        public static async Task<List<BankTransactionModel>> ReadBankTransactionsAsync(string csvFilePath)
        {
            var bankLines = await File.ReadAllLinesAsync(csvFilePath);

            var csvLines = bankLines.ToList();

            var csvString = string.Join(Environment.NewLine, csvLines).Replace("Payee/Security", "Payee"); ;
            using StringReader reader = new(csvString);
            var transactions = new List<BankTransactionModel>();

            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            }))
            {
                try 
                { 
                    transactions = csv.GetRecords<BankTransactionModel>().ToList();
                } 
                catch (Exception ex)
                { 
                    Console.WriteLine($"Error reading records: {ex.Message}");
                }
            }

            return transactions;
        }
    }

}
