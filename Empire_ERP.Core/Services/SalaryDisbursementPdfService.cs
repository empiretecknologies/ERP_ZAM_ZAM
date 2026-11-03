using Empire_ERP.Core.Entities;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Globalization;
using System.IO;
using Table = iText.Layout.Element.Table;

namespace Empire_ERP.Core.Services
{
    public class SalaryDisbursementPdfService
    {
        // #055a87
        private static readonly DeviceRgb PrimaryBlue = new DeviceRgb(5, 90, 135);
        private static readonly DeviceRgb LightBlue = new DeviceRgb(229, 241, 247);
        private static readonly DeviceRgb SoftBlue = new DeviceRgb(168, 204, 220);
        private static readonly DeviceRgb ValueBlue = new DeviceRgb(5, 90, 135);
        private static readonly DeviceRgb LabelGray = new DeviceRgb(84, 110, 122);
        private static readonly DeviceRgb AccentCyan = new DeviceRgb(79, 172, 204);

        public static void GeneratePayslip(SalaryDisbursementPayslip model, string filePath, string webRootPath)
        {
            var folder = System.IO.Path.GetDirectoryName(filePath);
            if (!String.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            var doc = new Document(pdf, PageSize.A4);
            doc.SetMargins(16, 18, 30, 18);

            var normal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            AddHeader(doc, pdf, model, webRootPath, normal, bold);
            AddMetaBar(doc, model, normal, bold);
            AddSectionHeader(doc, "01. EMPLOYEE PROFILE", bold);
            AddEmployeeProfile(doc, pdf, model, normal, bold);
            AddTwoColumnMiddle(doc, model, normal, bold);
            AddAuthorization(doc, model, normal, bold);
            AddFooter(pdf, model, bold);

            doc.Close();
        }

        private static void AddHeader(Document doc, PdfDocument pdf, SalaryDisbursementPayslip model, string webRootPath, PdfFont normal, PdfFont bold)
        {
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 12, 40, 48 }))
                .UseAllAvailableWidth()
                .SetMarginBottom(8);

            var logoCell = new Cell().SetBorder(Border.NO_BORDER).SetPadding(2).SetVerticalAlignment(VerticalAlignment.MIDDLE);
            if (!String.IsNullOrWhiteSpace(model.COMPANY_LOGO))
            {
                string logoPath = System.IO.Path.Combine(webRootPath, "Client", "Company", model.COMPANY_LOGO.TrimStart('/', '\\'));
                if (File.Exists(logoPath))
                {
                    logoCell.Add(new Image(ImageDataFactory.Create(logoPath)).ScaleToFit(48, 40));
                }
            }
            table.AddCell(logoCell);

            var titleCell = new Cell().SetBorder(Border.NO_BORDER).SetPaddingLeft(5).SetVerticalAlignment(VerticalAlignment.MIDDLE);
            titleCell.Add(new Paragraph(model.COMPANY_NAME ?? "")
                .SetFont(bold).SetFontSize(13).SetFontColor(PrimaryBlue).SetMargin(0).SetMultipliedLeading(1.05f));
            titleCell.Add(new Paragraph(model.COMPANY_TAGLINE ?? "Better People | Better Performance | A Stronger Tomorrow")
                .SetFont(normal).SetFontSize(7).SetFontColor(LabelGray).SetMarginTop(2).SetMarginBottom(0).SetMultipliedLeading(1.05f));
            table.AddCell(titleCell);

            var bannerCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetTextAlignment(TextAlignment.RIGHT);
            bannerCell.Add(BuildHeaderTitleBanner(pdf, normal, bold));
            table.AddCell(bannerCell);

