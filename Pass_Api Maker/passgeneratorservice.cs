// ==========================================
// FILE: PassGeneratorService.cs
// LOCATION: Pass_Api_Maker/ (root of API project)
// ACTION: CREATE NEW FILE (or REPLACE if already exists)
// ==========================================

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Pass_Api_Maker.Controllers;
using static Pass_Api_Maker.Controllers.PassesController;
using Font = System.Drawing.Font;  // FIX for ambiguous Font reference

namespace Pass_Api_Maker
{
    public class PassGeneratorService
    {
        private const int PassWidth = 468;
        private const int PassHeight = 564;

        public static string GenerateApprovedPass(PassModel passData, string approvalSignatureImagePath)
        {
            try
            {
                using (Bitmap passBitmap = new Bitmap(PassWidth, PassHeight))
                using (Graphics g = Graphics.FromImage(passBitmap))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                    g.Clear(Color.White);

                    DrawDottedBorder(g);
                    DrawCIALLogo(g);
                    DrawUnderEscortBanner(g);
                    DrawHeader(g);
                    DrawEmployeePhoto(g, passData.LabourImageBase64);
                    DrawLeftSideDetails(g, passData);
                    DrawRightSideDetails(g, passData);
                    DrawValiditySection(g, passData);
                    DrawAccessGatesSection(g, passData);
                    DrawFooter(g);
                    DrawApprovalSignature(g, approvalSignatureImagePath);

                    string savedPath = SavePass(passBitmap, passData.LaborID);
                    return savedPath;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating pass: {ex.Message}", ex);
            }
        }

        private static void DrawDottedBorder(Graphics g)
        {
            using (Pen dottedPen = new Pen(Color.Black, 1))
            {
                dottedPen.DashStyle = DashStyle.Dot;
                g.DrawRectangle(dottedPen, 2, 2, PassWidth - 4, PassHeight - 4);
            }
        }

