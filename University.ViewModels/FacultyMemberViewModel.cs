using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows.Input;
using University.Data;
using University.Interfaces;
using University.Models;

namespace University.ViewModels;

public class FacultyMemberViewModel : ViewModelBase
{
    private readonly UniversityContext _context;
    private readonly IDialogService _dialogService;

    private bool? _dialogResult = null;
    public bool? DialogResult
    {
        get => _dialogResult;
        set
        {
            _dialogResult = value;
            OnPropertyChanged(nameof(DialogResult));
        }
    }

    private ObservableCollection<FacultyMember>? _facultyMembers = null;
    public ObservableCollection<FacultyMember>? FacultyMembers
    {
        get => _facultyMembers ??= new ObservableCollection<FacultyMember>();
        set
        {
            _facultyMembers = value;
            OnPropertyChanged(nameof(FacultyMembers));
        }
    }

    private ICommand? _add = null;
    public ICommand? Add
    {
        get => _add ??= new RelayCommand<object>(AddNewFacultyMember);
    }

    private void AddNewFacultyMember(object? obj)
    {
        var instance = MainWindowViewModel.Instance();
        if (instance is not null)
        {
            instance.FacultyMembersSubView = new AddFacultyMemberViewModel(_context, _dialogService);
        }
    }

    private ICommand? _edit = null;
    public ICommand? Edit
    {
        get => _edit ??= new RelayCommand<object>(EditFacultyMember);
    }

    private void EditFacultyMember(object? obj)
    {
        if (obj is not null)
        {
            string facultyId = (string)obj;
            EditFacultyMemberViewModel editFacultyMemberViewModel = new EditFacultyMemberViewModel(_context, _dialogService)
            {
                FacultyId = facultyId
            };
            var instance = MainWindowViewModel.Instance();
            if (instance is not null)
            {
                instance.FacultyMembersSubView = editFacultyMemberViewModel;
            }
        }
    }

    private ICommand? _remove = null;
    public ICommand? Remove
    {
        get => _remove ??= new RelayCommand<object>(RemoveFacultyMember);
    }

    private void RemoveFacultyMember(object? obj)
    {
        if (obj is not null)
        {
            string facultyId = (string)obj;
            FacultyMember? facultyMember = _context.FacultyMembers.Find(facultyId);
            if (facultyMember is not null)
            {
                DialogResult = _dialogService.Show(facultyMember.Name);
                if (DialogResult == false)
                {
                    return;
                }

                var associatedProjects = _context.FacultyToProjects
                    .Where(f => f.FacultyId == facultyId)
                    .ToList();

                _context.FacultyToProjects.RemoveRange(associatedProjects);

                _context.FacultyMembers.Remove(facultyMember);
                _context.SaveChanges();
            }
        }
    }

    public FacultyMemberViewModel(UniversityContext context, IDialogService dialogService)
    {
        _context = context;
        _dialogService = dialogService;

        _context.Database.EnsureCreated();
        _context.FacultyMembers.Load();
        FacultyMembers = _context.FacultyMembers.Local.ToObservableCollection();
    }
}
