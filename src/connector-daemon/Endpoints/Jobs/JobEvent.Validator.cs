using FluentValidation;

namespace connector_daemon.Endpoints.Jobs;

internal static partial class JobEvent
{
    internal sealed class JobEventModelValidator : AbstractValidator<JobEventApiModel>
    {
        public JobEventModelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name must not be empty.");

            RuleFor(x => x.Build)
                .NotNull()
                .WithMessage("Build must not be null.")
                .GreaterThan(0);

            RuleFor(x => x.GitUrl)
                .NotEmpty()
                .WithMessage("Git URL must not be empty.");

            RuleFor(x => x.Branch)
                .NotNull()
                .NotEmpty()
                .WithMessage("Branch must not be empty.");

            RuleFor(x => x.Commit)
                .NotEmpty()
                .Matches("^[a-fA-F0-9]{40}$")
                .WithMessage("Commit must be a valid 40-character Git SHA.");

            RuleFor(x => x.Status)
                .NotEmpty()
                .WithMessage("Status must not be empty.")
                .Must(status => new[]
                {
                    "started",
                    "success",
                    "failure",
                    "aborted",
                }.Contains((status ?? string.Empty).ToLowerInvariant()))
                .WithMessage("Invalid build status.");
        }
    }
}