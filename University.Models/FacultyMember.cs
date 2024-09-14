using System;
using System.Collections.Generic;


namespace University.Models
{
    public class FacultyMember
    {
        public string FacultyId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; } = 0;
        public string Gender { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string OfficeRoomNumber { get; set; } = string.Empty;

    }
}
