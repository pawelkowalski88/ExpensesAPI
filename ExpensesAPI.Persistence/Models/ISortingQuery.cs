namespace ExpensesAPI.Domain.Models
{
    public interface ISortingQuery
    {
        string SortBy { get; set; }
        bool SortAscending { get; set; }
    }
}