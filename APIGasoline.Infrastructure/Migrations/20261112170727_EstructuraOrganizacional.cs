using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <summary>
    /// Fase 8: la empresa es la raíz de la estructura organizacional.
    /// <list type="bullet">
    /// <item>corporativo y division pasan a ser de una empresa (empresa_id) y se ligan N:M en corporativo_division.</item>
    /// <item>centro_costo gana corporativo_id y division_id opcionales (FK compuestas con empresa_id y con la liga).</item>
    /// <item>empresa deja de tener corporativo_id y division_id.</item>
    /// <item>Las tres tablas entran a la política de Row-Level Security.</item>
    /// <item>corporativo.gestionar pasa a ámbito empresa.</item>
    /// </list>
    /// Datos existentes: cada corporativo queda en la primera empresa que lo usaba (los que ninguna usaba
    /// se borran junto con sus divisiones), cada división en la empresa de su corporativo, y la relación
    /// división → corporativo se vuelve una liga. Los nombres repetidos dentro de una empresa llevan " #n".
    /// Los nombres van escritos aquí (y no tomados del código) porque una migración aplicada no debe cambiar.
    /// </summary>
    public partial class EstructuraOrganizacional : Migration
    {
        private const string Politica = "rls.aislamiento_empresa";
        private static readonly string[] TablasNuevasEnPolitica = ["corporativo", "division", "corporativo_division"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Quitar lo que dependía de la estructura vieja
            migrationBuilder.DropForeignKey(name: "fk_division_corporativo", table: "division");
            migrationBuilder.DropForeignKey(name: "fk_empresa_corporativo", table: "empresa");
            migrationBuilder.DropForeignKey(name: "fk_empresa_division", table: "empresa");
            migrationBuilder.DropIndex(name: "ix_empresa_corporativo_id", table: "empresa");
            migrationBuilder.DropIndex(name: "ix_empresa_corporativo_id_division_id", table: "empresa");
            migrationBuilder.DropIndex(name: "ix_empresa_division_id", table: "empresa");
            migrationBuilder.DropIndex(name: "uq_empresa_corporativo_id_empresa_id", table: "empresa");
            migrationBuilder.DropUniqueConstraint(name: "uq_division_corporativo_id_division_id", table: "division");
            migrationBuilder.DropIndex(name: "ix_division_corporativo_id", table: "division");

            // 2. Columnas nuevas (empresa_id primero NULL para poder llenarla)
            migrationBuilder.AddColumn<byte[]>(name: "empresa_id", table: "corporativo", type: "binary(16)", nullable: true);
            migrationBuilder.AddColumn<byte[]>(name: "empresa_id", table: "division", type: "binary(16)", nullable: true);
            migrationBuilder.AddColumn<byte[]>(name: "corporativo_id", table: "centro_costo", type: "binary(16)", nullable: true);
            migrationBuilder.AddColumn<byte[]>(name: "division_id", table: "centro_costo", type: "binary(16)", nullable: true);

            migrationBuilder.CreateTable(
                name: "corporativo_division",
                columns: table => new
                {
                    corporativo_division_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    corporativo_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    division_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "datetime2(0)", nullable: true, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_corporativo_division", x => x.corporativo_division_id);
                });



            // 3. Pasar los datos. empresa tiene Row-Level Security: esta conexión debe ver todas las empresas.
            Ejecutar(migrationBuilder, "EXEC sys.sp_set_session_context @key = N'apigasoline.ve_todas', @value = 1;");

            Ejecutar(migrationBuilder, """
                UPDATE c SET c.empresa_id = (SELECT MIN(e.empresa_id) FROM dbo.empresa AS e WHERE e.corporativo_id = c.corporativo_id)
                FROM dbo.corporativo AS c;
                """);

            Ejecutar(migrationBuilder, """
                UPDATE d SET d.empresa_id = c.empresa_id
                FROM dbo.division AS d
                JOIN dbo.corporativo AS c ON c.corporativo_id = d.corporativo_id;
                """);

            // Sin empresa no pueden quedarse: ninguna empresa los usaba
            Ejecutar(migrationBuilder, "DELETE FROM dbo.division WHERE empresa_id IS NULL;");
            Ejecutar(migrationBuilder, "DELETE FROM dbo.corporativo WHERE empresa_id IS NULL;");

            // La relación vieja división -> corporativo se vuelve una liga N:M
            Ejecutar(migrationBuilder, """
                INSERT INTO dbo.corporativo_division (corporativo_division_id, empresa_id, corporativo_id, division_id)
                SELECT CAST(NEWID() AS binary(16)), d.empresa_id, d.corporativo_id, d.division_id
                FROM dbo.division AS d;
                """);

            // El nombre pasa a ser único por empresa: los repetidos llevan " #n"
            Ejecutar(migrationBuilder, """
                WITH repetidos AS (
                    SELECT nombre, ROW_NUMBER() OVER (PARTITION BY empresa_id, nombre ORDER BY corporativo_id) AS n
                    FROM dbo.corporativo)
                UPDATE repetidos SET nombre = LEFT(nombre, 140) + N' #' + CAST(n AS nvarchar(10)) WHERE n > 1;
                """);
            Ejecutar(migrationBuilder, """
                WITH repetidos AS (
                    SELECT nombre, ROW_NUMBER() OVER (PARTITION BY empresa_id, nombre ORDER BY division_id) AS n
                    FROM dbo.division)
                UPDATE repetidos SET nombre = LEFT(nombre, 90) + N' #' + CAST(n AS nvarchar(10)) WHERE n > 1;
                """);

            // 4. Quitar las columnas viejas y volver obligatoria la empresa.
            //    empresa está en la política (SCHEMABINDING = ON): sale un momento para cambiarle columnas.
            Ejecutar(migrationBuilder, $"ALTER SECURITY POLICY {Politica}\n    {string.Join(",\n    ", Predicados("DROP", "empresa"))};");
            migrationBuilder.DropColumn(name: "corporativo_id", table: "empresa");
            migrationBuilder.DropColumn(name: "division_id", table: "empresa");
            Ejecutar(migrationBuilder, $"ALTER SECURITY POLICY {Politica}\n    {string.Join(",\n    ", Predicados("ADD", "empresa"))};");
            migrationBuilder.DropColumn(name: "corporativo_id", table: "division");
            migrationBuilder.AlterColumn<byte[]>(name: "empresa_id", table: "corporativo", type: "binary(16)", nullable: false, oldClrType: typeof(byte[]), oldType: "binary(16)", oldNullable: true);
            migrationBuilder.AlterColumn<byte[]>(name: "empresa_id", table: "division", type: "binary(16)", nullable: false, oldClrType: typeof(byte[]), oldType: "binary(16)", oldNullable: true);

            // 5. Llaves, índices y FK (compuestas con empresa_id: nada cruza de empresa)
            migrationBuilder.AddUniqueConstraint(name: "uq_corporativo_empresa_id_corporativo_id", table: "corporativo", columns: new[] { "empresa_id", "corporativo_id" });
            migrationBuilder.AddUniqueConstraint(name: "uq_division_empresa_id_division_id", table: "division", columns: new[] { "empresa_id", "division_id" });
            migrationBuilder.AddUniqueConstraint(name: "uq_corporativo_division_empresa_id_corporativo_id_division_id", table: "corporativo_division", columns: new[] { "empresa_id", "corporativo_id", "division_id" });

            migrationBuilder.CreateIndex(name: "uq_corporativo_empresa_id_nombre", table: "corporativo", columns: new[] { "empresa_id", "nombre" }, unique: true);
            migrationBuilder.CreateIndex(name: "uq_division_empresa_id_nombre", table: "division", columns: new[] { "empresa_id", "nombre" }, unique: true);
            migrationBuilder.CreateIndex(name: "uq_corporativo_division_corporativo_id_division_id", table: "corporativo_division", columns: new[] { "corporativo_id", "division_id" }, unique: true);
            migrationBuilder.CreateIndex(name: "ix_corporativo_division_empresa_id_division_id", table: "corporativo_division", columns: new[] { "empresa_id", "division_id" });
            migrationBuilder.CreateIndex(name: "ix_centro_costo_empresa_id_corporativo_id_division_id", table: "centro_costo", columns: new[] { "empresa_id", "corporativo_id", "division_id" });
            migrationBuilder.CreateIndex(name: "ix_centro_costo_empresa_id_division_id", table: "centro_costo", columns: new[] { "empresa_id", "division_id" });

            migrationBuilder.AddForeignKey(name: "fk_corporativo_empresa", table: "corporativo", column: "empresa_id", principalTable: "empresa", principalColumn: "empresa_id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "fk_division_empresa", table: "division", column: "empresa_id", principalTable: "empresa", principalColumn: "empresa_id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "fk_corporativo_division_corporativo", table: "corporativo_division", columns: new[] { "empresa_id", "corporativo_id" }, principalTable: "corporativo", principalColumns: new[] { "empresa_id", "corporativo_id" }, onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "fk_corporativo_division_division", table: "corporativo_division", columns: new[] { "empresa_id", "division_id" }, principalTable: "division", principalColumns: new[] { "empresa_id", "division_id" }, onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "fk_centro_costo_corporativo", table: "centro_costo", columns: new[] { "empresa_id", "corporativo_id" }, principalTable: "corporativo", principalColumns: new[] { "empresa_id", "corporativo_id" }, onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "fk_centro_costo_division", table: "centro_costo", columns: new[] { "empresa_id", "division_id" }, principalTable: "division", principalColumns: new[] { "empresa_id", "division_id" }, onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "fk_centro_costo_corporativo_division", table: "centro_costo", columns: new[] { "empresa_id", "corporativo_id", "division_id" }, principalTable: "corporativo_division", principalColumns: new[] { "empresa_id", "corporativo_id", "division_id" }, onDelete: ReferentialAction.Restrict);

            // 6. Administrar corporativos y divisiones ya es cosa de cada empresa
            Ejecutar(migrationBuilder, "UPDATE dbo.privilegio SET ambito = 'empresa' WHERE clave = 'corporativo.gestionar';");

            // 7. Row-Level Security: mismas reglas que las demás tablas con empresa_id
            Ejecutar(migrationBuilder, $"ALTER SECURITY POLICY {Politica}\n    {string.Join(",\n    ", Predicados("ADD", TablasNuevasEnPolitica))};");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Las tablas salen de la política antes de cambiarlas (empresa vuelve al final)
            Ejecutar(migrationBuilder, $"ALTER SECURITY POLICY {Politica}\n    {string.Join(",\n    ", Predicados("DROP", [.. TablasNuevasEnPolitica, "empresa"]))};");
            Ejecutar(migrationBuilder, "EXEC sys.sp_set_session_context @key = N'apigasoline.ve_todas', @value = 1;");

            // corporativo.gestionar vuelve a plataforma: se quita de los roles de empresa
            Ejecutar(migrationBuilder, """
                DELETE rp FROM dbo.rol_privilegio AS rp
                JOIN dbo.privilegio AS p ON p.privilegio_id = rp.privilegio_id
                JOIN dbo.rol AS r ON r.rol_id = rp.rol_id
                WHERE p.clave = 'corporativo.gestionar' AND r.empresa_id IS NOT NULL;
                """);
            Ejecutar(migrationBuilder, "UPDATE dbo.privilegio SET ambito = 'plataforma' WHERE clave = 'corporativo.gestionar';");

            migrationBuilder.DropForeignKey(name: "fk_centro_costo_corporativo", table: "centro_costo");
            migrationBuilder.DropForeignKey(name: "fk_centro_costo_corporativo_division", table: "centro_costo");
            migrationBuilder.DropForeignKey(name: "fk_centro_costo_division", table: "centro_costo");
            migrationBuilder.DropForeignKey(name: "fk_corporativo_empresa", table: "corporativo");
            migrationBuilder.DropForeignKey(name: "fk_division_empresa", table: "division");
            migrationBuilder.DropIndex(name: "ix_centro_costo_empresa_id_corporativo_id_division_id", table: "centro_costo");
            migrationBuilder.DropIndex(name: "ix_centro_costo_empresa_id_division_id", table: "centro_costo");
            migrationBuilder.DropColumn(name: "corporativo_id", table: "centro_costo");
            migrationBuilder.DropColumn(name: "division_id", table: "centro_costo");

            // Cada división vuelve a un solo corporativo (el primero con que estaba ligada); las sueltas se borran
            migrationBuilder.AddColumn<byte[]>(name: "corporativo_id", table: "division", type: "binary(16)", nullable: true);
            Ejecutar(migrationBuilder, """
                UPDATE d SET d.corporativo_id = (SELECT MIN(l.corporativo_id) FROM dbo.corporativo_division AS l WHERE l.division_id = d.division_id)
                FROM dbo.division AS d;
                """);
            migrationBuilder.DropTable(name: "corporativo_division");
            Ejecutar(migrationBuilder, "DELETE FROM dbo.division WHERE corporativo_id IS NULL;");
            migrationBuilder.AlterColumn<byte[]>(name: "corporativo_id", table: "division", type: "binary(16)", nullable: false, oldClrType: typeof(byte[]), oldType: "binary(16)", oldNullable: true);

            migrationBuilder.DropUniqueConstraint(name: "uq_division_empresa_id_division_id", table: "division");
            migrationBuilder.DropIndex(name: "uq_division_empresa_id_nombre", table: "division");
            migrationBuilder.DropUniqueConstraint(name: "uq_corporativo_empresa_id_corporativo_id", table: "corporativo");
            migrationBuilder.DropIndex(name: "uq_corporativo_empresa_id_nombre", table: "corporativo");

            // La empresa vuelve a apuntar a su corporativo (el primero que tenga)
            migrationBuilder.AddColumn<byte[]>(name: "corporativo_id", table: "empresa", type: "binary(16)", nullable: true);
            migrationBuilder.AddColumn<byte[]>(name: "division_id", table: "empresa", type: "binary(16)", nullable: true);
            Ejecutar(migrationBuilder, """
                UPDATE e SET e.corporativo_id = (SELECT MIN(c.corporativo_id) FROM dbo.corporativo AS c WHERE c.empresa_id = e.empresa_id)
                FROM dbo.empresa AS e;
                """);
            migrationBuilder.DropColumn(name: "empresa_id", table: "corporativo");
            migrationBuilder.DropColumn(name: "empresa_id", table: "division");

            migrationBuilder.AddUniqueConstraint(name: "uq_division_corporativo_id_division_id", table: "division", columns: new[] { "corporativo_id", "division_id" });
            migrationBuilder.CreateIndex(name: "ix_empresa_corporativo_id", table: "empresa", column: "corporativo_id");
            migrationBuilder.CreateIndex(name: "ix_empresa_corporativo_id_division_id", table: "empresa", columns: new[] { "corporativo_id", "division_id" });
            migrationBuilder.CreateIndex(name: "ix_empresa_division_id", table: "empresa", column: "division_id");
            migrationBuilder.CreateIndex(name: "uq_empresa_corporativo_id_empresa_id", table: "empresa", columns: new[] { "corporativo_id", "empresa_id" }, unique: true, filter: "[corporativo_id] IS NOT NULL");
            migrationBuilder.CreateIndex(name: "ix_division_corporativo_id", table: "division", column: "corporativo_id");
            migrationBuilder.AddForeignKey(name: "fk_division_corporativo", table: "division", column: "corporativo_id", principalTable: "corporativo", principalColumn: "corporativo_id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "fk_empresa_corporativo", table: "empresa", column: "corporativo_id", principalTable: "corporativo", principalColumn: "corporativo_id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "fk_empresa_division", table: "empresa", columns: new[] { "corporativo_id", "division_id" }, principalTable: "division", principalColumns: new[] { "corporativo_id", "division_id" }, onDelete: ReferentialAction.Restrict);

            Ejecutar(migrationBuilder, $"ALTER SECURITY POLICY {Politica}\n    {string.Join(",\n    ", Predicados("ADD", "empresa"))};");
        }

        /// <summary>Los tres predicados de una tabla con empresa_id: leer (FILTER) y escribir (BLOCK AFTER INSERT/UPDATE).</summary>
        private static IEnumerable<string> Predicados(string operacion, params string[] tablas)
        {
            var funcion = operacion == "ADD" ? " rls.fn_misma_empresa(empresa_id)" : "";

            foreach (var tabla in tablas)
            {
                yield return $"{operacion} FILTER PREDICATE{funcion} ON dbo.{tabla}";
                yield return $"{operacion} BLOCK PREDICATE{funcion} ON dbo.{tabla} AFTER INSERT";
                yield return $"{operacion} BLOCK PREDICATE{funcion} ON dbo.{tabla} AFTER UPDATE";
            }
        }

        /// <summary>
        /// Cada sentencia en su propio EXEC: en migraciones.sql (script idempotente) todo va en un lote
        /// y SQL Server compila el lote completo antes de que existan las columnas nuevas.
        /// </summary>
        private static void Ejecutar(MigrationBuilder migrationBuilder, string sql)
            => migrationBuilder.Sql($"EXEC(N'{sql.Replace("'", "''")}');");
    }
}
