using Microsoft.AspNetCore.Mvc;
using Tesseract;

namespace tesseract_test.Controllers;

[ApiController]
[Route("[controller]")]
public class TesseracController : ControllerBase
{

    [HttpPost]
    public List<OcrResult> Post2(IFormFile file)
    {
        var engineModes = Enum.GetNames(typeof(EngineMode)).ToList();
        var pageSegModes = Enum.GetNames(typeof(PageSegMode)).ToList();

        var result = new List<OcrResult>();

        foreach (var engineMode in engineModes)
        {
            foreach (var pageSegMode in pageSegModes)
            {
                try
                {
                    using var eng = new TesseractEngine(@"./tessdata", "eng", (EngineMode)Enum.Parse(typeof(EngineMode), engineMode, true));
                    eng.DefaultPageSegMode = (PageSegMode)Enum.Parse(typeof(PageSegMode), pageSegMode, true);

                    using var memoryStream = new MemoryStream();
                    file.CopyTo(memoryStream);
                    var fileBytes = memoryStream.ToArray();

                    using var img = Pix.LoadFromMemory(fileBytes);
                    using var page = eng.Process(img);

                    var text = page.GetText();
                    var meanConfidence = page.GetMeanConfidence();

                    result.Add(new OcrResult
                    {
                        Text = text,
                        EngineMode = engineMode,
                        PageSegMode = pageSegMode,
                        MeanConfidence = meanConfidence
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EX: " + ex.ToString());
                }
            }
        }

        return result
            .Where(x => x.MeanConfidence > 0)
            .OrderByDescending(x => x.MeanConfidence)
            .ToList();
    }

    [HttpPost]
    public List<OcrResult> Post(IFormFile file)
    {
        var engineModes = Enum.GetNames(typeof(EngineMode)).ToList();
        var pageSegModes = Enum.GetNames(typeof(PageSegMode)).ToList();

        var result = new List<OcrResult>();

        foreach (var item in engineModes)
        {
            foreach (var item2 in pageSegModes)
            {
                try
                {
                    using var eng = new TesseractEngine(@"./tessdata", "eng", (EngineMode)Enum.Parse(typeof(EngineMode), item, true));
                    eng.DefaultPageSegMode = (PageSegMode)Enum.Parse(typeof(PageSegMode), item2, true);

                    using var memoryStream = new MemoryStream();
                    file.CopyTo(memoryStream);
                    var fileBytes = memoryStream.ToArray();

                    using var img = Pix.LoadFromMemory(fileBytes);
                    using var page = eng.Process(img);

                    var text = page.GetText();
                    var meanConfidence = page.GetMeanConfidence();

                    result.Add(new OcrResult
                    {
                        Text = text,
                        EngineMode = item.ToString(),
                        PageSegMode = item2.ToString(),
                        MeanConfidence = meanConfidence
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EX: " + ex.ToString());
                }
            }
        }

        return result
            .Where(x => x.MeanConfidence > 0)
            .OrderByDescending(x => x.MeanConfidence)
            .ToList();
    }
}