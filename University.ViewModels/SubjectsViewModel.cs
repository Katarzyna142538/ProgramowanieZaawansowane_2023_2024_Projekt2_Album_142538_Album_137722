using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows.Input;
using University.Data;
using University.Interfaces;
using University.Models;

namespace University.ViewModels;

public class CoursesViewModel : ViewModelBase
{
    private readonly UniversityContext _context;
    private readonly IDialogService _dialogService;

    private bool? dialogResult;
    public bool? DialogResult
    {
        get
        {
            return dialogResult;
        }
        set
        {
            dialogResult = value;
        }
    }

    private ObservableCollection<Course>? courses = null;
    public ObservableCollection<Course>? Courses
    {
        get
        {
            if (courses is null)
            {
                courses = new ObservableCollection<Course>();
                return courses;
            }
            return courses;
        }
        set
        {
            courses = value;
            OnPropertyChanged(nameof(Courses));
        }
    }

    private ICommand? _add = null;
    public ICommand? Add
    {
        get
        {
            if (_add is null)
            {
                _add = new RelayCommand<object>(AddNewCourse);
            }
            return _add;
        }
    }

    private void AddNewCourse(object? obj)
    {
        var instance = MainWindowViewModel.Instance();
        if (instance is not null)
        {
            instance.CoursesSubView = new AddCourseViewModel(_context, _dialogService);
        }
    }

    private ICommand? _edit;
    public ICommand? Edit
    {
        get
        {
            if (_edit is null)
            {
                _edit = new RelayCommand<object>(EditCourse);
            }
            return _edit;
        }
    }

    private void EditCourse(object? obj)
    {
        if (obj is not null)
        {
            string courseCode = (string)obj;
            if (courseCode is not null)
            {
                EditCourseViewModel editCourseViewModel = new EditCourseViewModel(_context, _dialogService)
                {
                    Course_Code = courseCode 
                };
                var instance = MainWindowViewModel.Instance();
                if (instance is not null)
                {
                    instance.CoursesSubView = editCourseViewModel;
                }
            }
        }
    }


    private ICommand? _remove = null;
    public ICommand? Remove
    {
        get
        {
            if (_remove is null)
            {
                _remove = new RelayCommand<object>(RemoveCourse);
            }
            return _remove;
        }
    }

    private void RemoveCourse(object? obj)
{
    if (obj is not null)
    {
        string courseCode = (string)obj;
        if (courseCode is not null)
        {
            // Find the course by Course_Code
            Course? course = _context.Courses.FirstOrDefault(c => c.Course_Code == courseCode);
            if (course is not null)
            {
                // Show dialog with course name
                DialogResult = _dialogService.Show(course.Title);
                if (DialogResult == false)
                {
                    return;
                }

                // Remove the course and save changes
                _context.Courses.Remove(course);
                _context.SaveChanges();
            }
        }
    }
}


    public CoursesViewModel(UniversityContext context, IDialogService dialogService)
    {
        _context = context;
        _dialogService = dialogService;

        _context.Database.EnsureCreated();
        _context.Courses.Load();
        Courses = _context.Courses.Local.ToObservableCollection();
    }
}