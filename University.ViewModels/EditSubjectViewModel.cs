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
    public class EditCourseViewModel : ViewModelBase, IDataErrorInfo
    {
        private readonly UniversityContext _context;
        private readonly IDialogService _dialogService;
        private Course? _course = new Course();

        public string Error => string.Empty;

        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(Title))
                {
                    if (string.IsNullOrEmpty(Title))
                    {
                        return "Title is Required";
                    }
                }
                if (columnName == nameof(Schedule))
                {
                    if (string.IsNullOrEmpty(Schedule))
                    {
                        return "Schedule is Required";
                    }
                }
                if (columnName == nameof(Instructor))
                {
                    if (string.IsNullOrEmpty(Instructor))
                    {
                        return "Instructor is Required";
                    }
                }
                return string.Empty;
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

        private string _description= string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private string _department= string.Empty;
        public string Department
        {
            get => _department;
            set
            {
                _department = value;
                OnPropertyChanged(nameof(Department));
            }
        }

        private int _credits = 0;
        public int Credits
        {
            get => _credits;
            set
            {
                _credits = value;
                OnPropertyChanged(nameof(Credits));
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

        private string _courseCode = string.Empty;
        public string Course_Code
        {
            get => _courseCode;
            set
            {
                _courseCode = value;
                OnPropertyChanged(nameof(Course_Code));
                LoadCourseData();
            }
        }

        private ObservableCollection<Student>? _availableStudents = null;
        public ObservableCollection<Student> AvailableStudents
        {
            get
            {
                if (_availableStudents is null)
                {
                    _availableStudents = LoadStudents();
                }
                return _availableStudents;
            }
            set
            {
                _availableStudents = value;
                OnPropertyChanged(nameof(AvailableStudents));
            }
        }

        private ObservableCollection<Student>? _assignedStudents = null;
        public ObservableCollection<Student> AssignedStudents
        {
            get
            {
                if (_assignedStudents is null)
                {
                    _assignedStudents = new ObservableCollection<Student>();
                }
                return _assignedStudents;
            }
            set
            {
                _assignedStudents = value;
                OnPropertyChanged(nameof(AssignedStudents));
            }
        }

        private ICommand? _back = null;
        public ICommand Back
        {
            get
            {
                if (_back is null)
                {
                    _back = new RelayCommand<object>(NavigateBack);
                }
                return _back;
            }
        }

        private void NavigateBack(object? obj)
        {
            var instance = MainWindowViewModel.Instance();
            if (instance is not null)
            {
                instance.CoursesSubView = new CoursesViewModel(_context, _dialogService);
            }
        }

        private ICommand? _add = null;
        public ICommand Add
        {
            get
            {
                if (_add is null)
                {
                    _add = new RelayCommand<object>(AddStudent);
                }
                return _add;
            }
        }

        private void AddStudent(object? obj)
        {
            if (obj is Student student)
            {
                if (AssignedStudents is not null && !AssignedStudents.Contains(student))
                {
                    AssignedStudents.Add(student);
                }
            }
        }

        private ICommand? _remove = null;
        public ICommand Remove
        {
            get
            {
                if (_remove is null)
                {
                    _remove = new RelayCommand<object>(RemoveStudent);
                }
                return _remove;
            }
        }

        private void RemoveStudent(object? obj)
        {
            if (obj is Student student)
            {
                if (AssignedStudents is not null)
                {
                    AssignedStudents.Remove(student);
                }
            }
        }

        private ICommand? _save = null;
        public ICommand Save
        {
            get
            {
                if (_save is null)
                {
                    _save = new RelayCommand<object>(SaveData);
                }
                return _save;
            }
        }

        private void SaveData(object? obj)
        {
            if (!IsValid())
            {
                Response = "Please complete all required fields";
                return;
            }

            if (_course is null)
            {
                return;
            }

            _course.Title = Title;
            _course.Schedule = Schedule;
            _course.Instructor = Instructor;
            _course.Course_Code = Course_Code;
            _course.Description = Description;
            _course.Credits = Credits;
            _course.Department = Department;

            _course.Students = AssignedStudents.ToList(); // Convert to List if needed

            _context.Entry(_course).State = EntityState.Modified;
            _context.SaveChanges();

            Response = "Data Saved";
        }

        public EditCourseViewModel(UniversityContext context, IDialogService dialogService)
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
            string[] properties = { nameof(Title), nameof(Schedule), nameof(Instructor) };
            foreach (string property in properties)
            {
                if (!string.IsNullOrEmpty(this[property]))
                {
                    return false;
                }
            }
            return true;
        }

        private void LoadCourseData()
        {
            var course = _context.Courses
                .Include(c => c.Students) // Ensure related Students are loaded
                .FirstOrDefault(c => c.Course_Code == Course_Code);
            
            if (course is null)
            {
                return;
            }

            _course = course;
            Title = _course.Title;
            Schedule = _course.Schedule;
            Instructor = _course.Instructor;
            Department = _course.Department;
            Credits = _course.Credits;
            Description = _course.Description;

            if (_course.Students is not null)
            {
                AssignedStudents = new ObservableCollection<Student>(_course.Students);
            }
        }
    }
}
