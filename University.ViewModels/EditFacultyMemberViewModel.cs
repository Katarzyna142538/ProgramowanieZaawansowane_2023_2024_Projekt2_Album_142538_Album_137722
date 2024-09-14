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
    public class EditFacultyMemberViewModel : ViewModelBase, IDataErrorInfo
    {
        private readonly UniversityContext _context;
        private readonly IDialogService _dialogService;
        private FacultyMember? _facultyMember = new FacultyMember();

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
                LoadFacultyMemberData();
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

        private ObservableCollection<ResearchProject>? _availableResearchProjects = null;
        public ObservableCollection<ResearchProject> AvailableResearchProjects
        {
            get
            {
                if (_availableResearchProjects is null)
                {
                    _availableResearchProjects = LoadResearchProjects();
                }
                return _availableResearchProjects;
            }
            set
            {
                _availableResearchProjects = value;
                OnPropertyChanged(nameof(AvailableResearchProjects));
            }
        }

        private ObservableCollection<ResearchProject>? _assignedResearchProjects = null;
        public ObservableCollection<ResearchProject> AssignedResearchProjects
        {
            get
            {
                if (_assignedResearchProjects is null)
                {
                    _assignedResearchProjects = LoadAssignedResearchProjects();
                }
                return _assignedResearchProjects;
            }
            set
            {
                _assignedResearchProjects = value;
                OnPropertyChanged(nameof(AssignedResearchProjects));
            }
        }

        private ICommand? _back = null;
        public ICommand Back => _back ??= new RelayCommand<object>(NavigateBack);

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
        public ICommand Save => _save ??= new RelayCommand<object>(SaveData);

        private void SaveData(object? obj)
        {
            if (!IsValid())
            {
                Response = "Please complete all required fields";
                return;
            }

            if (_facultyMember is null)
            {
                Response = "_facultyMember is null";
                return;
            }

            //_facultyMember.FacultyId = FacultyId;
            _facultyMember.Name = Name;
            _facultyMember.Age = Age;
            _facultyMember.Gender = Gender;
            _facultyMember.Department = Department;
            _facultyMember.Position = Position;
            _facultyMember.Email = Email;
            _facultyMember.OfficeRoomNumber = OfficeRoomNumber;

            _context.Entry(_facultyMember).State = EntityState.Modified;

            //Managing FacultyToProject many to many relation

            var existingAssociations = _context.FacultyToProjects
                .Where(f => f.FacultyId == _facultyMember.FacultyId)
                .ToList();

            _context.FacultyToProjects.RemoveRange(existingAssociations);

            
            foreach (var project in AssignedResearchProjects)
            {
                var newAssociation = new FacultyToProject
                {
                    FacultyId = _facultyMember.FacultyId,
                    ProjectId = project.ProjectId
                };

                _context.FacultyToProjects.Add(newAssociation);
            }




            
            _context.SaveChanges();

            Response = "Data Updated";
        }

        public EditFacultyMemberViewModel(UniversityContext context, IDialogService dialogService)
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

        private ObservableCollection<ResearchProject> LoadAssignedResearchProjects()
        {
            if (_context is null || string.IsNullOrEmpty(FacultyId))
            {
                return new ObservableCollection<ResearchProject>();
            }

           
            var assignedProjects = _context.ResearchProjects
                                        .Where(rp => _context.FacultyToProjects
                                                                .Where(fp => fp.FacultyId == FacultyId)
                                                                .Select(fp => fp.ProjectId)
                                                                .Contains(rp.ProjectId))
                                        .ToList();

            return new ObservableCollection<ResearchProject>(assignedProjects);
        }


        private bool IsValid()
        {
            string[] properties = { nameof(FacultyId), nameof(Name), nameof(Age), nameof(Gender), nameof(Department), nameof(Position), nameof(Email), nameof(OfficeRoomNumber) };
            return properties.All(property => string.IsNullOrEmpty(this[property]));
        }

        private void LoadFacultyMemberData()
        {
            if (_context?.FacultyMembers is null)
            {
                return;
            }
            _facultyMember = _context.FacultyMembers.Find(FacultyId);
            if (_facultyMember is null)
            {
                return;
            }


            //FacultyId = _facultyMember.FacultyId;
            Name = _facultyMember.Name;
            Age = _facultyMember.Age;
            Gender = _facultyMember.Gender;
            Department = _facultyMember.Department;
            Position = _facultyMember.Position;
            Email = _facultyMember.Email;
            OfficeRoomNumber = _facultyMember.OfficeRoomNumber;
        }
    }
}