        private static void DrawCIALLogo(Graphics g)
        {
            using (SolidBrush logoBrush = new SolidBrush(Color.FromArgb(139, 195, 74)))
            using (Font logoFont = new Font("Arial", 18, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                g.FillRectangle(logoBrush, 10, 10, 75, 50);
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString("CIAL", logoFont, textBrush, new RectangleF(10, 10, 75, 50), sf);
            }
        }

        private static void DrawUnderEscortBanner(Graphics g)
        {
            using (SolidBrush redBrush = new SolidBrush(Color.FromArgb(211, 47, 47)))
            using (Font bannerFont = new Font("Arial", 10, FontStyle.Bold))
            using (SolidBrush whiteBrush = new SolidBrush(Color.White))
            {
                g.FillRectangle(redBrush, 0, 208, 30, 150);

                g.TranslateTransform(15, 283);
                g.RotateTransform(-90);
                g.DrawString("UNDER ESCORT", bannerFont, whiteBrush, 0, 0);
                g.ResetTransform();
            }
        }

        private static void DrawHeader(Graphics g)
        {
            using (Font headerFont = new Font("Arial", 18, FontStyle.Bold))
            using (SolidBrush blackBrush = new SolidBrush(Color.Black))
            {
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("AERODOME ENTRY PERMIT", headerFont, blackBrush, PassWidth / 2, 25, sf);
            }
        }

        private static void DrawEmployeePhoto(Graphics g, string base64Image)
        {
            int photoX = 186;
            int photoY = 68;
            int photoWidth = 112;
            int photoHeight = 160;

            using (Pen photoBorder = new Pen(Color.Black, 2))
            {
                g.DrawRectangle(photoBorder, photoX, photoY, photoWidth, photoHeight);
            }

            if (!string.IsNullOrEmpty(base64Image))
            {
                try
                {
                    byte[] imageBytes = Convert.FromBase64String(base64Image);
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    using (Image photo = Image.FromStream(ms))
                    {
                        g.DrawImage(photo, photoX + 2, photoY + 2, photoWidth - 4, photoHeight - 4);
                    }
                }
                catch
                {
                    DrawPhotoPlaceholder(g, photoX, photoY, photoWidth, photoHeight);
                }
            }
            else
            {
                DrawPhotoPlaceholder(g, photoX, photoY, photoWidth, photoHeight);
            }
        }

        private static void DrawPhotoPlaceholder(Graphics g, int x, int y, int width, int height)
        {
            using (SolidBrush placeholderBrush = new SolidBrush(Color.LightGray))
            {
                g.FillRectangle(placeholderBrush, x + 2, y + 2, width - 4, height - 4);
            }
        }

        private static void DrawLeftSideDetails(Graphics g, PassModel passData)
        {
            using (Font labelFont = new Font("Arial", 9, FontStyle.Bold))
            using (Font valueFont = new Font("Arial", 9, FontStyle.Regular))
            using (SolidBrush blackBrush = new SolidBrush(Color.Black))
            {
                g.DrawString("Labour Id:", labelFont, blackBrush, 38, 257);

                using (Pen boxPen = new Pen(Color.Black, 1))
                {
                    g.DrawRectangle(boxPen, 112, 253, 115, 20);
                    g.DrawString(passData.LaborID ?? "", valueFont, blackBrush, 115, 256);
                }

                g.DrawString("Contractor:", labelFont, blackBrush, 75, 291);
                g.DrawString(passData.ContractorName ?? "", valueFont, blackBrush, 145, 291);
            }
        }

        private static void DrawRightSideDetails(Graphics g, PassModel passData)
        {
            using (Font labelFont = new Font("Arial", 9, FontStyle.Bold))
            using (Font valueFont = new Font("Arial", 9, FontStyle.Regular))
            using (SolidBrush blackBrush = new SolidBrush(Color.Black))
            {
                g.DrawString("DOB:", labelFont, blackBrush, 271, 257);
                g.DrawString(passData.DOB ?? "", valueFont, blackBrush, 305, 257);
            }
        }

        private static void DrawValiditySection(Graphics g, PassModel passData)
        {
            using (Font sectionFont = new Font("Arial", 11, FontStyle.Bold))
            using (Font labelFont = new Font("Arial", 9, FontStyle.Regular))
            using (Font valueFont = new Font("Arial", 9, FontStyle.Regular))
            using (SolidBrush blackBrush = new SolidBrush(Color.Black))
            {
                g.DrawString("Validity", sectionFont, blackBrush, 12, 383);

                g.DrawString("From:", labelFont, blackBrush, 17, 418);
                g.DrawString($"{passData.EntryDate} {passData.EntryTime}", valueFont, blackBrush, 55, 418);

                g.DrawString("To:", labelFont, blackBrush, 17, 457);
                g.DrawString($"{passData.ExitDate} {passData.CheckoutTime}", valueFont, blackBrush, 42, 457);
            }
        }

        private static void DrawAccessGatesSection(Graphics g, PassModel passData)
        {
            using (Font sectionFont = new Font("Arial", 11, FontStyle.Bold))
            using (Font labelFont = new Font("Arial", 9, FontStyle.Regular))
            using (Font valueFont = new Font("Arial", 9, FontStyle.Regular))
            using (SolidBrush blackBrush = new SolidBrush(Color.Black))
            {
                g.DrawString("Access Gates:", sectionFont, blackBrush, 271, 383);

                g.DrawString("Areas:", labelFont, blackBrush, 271, 418);
                g.DrawString(passData.Area ?? "", valueFont, blackBrush, 315, 418);

                g.DrawString("Gates:", labelFont, blackBrush, 271, 454);
                g.DrawString(passData.GateAccess ?? "", valueFont, blackBrush, 315, 454);
            }
        }

        private static void DrawFooter(Graphics g)
        {
            using (SolidBrush blueBrush = new SolidBrush(Color.FromArgb(63, 81, 181)))
            using (Font footerFont = new Font("Arial", 16, FontStyle.Bold))
            using (SolidBrush whiteBrush = new SolidBrush(Color.White))
            {
                g.FillRectangle(blueBrush, 0, 520, PassWidth, 44);

                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString("TEMPORARY PASS", footerFont, whiteBrush,
                    new RectangleF(0, 520, PassWidth, 44), sf);
            }
        }

        private static void DrawApprovalSignature(Graphics g, string signatureImagePath)
        {
            int signatureWidth = 200;
            int signatureHeight = 80;
            int signatureX = PassWidth - signatureWidth - 10;
            int signatureY = PassHeight - signatureHeight - 50;

            if (!string.IsNullOrEmpty(signatureImagePath) && File.Exists(signatureImagePath))
            {
                try
                {
                    using (Image signatureImage = Image.FromFile(signatureImagePath))
                    {
                        g.DrawImage(signatureImage, signatureX, signatureY, signatureWidth, signatureHeight);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading signature: {ex.Message}");
                }
            }
        }

        private static string SavePass(Bitmap passBitmap, string laborId)
        {
            string passesFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "CIAL_Entry_Passes"
            );

            Directory.CreateDirectory(passesFolder);

            string fileName = $"Pass_{laborId}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string fullPath = Path.Combine(passesFolder, fileName);

            passBitmap.Save(fullPath, ImageFormat.Png);

            return fullPath;
        }
    }
}