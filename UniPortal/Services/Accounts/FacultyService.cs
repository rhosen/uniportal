using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.Helpers;
using UniPortal.Services.Infrastructures;
using UniPortal.ViewModels.Users;

namespace UniPortal.Services.Accounts
{
    public class FacultyService : BaseService<Faculty>
    {
        private readonly AccountService _accountService;
        private readonly FacultyNumberGenerator _generator;
        private readonly IUnitOfWork _unitOfWork;

        public FacultyService(UniPortalContext context, LogService logService,
                              AccountService accountService,
                              FacultyNumberGenerator generator,
                              IUnitOfWork unitOfWork)
            : base(context, logService)
        {
            _accountService = accountService;
            _generator = generator;
            _unitOfWork = unitOfWork;
        }

        public async Task<Faculty?> GetFacultyByAccountIdAsync(Guid accountId)
        {
            return await _context.Faculties
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.AccountId == accountId);
        }

        public async Task<List<SelectOption>> GetSelectOptionsAsync()
        {
            return await (
                from f in _unitOfWork.Context.Faculties
                join a in _unitOfWork.Context.Accounts on f.AccountId equals a.Id
                join ft in _unitOfWork.Context.FacultyTypes on f.FacultyTypeId equals ft.Id
                where a.IsActive && !f.IsDeleted
                orderby a.FirstName, a.LastName
                select new SelectOption
                {
                    Id = f.Id,
                    Name = $"{a.FirstName} {a.LastName} ({ft.Name})"
                }
            ).ToListAsync();
        }

        public async Task<List<SelectOption>> GetFacultiesByProgramIdAsync(Guid programId)
        {
            var query =
                from f in _context.Faculties
                join d in _context.Departments on f.DepartmentId equals d.Id
                join p in _context.Programs on d.Id equals p.DepartmentId
                join a in _context.Accounts on f.AccountId equals a.Id
                join ft in _context.FacultyTypes on f.FacultyTypeId equals ft.Id
                where p.Id == programId && !f.IsDeleted && !a.IsDeleted
                orderby a.FirstName, a.LastName, f.FacultyNumber
                select new SelectOption
                {
                    Id = f.Id,
                    Name = !string.IsNullOrWhiteSpace(a.FirstName + " " + a.LastName)
                        ? $"{a.FirstName} {a.LastName} ({ft.Name})"
                        : $"{f.FacultyNumber} ({ft.Name})"
                };

            return await query.ToListAsync();
        }

        public async Task CreateAsync(string email, string password, string firstName, string lastName,
                                      Guid departmentId, Guid facultyTypeId, bool isAdvisor = false)
        {
            var facultyNumber = await GenerateNextFacultyNumberAsync();

            var account = await _accountService.CreateAccountAsync(email, password, Roles.Faculty, firstName, lastName);

            var faculty = BuildFacultyEntity(account.Id, departmentId, facultyNumber, facultyTypeId, isAdvisor);

            _unitOfWork.Context.Faculties.Add(faculty);
            await _unitOfWork.CommitAsync();

            await LogAsync(account.Id, ActionType.Create, nameof(Faculty), faculty.AccountId, faculty);
        }

        public async Task UpdateAsync(FacultyViewModel viewModel)
        {
            if (viewModel == null || viewModel.AccountId == Guid.Empty)
                throw new ArgumentException("Invalid faculty view model.");

            var profile = new AccountViewModel
            {
                AccountId = viewModel.AccountId,
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName
            };
            await _accountService.UpdateProfileAsync(profile);

            var account = await _accountService.GetAccountAsync(viewModel.AccountId);
            if (account != null && !string.Equals(account.Email, viewModel.Email, StringComparison.OrdinalIgnoreCase))
                await _accountService.UpdateEmailAsync(viewModel.AccountId, viewModel.Email);

            var faculty = await _unitOfWork.Context.Faculties
                .FirstOrDefaultAsync(f => f.AccountId == viewModel.AccountId);

            if (faculty == null)
                throw new Exception("Faculty not found");

            faculty.DepartmentId = viewModel.DepartmentId;
            faculty.FacultyTypeId = viewModel.FacultyTypeId;
            faculty.IsAdvisor = viewModel.IsAdvisor;

            _unitOfWork.Context.Faculties.Update(faculty);
            await _unitOfWork.CommitAsync();
            await LogAsync(viewModel.AccountId, ActionType.Update, nameof(Faculty), faculty.AccountId, viewModel);
        }

