using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TravelAgencyBackend.Helpers
{
    public static class EnumHelper
    {
        // ✅ 這個方法會產生顯示中文的下拉選單清單
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

        // ✅ 如果你之前有這段，也可以保留
        public static string GetDisplayName(this Enum value)
        {
            return value.GetType()
                        .GetMember(value.ToString())
                        .First()
                        .GetCustomAttribute<DisplayAttribute>()?.Name ?? value.ToString();
        }
    }
}
