using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace LibrarySystem.Validation

/// Custom validation attribute for year fields (e.g. publication year, release year).
/// Validates the year is between a minimum (1450) and the current year.
/// Supports an "unknown" checkbox flag that bypasses validation.
/// Implements IClientModelValidator for client-side validation using jQuery Validate.
{
    public class YearRangeAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly int _minimumYear;
        private readonly string _unknownFlagProperty;

        public YearRangeAttribute(int minimumYear, string unknownFlagProperty)
        {
            _minimumYear = minimumYear;
            _unknownFlagProperty = unknownFlagProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var unknownProperty = validationContext.ObjectType.GetProperty(_unknownFlagProperty);
            if (unknownProperty == null)
            {
                return new ValidationResult($"Property '{_unknownFlagProperty}' not found.");
            }

            var unknownValue = (bool?)unknownProperty.GetValue(validationContext.ObjectInstance);

            if (unknownValue == true)
            {
                return ValidationResult.Success;
            }

            if (value == null)
            {
                return new ValidationResult("A publication year is required unless marked as unknown.");
            }

            if (value is not int year)
            {
                return new ValidationResult("Please enter a valid four-digit year (e.g. 1999).");
            }

            int currentYear = DateTime.Now.Year;

            if (year < _minimumYear || year > currentYear)
            {
                return new ValidationResult($"Year must be between {_minimumYear} and {currentYear}.");
            }

            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.TryAdd("data-val", "true");
            context.Attributes.TryAdd("data-val-yearrange", $"Year must be between {_minimumYear} and {DateTime.Now.Year}.");
            context.Attributes.TryAdd("data-val-yearrange-min", _minimumYear.ToString());
            context.Attributes.TryAdd("data-val-yearrange-max", DateTime.Now.Year.ToString());
            context.Attributes.TryAdd("data-val-yearrange-unknownflag", _unknownFlagProperty);
        }
    }
}