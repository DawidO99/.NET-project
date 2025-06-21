// Extensions/EnumExtensions.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace CarWorkshopManagementSystem.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            // Pobiera DisplayAttribute.Name, jeśli istnieje; w przeciwnym razie zwraca domyślną nazwę enuma.
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .FirstOrDefault()?
                            .GetCustomAttribute<DisplayAttribute>()?
                            .Name ?? enumValue.ToString();
        }
    }
}