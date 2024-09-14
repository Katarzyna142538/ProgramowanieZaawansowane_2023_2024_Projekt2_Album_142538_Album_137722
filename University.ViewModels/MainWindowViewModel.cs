using System;
using University.Interfaces;
using University.Data;
using University.Models;

namespace University.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly UniversityContext _context;
    private readonly IDialogService _dialogService;

    private int _selectedTab;
    public int SelectedTab
    {
        get
        {
            return _selectedTab;
        }
        set
        {
            _selectedTab = value;
            OnPropertyChanged(nameof(SelectedTab));
        }
    }

    private object? _studentsSubView = null;
    public object? StudentsSubView
    {
        get
        {
            return _studentsSubView;
        }
        set
        {
            _studentsSubView = value;
            OnPropertyChanged(nameof(StudentsSubView));
        }
    }

    private object? _coursesSubView = null;
    public object? CoursesSubView
    {
        get
        {
            return _coursesSubView;
        }
        set
        {
            _coursesSubView = value;
            OnPropertyChanged(nameof(CoursesSubView));
        }
    }

    private object? _searchSubView = null;
    public object? SearchSubView
    {
        get
        {
            return _searchSubView;
        }
        set
        {
            _searchSubView = value;
            OnPropertyChanged(nameof(SearchSubView));
        }
    }

    private object? _facultyMemberSubView = null;
    public object? FacultyMembersSubView
    {
        get
        {
            return _facultyMemberSubView;
        }
        set
        {
            _facultyMemberSubView = value;
            OnPropertyChanged(nameof(FacultyMembersSubView));
        }
    }

    private object? _researchProjectsSubView = null;
        public object? ResearchProjectsSubView
        {
            get => _researchProjectsSubView;
            set
            {
                _researchProjectsSubView = value;
                OnPropertyChanged(nameof(ResearchProjectsSubView));
            }
        }


    private static MainWindowViewModel? _instance = null;
    public static MainWindowViewModel? Instance()
    {
        return _instance;
    }

    public MainWindowViewModel(UniversityContext context, IDialogService dialogService)
    {
        _context = context;
        _dialogService = dialogService;

        if (_instance is null)
        {
            _instance = this;
        }

        StudentsSubView = new StudentsViewModel(_context, _dialogService);
        CoursesSubView = new CoursesViewModel(_context, _dialogService);
        SearchSubView = new SearchViewModel(_context, _dialogService);
        FacultyMembersSubView = new FacultyMemberViewModel(_context, _dialogService);
        ResearchProjectsSubView = new ResearchProjectViewModel(_context, _dialogService);
    }
}
