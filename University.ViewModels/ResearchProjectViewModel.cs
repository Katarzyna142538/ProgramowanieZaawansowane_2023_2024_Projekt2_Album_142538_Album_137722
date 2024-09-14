using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows.Input;
using University.Data;
using University.Interfaces;
using University.Models;

namespace University.ViewModels
{
    public class ResearchProjectViewModel : ViewModelBase
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

        private ObservableCollection<ResearchProject>? _researchProjects = null;
        public ObservableCollection<ResearchProject>? ResearchProjects
        {
            get => _researchProjects ??= new ObservableCollection<ResearchProject>();
            set
            {
                _researchProjects = value;
                OnPropertyChanged(nameof(ResearchProjects));
            }
        }

        private ICommand? _add = null;
        public ICommand? Add
        {
            get => _add ??= new RelayCommand<object>(AddNewResearchProject);
        }

        private void AddNewResearchProject(object? obj)
        {
            var instance = MainWindowViewModel.Instance();
            if (instance is not null)
            {
                instance.ResearchProjectsSubView = new AddResearchProjectViewModel(_context, _dialogService);
            }
        }

        private ICommand? _edit = null;
        public ICommand? Edit
        {
            get => _edit ??= new RelayCommand<object>(EditResearchProject);
        }

        private void EditResearchProject(object? obj)
        {
            if (obj is not null)
            {
                string projectId = (string)obj;
                EditResearchProjectViewModel editResearchProjectViewModel = new EditResearchProjectViewModel(_context, _dialogService)
                {
                    ProjectId = projectId
                };
                var instance = MainWindowViewModel.Instance();
                if (instance is not null)
                {
                    instance.ResearchProjectsSubView = editResearchProjectViewModel;
                }
            }
        }

        private ICommand? _remove = null;
        public ICommand? Remove
        {
            get => _remove ??= new RelayCommand<object>(RemoveResearchProject);
        }

        private void RemoveResearchProject(object? obj)
        {
            if (obj is not null)
            {
                string projectId = (string)obj;
                ResearchProject? researchProject = _context.ResearchProjects.Find(projectId);
                if (researchProject is not null)
                {
                    DialogResult = _dialogService.Show(researchProject.Title);
                    if (DialogResult == false)
                    {
                        return;
                    }

                    _context.ResearchProjects.Remove(researchProject);
                    _context.SaveChanges();
                }
            }
        }

        public ResearchProjectViewModel(UniversityContext context, IDialogService dialogService)
        {
            _context = context;
            _dialogService = dialogService;

            _context.Database.EnsureCreated();
            _context.ResearchProjects.Load();
            ResearchProjects = _context.ResearchProjects.Local.ToObservableCollection();
        }
    }
}
