using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpensesAPI.Domain.ExternalAPIUtils
{
    public class LastTokenRepository : ITokenRepository
    {
        private string _token;
        public string RetrieveToken()
        {
            return _token;
        }

        public void SetToken(string token)
        {
            var splitInput = token.Split(' ').ToList();

            if (splitInput.Count > 1)
            {
                _token = splitInput[1];
            }
            else
            {
                _token = splitInput[0];
            }
        }
    }
}
