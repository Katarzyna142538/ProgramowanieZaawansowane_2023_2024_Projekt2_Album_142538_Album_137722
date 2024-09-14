using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using University.Data;
using University.Interfaces;
using University.Models;

namespace University.ViewModels
{
    public class AddCourseViewModel : ViewModelBase, IDataErrorInfo
    {
        private readonly UniversityContext _context;
        private readonly IDialogService _dialogService;

        public string Error => string.Empty;

        public string this[string columnName]
        {
            get
            {
                // Validation logic for all fields including Course_Code
                if (columnName == nameof(Course_Code) && string.IsNullOrEmpty(Course_Code))
                {
                    return "Course code is required.";
                }
                if (columnName == nameof(Title) && string.IsNullOrEmpty(Title))
                {
                    return "Title is required.";
                }
                if (columnName == nameof(Schedule) && string.IsNullOrEmpty(Schedule))
                {
                    return "Schedule is required.";
                }
                if (columnName == nameof(Instructor) && string.IsNullOrEmpty(Instructor))
                {
                    return "Instructor is required.";
                }
                if (columnName == nameof(Description) && string.IsNullOrEmpty(Description))
                {
                    return "Description is required.";
                }
                if (columnName == nameof(Credits) && Credits <= 0)
                {
                    return "Credits must be greater than 0.";
                }
                if (columnName == nameof(Department) && string.IsNullOrEmpty(Department))
                {
                    return "Department is required.";
                }
                return string.Empty;
            }
        }

        // Properties for all fields
        private string _courseCode = string.Empty;
        public string Course_Code
        {
            get => _courseCode;
            set
            {
                _courseCode = value;
                OnPropertyChanged(nameof(Course_Code));
            }
        }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        private string _schedule = string.Empty;
        public string Schedule
        {
            get => _schedule;
            set
            {
                _schedule = value;
                OnPropertyChanged(nameof(Schedule));
            }
        }

        private string _instructor = string.Empty;
        public string Instructor
        {
            get => _instructor;
            set
            {
                _instructor = value;
                OnPropertyChanged(nameof(Instructor));
            }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private int _credits;
        public int Credits
        {
            get => _credits;
            set
            {
                _credits = value;
                OnPropertyChanged(nameof(Credits));
            }
        }

        private string _department = string.Empty;
        public string Department
        {
            get => _department;
            set
            {
                _department = value;
                OnPropertyChanged(nameof(Department));
            }
        }

        private string _prerequisites = string.Empty;
        public string Prerequisites
        {
            get => _prerequisites;
            set
            {
                _prerequisites = value;
                OnPropertyChanged(nameof(Prerequisites));
            }
        }

        private string _response = string.Empty;
        public string Response
        {
            get => _response;
            set
            {
                _response = value;
                OnPropertyChanged(nameof(Response));
            }
        }

        private ObservableCollection<Student>? _availableStudents;
        public ObservableCollection<Student> AvailableStudents
        {
            get => _availableStudents ??= LoadStudents();
            set
            {
                _availableStudents = value;
                OnPropertyChanged(nameof(AvailableStudents));
            }
        }

        private ObservableCollection<Student>? _assignedStudents;
        public ObservableCollection<Student> AssignedStudents
        {
            get => _assignedStudents ??= new ObservableCollection<Student>();
            set
            {
                _assignedStudents = value;
                OnPropertyChanged(nameof(AssignedStudents));
            }
        }

        private ICommand? _back;
        public ICommand Back => _back ??= new RelayCommand<object>(NavigateBack);

        private void NavigateBack(object? obj)
        {
            var instance = MainWindowViewModel.Instance();
            if (instance != null)
            {
                instance.CoursesSubView = new CoursesViewModel(_context, _dialogService);
            }
        }

        private ICommand? _add;
        public ICommand Add => _add ??= new RelayCommand<object>(AddStudent);

        private void AddStudent(object? obj)
        {
            if (obj is Student student)
            {
                if (!AssignedStudents.Contains(student))
                {
                    AssignedStudents.Add(student);
                }
            }
        }

        private ICommand? _remove;
        public ICommand Remove => _remove ??= new RelayCommand<object>(RemoveStudent);

        private void RemoveStudent(object? obj)
        {
            if (obj is Student student)
            {
                AssignedStudents.Remove(student);
            }
        }

        private ICommand? _save;
        public ICommand Save => _save ??= new RelayCommand<object>(SaveData);

        private void SaveData(object? obj)
        {
            if (!IsValid())
            {
                Response = "Please complete all required fields.";
                return;
            }
            
            bool courseExists = _context.Courses.Any(c => c.Course_Code == Course_Code);

            if (courseExists)
            {
                Response = "Course with this Course_Code already exists.";
                return;
            }

            var course = new Course
            {
                Course_Code = Course_Code, 
                Title = Title,
                Schedule = Schedule,
                Instructor = Instructor,
                Description = Description,
                Credits = Credits,
                Department = Department,
                Prerequisites = Prerequisites.Split(',').Select(p => p.Trim()).ToList(),
                Students = AssignedStudents.ToList()
            };

            _context.Courses.Add(course);
            _context.SaveChanges();

            string assignedStudentsList = string.Join(", ",  course.Students.Select(s => s.Name));

            // Prepare the response message
            Response = $"Course saved successfully. Students assigned: {assignedStudentsList}";
        }


        public AddCourseViewModel(UniversityContext context, IDialogService dialogService)
        {
            _context = context;
            _dialogService = dialogService;
        }

        private ObservableCollection<Student> LoadStudents()
        {
            _context.Database.EnsureCreated();
            _context.Students.Load();
            return _context.Students.Local.ToObservableCollection();
        }

        private bool IsValid()
        {
            string[] properties = { nameof(Course_Code), nameof(Title), nameof(Schedule), nameof(Instructor), nameof(Description), nameof(Credits), nameof(Department) };
            foreach (string property in properties)
            {
                if (!string.IsNullOrEmpty(this[property]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}