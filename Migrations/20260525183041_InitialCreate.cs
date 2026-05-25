using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudioFlow.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "equipment",
                columns: table => new
                {
                    equipmentid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    rentalprice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    imageurl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment", x => x.equipmentid);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    roleid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rolename = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.roleid);
                });

            migrationBuilder.CreateTable(
                name: "studios",
                columns: table => new
                {
                    studioid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    area = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    ceilingheight = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    wallcolor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    hasnaturallight = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    baseprice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    imageurl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_studios", x => x.studioid);
                });

            migrationBuilder.CreateTable(
                name: "timeslots",
                columns: table => new
                {
                    slotid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    starttime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    endtime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    durationminutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timeslots", x => x.slotid);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    userid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    passwordhash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    fullname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    roleid = table.Column<int>(type: "integer", nullable: false),
                    isblocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    lastloginat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.userid);
                    table.ForeignKey(
                        name: "users_roleid_fkey",
                        column: x => x.roleid,
                        principalTable: "roles",
                        principalColumn: "roleid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pricingrules",
                columns: table => new
                {
                    ruleid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    studioid = table.Column<int>(type: "integer", nullable: true),
                    dayofweek = table.Column<int>(type: "integer", nullable: true),
                    starttime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    endtime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    multiplier = table.Column<decimal>(type: "numeric(3,2)", nullable: false, defaultValue: 1m),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pricingrules", x => x.ruleid);
                    table.ForeignKey(
                        name: "pricingrules_studioid_fkey",
                        column: x => x.studioid,
                        principalTable: "studios",
                        principalColumn: "studioid",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "studioequipment",
                columns: table => new
                {
                    studioid = table.Column<int>(type: "integer", nullable: false),
                    equipmentid = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_studioequipment", x => new { x.studioid, x.equipmentid });
                    table.ForeignKey(
                        name: "FK_studioequipment_equipment_equipmentid",
                        column: x => x.equipmentid,
                        principalTable: "equipment",
                        principalColumn: "equipmentid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_studioequipment_studios_studioid",
                        column: x => x.studioid,
                        principalTable: "studios",
                        principalColumn: "studioid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    bookingid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    studioid = table.Column<int>(type: "integer", nullable: false),
                    bookingdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    slotid = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    totalprice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    bookingcomment = table.Column<string>(type: "text", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    confirmedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelledat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.bookingid);
                    table.ForeignKey(
                        name: "bookings_slotid_fkey",
                        column: x => x.slotid,
                        principalTable: "timeslots",
                        principalColumn: "slotid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "bookings_studioid_fkey",
                        column: x => x.studioid,
                        principalTable: "studios",
                        principalColumn: "studioid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "bookings_userid_fkey",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "equipmentbookings",
                columns: table => new
                {
                    bookingid = table.Column<int>(type: "integer", nullable: false),
                    equipmentid = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipmentbookings", x => new { x.bookingid, x.equipmentid });
                    table.ForeignKey(
                        name: "equipmentbookings_bookingid_fkey",
                        column: x => x.bookingid,
                        principalTable: "bookings",
                        principalColumn: "bookingid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "equipmentbookings_equipmentid_fkey",
                        column: x => x.equipmentid,
                        principalTable: "equipment",
                        principalColumn: "equipmentid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "bookings_studioid_bookingdate_slotid_status_key",
                table: "bookings",
                columns: new[] { "studioid", "bookingdate", "slotid", "status" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bookings_slotid",
                table: "bookings",
                column: "slotid");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_userid",
                table: "bookings",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_equipmentbookings_equipmentid",
                table: "equipmentbookings",
                column: "equipmentid");

            migrationBuilder.CreateIndex(
                name: "IX_pricingrules_studioid",
                table: "pricingrules",
                column: "studioid");

            migrationBuilder.CreateIndex(
                name: "IX_roles_rolename",
                table: "roles",
                column: "rolename",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_studioequipment_equipmentid",
                table: "studioequipment",
                column: "equipmentid");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_roleid",
                table: "users",
                column: "roleid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "equipmentbookings");

            migrationBuilder.DropTable(
                name: "pricingrules");

            migrationBuilder.DropTable(
                name: "studioequipment");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "equipment");

            migrationBuilder.DropTable(
                name: "timeslots");

            migrationBuilder.DropTable(
                name: "studios");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
