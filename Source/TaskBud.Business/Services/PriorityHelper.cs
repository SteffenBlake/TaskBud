using System;
using System.Collections.Generic;
using System.Text;
using TaskBud.Business.Data;

namespace TaskBud.Business.Services
{
    public class PriorityHelper
    {
        public string IconFor(TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.VeryLow => "fa-angle-double-down",
                TaskPriority.Low => "fa-angle-down",
                TaskPriority.Medium => "fa-grip-lines",
                TaskPriority.High => "fa-angle-up",
                TaskPriority.VeryHigh => "fa-angle-double-up",
                TaskPriority.Urgent => "fa-exclamation",
                TaskPriority.Critical => "fa-exclamation-circle",
                _ => throw new ArgumentOutOfRangeException(nameof(priority))
            };
        }

        public string ColorFor(TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.VeryLow => "blue",
                TaskPriority.Low => "lightskyblue",
                TaskPriority.Medium => "green",
                TaskPriority.High => "yellow",
                TaskPriority.VeryHigh => "orange",
                TaskPriority.Urgent => "red",
                TaskPriority.Critical => "red",
                _ => throw new ArgumentOutOfRangeException(nameof(priority))
            };
        }
    }
}
