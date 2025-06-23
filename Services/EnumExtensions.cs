// Services/EnumExtensions.cs (lub Utils/EnumExtensions.cs)
using System;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations; // Ważne: to using jest potrzebne dla DisplayAttribute

namespace CarWorkshopManagementSystem.Services // Możesz użyć istniejącej przestrzeni nazw np. CarWorkshopManagementSystem.Services
                                            // lub stworzyć nową jak CarWorkshopManagementSystem.Extensions
{
    public static class EnumExtensions // Klasa musi być publiczna i statyczna
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .FirstOrDefault()?
                            .GetCustomAttribute<DisplayAttribute>()? // DisplayAttribute jest w System.ComponentModel.DataAnnotations
                            .Name ?? enumValue.ToString();
        }
    }
}