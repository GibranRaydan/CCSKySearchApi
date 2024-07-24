using System.Text.RegularExpressions;

namespace CCSWebKySearch.Utils
{
    public static class CCSImageHelper
    {

        /// <summary>
        /// Pass in the name of the directory hosting the individual TIFF images and a merged.tiff file will be created
        /// </summary>
        /// <param name="dirName"></param>
        /// <returns></returns>
        public static bool MakePDFFile(string dirName)
        {
            //Declare the return object
            bool aRET = false;

            try
            {
                //Created the name of the merged TIFF file
                string strMergedFile = string.Format("{0}\\working\\{1}.tiff", dirName, "merged");
                string workingFolder = Path.Combine(dirName, "Working");
                Directory.CreateDirectory(workingFolder);
                //Merge the tif files together
                MergeSingleTIFFPages(dirName, strMergedFile);

                //Now convert the merged TIFF file into a PDF document
                using (PDFUtility pdfObject = new PDFUtility())
                {
                    aRET = pdfObject.CCSTiff2PDF(strMergedFile);
                }
            }
            catch (Exception ex)
            {

            }
            //Return what we have
            return aRET;
        }

        public static void MergeSingleTIFFPages(string dirName, string outFile)
        {
            try
            {
                TiffUtility x = new TiffUtility(dirName);

                //DirectoryInfo d = new DirectoryInfo(dirName);               //Set the source directory info object
                DirectoryInfo d = Directory.CreateDirectory(dirName);

                //JIRA 2332 need to do a padded sort
                FileInfo[] fis = d.GetFiles().Where(f => !f.Extension.Equals(".db", StringComparison.InvariantCultureIgnoreCase)).OrderBy(n => Regex.Replace(n.Name, @"\d+", z => z.Value.PadLeft(4, '0'))).ToArray();
                //FileInfo[] fis = d.GetFiles();

                string[] sourceFile = new string[fis.Count()];
                sourceFile = fis.Select(o => o.FullName).ToArray();
                //foreach (FileInfo fi in fis)
                //{
                //    sourceFile[counter] = fi.FullName;
                //    counter++;
                //}

                x.mergeTiffPages(sourceFile, outFile);
            }
            catch (Exception ex)
            {

            }
        }

    }
}
