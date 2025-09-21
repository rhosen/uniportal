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


        public async Task<List<SelectOption>> GetSelectOptionsAsync()
        {
            return await (
                from f in _unitOfWork.Context.Faculties
                join a in _unitOfWork.Context.Accounts on f.AccountId equals a.Id
                where a.IsActive
                select new SelectOption
                {
                    Id = f.Id,
                    Name = a.FirstName + " " + a.LastName
                }
            ).ToListAsync();
        }

        public async Task<List<SelectOption>> GetFacultiesAsync(Guid programId)
        {
            var query =
                from f in _context.Faculties
                join d in _context.Departments on f.DepartmentId equals d.Id
                join p in _context.Programs on d.Id equals p.DepartmentId
                join a in _context.Accounts on f.AccountId equals a.Id
                where p.Id == programId && !f.IsDeleted && !a.IsDeleted
                orderby a.FirstName, a.LastName, f.FacultyNumber
                select new SelectOption
                {
                    Id = f.Id,
                    Name = (a.FirstName + " " + a.LastName).Trim() != string.Empty
                        ? (a.FirstName + " " + a.LastName).Trim()
                        : f.FacultyNumber
                };

            return await query.ToListAsync();
        }

        /// <summary>
        /// Public method to create a new faculty
        /// </summary>
        public async Task CreateAsync(string email, string password, string firstName, string lastName,
                                      Guid departmentId)
        {
            // 1️⃣ Generate Faculty Number
            var facultyNumber = await GenerateNextFacultyNumberAsync();

            // 2️⃣ Create Account
            var account = await _accountService.CreateAccountAsync(email, password, Roles.Faculty, firstName, lastName);

            // 3️⃣ Create Faculty entity in-memory
            var faculty = BuildFacultyEntity(account.Id, departmentId, facultyNumber);

            // 4️⃣ Add entity and commit via UnitOfWork
            _unitOfWork.Context.Faculties.Add(faculty);

            await _unitOfWork.CommitAsync();

            // 5️⃣ Log creation
            await LogAsync(account.Id, ActionType.Create, nameof(Faculty), faculty.AccountId, faculty);
        }

        public async Task UpdateAsync(FacultyViewModel viewModel)
        {
            if (viewModel == null || viewModel.AccountId == Guid.Empty)
                throw new ArgumentException("Invalid faculty view model.");

            // Update account profile
            var profile = new AccountViewModel
            {
                AccountId = viewModel.AccountId,
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName
            };
            await _accountService.UpdateProfileAsync(profile);

            // Update email if changed
            var account = await _accountService.GetAccountAsync(viewModel.AccountId);
            if (account != null && !string.Equals(account.Email, viewModel.Email, StringComparison.OrdinalIgnoreCase))
                await _accountService.UpdateEmailAsync(viewModel.AccountId, viewModel.Email);

            // Update Faculty-specific field
            var faculty = await _unitOfWork.Context.Faculties
                .FirstOrDefaultAsync(f => f.AccountId == viewModel.AccountId);

            if (faculty == null)
                throw new Exception("Faculty not found");

            faculty.DepartmentId = viewModel.DepartmentId;
            _unitOfWork.Context.Faculties.Update(faculty);

            // Commit and log
            await _unitOfWork.CommitAsync();
            await LogAsync(viewModel.AccountId, ActionType.Update, nameof(Faculty), faculty.AccountId, viewModel);
        }


        #region Private Helper Methods

        /// <summary>
        /// Generates the next FacultyNumber (FYY-XXXX)
        /// </summary>
        private async Task<string> GenerateNextFacultyNumberAsync()
        {
            var year = DateTime.Now.Year;

            // Get last sequence for this year
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


        /// <summary>
        /// Builds the Faculty entity
        /// </summary>
        private Faculty BuildFacultyEntity(Guid accountId, Guid departmentId, string facultyNumber)
        {
            return new Faculty
            {
                AccountId = accountId,
                DepartmentId = departmentId,
                FacultyNumber = facultyNumber,
            };
        }

        #endregion

        #region Read / Update / Delete Methods

        public async Task<List<FacultyViewModel>> GetAllAsync()
        {
            return await (
                from f in _unitOfWork.Context.Faculties
                join a in _unitOfWork.Context.Accounts on f.AccountId equals a.Id
                join d in _unitOfWork.Context.Departments on f.DepartmentId equals d.Id
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

        #endregion
    }
}
