﻿namespace NCS.DSS.Diversity.GetDiversityByIdHttpTrigger.Service
{
    public interface IGetDiversityByIdHttpTriggerService
    {
        Task<Models.Diversity> GetDiversityDetailByIdAsync(Guid customerId, Guid diversityId);
    }
}