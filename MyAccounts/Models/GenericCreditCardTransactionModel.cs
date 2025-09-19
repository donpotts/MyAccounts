using CsvHelper.Configuration.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyAccounts.Models
{
    public class GenericCreditCardTransactionModel
    {
        [Name("Transaction Date")]
        public DateOnly? TransactionDate { get; set; }

        [Name("Posted Date")]
        public DateOnly? PostedDate { get; set; }

        [Name("Card No.")]
        public string CardNo { get; set; }

        [Name("Description")]
        public string Description { get; set; }

        [Name("Category")]
        public string Category { get; set; }

        [Name("Debit")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Debit { get; set; }

        [Name("Credit")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Credit { get; set; }
    }
}
