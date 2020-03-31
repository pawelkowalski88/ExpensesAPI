namespace ExpensesAPI.Persistence.Models
{
    public interface ISortingQuery
    {
        string SortBy { get; set; }
        bool SortAscending { get; set; }
    }
}