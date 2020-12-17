using System.ComponentModel.DataAnnotations;

namespace TaskBud.Business.Data
{
    public enum TaskAction
    {
        [Display(Name = "Completed")]
        Completed,

        [Display(Name = "Assigned")]
        Assigned,

        [Display(Name = "Title")]
        TitleChange,

        [Display(Name = "Description")]
        DescriptionChange,

        [Display(Name = "Priority")]
        PriorityChange,

        [Display(Name = "Created")]
        Created,

        [Display(Name = "Wait Until")]
        WaitUntilChange,

        [Display(Name = "Repeat After")]
        RepeatAfterChange,


        [Display(Name = "Starter Assignee")]
        StarterAssignee,
    }
}