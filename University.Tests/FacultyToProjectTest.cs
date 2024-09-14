using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using University.Data;
using University.Models;

namespace University.Tests
{
    [TestClass]
    public class FacultyToProjectTests
    {
        private DbContextOptions<UniversityContext> _options;

        [TestInitialize]
        public void Initialize()
        {
            _options = new DbContextOptionsBuilder<UniversityContext>()
                .UseInMemoryDatabase(databaseName: "UniversityTestDB")
                .Options;
            SeedTestDB();
        }

        private void SeedTestDB()
        {
            using (var context = new UniversityContext(_options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Seed faculty members
                var facultyMembers = new List<FacultyMember>
                {
                    new FacultyMember { FacultyId = "F1", Name = "Dr. Smith" },
                    new FacultyMember { FacultyId = "F2", Name = "Dr. Johnson" }
                };

                // Seed research projects
                var researchProjects = new List<ResearchProject>
                {
                    new ResearchProject { ProjectId = "P6", Title = "Quantum Computing" },
                    new ResearchProject { ProjectId = "P7", Title = "Artificial Intelligence" }
                };

                // Add entities to the context
                context.FacultyMembers.AddRange(facultyMembers);
                context.ResearchProjects.AddRange(researchProjects);
                context.SaveChanges();
            }
        }

        [TestMethod]
        public void Add_faculty_to_project()
        {
            using (var context = new UniversityContext(_options))
            {
                var faculty = context.FacultyMembers.First(f => f.FacultyId == "F1");
                var project = context.ResearchProjects.First(p => p.ProjectId == "P6");

                var facultyToProject = new FacultyToProject
                {
                    FacultyId = faculty.FacultyId,
                    ProjectId = project.ProjectId
                };
                context.Add(facultyToProject);
                context.SaveChanges();

                var isAssociated = context.Set<FacultyToProject>()
                    .Any(fp => fp.FacultyId == "F1" && fp.ProjectId == "P6");

                Assert.IsTrue(isAssociated, "The project was not associated with the faculty member.");
            }
        }

        [TestMethod]
        public void Retrieve_faculty_projects()
        {
            using (var context = new UniversityContext(_options))
            {
                var faculty = context.FacultyMembers.First(f => f.FacultyId == "F1");
                var project = context.ResearchProjects.First(p => p.ProjectId == "P6");

                var facultyToProject = new FacultyToProject
                {
                    FacultyId = faculty.FacultyId,
                    ProjectId = project.ProjectId
                };
                context.Add(facultyToProject);
                context.SaveChanges();

                var facultyProjects = context.Set<FacultyToProject>()
                    .Where(fp => fp.FacultyId == "F1")
                    .Select(fp => fp.ProjectId)
                    .ToList();

                Assert.AreEqual(1, facultyProjects.Count, "The number of associated projects is incorrect.");
                Assert.AreEqual("P6", facultyProjects.First(), "The associated project is incorrect.");
            }
        }

        [TestMethod]
        public void Remove_faculty_from_project()
        {
            using (var context = new UniversityContext(_options))
            {
                var faculty = context.FacultyMembers.First(f => f.FacultyId == "F1");
                var project = context.ResearchProjects.First(p => p.ProjectId == "P6");

                var facultyToProject = new FacultyToProject
                {
                    FacultyId = faculty.FacultyId,
                    ProjectId = project.ProjectId
                };
                context.Add(facultyToProject);
                context.SaveChanges();

                context.Remove(facultyToProject);
                context.SaveChanges();

                var isAssociated = context.Set<FacultyToProject>()
                    .Any(fp => fp.FacultyId == "F1" && fp.ProjectId == "P6");

                Assert.IsFalse(isAssociated, "The project was not removed from the faculty member.");
            }
        }

        [TestMethod]
        public void Add_multiple_faculty_to_project()
        {
            using (var context = new UniversityContext(_options))
            {
                var project = context.ResearchProjects.First(p => p.ProjectId == "P6");
                var faculty1 = context.FacultyMembers.First(f => f.FacultyId == "F1");
                var faculty2 = context.FacultyMembers.First(f => f.FacultyId == "F2");

                var facultyToProject1 = new FacultyToProject
                {
                    FacultyId = faculty1.FacultyId,
                    ProjectId = project.ProjectId
                };

                var facultyToProject2 = new FacultyToProject
                {
                    FacultyId = faculty2.FacultyId,
                    ProjectId = project.ProjectId
                };

                context.AddRange(facultyToProject1, facultyToProject2);
                context.SaveChanges();

                var projectAssociations = context.Set<FacultyToProject>()
                    .Where(fp => fp.ProjectId == "P6")
                    .ToList();

                Assert.AreEqual(2, projectAssociations.Count, "The number of associated faculty members is incorrect.");
                Assert.IsTrue(projectAssociations.Any(fp => fp.FacultyId == "F1"), "Faculty member F1 was not associated with the project.");
                Assert.IsTrue(projectAssociations.Any(fp => fp.FacultyId == "F2"), "Faculty member F2 was not associated with the project.");
            }
        }

        [TestMethod]
        public void Modify_associated_faculty_member()
        {
            using (var context = new UniversityContext(_options))
            {
                // Arrange
                var faculty = context.FacultyMembers.First(f => f.FacultyId == "F1");
                var project = context.ResearchProjects.First(p => p.ProjectId == "P6");

                var facultyToProject = new FacultyToProject
                {
                    FacultyId = faculty.FacultyId,
                    ProjectId = project.ProjectId
                };
                context.Add(facultyToProject);
                context.SaveChanges();

                // Act - Modify Faculty Member
                faculty.Name = "Dr. John Smith";
                context.SaveChanges();

                // Assert
                var updatedFaculty = context.FacultyMembers.First(f => f.FacultyId == "F1");
                Assert.AreEqual("Dr. John Smith", updatedFaculty.Name, "The faculty member's name was not updated correctly.");
            }
        }

        [TestMethod]
        public void Modify_associated_research_project()
        {
            using (var context = new UniversityContext(_options))
            {
                // Arrange
                var faculty = context.FacultyMembers.First(f => f.FacultyId == "F1");
                var project = context.ResearchProjects.First(p => p.ProjectId == "P6");

                var facultyToProject = new FacultyToProject
                {
                    FacultyId = faculty.FacultyId,
                    ProjectId = project.ProjectId
                };
                context.Add(facultyToProject);
                context.SaveChanges();

                // Act - Modify Research Project
                project.Title = "Advanced Quantum Computing";
                context.SaveChanges();

                // Assert
                var updatedProject = context.ResearchProjects.First(p => p.ProjectId == "P6");
                Assert.AreEqual("Advanced Quantum Computing", updatedProject.Title, "The research project's title was not updated correctly.");
            }
        }
    }
}
