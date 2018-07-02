using System;
using System.Threading.Tasks;

namespace NCS.DSS.Diversity.PostDiversityHttpTrigger.Service
{
    public interface IPostDiversityHttpTriggerService
    {
        Task<Guid?> CreateAsync(Models.Diversity diversity);
    }
}