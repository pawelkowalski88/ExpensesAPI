using System;
using System.Collections.Generic;
using System.Text;

namespace ExpensesAPI.Domain.ExternalAPIUtils
{
    public interface ITokenRepository
    {
        void SetToken(string token);
        string RetrieveToken();
    }
}
