using FluentValidation;
using VbMerchant.DTOs;

namespace VbMerchant.Validators
{
    public class BasvuruValidator : AbstractValidator<BasvuruCreateRequest>
    {
        public BasvuruValidator()
        {
            RuleFor(x => x.VergiNoTCKN)
                .NotEmpty().WithMessage("Vergi No boş olamaz.")
                .Length(10).WithMessage("Vergi No tam 10 karakter olmalıdır.")
                .Matches(@"^\d{10}$").WithMessage("Vergi No sadece rakamlardan oluşmalıdır.");

            RuleFor(x => x.YetkiliTCKN)
                .Length(11).WithMessage("Yetkili TCKN tam 11 karakter olmalıdır.")
                .Matches(@"^\d{11}$").WithMessage("Yetkili TCKN sadece rakamlardan oluşmalıdır.")
                .When(x => !string.IsNullOrEmpty(x.YetkiliTCKN));

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email boş olamaz.")
                .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");

            RuleFor(x => x.CepTelefon)
                .NotEmpty().WithMessage("Cep Telefon boş olamaz.")
                .Must(value => IsValidMobilePhone(value)).WithMessage("Geçerli bir telefon numarası giriniz.");

            RuleFor(x => x.IsTelefon)
                .Must(value => string.IsNullOrWhiteSpace(value) || IsValidBusinessPhone(value))
                .WithMessage("İş telefonu 2XX ile başlamalıdır.");

            RuleFor(x => x.Adres)
                .NotEmpty().WithMessage("Adres boş olamaz.")
                .MinimumLength(10).WithMessage("Adres en az 10 karakter olmalıdır.");

            RuleFor(x => x.IlId)
                .GreaterThan(0).WithMessage("İl seçimi zorunludur.");

            RuleFor(x => x.IlceId)
                .GreaterThan(0).WithMessage("İlçe seçimi zorunludur.");
        }

        private static bool IsValidMobilePhone(string? phone)
        {
            var normalizedPhone = NormalizePhone(phone);
            return !string.IsNullOrWhiteSpace(normalizedPhone) && System.Text.RegularExpressions.Regex.IsMatch(normalizedPhone, @"^(05|5)\d{9}$");
        }

        private static bool IsValidBusinessPhone(string? phone)
        {
            var normalizedPhone = NormalizePhone(phone);
            return System.Text.RegularExpressions.Regex.IsMatch(normalizedPhone, @"^(02|2)\d{9}$");
        }

        private static string NormalizePhone(string? phone)
        {
            return System.Text.RegularExpressions.Regex.Replace(phone ?? string.Empty, @"\s+", string.Empty);
        }
    }
}
