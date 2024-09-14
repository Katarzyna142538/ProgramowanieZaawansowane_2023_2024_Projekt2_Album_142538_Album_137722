using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows.Input;
using University.Data;
using University.Interfaces;
using University.Models;

namespace University.ViewModels;

public class SearchViewModel : ViewModelBase
{
    private readonly UniversityContext _context;
    private readonly IDialogService _dialogService;

    private bool? _dialogResult = null;
    public bool? DialogResult
    {
        get
        {
            return _dialogResult;
        }
        set
        {
            _dialogResult = value;
        }
    }

    private string _firstCondition = string.Empty;
    public string FirstCondition
    {
        get
        {
            return _firstCondition;
        }
        set
        {
            _firstCondition = value;
            OnPropertyChanged(nameof(FirstCondition));
        }
    }

    private string _secondCondition = string.Empty;
    public string SecondCondition
    {
        get
        {
            return _secondCondition;
        }
        set
        {
            _secondCondition = value;
            OnPropertyChanged(nameof(SecondCondition));
        }
    }

    private bool _isVisible;
    public bool IsVisible
    {
        get
        {
            return _isVisible;
        }
        set
        {
            _isVisible = value;
            OnPropertyChanged(nameof(IsVisible));
        }
    }

    private bool _areStudentsVisible;
    public bool AreStudentsVisible
    {
        get
        {
            return _areStudentsVisible;
        }
        set
        {
            _areStudentsVisible = value;
            OnPropertyChanged(nameof(AreStudentsVisible));
        }
    }

    private bool _areCoursesVisible;
    public bool AreCoursesVisible
    {
        get
        {
            return _areCoursesVisible;
        }
        set
        {
            _areCoursesVisible = value;
            OnPropertyChanged(nameof(AreCoursesVisible));
        }
    }

    private ObservableCollection<Student>? _students = null;
    public ObservableCollection<Student>? Students
    {
        get
        {
            if (_students is null)
            {
                _students = new ObservableCollection<Student>();
                return _students;
            }
            return _students;
        }
        set
        {
            _students = value;
            OnPropertyChanged(nameof(Students));
        }
    }

    private ObservableCollection<Course>? _courses = null;
    public ObservableCollection<Course>? Courses
    {
        get
        {
            if (_courses is null)
            {
                _courses = new ObservableCollection<Course>();
                return _courses;
            }
            return _courses;
        }
        set
        {
            _courses = value;
            OnPropertyChanged(nameof(Courses));
        }
    }

    private ICommand? _comboBoxSelectionChanged = null;
    public ICommand? ComboBoxSelectionChanged
    {
        get
        {
            if (_comboBoxSelectionChanged is null)
            {
                _comboBoxSelectionChanged = new RelayCommand<object>(UpdateCondition);
            }
            return _comboBoxSelectionChanged;
        }
    }

    private void UpdateCondition(object? obj)
    {
        if (obj is string objAsString)
        {
            IsVisible = true;
            string selectedValue = objAsString;
            SecondCondition = string.Empty;
            if (selectedValue == "Students")
            {
                FirstCondition = "who attends";
            }
            else if (selectedValue == "Courses")
            {
                FirstCondition = "attended by Student with PESEL";
            }
        }
    }

    private ICommand? _search = null;
    public ICommand? Search
    {
        get
        {
            if (_search is null)
            {
                _search = new RelayCommand<object>(SelectData);
            }
            return _search;
        }
    }

    private void SelectData(object? obj)
    {
        if (FirstCondition == "who attends")
        {
            _context.Database.EnsureCreated();
            Course? course = _context.Courses.Where(s => s.Title == SecondCondition).FirstOrDefault();
            if (course is not null)
            {
                var students = _context.Students
                    .Include(s => s.Courses)
                    .ToList();

                var filteredStudents = students
                    .Where(s => s.Courses != null && s.Courses.Any(sub => sub.Title == course.Title))
                    .ToList();

                Students = new ObservableCollection<Student>(filteredStudents);
                AreCoursesVisible = false;
                AreStudentsVisible = true;
            }
        }
        else if (FirstCondition == "attended by Student with PESEL")
        {
            _context.Database.EnsureCreated();
            Student? student = _context.Students
                .Where(s => s.PESEL == SecondCondition)
                .FirstOrDefault();
            if (student is not null)
            {
                var courses = _context.Courses
                    .Include(s => s.Students)
                    .ToList();

                var filteredCourses = courses
                    .Where(s => s.Students != null && s.Students.Any(sub => sub.PESEL == SecondCondition))
                    .ToList();

                Courses = new ObservableCollection<Course>(filteredCourses);
                AreStudentsVisible = false;
                AreCoursesVisible = true;
            }
        }
    }

    private ICommand? _edit = null;
    public ICommand? Edit
    {
        get
        {
            if (_edit is null)
            {
                _edit = new RelayCommand<object>(EditItem);
            }
            return _edit;
        }
    }

    private void EditItem(object? obj)
    {
        if (obj is not null)
        {
            if (FirstCondition == "who attends")
            {
                long studentId = (long)obj;
                EditStudentViewModel editStudentViewModel = new EditStudentViewModel(_context, _dialogService)
                {
                    StudentId = studentId
                };
                var instance = MainWindowViewModel.Instance();
                if (instance is not null)
                {
                    instance.StudentsSubView = editStudentViewModel;
                    instance.SelectedTab = 0;
                }
            }
            else if (FirstCondition == "attended by Student with PESEL")
            {
                string courseCode = (string)obj;
                if (courseCode is not null)
                {
                    EditCourseViewModel editCourseViewModel = new EditCourseViewModel(_context, _dialogService)
                    {
                        Course_Code = courseCode // Set Course_Code instead of CourseId
                    };
                    var instance = MainWindowViewModel.Instance();
                    if (instance is not null)
                    {
                        instance.CoursesSubView = editCourseViewModel;
                        instance.SelectedTab = 1;
                    }
                }
            }
        }
    }

    private ICommand ?_remove = null;
    public ICommand? Remove
    {
        get
        {
            if (_remove is null)
            {
                _remove = new RelayCommand<object>(RemoveItem);
            }
            return _remove;
        }
    }

    private void RemoveItem(object? obj)
    {
        if (obj is not null)
        {
            if (FirstCondition == "who attends")
            {
                long studentId = (long)obj;
                Student? student = _context.Students.Find(studentId);
                if (student is null)
                {
                    return;
                }

                DialogResult = _dialogService.Show(student.Name + " " + student.LastName);
                if (DialogResult == false)
                {
                    return;
                }
                _context.Students.Remove(student);
                _context.SaveChanges();
            }
            else if (FirstCondition == "attended by Student with PESEL")
            {
                try {
                    string courseCode = (string)obj;
                if (courseCode is not null)
                {
                    Course? course = _context.Courses.FirstOrDefault(c => c.Course_Code == courseCode);
                    if (course is null)
                    {
                        return;
                    }

                    DialogResult = _dialogService.Show(course.Title);
                    if (DialogResult == false)
                    {
                        return;
                    }

                    _context.Courses.Remove(course);
                    _context.SaveChanges();
                }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Zepsute {ex.Message}");
                }
                
            }
        }
    }

    public SearchViewModel(UniversityContext context, IDialogService dialogService)
    {
        _context = context;
        _dialogService = dialogService;

        IsVisible = false;
        AreStudentsVisible = false;
        AreCoursesVisible = false;
    }
}
