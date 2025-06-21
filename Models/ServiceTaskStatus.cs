using System.ComponentModel.DataAnnotations; // Dodaj ten using dla atrybutu Display

namespace CarWorkshopManagementSystem.Models
{
    public enum ServiceTaskStatus
    {
        [Display(Name = "Nowe")]
        New,
        [Display(Name = "W toku")]
        InProgress,
        [Display(Name = "Zakończone")]
        Completed,
        [Display(Name = "Anulowane")]
        Canceled
    }
}