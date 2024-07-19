using System;
using System.IO;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp;
using static System.Net.Mime.MediaTypeNames;

namespace CCSWebKySearch.Models
{
    /// <summary>
    /// CCS PDF Utility class to image manipulation between formats
    /// </summary>
    internal class PDFUtility : IDisposable
    {
        /// <summary>
        /// Is this instance disposed?
        /// </summary>
        protected bool Disposed { get; private set; }

        #region IDisposable Interface Method Implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose worker method
        /// </summary>
        /// <param name="disposing">Are we disposing? 
        /// Otherwise we're finalizing.</param>
        protected virtual void Dispose(bool disposing)
        {
            Disposed = true;
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~PDFUtility()
        {
            Dispose(false);
        }
        #endregion



        //internal bool CCSTiff2PDF(string fileName)
        //{

        //    //Declare the return object
        //    bool aRET = false;

        //    try
        //    {
        //        //Create a new instance of the PDF document
        //        PdfDocument doc = new PdfDocument();

        //        //Get the page count
        //        int pageCount = getPageCount(fileName);

        //        //Iterate over the pages found in the TIFF file
        //        for (int i = 0; i < pageCount; i++)
        //        {
        //            try
        //            {
        //                //Create new page
        //                PdfPage page = new PdfPage();

        //                //Create the image from the tiff file
        //                System.Drawing.Image tiffImg = getTiffImage(fileName, i);

        //                var ms = new MemoryStream();
        //                tiffImg.Save(ms, ImageFormat.Png);

        //                // If you're going to read from the stream, you may need to reset the position to the start
        //                ms.Position = 0;

        //                //Convert to XImage
        //                XImage img = XImage.FromStream(ms);



        //                if (img != null)
        //                {
        //                    page.Width = img.PointWidth;
        //                    page.Height = img.PointHeight;
        //                    doc.Pages.Add(page);

        //                    XGraphics xgr = XGraphics.FromPdfPage(doc.Pages[i]);

        //                    xgr.DrawImage(img, 0, 0);
        //                }
        //            }
        //            catch (Exception ex)
        //            {

        //            }
        //        }

        //        //OK we are done with the pages now save the PDF file
        //        string strOutFile = String.Format("{0}\\{1}.pdf", Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
        //        doc.Save(strOutFile);
        //        doc.Close();

        //        //We reached here without issue
        //        aRET = true;
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //    //Return what we have
        //    return aRET;
        //}

        internal bool CCSTiff2PDF(string fileName)
        {
            bool aRET = false;

            try
            {
                // Create a new instance of the PDF document
                PdfDocument doc = new PdfDocument();

                // Get the page count
                int pageCount = getPageCount(fileName);
                Console.WriteLine($"Page count: {pageCount}");

                // Iterate over the pages found in the TIFF file
                for (int i = 0; i < pageCount; i++)
                {
                    try
                    {
                        // Create the image from the TIFF file
                        System.Drawing.Image tiffImg = getTiffImage(fileName, i);

                        if (tiffImg != null)
                        {
                            Console.WriteLine($"Processing page {i + 1} with dimensions: {tiffImg.Width}x{tiffImg.Height}");

                            // Create new page with dimensions matching the TIFF image
                            PdfPage page = new PdfPage
                            {
                                Width = tiffImg.Width * 72 / tiffImg.HorizontalResolution,
                                Height = tiffImg.Height * 72 / tiffImg.VerticalResolution
                            };
                            doc.Pages.Add(page);

                            using (XGraphics xgr = XGraphics.FromPdfPage(page))
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    // Save the TIFF image as JPEG format to the memory stream
                                    tiffImg.Save(ms, ImageFormat.Jpeg);
                                    ms.Position = 0;

                                    XImage img = XImage.FromStream(ms);
                                    xgr.DrawImage(img, 0, 0, page.Width, page.Height);
                                }
                            }

                            tiffImg.Dispose(); // Dispose the image after use
                        }
                        else
                        {
                            Console.WriteLine($"Error: Could not load image for page {i + 1}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log or handle the error
                        Console.WriteLine($"Error processing page {i + 1}: {ex.Message}");
                    }
                }

                // Save the PDF file
                string strOutFile = Path.Combine(Path.GetDirectoryName(fileName), $"{Path.GetFileNameWithoutExtension(fileName)}.pdf");
                doc.Save(strOutFile);
                doc.Close();

                aRET = true;
                Console.WriteLine($"Successfully created PDF: {strOutFile}");
            }
            catch (Exception ex)
            {
                // Log or handle the error
                Console.WriteLine($"Error creating PDF: {ex.Message}");
            }

            return aRET;
        }


        // Retrive PageCount of a multi-page tiff image
        int getPageCount(String fileName)
        {
            int pageCount = -1;
            try
            {
                System.Drawing.Image img = Bitmap.FromFile(fileName);
                pageCount = img.GetFrameCount(FrameDimension.Page);
                img.Dispose();

            }
            catch (Exception ex)
            {
                pageCount = 0;
            }
            return pageCount;
        }


        System.Drawing.Image getTiffImage(String sourceFile, int pageNumber)
        {
            System.Drawing.Image returnImage = null;

            try
            {
                System.Drawing.Image sourceIamge = Bitmap.FromFile(sourceFile);
                returnImage = getTiffImage(sourceIamge, pageNumber);
                sourceIamge.Dispose();
            }
            catch (Exception ex)
            {
                returnImage = null;
            }
            return returnImage;
        }

        System.Drawing.Image getTiffImage(System.Drawing.Image sourceImage, int pageNumber)
        {
            MemoryStream ms = null;
            System.Drawing.Image returnImage = null;

            try
            {
                ms = new MemoryStream();
                Guid objGuid = sourceImage.FrameDimensionsList[0];
                FrameDimension objDimension = new FrameDimension(objGuid);
                sourceImage.SelectActiveFrame(objDimension, pageNumber);
                sourceImage.Save(ms, ImageFormat.Tiff);
                returnImage = System.Drawing.Image.FromStream(ms);
            }
            catch (Exception ex)
            {
                returnImage = null;
            }
            return returnImage;
        }


    }
}
