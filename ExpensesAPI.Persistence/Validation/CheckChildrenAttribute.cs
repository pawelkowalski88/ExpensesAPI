using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Domain.Validation
{
    public class CheckChildrenAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var result = new ListValidationResult();
            //result.ErrorMessage = $"Error occurred at {validationContext.DisplayName}";

            IEnumerable list = value as IEnumerable;

            if (list == null)
            {
                var results = new List<ValidationResult>();
                Validator.TryValidateObject(value, validationContext, results);
                result.ListResult = results;

                return result;
            }
            else
            {
                var recursiveResultList = new List<ValidationResult>();
                foreach (var item in list)
                {
                    List<ValidationResult> nestedItemResult = new List<ValidationResult>();
                    var context = new ValidationContext(item);

                    var nestedParentResult = new ListValidationResult();
                    //result.ErrorMessage = $"Error occurred at {validationContext.DisplayName}";

                    Validator.TryValidateObject(item, context, nestedItemResult, true);

                    nestedParentResult.ListResult = nestedItemResult;
                    recursiveResultList.Add(nestedParentResult);
                }

                result.ListResult = recursiveResultList;
                return result;
            }
        }
    }
}
