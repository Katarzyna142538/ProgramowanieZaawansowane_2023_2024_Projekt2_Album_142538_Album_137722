﻿using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using University.Data;
using University.Extensions;
using University.Interfaces;
using University.Models;

namespace University.ViewModels;

public class AddStudentViewModel : ViewModelBase, IDataErrorInfo
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
                case nameof(Name):
                    if (string.IsNullOrEmpty(Name)) return "Name is Required";
                    break;
                case nameof(LastName):
                    if (string.IsNullOrEmpty(LastName)) return "Last Name is Required";
                    break;
                case nameof(PESEL):
                    if (string.IsNullOrEmpty(PESEL)) return "PESEL is Required";
                    if (!PESEL.IsValidPESEL()) return "PESEL is Invalid";
                    break;
                case nameof(BirthDate):
                    if (BirthDate is null) return "Birth Date is Required";
                    break;
                case nameof(Gender):
                    if (string.IsNullOrEmpty(Gender)) return "Gender is Required";
                    break;
                case nameof(PlaceOfBirth):
                    if (string.IsNullOrEmpty(PlaceOfBirth)) return "Place of Birth is Required";
                    break;
                case nameof(PlaceOfResidence):
                    if (string.IsNullOrEmpty(PlaceOfResidence)) return "Place of Residence is Required";
                    break;
                case nameof(AddressLine1):
                    if (string.IsNullOrEmpty(AddressLine1)) return "Address Line 1 is Required";
                    break;
                case nameof(PostalCode):
                    if (string.IsNullOrEmpty(PostalCode)) return "Postal Code is Required";
                    break;
            }
            return string.Empty;
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

    private string _lastName = string.Empty;
    public string LastName
    {
        get => _lastName;
        set
        {
            _lastName = value;
            OnPropertyChanged(nameof(LastName));
        }
    }

    private string _pesel = string.Empty;
    public string PESEL
    {
        get => _pesel;
        set
        {
            _pesel = value;
            OnPropertyChanged(nameof(PESEL));
        }
    }

    private DateTime? _birthDate = null;
    public DateTime? BirthDate
    {
        get => _birthDate;
        set
        {
            _birthDate = value;
            OnPropertyChanged(nameof(BirthDate));
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

    private string _placeOfBirth = string.Empty;
    public string PlaceOfBirth
    {
        get => _placeOfBirth;
        set
        {
            _placeOfBirth = value;
            OnPropertyChanged(nameof(PlaceOfBirth));
        }
    }

    private string _placeOfResidence = string.Empty;
    public string PlaceOfResidence
    {
        get => _placeOfResidence;
        set
        {
            _placeOfResidence = value;
            OnPropertyChanged(nameof(PlaceOfResidence));
        }
    }

    private string _addressLine1 = string.Empty;
    public string AddressLine1
    {
        get => _addressLine1;
        set
        {
            _addressLine1 = value;
            OnPropertyChanged(nameof(AddressLine1));
        }
    }

    private string _addressLine2 = string.Empty;
    public string AddressLine2
    {
        get => _addressLine2;
        set
        {
            _addressLine2 = value;
            OnPropertyChanged(nameof(AddressLine2));
        }
    }

    private string _postalCode = string.Empty;
    public string PostalCode
    {
        get => _postalCode;
        set
        {
            _postalCode = value;
            OnPropertyChanged(nameof(PostalCode));
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

    private ObservableCollection<Course>? _assignedCourses = null;
    public ObservableCollection<Course>? AssignedCourses
    {
        get
        {
            if (_assignedCourses is null)
            {
                _assignedCourses = LoadCourses();
                return _assignedCourses;
            }
            return _assignedCourses;
        }
        set
        {
            _assignedCourses = value;
            OnPropertyChanged(nameof(AssignedCourses));
        }
    }

    private ICommand? _back = null;
    public ICommand? Back => _back ??= new RelayCommand<object>(NavigateBack);

    private void NavigateBack(object? obj)
    {
        var instance = MainWindowViewModel.Instance();
        if (instance is not null)
        {
            instance.StudentsSubView = new StudentsViewModel(_context, _dialogService);
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

        Student student = new Student
        {
            Name = this.Name,
            LastName = this.LastName,
            PESEL = this.PESEL,
            BirthDate = this.BirthDate,
            Gender = this.Gender,
            PlaceOfBirth = this.PlaceOfBirth,
            PlaceOfResidence = this.PlaceOfResidence,
            AddressLine1 = this.AddressLine1,
            AddressLine2 = this.AddressLine2,
            PostalCode = this.PostalCode,
            Courses = AssignedCourses?.Where(s => s.IsSelected).ToList()
        };

        _context.Students.Add(student);
        _context.SaveChanges();

        Response = "Data Saved";
    }

    public AddStudentViewModel(UniversityContext context, IDialogService dialogService)
    {
        _context = context;
        _dialogService = dialogService;
    }

    private ObservableCollection<Course> LoadCourses()
    {
        _context.Database.EnsureCreated();
        _context.Courses.Load();
        return _context.Courses.Local.ToObservableCollection();
    }

    private bool IsValid()
    {
        string[] properties = { nameof(Name), nameof(LastName), nameof(PESEL), nameof(BirthDate)};
        return properties.All(property => string.IsNullOrEmpty(this[property]));
    }
}