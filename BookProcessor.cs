using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace JisuiBatch
{
    public class BookProcessor
    {
        private AppSettings AppSettings { get; }
        private string Title { get; }
        private string RootPath { get; }
        private string BookPath { get; }
        private string WorkPath { get; }
        private string ColorImagePath { get; }
        private string GrayImagePath { get; }
        private string TextImagePath { get; }
        private string ProcessedImagePath { get; }

        public BookProcessor(AppSettings appSettings, string title)
        {
            AppSettings = appSettings;
            Title = title;
            RootPath = AppSettings.WorkingDirectory.Root;
            BookPath = Path.Combine(RootPath, Title);
            ColorImagePath = Path.Combine(BookPath, AppSettings.WorkingDirectory.ColorImage);
            GrayImagePath = Path.Combine(BookPath, AppSettings.WorkingDirectory.GrayImage);
            TextImagePath = Path.Combine(BookPath, AppSettings.WorkingDirectory.TextImage);
            ProcessedImagePath = Path.Combine(BookPath, AppSettings.WorkingDirectory.ProcessedImage);
        }

        public void Execute()
        {
            var sw = Stopwatch.StartNew();

            CheckDirectoryExists();

            ProcessColorImages();

            ProcessGrayImages();

            ProcessTextImages();

            ArchiveImages();

            hoge();

            Console.WriteLine("■ " + Title + " " + sw.Elapsed);
        }

        private void CheckDirectoryExists()
        {
            if (!Directory.Exists(ColorImagePath))
            {
                throw new DirectoryNotFoundException(ColorImagePath);
            }

            if (!Directory.Exists(GrayImagePath))
            {
                throw new DirectoryNotFoundException(GrayImagePath);
            }

            if (!Directory.Exists(TextImagePath))
            {
                throw new DirectoryNotFoundException(TextImagePath);
            }

            if (!Directory.Exists(ProcessedImagePath))
            {
                Directory.CreateDirectory(ProcessedImagePath);
            }
        }

        private void ProcessColorImages()
        {
            Directory.GetFiles(ColorImagePath)
                .AsParallel()
                .WithDegreeOfParallelism(AppSettings.DegreeOfParallelism)
                .ForAll(file => ProcessColorImage(file));
        }

        private void ProcessColorImage(string file)
        {
            var sw = Stopwatch.StartNew();

            Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " start: " + file);

            var target = Path.Combine(ProcessedImagePath, Path.GetFileName(file));
            ExecuteJpegtranOptimizeOnly(file, target);

            Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " end  : " + file + " : " + sw.ElapsedMilliseconds);
        }

        private void ExecuteJpegtranOptimizeOnly(string source, string target)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = Path.Combine(AppSettings.UtilityDirectory.Mozjpeg, "jpegtran.exe"),
                    Arguments = $@"-copy none -optimize -outfile ""{target}"" ""{source}""",
                    RedirectStandardOutput = true,
                }
            };

            process.Start();
            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }

        private void ProcessGrayImages()
        {
            Directory.GetFiles(GrayImagePath)
                .AsParallel()
                .WithDegreeOfParallelism(AppSettings.DegreeOfParallelism)
                .ForAll(file => ProcessGrayImage(file));
        }

        private void ProcessGrayImage(string file)
        {
            var sw = Stopwatch.StartNew();

            Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " start: " + file);

            var target = Path.Combine(ProcessedImagePath, Path.GetFileName(file));
            ExecuteJpegtranOptimizeAndGrayscale(file, target);

            Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " end  : " + file + " : " + sw.ElapsedMilliseconds);
        }

        private void ExecuteJpegtranOptimizeAndGrayscale(string source, string target)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = Path.Combine(AppSettings.UtilityDirectory.Mozjpeg, "jpegtran.exe"),
                    Arguments = $@"-copy none -optimize -grayscale -outfile ""{target}"" ""{source}""",
                    RedirectStandardOutput = true,
                }
            };

            process.Start();
            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }

        private void ProcessTextImages()
        {
            var bmpPath = Path.Combine(TextImagePath, "bmp");

            if (!Directory.Exists(bmpPath))
            {
                Directory.CreateDirectory(bmpPath);
            }

            var pngPath = Path.Combine(TextImagePath, "png");

            if (!Directory.Exists(pngPath))
            {
                Directory.CreateDirectory(pngPath);
            }

            Directory.GetFiles(TextImagePath)
                .AsParallel()
                .WithDegreeOfParallelism(AppSettings.DegreeOfParallelism)
                .ForAll(file => ProcessTextImage(bmpPath, pngPath, file));
        }

        private void ProcessTextImage(string bmpPath, string pngPath, string file)
        {
            var sw = Stopwatch.StartNew();

            Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " start: " + file);

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

            var bmpFile = Path.Combine(bmpPath, fileNameWithoutExtension + ".bmp");
            ExecuteDjpeg(file, bmpFile);
            var pngTempFile = Path.Combine(pngPath, fileNameWithoutExtension + ".png");
            ExecuteImageMagick(bmpFile, pngTempFile);
            var optimizedPngFile = Path.Combine(ProcessedImagePath, fileNameWithoutExtension + ".png");
            ExecutePngquant(pngTempFile, optimizedPngFile);

            Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " end  : " + file + " : " + sw.ElapsedMilliseconds);
        }

        private void ExecuteDjpeg(string source, string target)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = Path.Combine(AppSettings.UtilityDirectory.Mozjpeg, "djpeg.exe"),
                    Arguments = $@"-bmp -grayscale -outfile ""{target}"" ""{source}""",
                    RedirectStandardOutput = true,
                }
            };

            process.Start();
            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }

        private void ExecuteImageMagick(string source, string target)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = Path.Combine(AppSettings.UtilityDirectory.ImageMagick, "convert.exe"),
                    Arguments = $@"""{source}"" ""{target}""",
                    RedirectStandardOutput = true,
                }
            };

            process.Start();
            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }

        private void ExecutePngquant(string source, string target)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = Path.Combine(AppSettings.UtilityDirectory.Pngquant, "pngquant.exe"),
                    Arguments = $@"--speed 1 --output "".\_p\{Path.GetFileName(target)}"" 4 "".\_t\png\{Path.GetFileName(source)}""",
                    RedirectStandardOutput = true,
                    WorkingDirectory = BookPath,
                }
            };

            process.Start();
            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }

        private void ArchiveImages()
        {
            var target = Path.Combine(RootPath, Title + ".zip");
            SevenZip(target, ProcessedImagePath);
        }

        private void SevenZip(string target, string source)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = Path.Combine(AppSettings.UtilityDirectory.SevenZip, "7z.exe"),
                    Arguments = $@"a -mx0 ""{target}"" ""{source}\*""",
                    RedirectStandardOutput = true,
                    WorkingDirectory = BookPath,
                }
            };

            process.Start();
            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }

        private void hoge()
        {
            // あとしまつ
        }
    }
}
