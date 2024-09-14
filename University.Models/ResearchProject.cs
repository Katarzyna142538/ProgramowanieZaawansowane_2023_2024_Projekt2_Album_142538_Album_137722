using System;
using System.Collections.Generic;

namespace University.Models
{
    public class ResearchProject
    {
        public string ProjectId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string>? TeamMembers { get; set; } = null;
        public string Supervisor { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public float Budget { get; set; } = 0;
    }
}

