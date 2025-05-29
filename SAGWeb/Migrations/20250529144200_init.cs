using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGWeb.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SAGPreciosEnLinea",
                columns: table => new
                {
                    CodProducto = table.Column<int>(type: "int", nullable: false),
                    NomProducto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Bodega = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Existencia = table.Column<int>(type: "int", nullable: true),
                    Pbase = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    Costo = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    Peso = table.Column<decimal>(type: "decimal(10,3)", nullable: true),
                    ListaPrecio = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PorcentajeDesc = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    PrecioVenta = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    Pais = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Clase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrecioSinIVA = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    CantDecimales = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SAGPreci__0D06FDF3ACAA8CAE", x => x.CodProducto);
                });

            migrationBuilder.CreateTable(
                name: "SAGUsuariosMovil",
                columns: table => new
                {
                    CodVendedor = table.Column<int>(type: "int", nullable: false),
                    Pin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Pais = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SAGUsuar__25F4FC1B6412344B", x => x.CodVendedor);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    CodCliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomCliente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Clase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Vendedor = table.Column<int>(type: "int", nullable: true),
                    Ciudad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LPrecios = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MontoCredito = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    TotalDeuda = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    SaldoCredito = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Clientes__DF8324D731A31185", x => x.CodCliente);
                    table.ForeignKey(
                        name: "FK__Clientes__Vended__398D8EEE",
                        column: x => x.Vendedor,
                        principalTable: "SAGUsuariosMovil",
                        principalColumn: "CodVendedor");
                });

            migrationBuilder.CreateTable(
                name: "Cotizaciones",
                columns: table => new
                {
                    CodCotizacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodCliente = table.Column<int>(type: "int", nullable: false),
                    CodVendedor = table.Column<int>(type: "int", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    PrecioTotal = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Cotizaci__79BA079E7BFF028F", x => x.CodCotizacion);
                    table.ForeignKey(
                        name: "FK__Cotizacio__CodCl__46E78A0C",
                        column: x => x.CodCliente,
                        principalTable: "Clientes",
                        principalColumn: "CodCliente");
                    table.ForeignKey(
                        name: "FK__Cotizacio__CodVe__47DBAE45",
                        column: x => x.CodVendedor,
                        principalTable: "SAGUsuariosMovil",
                        principalColumn: "CodVendedor");
                });

            migrationBuilder.CreateTable(
                name: "CotizacionDetalle",
                columns: table => new
                {
                    IdDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodCotizacion = table.Column<int>(type: "int", nullable: false),
                    CodProducto = table.Column<int>(type: "int", nullable: false),
                    NombreProducto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(12,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Cotizaci__E43646A58B6A4851", x => x.IdDetalle);
                    table.ForeignKey(
                        name: "FK__Cotizacio__CodCo__4BAC3F29",
                        column: x => x.CodCotizacion,
                        principalTable: "Cotizaciones",
                        principalColumn: "CodCotizacion");
                    table.ForeignKey(
                        name: "FK__Cotizacio__CodPr__4CA06362",
                        column: x => x.CodProducto,
                        principalTable: "SAGPreciosEnLinea",
                        principalColumn: "CodProducto");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Vendedor",
                table: "Clientes",
                column: "Vendedor");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionDetalle_CodCotizacion",
                table: "CotizacionDetalle",
                column: "CodCotizacion");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionDetalle_CodProducto",
                table: "CotizacionDetalle",
                column: "CodProducto");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_CodCliente",
                table: "Cotizaciones",
                column: "CodCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_CodVendedor",
                table: "Cotizaciones",
                column: "CodVendedor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CotizacionDetalle");

            migrationBuilder.DropTable(
                name: "Cotizaciones");

            migrationBuilder.DropTable(
                name: "SAGPreciosEnLinea");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "SAGUsuariosMovil");
        }
    }
}