        private async Task<string> GenerateNextFacultyNumberAsync()
        {
            var year = DateTime.Now.Year;

            int lastSequence = 0;
            var lastFacultyNumber = await _unitOfWork.Context.Faculties
                .Where(f => f.FacultyNumber.StartsWith($"F{year % 100:D2}"))
                .OrderByDescending(f => f.FacultyNumber)
                .Select(f => f.FacultyNumber.Substring(4))
                .FirstOrDefaultAsync();

            if (lastFacultyNumber != null && int.TryParse(lastFacultyNumber, out var seq))
                lastSequence = seq;

            return _generator.GenerateNext(year, lastSequence);
        }

        private Faculty BuildFacultyEntity(Guid accountId, Guid departmentId, string facultyNumber,
                                           Guid facultyTypeId, bool isAdvisor = false)
        {
            return new Faculty
            {
                AccountId = accountId,
                DepartmentId = departmentId,
                FacultyNumber = facultyNumber,
                FacultyTypeId = facultyTypeId,
                IsAdvisor = isAdvisor
            };
        }



        public async Task<List<FacultyViewModel>> GetAllAsync()
        {
            return await (
                from f in _unitOfWork.Context.Faculties
                join a in _unitOfWork.Context.Accounts on f.AccountId equals a.Id
                join d in _unitOfWork.Context.Departments on f.DepartmentId equals d.Id
                join ft in _unitOfWork.Context.FacultyTypes on f.FacultyTypeId equals ft.Id
                where f.Account.IsActive && !a.IsDeleted
                orderby f.FacultyNumber
                select new FacultyViewModel
                {
                    AccountId = f.AccountId,
                    FacultyNumber = f.FacultyNumber,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Email = a.Email,
                    DepartmentId = f.DepartmentId,
                    DepartmentName = d.Name,
                    FacultyTypeId = f.FacultyTypeId,
                    FacultyTypeName = ft.Name,
                    IsAdvisor = f.IsAdvisor
                }
            ).ToListAsync();
        }

        public async Task<FacultyViewModel?> GetAsync(
            Guid? facultyId = null,
            string? facultyNumber = null,
            Guid? accountId = null)
        {
            if (facultyId == null && facultyNumber == null && accountId == null)
                throw new ArgumentException("At least one identifier must be provided.");

            return await (
                from f in _unitOfWork.Context.Faculties
                join a in _unitOfWork.Context.Accounts on f.AccountId equals a.Id
                join d in _unitOfWork.Context.Departments on f.DepartmentId equals d.Id
                join ft in _unitOfWork.Context.FacultyTypes on f.FacultyTypeId equals ft.Id
                where f.Account.IsActive && !a.IsDeleted
                      && (facultyId == null || f.Id == facultyId)
                      && (facultyNumber == null || f.FacultyNumber == facultyNumber)
                      && (accountId == null || f.AccountId == accountId)
                select new FacultyViewModel
                {
                    AccountId = f.AccountId,
                    FacultyNumber = f.FacultyNumber,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Email = a.Email,
                    DepartmentId = f.DepartmentId,
                    DepartmentName = d.Name,
                    FacultyTypeId = f.FacultyTypeId,
                    FacultyTypeName = ft.Name,
                    IsAdvisor = f.IsAdvisor
                }
            ).FirstOrDefaultAsync();
        }

        public async Task DeleteAsync(Guid accountId)
        {
            var account = await _accountService.GetAccountAsync(accountId);
            if (account == null) return;

            await _accountService.SoftDeleteAsync(account.Id);

            var faculty = await _unitOfWork.Context.Faculties.FirstOrDefaultAsync(f => f.AccountId == accountId);
            if (faculty != null)
            {
                faculty.IsDeleted = true;
                _unitOfWork.Context.Faculties.Update(faculty);
            }

            await _unitOfWork.CommitAsync();
            await LogAsync(accountId, ActionType.Delete, nameof(Faculty), accountId, faculty);
        }

        public async Task ActivateAsync(Guid accountId)
        {
            var account = await _accountService.GetAccountAsync(accountId);
            if (account == null) return;

            await _accountService.ActivateAsync(account.Id);

            var faculty = await _unitOfWork.Context.Faculties.FirstOrDefaultAsync(f => f.AccountId == accountId);
            if (faculty != null)
            {
                faculty.IsDeleted = false;
                _unitOfWork.Context.Faculties.Update(faculty);
            }

            await _unitOfWork.CommitAsync();
            await LogAsync(accountId, ActionType.Activate, nameof(Faculty), accountId, faculty);
        }

    }
}
