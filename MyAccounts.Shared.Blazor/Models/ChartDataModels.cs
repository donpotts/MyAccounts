namespace MyAccounts.Shared.Blazor.Models;

/// <summary>
/// Base chart data item for simple label/value charts (pie, bar, etc.)
/// </summary>
public class ChartDataItem
{
    public string Label { get; set; } = "";
    public decimal Value { get; set; }
}

/// <summary>
/// Data item for trend charts with period and type (income/expenses)
/// </summary>
public class TrendDataItem
{
    public string Period { get; set; } = "";
    public string Type { get; set; } = "";
    public decimal Value { get; set; }
}

/// <summary>
/// Data item for account comparison charts with credits and debits
/// </summary>
public class AccountChartDataItem
{
    public string Account { get; set; } = "";
    public decimal Credits { get; set; }
    public decimal Debits { get; set; }
}

/// <summary>
/// Data item for donut charts with percentage calculation
/// </summary>
public class DonutDataItem
{
    public string Label { get; set; } = "";
    public decimal Value { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Data item for stacked area charts showing multiple categories over time
/// </summary>
public class StackedAreaDataItem
{
    public string Period { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Value { get; set; }
}

/// <summary>
/// Data item for treemap charts showing hierarchical data
/// </summary>
public class TreemapDataItem
{
    public string Category { get; set; } = "";
    public string? ParentCategory { get; set; }
    public decimal Value { get; set; }
}

/// <summary>
/// Data item for heatmap charts showing grid data
/// </summary>
public class HeatmapDataItem
{
    public string XLabel { get; set; } = "";
    public string YLabel { get; set; } = "";
    public decimal Value { get; set; }
}

/// <summary>
/// Data item for account balance charts
/// </summary>
public class AccountBalanceDataItem
{
    public string AccountName { get; set; } = "";
    public decimal Balance { get; set; }
}
