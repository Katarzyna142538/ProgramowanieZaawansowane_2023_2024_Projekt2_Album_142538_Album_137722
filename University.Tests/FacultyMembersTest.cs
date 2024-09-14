using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using University.Data;
using University.Interfaces;
using University.Models;
using University.Services;
using University.ViewModels;

namespace University.Tests
{
    [TestClass]
    public class FacultyMemberTests
    {
        private IDialogService _dialogService;
        private DbContextOptions<UniversityContext> _options;

        [TestInitialize()]
        public void Initialize()
        {
            _options = new DbContextOptionsBuilder<UniversityContext>()
                .UseInMemoryDatabase(databaseName: "UniversityTestDB")
                .Options;
            SeedTestDB();
            _dialogService = new DialogService();
        }

        private void SeedTestDB()
        {
            using (var context = new UniversityContext(_options))
            {
                context.Database.EnsureDeleted();
                List<FacultyMember> facultyMembers = new List<FacultyMember>
                {
                    new FacultyMember { FacultyId = "F1", Name = "Dr. Smith", Age = 45, Gender = "Male", Department = "Physics", Position = "Professor", Email = "smith@university.com", OfficeRoomNumber = "101" },
                    new FacultyMember { FacultyId = "F2", Name = "Dr. Johnson", Age = 39, Gender = "Female", Department = "Mathematics", Position = "Assistant Professor", Email = "johnson@university.com", OfficeRoomNumber = "102" }
                };

                context.FacultyMembers.AddRange(facultyMembers);
                context.SaveChanges();
            }
        }

        [TestMethod]
        public void Show_all_faculty_members()
        {
            using (var context = new UniversityContext(_options))
            {
                FacultyMemberViewModel facultyViewModel = new FacultyMemberViewModel(context, _dialogService);
                bool hasData = facultyViewModel.FacultyMembers.Any();
                Assert.IsTrue(hasData);
            }
        }

        [TestMethod]
        public void Add_faculty_member_without_email()
        {
            using (var context = new UniversityContext(_options))
            {
                var addFacultyViewModel = new AddFacultyMemberViewModel(context, _dialogService)
                {
                    FacultyId = "F3",
                    Name = "Dr. Williams",
                    Age = 50,
                    Gender = "Female",
                    Department = "Computer Science",
                    Position = "Lecturer",
                    OfficeRoomNumber = "103",
                };
                addFacultyViewModel.Save.Execute(null);

                bool newFacultyExists = context.FacultyMembers.Any(f => f.FacultyId == "F3");
                Assert.IsFalse(newFacultyExists);
            }
        }

        [TestMethod]
        public void Add_faculty_member_without_name()
        {
            using (var context = new UniversityContext(_options))
            {
                var addFacultyViewModel = new AddFacultyMemberViewModel(context, _dialogService)
                {
                    FacultyId = "F4",
                    Age = 42,
                    Gender = "Male",
                    Department = "Chemistry",
                    Position = "Professor",
                    Email = "williams@university.com",
                    OfficeRoomNumber = "104",
                };
                addFacultyViewModel.Save.Execute(null);

                bool newFacultyExists = context.FacultyMembers.Any(f => f.FacultyId == "F4");
                Assert.IsFalse(newFacultyExists);
            }
        }

        [TestMethod]
        public void Add_faculty_member_and_verify()
        {
            using (var context = new UniversityContext(_options))
            {
                // Arrange
                var addFacultyViewModel = new AddFacultyMemberViewModel(context, _dialogService)
                {
                    FacultyId = "F5",
                    Name = "Dr. Green",
                    Age = 37,
                    Gender = "Male",
                    Department = "Biology",
                    Position = "Assistant Professor",
                    Email = "green@university.com",
                    OfficeRoomNumber = "105",
                };

                // Act
                addFacultyViewModel.Save.Execute(null);

                // Assert
                var addedFaculty = context.FacultyMembers.SingleOrDefault(f => f.FacultyId == "F5");
                Assert.IsNotNull(addedFaculty, "The faculty member was not added to the database.");
                Assert.AreEqual("Dr. Green", addedFaculty.Name, "The name does not match.");
                Assert.AreEqual(37, addedFaculty.Age, "The age does not match.");
                Assert.AreEqual("Male", addedFaculty.Gender, "The gender does not match.");
                Assert.AreEqual("Biology", addedFaculty.Department, "The department does not match.");
                Assert.AreEqual("Assistant Professor", addedFaculty.Position, "The position does not match.");
                Assert.AreEqual("green@university.com", addedFaculty.Email, "The email does not match.");
                Assert.AreEqual("105", addedFaculty.OfficeRoomNumber, "The office room number does not match.");
            }
        }

        [TestMethod]
        public void Delete_faculty_member()
        {
            using (var context = new UniversityContext(_options))
            {
                FacultyMember faculty = context.FacultyMembers.FirstOrDefault(f => f.FacultyId == "F1");
                if (faculty != null)
                {
                    context.FacultyMembers.Remove(faculty);
                    context.SaveChanges();
                }

                bool facultyExists = context.FacultyMembers.Any(f => f.FacultyId == "F1");
                Assert.IsFalse(facultyExists);
            }
        }

        [TestMethod]
        public void Update_faculty_member_details()
        {
            using (var context = new UniversityContext(_options))
            {
                FacultyMember faculty = context.FacultyMembers.FirstOrDefault(f => f.FacultyId == "F2");
                if (faculty != null)
                {
                    faculty.Name = "Dr. Emily Johnson";
                    faculty.Age = 40;
                    context.SaveChanges();
                }

                FacultyMember updatedFaculty = context.FacultyMembers.FirstOrDefault(f => f.FacultyId == "F2");
                Assert.IsNotNull(updatedFaculty);
                Assert.AreEqual("Dr. Emily Johnson", updatedFaculty.Name);
                Assert.AreEqual(40, updatedFaculty.Age);
            }
        }

        [TestMethod]
        public void Validate_faculty_member_age()
        {
            using (var context = new UniversityContext(_options))
            {
                var addFacultyViewModel = new AddFacultyMemberViewModel(context, _dialogService)
                {
                    FacultyId = "F6",
                    Name = "Dr. Blue",
                    Age = -1, // Invalid value
                    Gender = "Non-binary",
                    Department = "History",
                    Position = "Lecturer",
                    Email = "blue@university.com",
                    OfficeRoomNumber = "106",
                };
                addFacultyViewModel.Save.Execute(null);

                bool newFacultyExists = context.FacultyMembers.Any(f => f.FacultyId == "F6");
                Assert.IsFalse(newFacultyExists);
            }
        }
    }
}
