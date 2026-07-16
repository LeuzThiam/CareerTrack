using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddImportedEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportedEmails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExternalMessageId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExternalThreadId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SenderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SenderEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ImportedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    MessageType = table.Column<int>(type: "int", nullable: false),
                    IsJobRelated = table.Column<bool>(type: "bit", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false),
                    RequiresReview = table.Column<bool>(type: "bit", nullable: false),
                    CompanyNameDetected = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    JobTitleDetected = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RecruiterNameDetected = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RecruiterEmailDetected = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    InterviewDateDetected = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    MeetingUrlDetected = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SuggestedAction = table.Column<int>(type: "int", nullable: true),
                    SuggestedApplicationStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ImportantExcerpt = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProcessingStatus = table.Column<int>(type: "int", nullable: false),
                    ProcessingError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RawPayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportedEmails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportedEmails_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ImportedEmails_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportedEmails_ApplicationId",
                table: "ImportedEmails",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportedEmails_UserId_ProcessingStatus",
                table: "ImportedEmails",
                columns: new[] { "UserId", "ProcessingStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportedEmails_UserId_Source_ExternalMessageId",
                table: "ImportedEmails",
                columns: new[] { "UserId", "Source", "ExternalMessageId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportedEmails");
        }
    }
}
