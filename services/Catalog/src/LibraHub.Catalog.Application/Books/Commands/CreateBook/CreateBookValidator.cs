using FluentValidation;

namespace LibraHub.Catalog.Application.Books.Commands.CreateBook;

public class CreateBookValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(500).WithMessage("Title must not exceed 500 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required")
            .MaximumLength(50).WithMessage("Language must not exceed 50 characters");

        RuleFor(x => x.Publisher)
            .NotEmpty().WithMessage("Publisher is required")
            .MaximumLength(200).WithMessage("Publisher must not exceed 200 characters");

        RuleFor(x => x.PublicationDate)
            .NotEmpty().WithMessage("Publication date is required");

        RuleFor(x => x.Isbn)
            .NotEmpty().WithMessage("ISBN is required")
            .MaximumLength(17).WithMessage("ISBN must not exceed 17 characters");

        RuleFor(x => x.Authors)
            .NotNull().WithMessage("Authors list is required")
            .NotEmpty().WithMessage("At least one author is required");

        RuleForEach(x => x.Authors)
            .NotEmpty().WithMessage("Author name cannot be empty")
            .MaximumLength(200).WithMessage("Author name must not exceed 200 characters");

        RuleFor(x => x.Categories)
            .NotNull().WithMessage("Categories list is required")
            .NotEmpty().WithMessage("At least one category is required");

        RuleForEach(x => x.Categories)
            .NotEmpty().WithMessage("Category name cannot be empty")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters");

        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("Tag name cannot be empty")
            .MaximumLength(50).WithMessage("Tag name must not exceed 50 characters")
            .When(x => x.Tags != null && x.Tags.Any());
    }
}
