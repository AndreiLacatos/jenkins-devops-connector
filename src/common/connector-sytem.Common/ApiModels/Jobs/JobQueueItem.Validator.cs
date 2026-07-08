using FluentValidation;

namespace connector_sytem.Common.ApiModels.Jobs;

public sealed class JobQueueItemValidator : AbstractValidator<JobQueueItem>
{
    public JobQueueItemValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage("Name must not be empty.");
        RuleFor(x => x.Build)
            .NotNull()
            .GreaterThan(0)
            .WithMessage("Build must be non-negative.");
        RuleFor(x => x.Commit)
            .NotNull()
            .NotEmpty()
            .WithMessage("Commit must not be empty.");
    }
}
