using Microsoft.AspNetCore.Mvc;
using Tesseract;

namespace tesseract_test.Controllers;

[ApiController]
[Route("[controller]")]
public class TesseracController : ControllerBase
{

    [HttpPost]
    public List<OcrResult> Post(IFormFile file)
    {
        var notValidPageSegModes = new List<string>() { "Count", "OsdOnly" };

        var engineModes = Enum.GetNames(typeof(EngineMode)).ToList();
        var pageSegModes = Enum.GetNames(typeof(PageSegMode)).Where(x => !notValidPageSegModes.Contains(x)).ToList();

        var result = new List<OcrResult>();

        var initVars = new Dictionary<string, object>() {
            { "load_system_dawg", true },
            { "user_words_suffix", "user-words" }
        };

        foreach (var engineMode in engineModes)
        {
            Console.WriteLine("1");
            using var eng = new TesseractEngine(@"./tessdata", "eng", (EngineMode)Enum.Parse(typeof(EngineMode), engineMode, true), Enumerable.Empty<string>(), initVars, false);
            Console.WriteLine("2");

            foreach (var pageSegMode in pageSegModes)
            {
                try
                {
                    using var memoryStream = new MemoryStream();
                    file.CopyTo(memoryStream);
                    var fileBytes = memoryStream.ToArray();

                    using var img = Pix.LoadFromMemory(fileBytes);
                    using var page = eng.Process(img, (PageSegMode)Enum.Parse(typeof(PageSegMode), pageSegMode, true));

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
                    result.Add(new OcrResult
                    {
                        Text = ex.ToString(),
                        EngineMode = engineMode,
                        PageSegMode = pageSegMode,
                        MeanConfidence = 0
                    });
                }
            }
        }

        return result
            .OrderByDescending(x => x.MeanConfidence)
            .ToList();
    }
}