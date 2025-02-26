using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Web;
using MyAccounts.Shared.Blazor.Authorization;
using MyAccounts.Shared.Blazor.Models;
using MyAccounts.Shared.Models;

namespace MyAccounts.Shared.Blazor.Services;

public class AppService(
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider)
{
    private readonly IdentityAuthenticationStateProvider authenticationStateProvider
            = authenticationStateProvider as IdentityAuthenticationStateProvider
                ?? throw new InvalidOperationException();

    private static async Task HandleResponseErrorsAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode
            && response.StatusCode != HttpStatusCode.Unauthorized
            && response.StatusCode != HttpStatusCode.NotFound)
        {
            var message = await response.Content.ReadAsStringAsync();
            throw new Exception(message);
        }

        response.EnsureSuccessStatusCode();
    }

    public class ODataResult<T>
    {
        [JsonPropertyName("@odata.count")]
        public int? Count { get; set; }

        public IEnumerable<T>? Value { get; set; }
    }

    public async Task<ODataResult<T>?> GetODataAsync<T>(
            string entity,
            int? top = null,
            int? skip = null,
            string? orderby = null,
            string? filter = null,
            bool count = false,
            string? expand = null)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        var queryString = HttpUtility.ParseQueryString(string.Empty);

        if (top.HasValue)
        {
            queryString.Add("$top", top.ToString());
        }

        if (skip.HasValue)
        {
            queryString.Add("$skip", skip.ToString());
        }

        if (!string.IsNullOrEmpty(orderby))
        {
            queryString.Add("$orderby", orderby);
        }

        if (!string.IsNullOrEmpty(filter))
        {
            queryString.Add("$filter", filter);
        }

        if (count)
        {
            queryString.Add("$count", "true");
        }

        if (!string.IsNullOrEmpty(expand))
        {
            queryString.Add("$expand", expand);
        }
        
        var uri = $"/odata/{entity}?{queryString}";
        
        HttpRequestMessage request = new(HttpMethod.Get, uri);
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        
        await HandleResponseErrorsAsync(response);
        try
        {
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Response Content=" + json);
        }
        catch(Exception ex) {
            Console.WriteLine("ERROR! Inner:", ex.InnerException);
            Console.WriteLine("ERROR! Msg:", ex.Message);
            Console.WriteLine("ERROR! Stack:", ex.StackTrace);
        }

        return await response.Content.ReadFromJsonAsync<ODataResult<T>>();
    }


    public async Task<Dictionary<string, List<string>>> RegisterUserAsync(RegisterModel registerModel)
    {
        var response = await httpClient.PostAsJsonAsync(
            "/identity/register",
            new { registerModel.Email, registerModel.Password });

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var json = await response.Content.ReadAsStringAsync();

            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            return problemDetails?.Errors != null
                ? problemDetails.Errors
                : throw new Exception("Bad Request");
        }

        response.EnsureSuccessStatusCode();

        response = await httpClient.PostAsJsonAsync(
            "/identity/login",
            new { registerModel.Email, registerModel.Password });

        response.EnsureSuccessStatusCode();

        var accessTokenResponse = await response.Content.ReadFromJsonAsync<AccessTokenResponse>()
            ?? throw new Exception("Failed to authenticate");

        HttpRequestMessage request = new(HttpMethod.Put, "/api/user/@me");
        request.Headers.Authorization = new("Bearer", accessTokenResponse.AccessToken);
        request.Content = JsonContent.Create(new UpdateApplicationUserDto
        {
            FirstName = registerModel.FirstName,
            LastName = registerModel.LastName,
            Title = registerModel.Title,
            CompanyName = registerModel.CompanyName,
            Photo = registerModel.Photo,
        });

        response = await httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return [];
    }

    public async Task<ApplicationUserDto[]?> ListUserAsync()
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, "/api/user");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<ApplicationUserDto[]>();
    }

    public Task<ODataResult<ApplicationUserDto>?> ListUserODataAsync(
        int? top = null,
        int? skip = null,
        string? orderby = null,
        string? filter = null,
        bool count = false,
        string? expand = null)
    {
        return GetODataAsync<ApplicationUserDto>("User", top, skip, orderby, filter, count, expand);
    }

    public async Task<ApplicationUserWithRolesDto?> GetUserByIdAsync(string id)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, $"/api/user/{id}");
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<ApplicationUserWithRolesDto>();
    }

    public async Task UpdateUserAsync(string id, UpdateApplicationUserDto data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Put, $"/api/user/{id}");
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task DeleteUserAsync(string id)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Delete, $"/api/user/{id}");
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<AccountType[]?> ListAccountTypeAsync()
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, "/api/accounttype");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<AccountType[]>();
    }

    public Task<ODataResult<AccountType>?> ListAccountTypeODataAsync(
        int? top = null,
        int? skip = null,
        string? orderby = null,
        string? filter = null,
        bool count = false,
        string? expand = null)
    {
        return GetODataAsync<AccountType>("AccountType", top, skip, orderby, filter, count, expand);
    }

    public async Task<AccountType?> GetAccountTypeByIdAsync(long key)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, $"/api/accounttype/{key}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<AccountType>();
    }

    public async Task UpdateAccountTypeAsync(long key, AccountType data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Put, $"/api/accounttype/{key}");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<AccountType?> InsertAccountTypeAsync(AccountType data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Post, "/api/accounttype");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<AccountType>();
    }

    public async Task DeleteAccountTypeAsync(long key)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Delete, $"/api/accounttype/{key}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<Account[]?> ListAccountAsync()
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, "/api/account");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<Account[]>();
    }

    public Task<ODataResult<Account>?> ListAccountODataAsync(
        int? top = null,
        int? skip = null,
        string? orderby = null,
        string? filter = null,
        bool count = false,
        string? expand = null)
    {
        return GetODataAsync<Account>("Account", top, skip, orderby, filter, count, expand);
    }

    public async Task<Account?> GetAccountByIdAsync(long? key)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, $"/api/account/{key}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<Account>();
    }

    public async Task UpdateAccountAsync(long key, Account data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Put, $"/api/account/{key}");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<Account?> InsertAccountAsync(Account data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Post, "/api/account");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<Account>();
    }

    public async Task DeleteAccountAsync(long key)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Delete, $"/api/account/{key}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<BudgetAccount[]?> ListBudgetAccountAsync()
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, "/api/budgetaccount");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<BudgetAccount[]>();
    }

    public Task<ODataResult<BudgetAccount>?> ListBudgetAccountODataAsync(
        int? top = null,
        int? skip = null,
        string? orderby = null,
        string? filter = null,
        bool count = false,
        string? expand = null)
    {
        return GetODataAsync<BudgetAccount>("BudgetAccount", top, skip, orderby, filter, count, expand);
    }

    public async Task<BudgetAccount?> GetBudgetAccountByIdAsync(long key)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, $"/api/budgetaccount/{key}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<BudgetAccount>();
    }

    public async Task UpdateBudgetAccountAsync(long key, BudgetAccount data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Put, $"/api/budgetaccount/{key}");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<BudgetAccount?> InsertBudgetAccountAsync(BudgetAccount data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Post, "/api/budgetaccount");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<BudgetAccount>();
    }

    public async Task DeleteBudgetAccountAsync(long key)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Delete, $"/api/budgetaccount/{key}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<BudgetExpense[]?> ListBudgetExpenseAsync()
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, "/api/budgetexpense");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<BudgetExpense[]>();
    }

    public Task<ODataResult<BudgetExpense>?> ListBudgetExpenseODataAsync(
        int? top = null,
        int? skip = null,
        string? orderby = null,
        string? filter = null,
        bool count = false,
        string? expand = null)
    {
        return GetODataAsync<BudgetExpense>("BudgetExpense", top, skip, orderby, filter, count, expand);
    }

    public async Task<BudgetExpense?> GetBudgetExpenseByIdAsync(long key)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, $"/api/budgetexpense/{key}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<BudgetExpense>();
    }

    public async Task UpdateBudgetExpenseAsync(long key, BudgetExpense data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Put, $"/api/budgetexpense/{key}");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<BudgetExpense?> InsertBudgetExpenseAsync(BudgetExpense data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Post, "/api/budgetexpense");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<BudgetExpense>();
    }

    public async Task DeleteBudgetExpenseAsync(long key)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Delete, $"/api/budgetexpense/{key}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<BudgetMonth[]?> ListBudgetMonthAsync()
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, "/api/budgetmonth");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<BudgetMonth[]>();
    }

    public Task<ODataResult<BudgetMonth>?> ListBudgetMonthODataAsync(
        int? top = null,
        int? skip = null,
        string? orderby = null,
        string? filter = null,
        bool count = false,
        string? expand = null)
    {
        return GetODataAsync<BudgetMonth>("BudgetMonth", top, skip, orderby, filter, count, expand);
    }

    public async Task<BudgetMonth?> GetBudgetMonthByIdAsync(long key)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, $"/api/budgetmonth/{key}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<BudgetMonth>();
    }

    public async Task UpdateBudgetMonthAsync(long key, BudgetMonth data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Put, $"/api/budgetmonth/{key}");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<BudgetMonth?> InsertBudgetMonthAsync(BudgetMonth data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Post, "/api/budgetmonth");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<BudgetMonth>();
    }

    public async Task DeleteBudgetMonthAsync(long key)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Delete, $"/api/budgetmonth/{key}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<BudgetIncome[]?> ListBudgetIncomeAsync()
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, "/api/budgetincome");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<BudgetIncome[]>();
    }

    public Task<ODataResult<BudgetIncome>?> ListBudgetIncomeODataAsync(
        int? top = null,
        int? skip = null,
        string? orderby = null,
        string? filter = null,
        bool count = false,
        string? expand = null)
    {
        return GetODataAsync<BudgetIncome>("Budget Income", top, skip, orderby, filter, count, expand);
    }

    public async Task<BudgetIncome?> GetBudgetIncomeByIdAsync(long key)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, $"/api/budgetincome/{key}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<BudgetIncome>();
    }

    public async Task UpdateBudgetIncomeAsync(long key, BudgetIncome data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Put, $"/api/budgetincome/{key}");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<BudgetIncome?> InsertBudgetIncomeAsync(BudgetIncome data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Post, "/api/budgetincome");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<BudgetIncome>();
    }

    public async Task DeleteBudgetIncomeAsync(long key)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Delete, $"/api/budgetincome/{key}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<Category[]?> ListCategoryAsync()
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, "/api/category");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<Category[]>();
    }

    public Task<ODataResult<Category>?> ListCategoryODataAsync(
        int? top = null,
        int? skip = null,
        string? orderby = null,
        string? filter = null,
        bool count = false,
        string? expand = null)
    {
        return GetODataAsync<Category>("Category", top, skip, orderby, filter, count, expand);
    }

    public async Task<Category?> GetCategoryByIdAsync(long key)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, $"/api/category/{key}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<Category>();
    }

    public async Task UpdateCategoryAsync(long key, Category data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Put, $"/api/category/{key}");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<Category?> InsertCategoryAsync(Category data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Post, "/api/category");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<Category>();
    }

    public async Task DeleteCategoryAsync(long key)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Delete, $"/api/category/{key}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<Transaction[]?> ListTransactionAsync()
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, "/api/transaction");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<Transaction[]>();
    }

    public Task<ODataResult<Transaction>?> ListTransactionODataAsync(
        int? top = null,
        int? skip = null,
        string? orderby = null,
        string? filter = null,
        bool count = false,
        string? expand = null)
    {
        return GetODataAsync<Transaction>("Transaction", top, skip, orderby, filter, count, expand);
    }

    public async Task<Transaction?> GetTransactionByIdAsync(long key)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, $"/api/transaction/{key}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<Transaction>();
    }

    public async Task UpdateTransactionAsync(long key, Transaction data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Put, $"/api/transaction/{key}");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<Transaction?> InsertTransactionAsync(Transaction data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Post, "/api/transaction");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<Transaction>();
    }

    public async Task DeleteTransactionAsync(long key)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Delete, $"/api/transaction/{key}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<TransactionSplit[]?> ListTransactionSplitAsync()
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, "/api/transactionsplit");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<TransactionSplit[]>();
    }

    public Task<ODataResult<TransactionSplit>?> ListTransactionSplitODataAsync(
        int? top = null,
        int? skip = null,
        string? orderby = null,
        string? filter = null,
        bool count = false,
        string? expand = null)
    {
        return GetODataAsync<TransactionSplit>("TransactionSplit", top, skip, orderby, filter, count, expand);
    }

    public async Task<TransactionSplit?> GetTransactionSplitByIdAsync(long key)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, $"/api/transactionsplit/{key}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<TransactionSplit>();
    }

    public async Task UpdateTransactionSplitAsync(long key, TransactionSplit data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Put, $"/api/transactionsplit/{key}");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<TransactionSplit?> InsertTransactionSplitAsync(TransactionSplit data)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Post, "/api/transactionsplit");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(data);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<TransactionSplit>();
    }

    public async Task DeleteTransactionSplitAsync(long key)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Delete, $"/api/transactionsplit/{key}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task<string> GetTransactionTotals(string AccountName)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Get, $"/api/transaction/totals?accountName={HttpUtility.UrlEncode(AccountName)}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string?> UploadImageAsync(Stream stream, int bufferSize, string contentType)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        MultipartFormDataContent content = [];
        StreamContent fileContent = new(stream, bufferSize);
        fileContent.Headers.ContentType = new(contentType);
        content.Add(fileContent, "image", "image");

        HttpRequestMessage request = new(HttpMethod.Post, $"/api/image");
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = content;

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string?> UploadCsvAsync(Stream stream, int bufferSize, string contentType)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        MultipartFormDataContent content = [];
        StreamContent fileContent = new(stream, bufferSize);
        fileContent.Headers.ContentType = new(contentType);
        content.Add(fileContent, "file", "CsvImport.csv");

        HttpRequestMessage request = new(HttpMethod.Post, $"/api/csv");
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = content;
        
        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string?> UploadBankAccountCsvAsync(Stream stream, int bufferSize, string contentType, string accountName)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        MultipartFormDataContent content = [];
        StreamContent fileContent = new(stream, bufferSize);
        fileContent.Headers.ContentType = new(contentType);
        content.Add(fileContent, "file", "CsvImport.csv");
        content.Add(new StringContent(accountName), "accountName");

        HttpRequestMessage request = new(HttpMethod.Post, $"/api/bankcsv");
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = content;

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string?> UploadImageAsync(IBrowserFile image)
    {
        using var stream = image.OpenReadStream(image.Size);

        return await UploadImageAsync(stream, Convert.ToInt32(image.Size), image.ContentType);
    }

    public async Task<string?> UploadCsvAsync(IBrowserFile image)
    {
        using var stream = image.OpenReadStream(image.Size);

        return await UploadCsvAsync(stream, Convert.ToInt32(image.Size), image.ContentType);
    }

    public async Task<string?> UploadBankAccountCsvAsync(IBrowserFile image, string accountName)
    {
        using var stream = image.OpenReadStream(image.Size);

        return await UploadBankAccountCsvAsync(stream, Convert.ToInt32(image.Size), image.ContentType, accountName);
    }

    public async Task ChangePasswordAsync(string oldPassword, string newPassword)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Post, $"/identity/manage/info");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(new { oldPassword, newPassword });

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }

    public async Task ModifyRolesAsync(string key, IEnumerable<string> roles)
    {
        var token = await authenticationStateProvider.GetBearerTokenAsync()
            ?? throw new Exception("Not authorized");

        HttpRequestMessage request = new(HttpMethod.Put, $"/api/user/{key}/roles");
        request.Headers.Authorization = new("Bearer", token);
        request.Content = JsonContent.Create(roles);

        var response = await httpClient.SendAsync(request);

        await HandleResponseErrorsAsync(response);
    }
}
