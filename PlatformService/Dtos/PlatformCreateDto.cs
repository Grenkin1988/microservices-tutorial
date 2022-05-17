using System.ComponentModel.DataAnnotations;

namespace PlatformService.Dtos;

public record PlatformCreateDto
(
    [Required] string Name,

    [Required] string Publisher,

    [Required] string Cost
);
