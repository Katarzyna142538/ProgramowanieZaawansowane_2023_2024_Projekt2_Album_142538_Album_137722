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
    public class AddFacultyMemberViewModel : ViewModelBase, IDataErrorInfo
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
                    case nameof(FacultyId):
                        if (string.IsNullOrEmpty(FacultyId)) return "Faculty ID is Required";
                        break;
                    case nameof(Name):
                        if (string.IsNullOrEmpty(Name)) return "Name is Required";
                        break;
                    case nameof(Age):
                        if (Age <= 0) return "Age must be greater than zero";
                        break;
                    case nameof(Gender):
                        if (string.IsNullOrEmpty(Gender)) return "Gender is Required";
                        break;
                    case nameof(Department):
                        if (string.IsNullOrEmpty(Department)) return "Department is Required";
                        break;
                    case nameof(Position):
                        if (string.IsNullOrEmpty(Position)) return "Position is Required";
                        break;
                    case nameof(Email):
                        if (string.IsNullOrEmpty(Email)) return "Email is Required";
                        if (!Email.Contains('@')) return "Email is Invalid";
                        break;
                    case nameof(OfficeRoomNumber):
                        if (string.IsNullOrEmpty(OfficeRoomNumber)) return "Office Room Number is Required";
                        break;
                }
                return string.Empty;
            }
        }

        private string _facultyId = string.Empty;
        public string FacultyId
        {
            get => _facultyId;
            set
            {
                _facultyId = value;
                OnPropertyChanged(nameof(FacultyId));
            }
        }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private int _age = 0;
        public int Age
        {
            get => _age;
            set
            {
                _age = value;
                OnPropertyChanged(nameof(Age));
            }
        }

        private string _gender = string.Empty;
        public string Gender
        {
            get => _gender;
            set
            {
                _gender = value;
                OnPropertyChanged(nameof(Gender));
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

        private string _position = string.Empty;
        public string Position
        {
            get => _position;
            set
            {
                _position = value;
                OnPropertyChanged(nameof(Position));
            }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        private string _officeRoomNumber = string.Empty;
        public string OfficeRoomNumber
        {
            get => _officeRoomNumber;
            set
            {
                _officeRoomNumber = value;
                OnPropertyChanged(nameof(OfficeRoomNumber));
            }
        }

        private ObservableCollection<ResearchProject>? _availableResearchProjects;
        public ObservableCollection<ResearchProject> AvailableResearchProjects
        {
            get => _availableResearchProjects ??= LoadResearchProjects();
            set
            {
                _availableResearchProjects = value;
                OnPropertyChanged(nameof(AvailableResearchProjects));
            }
        }

        private ObservableCollection<ResearchProject>? _assignedResearchProjects;
        public ObservableCollection<ResearchProject> AssignedResearchProjects
        {
            get => _assignedResearchProjects ??= new ObservableCollection<ResearchProject>();
            set
            {
                _assignedResearchProjects = value;
                OnPropertyChanged(nameof(AssignedResearchProjects));
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
                instance.FacultyMembersSubView = new FacultyMemberViewModel(_context, _dialogService);
            }
        }

        private ICommand? _add;
        public ICommand Add => _add ??= new RelayCommand<object>(AddResearchProject);

        private void AddResearchProject(object? obj)
        {
            if (obj is ResearchProject researchProject)
            {
                if (!AssignedResearchProjects.Contains(researchProject))
                {
                    AssignedResearchProjects.Add(researchProject);
                }
            }
        }

        private ICommand? _remove;
        public ICommand Remove => _remove ??= new RelayCommand<object>(RemoveResearchProject);

        private void RemoveResearchProject(object? obj)
        {
            if (obj is ResearchProject researchProject)
            {
                AssignedResearchProjects.Remove(researchProject);
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

            bool facultyExists = _context.FacultyMembers.Any(f => f.FacultyId == FacultyId);

            if (facultyExists)
            {
                Response = "FacultyMember with this FacultyID already exists.";
                return;
            }

            FacultyMember facultyMember = new FacultyMember
            {
                FacultyId = this.FacultyId,
                Name = this.Name,
                Age = this.Age,
                Gender = this.Gender,
                Department = this.Department,
                Position = this.Position,
                Email = this.Email,
                OfficeRoomNumber = this.OfficeRoomNumber
            };

            _context.FacultyMembers.Add(facultyMember);

            //Zarzadanie relacja many to many
            foreach (var project in AssignedResearchProjects)
            {
                FacultyToProject facultyToProject = new FacultyToProject
                {
                    FacultyId = this.FacultyId,
                    ProjectId = project.ProjectId 
                };

                _context.FacultyToProjects.Add(facultyToProject);
            }

            _context.SaveChanges();

            Response = "Data Saved";
        }

        public AddFacultyMemberViewModel(UniversityContext context, IDialogService dialogService)
        {
            _context = context;
            _dialogService = dialogService;
        }

         private ObservableCollection<ResearchProject> LoadResearchProjects()
        {
            _context.Database.EnsureCreated();
            _context.ResearchProjects.Load();
            return _context.ResearchProjects.Local.ToObservableCollection();
        }


        private bool IsValid()
        {
            string[] properties = { nameof(FacultyId), nameof(Name), nameof(Age), nameof(Gender), nameof(Department), nameof(Position), nameof(Email), nameof(OfficeRoomNumber) };
            return properties.All(property => string.IsNullOrEmpty(this[property]));
        }
    }
}
