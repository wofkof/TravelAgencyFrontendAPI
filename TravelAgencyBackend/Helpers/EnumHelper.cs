using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TravelAgencyBackend.Helpers
{
    public static class EnumHelper
    {
        public static List<SelectListItem> GetSelectListWithDisplayName<TEnum>(bool excludeDeleted = false) where TEnum : Enum
        {
            var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();

            if (excludeDeleted && typeof(TEnum) == typeof(TravelAgency.Shared.Models.EmployeeStatus))
            {
                values = values.Where(v => !v.ToString().Equals("Deleted")).Cast<TEnum>();
            }

            return values.Select(e => new SelectListItem
            {
                Text = e.GetType()
                         .GetMember(e.ToString())
                         .First()
                         .GetCustomAttribute<DisplayAttribute>()?.Name ?? e.ToString(),
                Value = Convert.ToInt32(e).ToString()
            }).ToList();
        }

        public static string GetDisplayName(this Enum value)
        {
            return value.GetType()
                        .GetMember(value.ToString())
                        .First()
                        .GetCustomAttribute<DisplayAttribute>()?.Name ?? value.ToString();
        }
    }
}
