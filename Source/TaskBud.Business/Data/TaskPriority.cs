using System.ComponentModel.DataAnnotations;

namespace TaskBud.Business.Data
{
    public enum TaskPriority
    {
        [Display(Name = "Very Low")]
        VeryLow = 0,

        Low = 1,

        Medium = 2,

        High = 3,

        [Display(Name = "Very High")]
        VeryHigh = 4,

        Urgent = 5,

        Critical = 6
    }
}