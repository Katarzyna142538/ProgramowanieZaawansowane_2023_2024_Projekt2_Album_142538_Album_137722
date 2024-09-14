using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using University.Data;
using University.Interfaces;
using University.Models;
using University.Services;
using University.ViewModels;

namespace University.Tests
{
    [TestClass]
    public class CoursesTest
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
            using UniversityContext context = new UniversityContext(_options);
            {
                context.Database.EnsureDeleted();
                List<Course> courses = new List<Course>
                {
                    new Course { Course_Code = "C1", Title = "Matematyka", Instructor = "Michalina Beldzik", Schedule = "Fall 2024", Description = "Basic Mathematics", Credits = 5, Department = "Mathematics", Prerequisites = new List<string> { "None" } },
                    new Course { Course_Code = "C2", Title = "Biologia", Instructor = "Halina Kopeć", Schedule = "Spring 2024", Description = "Introduction to Biology", Credits = 4, Department = "Biology", Prerequisites = new List<string> { "None" } },
                    new Course { Course_Code = "C3", Title = "Chemia", Instructor = "Jan Nowak", Schedule = "Winter 2024", Description = "Basic Chemistry", Credits = 4, Department = "Chemistry", Prerequisites = new List<string> { "None" } }
                };

                context.Courses.AddRange(courses);
                context.SaveChanges();
            }
        }

        [TestMethod]
        public void Show_all_courses()
        {
            using UniversityContext context = new UniversityContext(_options);
            {
                CoursesViewModel coursesViewModel = new CoursesViewModel(context, _dialogService);
                bool hasData = coursesViewModel.Courses.Any();
                Assert.IsTrue(hasData);
            }
        }

    

        [TestMethod]
        public void Add_course_without_instructor()
        {
            using UniversityContext context = new UniversityContext(_options);
            {
                AddCourseViewModel addCourseViewModel = new AddCourseViewModel(context, _dialogService)
                {
                    Course_Code = "C7",
                    Title = "Quantum Mechanics",
                    Schedule = "Winter 2024",
                    Description = "Advanced Quantum Mechanics",
                    Credits = 6,
                    Department = "Physics",
                };
                addCourseViewModel.Save.Execute(null);

                bool newCourseExists = context.Courses.Any(c => c.Course_Code == "C7");
                Assert.IsFalse(newCourseExists);
            }
        }

        [TestMethod]
        public void Add_course_without_Schedule()
        {
            using UniversityContext context = new UniversityContext(_options);
            {
                AddCourseViewModel addCourseViewModel = new AddCourseViewModel(context, _dialogService)
                {
                    Course_Code = "C7",
                    Title = "Quantum Mechanics",
                    Instructor = "Instructor I",
                    Description = "Advanced Quantum Mechanics",
                    Credits = 6,
                    Department = "Physics",
                };
                addCourseViewModel.Save.Execute(null);

                bool newCourseExists = context.Courses.Any(c => c.Course_Code == "C7");
                Assert.IsFalse(newCourseExists);
            }
        }

                [TestMethod]
        public void add_verification()
        {
            using (var context = new UniversityContext(_options))
            {
                // Arrange
                var addCourseViewModel = new AddCourseViewModel(context, _dialogService)
                {
                    Course_Code = "C9",
                    Title = "Introduction to Quantum Computing",
                    Instructor = "Dr. Quantum",
                    Schedule = "Fall 2024",
                    Description = "An introductory course to Quantum Computing.",
                    Credits = 3,
                    Department = "Computer Science"
                };

                // Act
                addCourseViewModel.Save.Execute(null);

                // Assert
                var addedCourse = context.Courses.SingleOrDefault(c => c.Course_Code == "C9");
                Assert.IsNotNull(addedCourse, "The course was not added to the database.");
                Assert.AreEqual("Introduction to Quantum Computing", addedCourse.Title, "The course title does not match.");
                Assert.AreEqual("Dr. Quantum", addedCourse.Instructor, "The instructor name does not match.");
                Assert.AreEqual(3, addedCourse.Credits, "The credits do not match.");
                Assert.AreEqual("Computer Science", addedCourse.Department, "The department does not match.");
                Assert.AreEqual("Fall 2024", addedCourse.Schedule, "The schedule does not match.");
                Assert.AreEqual("An introductory course to Quantum Computing.", addedCourse.Description, "The description does not match.");
            }
        }




         [TestMethod]
        public void Delete_course()
        {
            using UniversityContext context = new UniversityContext(_options);
            {
                Course course = context.Courses.FirstOrDefault(c => c.Course_Code == "C1");
                if (course != null)
                {
                    context.Courses.Remove(course);
                    context.SaveChanges();
                }

                bool courseExists = context.Courses.Any(c => c.Course_Code == "C1");
                Assert.IsFalse(courseExists);
            }
        }

        [TestMethod]
        public void Update_course_details()
        {
            using UniversityContext context = new UniversityContext(_options);
            {
                Course course = context.Courses.FirstOrDefault(c => c.Course_Code == "C2");
                if (course != null)
                {
                    course.Title = "Updated Biology";
                    course.Credits = 5;
                    context.SaveChanges();
                }

                Course updatedCourse = context.Courses.FirstOrDefault(c => c.Course_Code == "C2");
                Assert.IsNotNull(updatedCourse);
                Assert.AreEqual("Updated Biology", updatedCourse.Title);
                Assert.AreEqual(5, updatedCourse.Credits);
            }
        }

        [TestMethod]
        public void Validate_course_credits()
        {
            using UniversityContext context = new UniversityContext(_options);
            {
                AddCourseViewModel addCourseViewModel = new AddCourseViewModel(context, _dialogService)
                {
                    Course_Code = "C8",
                    Title = "Invalid Course",
                    Instructor = "Jan Kowalski",
                    Schedule = "Fall 2024",
                    Description = "Course with invalid credits",
                    Credits = -5, // Invalid value
                    Department = "Physics"
                };
                addCourseViewModel.Save.Execute(null);

                bool newCourseExists = context.Courses.Any(c => c.Course_Code == "C8");
                Assert.IsFalse(newCourseExists);
            }
        }
    }
}
