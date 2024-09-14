using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using University.Data;
using University.Interfaces;
using University.Models;

namespace University.ViewModels
{
    public class EditResearchProjectViewModel : ViewModelBase, IDataErrorInfo
    {
        private readonly UniversityContext _context;
        private readonly IDialogService _dialogService;
        private ResearchProject? _researchProject = new ResearchProject();

        public string Error => string.Empty;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(ProjectId):
                        if (string.IsNullOrEmpty(ProjectId)) return "Project ID is Required";
                        break;
                    case nameof(Title):
                        if (string.IsNullOrEmpty(Title)) return "Title is Required";
                        break;
                    case nameof(Description):
                        if (string.IsNullOrEmpty(Description)) return "Description is Required";
                        break;
                    case nameof(Supervisor):
                        if (string.IsNullOrEmpty(Supervisor)) return "Supervisor is Required";
                        break;
                    case nameof(StartDate):
                        if (StartDate == null) return "Start Date is Required";
                        break;
                    case nameof(EndDate):
                        if (EndDate == null) return "End Date is Required";
                        if (EndDate < StartDate) return "End Date cannot be earlier than Start Date";
                        break;
                    case nameof(Budget):
                        if (Budget <= 0) return "Budget must be greater than zero";
                        break;
                }
                return string.Empty;
            }
        }

        private string _projectId = string.Empty;
        public string ProjectId
        {
            get => _projectId;
            set
            {
                _projectId = value;
                OnPropertyChanged(nameof(ProjectId));
                LoadResearchProjectData();
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

        private string _supervisor = string.Empty;
        public string Supervisor
        {
            get => _supervisor;
            set
            {
                _supervisor = value;
                OnPropertyChanged(nameof(Supervisor));
            }
        }

        private DateTime? _startDate = null;
        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        private DateTime? _endDate = null;
        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        private float _budget = 0;
        public float Budget
        {
            get => _budget;
            set
            {
                _budget = value;
                OnPropertyChanged(nameof(Budget));
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

        private ObservableCollection<FacultyMember>? _availableFacultyMembers;
        public ObservableCollection<FacultyMember> AvailableFacultyMembers
        {
            get
            {
                if (_availableFacultyMembers is null)
                {
                    _availableFacultyMembers = LoadFacultyMembers();
                }
                return _availableFacultyMembers;
            }
            set
            {
                _availableFacultyMembers = value;
                OnPropertyChanged(nameof(AvailableFacultyMembers));
            }
        }

        private ObservableCollection<FacultyMember>? _assignedFacultyMembers;
        public ObservableCollection<FacultyMember> AssignedFacultyMembers
        {
            get
            {
                if (_assignedFacultyMembers is null)
                {
                    _assignedFacultyMembers = LoadAssignedFacultyMembers();
                }
                return _assignedFacultyMembers;
            }
            set
            {
                _assignedFacultyMembers = value;
                OnPropertyChanged(nameof(AssignedFacultyMembers));
            }
        }

        private ICommand? _back = null;
        public ICommand Back => _back ??= new RelayCommand<object>(NavigateBack);

        private void NavigateBack(object? obj)
        {
            var instance = MainWindowViewModel.Instance();
            if (instance is not null)
            {
                instance.ResearchProjectsSubView = new ResearchProjectViewModel(_context, _dialogService);
            }
        }

        private ICommand? _add;
        public ICommand Add => _add ??= new RelayCommand<object>(AddFacultyMember);

        private void AddFacultyMember(object? obj)
        {
            if (obj is FacultyMember facultyMember)
            {
                if (!AssignedFacultyMembers.Contains(facultyMember))
                {
                    AssignedFacultyMembers.Add(facultyMember);
                }
            }
        }

        private ICommand? _remove;
        public ICommand Remove => _remove ??= new RelayCommand<object>(RemoveFacultyMember);

        private void RemoveFacultyMember(object? obj)
        {
            if (obj is FacultyMember facultyMember)
            {
                AssignedFacultyMembers.Remove(facultyMember);
            }
        }

        private ICommand? _save = null;
        public ICommand Save => _save ??= new RelayCommand<object>(SaveData);

        private void SaveData(object? obj)
        {
            if (!IsValid())
            {
                Response = "Please complete all required fields";
                return;
            }

            if (_researchProject is null)
            {
                Response = "_researchProject is null";
                return;
            }

            _researchProject.Title = Title;
            _researchProject.Description = Description;
            _researchProject.Supervisor = Supervisor;
            _researchProject.StartDate = StartDate;
            _researchProject.EndDate = EndDate;
            _researchProject.Budget = Budget;

            _context.Entry(_researchProject).State = EntityState.Modified;

            //Managing FacultyToProject many-to-many relation
            var existingAssociations = _context.FacultyToProjects
                .Where(fp => fp.ProjectId == _researchProject.ProjectId)
                .ToList();

            _context.FacultyToProjects.RemoveRange(existingAssociations);

            foreach (var facultyMember in AssignedFacultyMembers)
            {
                var newAssociation = new FacultyToProject
                {
                    FacultyId = facultyMember.FacultyId,
                    ProjectId = _researchProject.ProjectId
                };

                _context.FacultyToProjects.Add(newAssociation);
            }

            _context.SaveChanges();

            Response = "Data Updated";
        }

        public EditResearchProjectViewModel(UniversityContext context, IDialogService dialogService)
        {
            _context = context;
            _dialogService = dialogService;
        }

        private ObservableCollection<FacultyMember> LoadFacultyMembers()
        {
            _context.Database.EnsureCreated();
            _context.FacultyMembers.Load();
            return _context.FacultyMembers.Local.ToObservableCollection();
        }

        private ObservableCollection<FacultyMember> LoadAssignedFacultyMembers()
        {
            if (_context is null || string.IsNullOrEmpty(ProjectId))
            {
                return new ObservableCollection<FacultyMember>();
            }

            var assignedFaculty = _context.FacultyMembers
                                          .Where(f => _context.FacultyToProjects
                                                              .Where(fp => fp.ProjectId == ProjectId)
                                                              .Select(fp => fp.FacultyId)
                                                              .Contains(f.FacultyId))
                                          .ToList();

            return new ObservableCollection<FacultyMember>(assignedFaculty);
        }

        private bool IsValid()
        {
            string[] properties = { nameof(ProjectId), nameof(Title), nameof(Description), nameof(Supervisor), nameof(StartDate), nameof(EndDate), nameof(Budget) };
            return properties.All(property => string.IsNullOrEmpty(this[property]));
        }

        private void LoadResearchProjectData()
        {
            if (_context?.ResearchProjects is null)
            {
                return;
            }
            _researchProject = _context.ResearchProjects.Find(ProjectId);
            if (_researchProject is null)
            {
                return;
            }

            Title = _researchProject.Title;
            Description = _researchProject.Description;
            Supervisor = _researchProject.Supervisor;
            StartDate = _researchProject.StartDate;
            EndDate = _researchProject.EndDate;
            Budget = _researchProject.Budget;
        }
    }
}
