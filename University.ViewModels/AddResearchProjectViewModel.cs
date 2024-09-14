using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using University.Data;
using University.Interfaces;
using University.Models;

namespace University.ViewModels
{
    public class AddResearchProjectViewModel : ViewModelBase, IDataErrorInfo
    {
        private readonly UniversityContext _context;
        private readonly IDialogService _dialogService;

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
                        if (!StartDate.HasValue) return "Start Date is Required";
                        break;
                    case nameof(EndDate):
                        if (!EndDate.HasValue) return "End Date is Required";
                        break;
                    case nameof(Budget):
                        if (Budget <= 0) return "Budget must be greater than 0";
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

        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        private float _budget;
        public float Budget
        {
            get => _budget;
            set
            {
                _budget = value;
                OnPropertyChanged(nameof(Budget));
            }
        }

        private string _teamMembers = string.Empty;
        public string TeamMembers
        {
            get => _teamMembers;
            set
            {
                _teamMembers = value;
                OnPropertyChanged(nameof(TeamMembers));
            }
        }

        private ObservableCollection<FacultyMember>? _availableFacultyMembers;
        public ObservableCollection<FacultyMember> AvailableFacultyMembers
        {
            get => _availableFacultyMembers ??= LoadFacultyMembers();
            set
            {
                _availableFacultyMembers = value;
                OnPropertyChanged(nameof(AvailableFacultyMembers));
            }
        }

        private ObservableCollection<FacultyMember>? _assignedFacultyMembers;
        public ObservableCollection<FacultyMember> AssignedFacultyMembers
        {
            get => _assignedFacultyMembers ??= new ObservableCollection<FacultyMember>();
            set
            {
                _assignedFacultyMembers = value;
                OnPropertyChanged(nameof(AssignedFacultyMembers));
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

        private ICommand? _back = null;
        public ICommand? Back => _back ??= new RelayCommand<object>(NavigateBack);

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
        public ICommand? Save => _save ??= new RelayCommand<object>(SaveData);

        private void SaveData(object? obj)
        {
            if (!IsValid())
            {
                Response = "Please complete all required fields";
                return;
            }

            bool projectExists = _context.ResearchProjects.Any(p => p.ProjectId == ProjectId);

            if (projectExists)
            {
                Response = "Research Project with this Project ID already exists.";
                return;
            }

            ResearchProject researchProject = new ResearchProject
            {
                ProjectId = this.ProjectId,
                Title = this.Title,
                Description = this.Description,
                Supervisor = this.Supervisor,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
                Budget = this.Budget,
                TeamMembers = TeamMembers.Split(',').Select(p => p.Trim()).ToList(),
            };

            _context.ResearchProjects.Add(researchProject);

            foreach (var facultyMember in AssignedFacultyMembers)
            {
                FacultyToProject facultyToProject = new FacultyToProject
                {
                    FacultyId = facultyMember.FacultyId,
                    ProjectId = this.ProjectId
                };

                _context.FacultyToProjects.Add(facultyToProject);
            }

            _context.SaveChanges();

            // Prepare the response message
            Response = $"Course saved successfully.";
        }

        public AddResearchProjectViewModel(UniversityContext context, IDialogService dialogService)
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

        private bool IsValid()
        {
            string[] properties = { nameof(ProjectId), nameof(Title), nameof(Description), nameof(Supervisor), nameof(StartDate), nameof(EndDate), nameof(Budget) };
            return properties.All(property => string.IsNullOrEmpty(this[property]));
        }
    }
}
