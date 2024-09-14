using University.Models;
using Microsoft.EntityFrameworkCore;

namespace University.Data
{
    public class UniversityContext : DbContext
    {
        public UniversityContext()
        {
        }

        public UniversityContext(DbContextOptions<UniversityContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }

        public DbSet<FacultyMember> FacultyMembers { get; set; }

        public DbSet<ResearchProject> ResearchProjects { get; set; }

        public DbSet<FacultyToProject> FacultyToProjects { get; set; }
        


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase("UniversityDb");
                optionsBuilder.UseLazyLoadingProxies();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Course>().HasKey(c => c.Course_Code);
            modelBuilder.Entity<Course>().Ignore(s => s.IsSelected);
            modelBuilder.Entity<FacultyMember>().HasKey(f => f.FacultyId);
            modelBuilder.Entity<ResearchProject>().HasKey(r => r.ProjectId);

            modelBuilder.Entity<FacultyToProject>(entity =>
            {
                entity.HasKey(e => new { e.FacultyId, e.ProjectId });

                entity.HasOne<FacultyMember>()
                    .WithMany()
                    .HasForeignKey(e => e.FacultyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<ResearchProject>()
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            

            modelBuilder.Entity<Course>()
            .Property(c => c.Prerequisites)
            .HasConversion(
                v => v == null ? string.Empty : string.Join(',', v),  // Obsługuje null
                v => string.IsNullOrEmpty(v) ? new List<string>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()  // Obsługuje null i pusty ciąg
            );

            modelBuilder.Entity<ResearchProject>()
            .Property(c => c.TeamMembers)
            .HasConversion(
                v => v == null ? string.Empty : string.Join(',', v),  // Obsługuje null
                v => string.IsNullOrEmpty(v) ? new List<string>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()  // Obsługuje null i pusty ciąg
            );


            modelBuilder.Entity<Student>().HasData(
                new Student { StudentId = 1, Name = "Wieńczysław", LastName = "Nowakowicz", PESEL = "PESEL1", BirthDate = new DateTime(1987, 05, 22), Gender = "M", PlaceOfBirth = "Warszawa", PlaceOfResidence = "Wroclaw", AddressLine1 = "", AddressLine2 = "", PostalCode = "1234" },
                new Student { StudentId = 2, Name = "Stanisław", LastName = "Nowakowicz", PESEL = "PESEL2", BirthDate = new DateTime(2019, 06, 25) },
                new Student { StudentId = 3, Name = "Eugenia", LastName = "Nowakowicz", PESEL = "PESEL3", BirthDate = new DateTime(2021, 06, 08) });

            modelBuilder.Entity<Course>().HasData(
                new Course { 
                    Course_Code = "C1", 
                    Title = "Matematyka", 
                    Instructor = "Michalina Warszawa", 
                    Schedule = "Fall 2024", 
                    Description = "Basic Mathematics", 
                    Credits = 5, 
                    Department = "Mathematics", 
                    Prerequisites = new List<string> { "None" } 
                },
                new Course { 
                    Course_Code = "C2", 
                    Title = "Biologia", 
                    Instructor = "Halina Katowice", 
                    Schedule = "Spring 2024", 
                    Description = "Introduction to Biology", 
                    Credits = 4, 
                    Department = "Biology", 
                    Prerequisites = new List<string> { "None" } 
                },
                new Course { 
                    Course_Code = "C3", 
                    Title = "Chemia", 
                    Instructor = "Jan Nowak", 
                    Schedule = "Winter 2024", 
                    Description = "Basic Chemistry", 
                    Credits = 4, 
                    Department = "Chemistry", 
                    Prerequisites = new List<string> { "None" } 
                }
            );

            
            modelBuilder.Entity<FacultyMember>().HasData(
                new FacultyMember { 
                    FacultyId = "F001", 
                    Name = "Jan Kowalski", 
                    Age = 45, 
                    Gender = "M", 
                    Department = "Mathematics", 
                    Position = "Professor", 
                    Email = "jan.kowalski@university.edu", 
                    OfficeRoomNumber = "101" 
                },
                new FacultyMember { 
                    FacultyId = "F002", 
                    Name = "Anna Nowak", 
                    Age = 38, 
                    Gender = "F", 
                    Department = "Biology", 
                    Position = "Assistant Professor", 
                    Email = "anna.nowak@university.edu", 
                    OfficeRoomNumber = "202" 
                },
                new FacultyMember { 
                    FacultyId = "F003", 
                    Name = "Piotr Wójcik", 
                    Age = 50, 
                    Gender = "M", 
                    Department = "Chemistry", 
                    Position = "Lecturer", 
                    Email = "piotr.wojcik@university.edu", 
                    OfficeRoomNumber = "303" 
                }
            );

            modelBuilder.Entity<ResearchProject>().HasData(
                new ResearchProject
                {
                    ProjectId = "P1",
                    Title = "Quantum Computing Advances",
                    Description = "Research on the next generation of quantum processors.",
                    StartDate = new DateTime(2023, 01, 15),
                    EndDate = new DateTime(2025, 12, 31),
                    Supervisor = "Dr. Jan Kowalski",
                    Budget = 500000
                },
                new ResearchProject
                {
                    ProjectId = "P2",
                    Title = "Renewable Energy Solutions",
                    Description = "Developing new methods for solar and wind energy efficiency.",
                    StartDate = new DateTime(2022, 05, 01),
                    EndDate = new DateTime(2024, 10, 30),
                    Budget = 750000
                },
                new ResearchProject
                {
                    ProjectId = "P4",
                    Title = "AI in Healthcare",
                    Description = "Exploring AI-driven diagnostic tools and personalized medicine.",
                    StartDate = new DateTime(2023, 03, 01),
                    EndDate = new DateTime(2026, 06, 30),
                    Budget = 650000
                }
            ); 

            modelBuilder.Entity<FacultyToProject>().HasData(
                new FacultyToProject
                {
                    FacultyId = "F002",
                    ProjectId = "P4"
                },
                new FacultyToProject
                {
                    FacultyId = "F002",
                    ProjectId = "P2"
                }
            );
        }

    }
}
