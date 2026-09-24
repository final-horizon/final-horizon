using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Content.Server.Database;

[Table("fh_jobrank")]
public sealed class FHJobRank
{
    [Key]
    public Guid PlayerId { get; set; }

    public string? JobRank { get; set; } = default!;
}
