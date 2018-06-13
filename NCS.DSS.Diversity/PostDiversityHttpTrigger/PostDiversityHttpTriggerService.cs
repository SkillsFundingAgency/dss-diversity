using System;

namespace NCS.DSS.Diversity.PostDiversityHttpTrigger
{
    public class PostDiversityHttpTriggerService
    {
        public Guid? Create(Models.Diversity diversity)
        {
            if (diversity == null)
                return null;

            return Guid.NewGuid();
        }
    }
}
