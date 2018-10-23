using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Diagnostics;

namespace DuploAuth.Helpers
{
    public static class Errors
    {
        public static List<KeyValuePair<string, string>> AddErrorToResponse(IdentityResult identityResult)
        {
            var errors = new List<KeyValuePair<string, string>>();
            foreach (var e in identityResult.Errors)
            {
                Debug.WriteLine($"{e.Code}: {e.Description}");
                errors.Add(new KeyValuePair<string, string>(e.Code, e.Description));
            }

            return errors;
        }

        public static ModelStateDictionary AddErrorsToModelState(IdentityResult identityResult, ModelStateDictionary modelState)
        {
            foreach (var e in identityResult.Errors)
            {
                Debug.WriteLine($"{e.Code}: {e.Description}");
                modelState.TryAddModelError(e.Code, e.Description);
            }

            return modelState;
        }

        public static KeyValuePair<string, string> AddErrorToResponse(string code, string description)
        {
            return new KeyValuePair<string, string>(code, description);
        }

        public static ModelStateDictionary AddErrorToModelState(string code, string description, ModelStateDictionary modelState)
        {
            modelState.TryAddModelError(code, description);
            return modelState;
        }
    }
}