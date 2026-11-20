using FluentValidation;
using TaskService.Common;
using TaskService.Models;
using System.Linq;

namespace TaskService.Validation;


public class CreateTaskRequestValidator: AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator() 
    {
        RuleFor(x => x.Title)
            .NotEmpty().NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(Consts.TitleMaxLen)
            .WithMessage($"Title length cannot exceed {Consts.TitleMaxLen} characters.");

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage($"Invalid priority value. Please specify a valid Priority level. Acceptable values [{string.Join(", ", Enum.GetValues<Priority>().Select(p => ((int)p).ToString()))}]");
    }
}

