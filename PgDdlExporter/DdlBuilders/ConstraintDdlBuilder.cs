using System.Text;
using PgDdlExporter.Models;

namespace PgDdlExporter.DdlBuilders
{
    public class ConstraintDdlBuilder : IDdlBuilder<ConstraintInfo>
    {
        public string Build(ConstraintInfo constraint)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"-- Constraint: {constraint.Name} ({constraint.Type}) on {constraint.SchemaName}.{constraint.TableName}");
            sb.AppendLine($"ALTER TABLE {constraint.SchemaName}.{constraint.TableName}");
            sb.AppendLine($"    ADD CONSTRAINT {constraint.Name} {constraint.Definition};");

            return sb.ToString();
        }

        // Birden fazla constraint'i tek dosyada (tablo bazlý) toplamak için
        public string BuildMany(IEnumerable<ConstraintInfo> constraints)
        {
            var sb = new StringBuilder();

            // Sýralama önemli: PK/UNIQUE önce, sonra FK, en son CHECK
            // (FK genelde baþka tablonun PK/UNIQUE'ine referans verir)
            var ordered = constraints
                .OrderBy(c => c.Type switch
                {
                    ConstraintType.PrimaryKey => 0,
                    ConstraintType.Unique => 1,
                    ConstraintType.ForeignKey => 2,
                    ConstraintType.Check => 3,
                    _ => 4
                });

            foreach (var constraint in ordered)
            {
                sb.AppendLine(Build(constraint));
            }

            return sb.ToString();
        }
    }
}