            doc.Add(table);
        }

        private static Div BuildHeaderTitleBanner(PdfDocument pdf, PdfFont normal, PdfFont bold)
        {
            var banner = new Div()
                .SetBackgroundColor(PrimaryBlue)
                .SetPaddingTop(7)
                .SetPaddingBottom(7)
                .SetPaddingLeft(12)
                .SetPaddingRight(8)
                .SetWidth(UnitValue.CreatePercentValue(100));

            var textRow = new Table(UnitValue.CreatePercentArray(new float[] { 88, 12 }))
                .UseAllAvailableWidth();

            var textCell = new Cell().SetBorder(Border.NO_BORDER).SetPadding(0).SetVerticalAlignment(VerticalAlignment.MIDDLE);
            textCell.Add(new Paragraph("HUMAN RESOURCE MANAGEMENT")
                .SetFont(normal).SetFontSize(7).SetFontColor(ColorConstants.WHITE).SetMargin(0).SetMultipliedLeading(1.05f));
            textCell.Add(new Paragraph()
                .SetMarginTop(3).SetMarginBottom(3)
                .SetBorderBottom(new SolidBorder(AccentCyan, 1.1f))
                .SetWidth(UnitValue.CreatePercentValue(92))
                .Add(new Text(" ")));
            textCell.Add(new Paragraph("SALARY DISBURSEMENT STATEMENT")
                .SetFont(bold).SetFontSize(9).SetFontColor(ColorConstants.WHITE).SetMargin(0).SetMultipliedLeading(1.05f));
            textRow.AddCell(textCell);

            var iconCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
            iconCell.Add(CreateUsersIcon(pdf).SetHorizontalAlignment(HorizontalAlignment.RIGHT));
            textRow.AddCell(iconCell);

            banner.Add(textRow);
            return banner;
        }

        private static void AddMetaBar(Document doc, SalaryDisbursementPayslip model, PdfFont normal, PdfFont bold)
        {
            var bar = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .UseAllAvailableWidth()
                .SetBackgroundColor(LightBlue)
                .SetMarginBottom(10);

            var left = new Cell().SetBorder(Border.NO_BORDER).SetPadding(8);
            left.Add(KeyValueParagraph("Statement No", model.VOUCHER_NO, normal, bold));
            left.Add(KeyValueParagraph("Payroll Period", model.PAYROLL_PERIOD, normal, bold));
            bar.AddCell(left);

            var right = new Cell().SetBorder(Border.NO_BORDER).SetPadding(8);
            right.Add(KeyValueParagraph("Payment Date", model.V_DATE?.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture), normal, bold));
            right.Add(KeyValueParagraph("Payroll Month", model.PAYROLL_MONTH_TEXT, normal, bold));
            bar.AddCell(right);

            doc.Add(bar);
        }

        private static void AddSectionHeader(Document doc, string title, PdfFont bold)
        {
            var header = new Div()
                .SetBackgroundColor(PrimaryBlue)
                .SetPaddingTop(6)
                .SetPaddingBottom(6)
                .SetPaddingLeft(8)
                .SetPaddingRight(8)
                .SetMarginTop(2)
                .SetMarginBottom(0);
            header.Add(new Paragraph(title)
                .SetFont(bold).SetFontSize(8.5f).SetFontColor(ColorConstants.WHITE).SetMargin(0).SetMultipliedLeading(1.05f));
            doc.Add(header);
        }

        private static void AddEmployeeProfile(Document doc, PdfDocument pdf, SalaryDisbursementPayslip model, PdfFont normal, PdfFont bold)
        {
            var outer = new Table(UnitValue.CreatePercentArray(new float[] { 76, 24 }))
                .UseAllAvailableWidth()
                .SetBorder(new SolidBorder(SoftBlue, 0.8f))
                .SetMarginBottom(10);

            var details = new Table(UnitValue.CreatePercentArray(new float[] { 32, 68 }))
                .UseAllAvailableWidth();
            AddProfileRow(details, "Employee Code", model.EMPLOYEE_CODE, normal, bold);
            AddProfileRow(details, "Employee Name", model.EMP_NAME, normal, bold);
            AddProfileRow(details, "Department", model.DEPARTMENT, normal, bold);
            AddProfileRow(details, "Designation", model.DESIGNATION, normal, bold);
            AddProfileRow(details, "Payment Mode", model.PAYMENT_MODE, normal, bold);

            outer.AddCell(new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(6)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(details));

            var mottoCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetBorderLeft(new SolidBorder(SoftBlue, 0.7f))
                .SetPadding(8)
                .SetBackgroundColor(LightBlue)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetTextAlignment(TextAlignment.CENTER);
            mottoCell.Add(BuildHardWorkWidget(pdf, bold));
            outer.AddCell(mottoCell);

            doc.Add(outer);
        }

        private static Div BuildHardWorkWidget(PdfDocument pdf, PdfFont bold)
        {
            var widget = new Div().SetTextAlignment(TextAlignment.CENTER);
            widget.Add(CreateIdCardIcon(pdf).SetHorizontalAlignment(HorizontalAlignment.CENTER).SetMarginBottom(5));
            widget.Add(new Paragraph("Your Hard Work")
                .SetFont(bold).SetFontSize(8).SetFontColor(PrimaryBlue)
                .SetTextAlignment(TextAlignment.CENTER).SetMargin(0).SetMultipliedLeading(1.1f));
            widget.Add(new Paragraph("Builds Our Success")
                .SetFont(bold).SetFontSize(8).SetFontColor(PrimaryBlue)
                .SetTextAlignment(TextAlignment.CENTER).SetMargin(0).SetMultipliedLeading(1.1f));
            widget.Add(new Paragraph()
                .SetMarginTop(5)
                .SetBorderBottom(new SolidBorder(PrimaryBlue, 1.1f))
                .SetWidth(70)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                .Add(new Text(" ")));
            return widget;
        }

        private static Image CreateIdCardIcon(PdfDocument pdf)
        {
            var form = new PdfFormXObject(new Rectangle(0, 0, 34, 30));
            var canvas = new PdfCanvas(form, pdf);
            canvas.SetStrokeColor(PrimaryBlue);
            canvas.SetFillColor(PrimaryBlue);
            canvas.SetLineWidth(1.3f);
            canvas.RoundRectangle(8, 4, 22, 20, 1.5f);
            canvas.Stroke();
            canvas.RoundRectangle(3, 2, 22, 20, 1.5f);
            canvas.Stroke();
            canvas.Circle(14, 15.5f, 3.2f);
            canvas.Stroke();
            canvas.MoveTo(8.5f, 7.5f);
            canvas.CurveTo(10f, 11.5f, 18f, 11.5f, 19.5f, 7.5f);
            canvas.Stroke();
            return new Image(form).SetWidth(28).SetHeight(24);
        }

        private static Image CreateUsersIcon(PdfDocument pdf)
        {
            var form = new PdfFormXObject(new Rectangle(0, 0, 34, 28));
            var canvas = new PdfCanvas(form, pdf);
            canvas.SetStrokeColor(ColorConstants.WHITE);
            canvas.SetFillColor(ColorConstants.WHITE);
            canvas.SetLineWidth(1.1f);
            canvas.Circle(9, 18, 3.2f);
            canvas.Stroke();
            canvas.MoveTo(4, 8);
            canvas.CurveTo(5.5f, 13, 12.5f, 13, 14, 8);
            canvas.Stroke();
            canvas.Circle(25, 18, 3.2f);
            canvas.Stroke();
            canvas.MoveTo(20, 8);
            canvas.CurveTo(21.5f, 13, 28.5f, 13, 30, 8);
            canvas.Stroke();
            canvas.Circle(17, 19, 3.8f);
            canvas.FillStroke();
            canvas.MoveTo(10.5f, 7);
            canvas.CurveTo(12.5f, 14, 21.5f, 14, 23.5f, 7);
            canvas.FillStroke();
            return new Image(form).SetWidth(24).SetHeight(20);
        }

        private static void AddTwoColumnMiddle(Document doc, SalaryDisbursementPayslip model, PdfFont normal, PdfFont bold)
        {
            var outer = new Table(UnitValue.CreatePercentArray(new float[] { 48, 4, 48 }))
                .UseAllAvailableWidth()
                .SetMarginBottom(10);

            var leftWrap = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetVerticalAlignment(VerticalAlignment.TOP);
            leftWrap.Add(SectionTitleDiv("02. ATTENDANCE & WORKING SUMMARY", bold));
            leftWrap.Add(AttendanceTable(model, normal, bold));
            leftWrap.Add(NetPayableBlock(model, normal, bold));
            outer.AddCell(leftWrap);

            outer.AddCell(new Cell().SetBorder(Border.NO_BORDER));

            var rightWrap = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetVerticalAlignment(VerticalAlignment.TOP);
            rightWrap.Add(SectionTitleDiv("03. SALARY & DEDUCTION DETAILS", bold));
            rightWrap.Add(SalaryTable(model, normal, bold));
            outer.AddCell(rightWrap);

            doc.Add(outer);
        }

        private static Div SectionTitleDiv(string title, PdfFont bold)
        {
            return new Div()
                .SetBackgroundColor(PrimaryBlue)
                .SetPaddingTop(5)
                .SetPaddingBottom(5)
                .SetPaddingLeft(7)
                .SetPaddingRight(7)
                .Add(new Paragraph(title).SetFont(bold).SetFontSize(7.5f).SetFontColor(ColorConstants.WHITE).SetMargin(0).SetMultipliedLeading(1.05f));
        }

        private static Table AttendanceTable(SalaryDisbursementPayslip model, PdfFont normal, PdfFont bold)
        {
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 65, 35 }))
                .UseAllAvailableWidth()
                .SetBorder(new SolidBorder(SoftBlue, 0.8f));

            table.AddHeaderCell(HeaderCell("Attendance Particulars", bold));
            table.AddHeaderCell(HeaderCell("Quantity", bold));

            AddQtyRow(table, "Total Working Days", FormatDays(model.WORKING_DAYS), normal, bold);
            AddQtyRow(table, "Present Days", FormatDays(model.PRESENT_DAYS), normal, bold);
            AddQtyRow(table, "Weekend / Weekly Off", FormatDays(model.SUNDAY_DAYS), normal, bold);
            AddQtyRow(table, "Absent Days", FormatDays(model.ABSENT_DAYS), normal, bold);
            AddQtyRow(table, "Total Late Hours", (model.TOTAL_LATE_HOURS ?? "0") + " Hours", normal, bold);
            AddQtyRow(table, "Average Time-In", String.IsNullOrWhiteSpace(model.AVG_IN_TIME) ? "-" : model.AVG_IN_TIME, normal, bold);
            return table;
        }

        private static Table SalaryTable(SalaryDisbursementPayslip model, PdfFont normal, PdfFont bold)
        {
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 65, 35 }))
                .UseAllAvailableWidth()
                .SetBorder(new SolidBorder(SoftBlue, 0.8f));

            table.AddHeaderCell(HeaderCell("Salary Particulars", bold));
            table.AddHeaderCell(HeaderCell("Amount (PKR)", bold));

            AddAmountRow(table, "Basic Salary", model.BASIC_SALARY, normal, bold, false);
            AddAmountRow(table, "House Allowance", model.HOUSE_ALLOWANCE, normal, bold, false);
            AddAmountRow(table, "Medical Allowance", model.MEDICAL_ALLOWANCE, normal, bold, false);
            AddAmountRow(table, "Conveyance Allowance", model.CONVEYANCE_ALLOWANCE, normal, bold, false);
            if ((model.OVERTIME ?? 0) != 0) AddAmountRow(table, "Overtime", model.OVERTIME, normal, bold, false);
            if ((model.BONUS ?? 0) != 0) AddAmountRow(table, "Bonus", model.BONUS, normal, bold, false);
            AddAmountRow(table, "Gross Salary", model.GROSS_SALARY, normal, bold, true);
            AddAmountRow(table, "Per Day Salary", model.PER_DAY_SALARY, normal, bold, false);
            AddAmountRow(table, "Absent Deduction", model.ABSENT_DEDUCTION, normal, bold, false);
            AddAmountRow(table, "Total Advance", model.ADVANCE, normal, bold, false);
            AddAmountRow(table, "Loan Installment", model.LOAN, normal, bold, false);
            if ((model.INCOME_TAX ?? 0) != 0) AddAmountRow(table, "Income Tax", model.INCOME_TAX, normal, bold, false);
            AddAmountRow(table, "Total Deductions", model.TOTAL_DEDUCTIONS, normal, bold, true);

            var breakdown = new Cell(1, 2).SetBorder(Border.NO_BORDER).SetBackgroundColor(LightBlue).SetPadding(7);
            breakdown.Add(new Paragraph("Amount Breakdown").SetFont(bold).SetFontSize(7.5f).SetFontColor(PrimaryBlue).SetMarginBottom(4).SetMultipliedLeading(1.05f));
            breakdown.Add(MiniLine("Gross Salary", FormatAmount(model.GROSS_SALARY), normal, bold, false));
            breakdown.Add(MiniLine("Less: Total Deductions", FormatAmount(model.TOTAL_DEDUCTIONS), normal, bold, false));
            breakdown.Add(MiniLine("Net Salary Payable", FormatAmount(model.NET_SALARY), normal, bold, true));
            table.AddCell(breakdown);

            return table;
        }

        private static Div NetPayableBlock(SalaryDisbursementPayslip model, PdfFont normal, PdfFont bold)
        {
            var wrap = new Div().SetMarginTop(10);
            wrap.Add(SectionTitleDiv("04. NET SALARY PAYABLE", bold));

            var box = new Div()
                .SetBorder(new SolidBorder(SoftBlue, 0.8f))
                .SetBackgroundColor(LightBlue)
                .SetPaddingTop(14)
                .SetPaddingBottom(14)
                .SetPaddingLeft(10)
                .SetPaddingRight(10)
                .SetMinHeight(95);
            box.Add(new Paragraph("Net Salary Payable")
                .SetFont(normal).SetFontSize(8).SetFontColor(LabelGray).SetMargin(0).SetMultipliedLeading(1.05f));
            box.Add(new Paragraph("PKR  " + FormatAmount(model.NET_SALARY))
                .SetFont(bold).SetFontSize(16).SetFontColor(PrimaryBlue).SetMarginTop(6).SetMarginBottom(6).SetMultipliedLeading(1.05f));
            box.Add(new Paragraph(model.NET_SALARY_WORDS ?? "")
                .SetFont(normal).SetFontSize(7).SetItalic().SetFontColor(LabelGray).SetMargin(0).SetMultipliedLeading(1.1f));
            wrap.Add(box);
            return wrap;
        }

        private static void AddAuthorization(Document doc, SalaryDisbursementPayslip model, PdfFont normal, PdfFont bold)
        {
            AddSectionHeader(doc, "05. AUTHORIZATION & ACKNOWLEDGEMENT", bold);
            var box = new Div()
                .SetBorder(new SolidBorder(SoftBlue, 0.8f))
                .SetPaddingTop(10)
                .SetPaddingBottom(10)
                .SetPaddingLeft(8)
                .SetPaddingRight(8)
                .SetMarginBottom(2);
            box.Add(new Paragraph("I hereby confirm that the above salary details have been reviewed and the payable amount has been received by me.")
                .SetFont(normal).SetFontSize(7.5f).SetFontColor(LabelGray).SetMarginBottom(14).SetMultipliedLeading(1.15f));

            string dateText = model.V_DATE?.ToString("dd-MM-yyyy") ?? DateTime.Now.ToString("dd-MM-yyyy");
            var sigTable = new Table(UnitValue.CreatePercentArray(new float[] { 33, 34, 33 }))
                .UseAllAvailableWidth();
            sigTable.AddCell(SignatureCell("PREPARED BY", "HR Officer", dateText, normal, bold));
            sigTable.AddCell(SignatureCell("CHECKED BY", "Accounts Manager", dateText, normal, bold));
            sigTable.AddCell(SignatureCell("APPROVED BY", "Authorized Signatory", dateText, normal, bold));
            box.Add(sigTable);

            box.Add(new Paragraph("EMPLOYEE ACKNOWLEDGEMENT")
                .SetFont(bold).SetFontSize(8.5f).SetFontColor(PrimaryBlue).SetMarginTop(16).SetMarginBottom(6).SetMultipliedLeading(1.05f));
            var empAck = new Table(UnitValue.CreatePercentArray(new float[] { 40, 35, 25 }))
                .UseAllAvailableWidth();
            empAck.AddCell(AckCell("Employee Name", model.EMP_NAME, normal, bold));
            empAck.AddCell(AckCell("Employee Signature", "____________________", normal, bold));
            empAck.AddCell(AckCell("Date", dateText, normal, bold));
            box.Add(empAck);

            box.Add(new Paragraph("Together We Grow")
                .SetFont(bold).SetFontSize(8.5f).SetFontColor(PrimaryBlue).SetItalic()
                .SetTextAlignment(TextAlignment.RIGHT).SetMarginTop(12).SetMarginBottom(2));
            doc.Add(box);
        }

        private static void AddFooter(PdfDocument pdf, SalaryDisbursementPayslip model, PdfFont bold)
        {
            var page = pdf.GetLastPage();
            var pageSize = page.GetPageSize();
            var canvas = new PdfCanvas(page);
            canvas.SaveState();
            canvas.SetFillColor(PrimaryBlue);
            canvas.Rectangle(0, 0, pageSize.GetWidth(), 24);
            canvas.Fill();
            canvas.RestoreState();

            var footerCanvas = new Canvas(canvas, pageSize);
            footerCanvas.Add(new Paragraph((model.COMPANY_NAME ?? "") + "     |     HRM – Salary Disbursement     |     Confidential Document")
                .SetFont(bold)
                .SetFontSize(7)
                .SetFontColor(ColorConstants.WHITE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFixedPosition(0, 7, pageSize.GetWidth()));
            footerCanvas.Close();
        }

        private static Cell SignatureCell(string title, string role, string date, PdfFont normal, PdfFont bold)
        {
            var cell = new Cell().SetBorder(Border.NO_BORDER).SetPadding(4);
            cell.Add(new Paragraph(title).SetFont(bold).SetFontSize(8).SetFontColor(PrimaryBlue).SetMargin(0).SetMultipliedLeading(1.05f));
            cell.Add(new Paragraph(role).SetFont(normal).SetFontSize(7).SetFontColor(LabelGray).SetMarginBottom(26).SetMultipliedLeading(1.05f));
            cell.Add(new Paragraph("____________________").SetFont(normal).SetFontSize(8).SetMargin(0).SetMultipliedLeading(1.05f));
            cell.Add(new Paragraph("Date: " + date).SetFont(normal).SetFontSize(7).SetFontColor(LabelGray).SetMarginTop(3).SetMultipliedLeading(1.05f));
            return cell;
        }

        private static Cell AckCell(string label, string value, PdfFont normal, PdfFont bold)
        {
            var cell = new Cell().SetBorder(Border.NO_BORDER).SetPadding(3);
            cell.Add(new Paragraph(label).SetFont(normal).SetFontSize(7).SetFontColor(LabelGray).SetMargin(0).SetMultipliedLeading(1.05f));
            cell.Add(new Paragraph(value ?? "").SetFont(bold).SetFontSize(8).SetFontColor(ValueBlue).SetMarginTop(3).SetMultipliedLeading(1.05f));
            return cell;
        }

        private static Cell HeaderCell(string text, PdfFont bold)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(bold).SetFontSize(7.5f).SetFontColor(PrimaryBlue).SetMargin(0).SetMultipliedLeading(1.05f))
                .SetBackgroundColor(LightBlue)
                .SetPadding(5)
                .SetBorder(new SolidBorder(SoftBlue, 0.4f));
        }

        private static void AddProfileRow(Table table, string label, string? value, PdfFont normal, PdfFont bold)
        {
            table.AddCell(new Cell().Add(new Paragraph(label).SetFont(normal).SetFontSize(8).SetFontColor(LabelGray).SetMargin(0).SetMultipliedLeading(1.1f))
                .SetBorder(Border.NO_BORDER).SetPaddingTop(4).SetPaddingBottom(4).SetPaddingLeft(5).SetPaddingRight(5));
            table.AddCell(new Cell().Add(new Paragraph(value ?? "-").SetFont(bold).SetFontSize(8.5f).SetFontColor(ValueBlue).SetMargin(0).SetMultipliedLeading(1.1f))
                .SetBorder(Border.NO_BORDER).SetPaddingTop(4).SetPaddingBottom(4).SetPaddingLeft(5).SetPaddingRight(5));
        }

        private static void AddQtyRow(Table table, string label, string value, PdfFont normal, PdfFont bold)
        {
            table.AddCell(new Cell().Add(new Paragraph(label).SetFont(normal).SetFontSize(8).SetFontColor(LabelGray).SetMargin(0).SetMultipliedLeading(1.1f))
                .SetPaddingTop(5).SetPaddingBottom(5).SetPaddingLeft(5).SetPaddingRight(5).SetBorder(new SolidBorder(SoftBlue, 0.35f)));
            table.AddCell(new Cell().Add(new Paragraph(value).SetFont(bold).SetFontSize(8).SetFontColor(ValueBlue).SetMargin(0).SetMultipliedLeading(1.1f))
                .SetTextAlignment(TextAlignment.RIGHT).SetPaddingTop(5).SetPaddingBottom(5).SetPaddingLeft(5).SetPaddingRight(5).SetBorder(new SolidBorder(SoftBlue, 0.35f)));
        }

        private static void AddAmountRow(Table table, string label, double? amount, PdfFont normal, PdfFont bold, bool highlight)
        {
            var labelCell = new Cell().Add(new Paragraph(label).SetFont(highlight ? bold : normal).SetFontSize(7.5f)
                .SetFontColor(highlight ? PrimaryBlue : LabelGray).SetMargin(0).SetMultipliedLeading(1.05f))
                .SetPaddingTop(4).SetPaddingBottom(4).SetPaddingLeft(5).SetPaddingRight(5).SetBorder(new SolidBorder(SoftBlue, 0.35f));
            var valueCell = new Cell().Add(new Paragraph(FormatAmount(amount)).SetFont(highlight ? bold : normal).SetFontSize(7.5f)
                .SetFontColor(highlight ? PrimaryBlue : ValueBlue).SetMargin(0).SetMultipliedLeading(1.05f))
                .SetTextAlignment(TextAlignment.RIGHT).SetPaddingTop(4).SetPaddingBottom(4).SetPaddingLeft(5).SetPaddingRight(5).SetBorder(new SolidBorder(SoftBlue, 0.35f));
            if (highlight)
            {
                labelCell.SetBackgroundColor(LightBlue);
                valueCell.SetBackgroundColor(LightBlue);
            }
            table.AddCell(labelCell);
            table.AddCell(valueCell);
        }

        private static Paragraph MiniLine(string label, string value, PdfFont normal, PdfFont bold, bool strong)
        {
            return new Paragraph(label + " :  " + value)
                .SetFont(strong ? bold : normal)
                .SetFontSize(7.5f)
                .SetFontColor(strong ? PrimaryBlue : LabelGray)
                .SetMarginBottom(2)
                .SetMultipliedLeading(1.05f);
        }

        private static Paragraph KeyValueParagraph(string label, string? value, PdfFont normal, PdfFont bold)
        {
            return new Paragraph()
                .Add(new Text(label + ": ").SetFont(bold).SetFontSize(7.5f).SetFontColor(LabelGray))
                .Add(new Text(value ?? "-").SetFont(bold).SetFontSize(8.5f).SetFontColor(ValueBlue))
                .SetMarginBottom(3)
                .SetMultipliedLeading(1.1f);
        }

        private static string FormatAmount(double? value)
        {
            return (value ?? 0).ToString("N2", CultureInfo.InvariantCulture);
        }

        private static string FormatDays(double? value)
        {
            return (value ?? 0).ToString("0") + " Days";
        }
    }
}
