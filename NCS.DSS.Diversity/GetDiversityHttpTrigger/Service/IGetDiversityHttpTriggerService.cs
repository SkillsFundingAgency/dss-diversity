namespace NCS.DSS.Diversity.GetDiversityHttpTrigger.Service
{
    public interface IGetDiversityHttpTriggerService
    {
        Task<List<Models.Diversity>> GetDiversityDetailForCustomerAsync(Guid customerId);
    }
}