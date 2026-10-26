using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;
using SchoolNewspaperBlazorApp.Interfaces.Service;

namespace SchoolNewspaperBlazorApp.Service
{
    public class SubstitutionService : ISubstitutionService
    {
        private const int PublicListLimit = 500;
        private const int AdminListLimit = 300;

        private readonly ISubstitutionRepository _repository;
        private readonly IPermissionService _permissionService;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILogger<SubstitutionService> _logger;

        public SubstitutionService(
            ISubstitutionRepository repository,
            IPermissionService permissionService,
            AuthenticationStateProvider authenticationStateProvider,
            ILogger<SubstitutionService> logger)
        {
            _repository = repository;
            _permissionService = permissionService;
            _authenticationStateProvider = authenticationStateProvider;
            _logger = logger;
        }

        public async Task AddAsync(SubstitutionInput input)
        {
            if (!await HasAsync(EnumPermissions.AddSubstitution))
            {
                throw new UnauthorizedAccessException();
            }

            input = Normalize(input);

            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            int? userId = int.TryParse(state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

            var substitution = new Substitution
            {
                Date = input.Date,
                Lesson = input.Lesson,
                Kind = input.Kind,
                ClassId = input.ClassId,
                SubjectId = input.SubjectId,
                AbsentTeacherId = input.AbsentTeacherId,
                SubstituteTeacherId = input.SubstituteTeacherId,
                RoomId = input.RoomId,
                Note = input.Note ?? string.Empty,
                CreatedByUserId = userId,
                CreatedAt = DateTime.Now
            };

            await _repository.AddAsync(substitution);
        }

        public async Task UpdateAsync(int id, SubstitutionInput input)
        {
            if (!await HasAsync(EnumPermissions.ManageSubstitution))
            {
                throw new UnauthorizedAccessException();
            }

            await _repository.UpdateAsync(id, Normalize(input));
        }

        public async Task DeleteAsync(int id)
        {
            if (!await HasAsync(EnumPermissions.ManageSubstitution))
            {
                throw new UnauthorizedAccessException();
            }

            await _repository.DeleteAsync(id);
        }

        public Task<Substitution?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

        public Task<List<Substitution>> GetUpcomingAsync(int? classId = null, int? teacherId = null) =>
            _repository.GetListAsync(DateOnly.FromDateTime(DateTime.Today), classId, teacherId, false, PublicListLimit);

        public Task<List<Substitution>> GetRecentAsync() =>
            _repository.GetListAsync(null, null, null, true, AdminListLimit);

        private async Task<bool> HasAsync(EnumPermissions permission) =>
            await _permissionService.HasPermissionAsync(permission)
            || await _permissionService.HasPermissionAsync(EnumPermissions.Master);

        // Walidacja + czyszczenie pól, które nie pasują do wybranego typu wpisu
        private static SubstitutionInput Normalize(SubstitutionInput input)
        {
            if (input.ClassId <= 0)
            {
                throw new InvalidOperationException("Wybierz klasę.");
            }

            if (input.Date == default)
            {
                throw new InvalidOperationException("Wybierz datę.");
            }

            if (input.Lesson < 0 || input.Lesson > Substitution.MaxLesson)
            {
                throw new InvalidOperationException($"Numer lekcji musi być z zakresu 0–{Substitution.MaxLesson}.");
            }

            if (!Enum.IsDefined(input.Kind))
            {
                throw new InvalidOperationException("Nieznany typ wpisu.");
            }

            var note = (input.Note ?? string.Empty).Trim();
            if (note.Length > Substitution.MaxNoteLength)
            {
                throw new InvalidOperationException(
                    $"Uwagi mogą mieć maksymalnie {Substitution.MaxNoteLength} znaków.");
            }

            var subjectId = input.SubjectId;
            var absent = input.AbsentTeacherId;
            var substitute = input.SubstituteTeacherId;
            var roomId = input.RoomId;

            switch (input.Kind)
            {
                case SubstitutionKind.Substitution:
                    if (substitute is null)
                    {
                        throw new InvalidOperationException("Wybierz nauczyciela zastępującego.");
                    }

                    if (absent is not null && absent == substitute)
                    {
                        throw new InvalidOperationException("Nauczyciel nie może zastępować samego siebie.");
                    }

                    break;

                case SubstitutionKind.Cancelled:
                    // lekcja się nie odbywa – nie ma zastępcy ani sali
                    substitute = null;
                    roomId = null;
                    break;

                case SubstitutionKind.RoomChange:
                    if (roomId is null)
                    {
                        throw new InvalidOperationException("Wybierz nową salę.");
                    }

                    substitute = null;
                    break;
            }

            return input with
            {
                Note = note,
                SubjectId = subjectId,
                AbsentTeacherId = absent,
                SubstituteTeacherId = substitute,
                RoomId = roomId
            };
        }
    }
}
