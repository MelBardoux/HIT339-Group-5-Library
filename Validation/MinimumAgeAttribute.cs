using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Validation

/// Custom validation attribute that checks a date of birth meets a minimum age requirement.
/// Used to ensure borrowers are at least 12 years old.
{
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        public MinimumAgeAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateOnly dob)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var age = today.Year - dob.Year;
                if (dob > today.AddYears(-age)) age--;

                if (age < _minimumAge)
                {
                    return new ValidationResult($"Borrower must be at least {_minimumAge} years old.");
                }
            }

            return ValidationResult.Success;
        }
    }
}